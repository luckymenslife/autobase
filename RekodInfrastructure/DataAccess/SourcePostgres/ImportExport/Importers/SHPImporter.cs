using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.IO;
using System.Data;
using OGRFramework;
using System.ComponentModel;
using PgM = Rekod.DataAccess.SourcePostgres.Model;
using PgVM = Rekod.DataAccess.SourcePostgres.ViewModel;
using Npgsql;
using Rekod.Services;
using Rekod.ProjectionSelection; 

namespace Rekod.DataAccess.SourcePostgres.ImportExport.Importers
{
    public class SHPImporter : Importer
    {
        private FileInfo _inputFile;
        private PgM.PgTableBaseM _pgTable;
        private NpgsqlConnectionStringBuilder _connect;

        private SHPUC _shpUc;
        private int _rowsCount;
        private SHPWork _shpWork;
        private bool _loadGeom;
        private int? _srid;
        private bool _isMulti;

        override public UserControl SettingsPanel { get { return _shpUc; } }
        override public int RowsCount { get { return _rowsCount; } }

        public SHPImporter() { _shpUc = new SHPUC(); }

        override public void Init(FileInfo inputFile, PgM.PgTableBaseM pgTable)
        {
            _inputFile = inputFile;
            _pgTable = pgTable;
            if (pgTable != null)
                _connect = (pgTable.Source as PgVM.PgDataRepositoryVM).Connect; 

            _shpWork = new SHPWork(inputFile);
            _rowsCount = _shpWork.GetFeatureCount();
            WorkerReportsProgress = true;
            try
            {
                _srid = _shpWork.getSRID();
            }
            catch (Exception ex)
            {
                SelectProjectionV frmProj = new SelectProjectionV();
                SelectProjectionVM datacontext = new SelectProjectionVM(frmProj);
                frmProj.DataContext = datacontext;
                if (frmProj.ShowDialog() == true)
                {
                    var proj = datacontext.SelectedProj;
                    if (proj != null)
                        _srid = proj.Srid;
                }
                else
                {
                    throw new Exception("Не указана проекция слоя!");
                }
            }
        }

        override public DataTable GetPreviewTable(int previewRowsCount = 100)
        {
            DataTable result = new DataTable();
            foreach (var f in _shpWork.GetShapeFields())
                result.Columns.Add(f.Key, f.Value);
            _shpWork.StartRead();
            for (int i = 0; i < previewRowsCount && _shpWork.Read(); i++)
            {
                List<object> colData = new List<object>();
                for (int j = 0; j < result.Columns.Count; j++)
                    colData.Add(_shpWork[j]);
                result.Rows.Add(colData.ToArray());
            }
            _shpWork.EndRead();
            return result;
        }

        public int GetGeometryType()
        {
            return classesOfMetods.GetIntGeomType(_shpWork.getGeomName().ToUpper());
        }

        protected override void OnDoWork(DoWorkEventArgs e)
        {
            try
            {
                var pms = (object[])e.Argument;
                load((List<FieldMatch>)pms[0]);
            }
            catch (Exception ex)
            {
                e.Result = ex.Message;
            }
        }

        override public void Load(List<FieldMatch> fields)
        {
            _loadGeom = (bool)_shpUc.loadGeomCB.IsChecked;
            Load(fields, _loadGeom);
        }
        public void Load(List<FieldMatch> fields, bool loadGeom)
        {
            this._loadGeom = loadGeom;

            // ищем тип геометрии в таблице
            string pgTableGeom = _pgTable.GeomType.ToString();
            _isMulti = pgTableGeom.StartsWith("MULTI");

            pgTableGeom = (!_isMulti ? "MULTI" : "") + pgTableGeom;
            
            if (loadGeom && _shpWork.getGeomName() != pgTableGeom)
                throw new Exception(Rekod.Properties.Resources.SHPImporter_ExceptTypeGeom);
            else
                RunWorkerAsync(new object[] { fields });
        }

        private void load(List<FieldMatch> fields)
        {
            if (fields.Count == 0 && _loadGeom == false)
                return;

            // размер блока для загрузки в БД
            int rowsInSect = Math.Min(100, Math.Max(1, RowsCount / 10));

            // список параметров для загрузки в БД
            List<Interfaces.Params>[] paramList = new List<Interfaces.Params>[rowsInSect];

            // названия колонок таблицы
            List<String> columns = new List<string>();

            string wktTo = GetSRText(_pgTable.Srid);
            string wktFrom = GetSRText(_srid);
            
            //инициируем считывание
            _shpWork.StartRead();

            List<string> errors = new List<string>();

            using (var sw = new SqlWork(_connect, true))
            {
                sw.BeginTransaction();
                sw.sql = FormQuerySQL(fields, ref paramList, ref columns);

                for (int i = 0, report = 0; _shpWork.Read() && report < RowsCount; report++)
                {
                    //добавляем в таблицу
                    for (int j = 0; j < columns.Count; j++)
                    {
                        try
                        {
                            if (_loadGeom == true && j == columns.Count - 1)
                            {
                                ImporterHelper.SetParamValue(
                                    paramList[i][j],
                                    _shpWork.TransformedCurrentGeometry(wktTo, wktFrom));
                            }
                            else
                            {
                                ImporterHelper.SetParamValue(
                                    paramList[i][j],
                                    _shpWork[columns[j]]);
                            }
                        }
                        catch (Exception e)
                        {
                            if (!errors.Contains(e.Message))
                                errors.Add(e.Message);
                        }
                    }

                    if (++i >= rowsInSect || report == RowsCount - 1)
                    {
                        //кладем в базу
                        for (int k = 0; k < i; k++)
                        {
                            try
                            {
                                sw.ExecuteNonQuery(paramList[k].ToArray());
                            }
                            catch (Exception e)
                            {
                                if (!errors.Contains(e.Message))
                                    errors.Add(e.Message);
                            }
                        }
                        i = 0;
                        ReportProgress(0, report);
                    }
                }
                _shpWork.EndRead();
                sw.EndTransaction();

                if (errors.Count > 0)
                    throw new Exception(String.Join(Environment.NewLine, errors));
            }
        }

        /// <summary>
        /// Выгружаем поля
        /// </summary>
        private string FormQuerySQL(List<FieldMatch> fields, ref List<Interfaces.Params>[] paramList, ref List<String> columns)
        {
            for (int i = 0; i < paramList.Count(); i++)
            {
                paramList[i] = new List<Interfaces.Params>();
            }

            string fieldsStr = String.Empty;
            string valuesStr = String.Empty;

            foreach (var f in fields)
            {
                fieldsStr += String.Format("\"{0}\",", f.Dest.Name);
                valuesStr += String.Format(":{0},", f.Dest.Name);

                var p = new Interfaces.Params() { paramName = f.Dest.Name };
                ImporterHelper.SetParamType(f.Dest, p);

                foreach (var par in paramList)
                    par.Add(p.Clone());

                columns.Add(f.Src);
            }

            if (!_loadGeom)
            {
                fieldsStr = fieldsStr.TrimEnd(',');
                valuesStr = valuesStr.TrimEnd(',');
            }
            else
            {
                // добавляем поле геометрии

                string geomSql = string.Format("st_geomfromwkb(:{0},{1})", _pgTable.GeomField, _pgTable.Srid);

                // нужно ли привести геометрию к мульти
                if (_isMulti)
                    geomSql = String.Format("st_multi({0})", geomSql);

                valuesStr += geomSql;
                fieldsStr += _pgTable.GeomField;
                columns.Add(_pgTable.GeomField);

                var p = new Interfaces.Params() { paramName = _pgTable.GeomField, type = DbType.Binary, typeData = NpgsqlTypes.NpgsqlDbType.Bytea };

                foreach (var par in paramList)
                    par.Add(p.Clone());
            }

            return string.Format("INSERT INTO {0}.{1} ({2}) VALUES ({3});",
                    _pgTable.SchemeName, _pgTable.Name, fieldsStr, valuesStr);
        }

        /// <summary>
        /// Получаем srtext проекции
        /// </summary>
        private string GetSRText(int? srid)
        {
            if (srid != null)
            {
                using (var swork = new SqlWork())
                {
                    swork.sql = string.Format(
                        "SELECT srtext from spatial_ref_sys WHERE srid={0};",
                        srid);
                    swork.ExecuteReader();
                    if (swork.CanRead())
                    {
                        return swork.GetString(0);
                    }
                }
            }
            return null;
        }
    }
}