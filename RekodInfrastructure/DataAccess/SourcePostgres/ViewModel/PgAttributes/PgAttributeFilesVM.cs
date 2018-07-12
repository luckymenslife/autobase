using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using PgM = Rekod.DataAccess.SourcePostgres.Model;
using PgVM = Rekod.DataAccess.SourcePostgres.ViewModel;
using PgAtM = Rekod.DataAccess.SourcePostgres.Model.PgAttributes;
using PgAtVM = Rekod.DataAccess.SourcePostgres.ViewModel.PgAttributes;
using AbsVM = Rekod.DataAccess.AbstractSource.ViewModel;
using AbsM = Rekod.DataAccess.AbstractSource.Model;
using Rekod.DataAccess.AbstractSource;
using Interfaces;
using System.Drawing;
using Rekod.Controllers;
using System.IO;
using System.Data;
using System.Windows.Input;
using System.Diagnostics;
using System.Threading;
using Rekod.Controllers;
using Rekod.Behaviors;
using System.Windows.Controls;
using System.Windows;
using Rekod.Services;

namespace Rekod.DataAccess.SourcePostgres.ViewModel.PgAttributes
{
    // todo: (Dias) ПРодумать концепцию загрузки, удаления, выгрузки файлов в потоках
    public class PgAttributeFilesVM : ViewModelBase
    {
        #region Поля
        private PgAttributesVM _attributeVM;
        private readonly Npgsql.NpgsqlConnectionStringBuilder _connect;
        private bool _isReadOnly;

        PgAtM.IPgAttributesVM _attribute;

        private readonly bool _isDebug = false;
        private readonly ObservableCollection<PgAtM.PgAttributeFileM> _files;
        private readonly ObservableCollection<PgAtM.PgAttributeFileM> _filesSelected;

        private bool _isLoad;

        private ICommand _reloadCommand;
        private ICommand _addCommand;
        private ICommand _deleteCommand;
        private ICommand _saveCommand;
        private ICommand _PreviewCommand;
        #endregion // Поля

        #region Свойства
        /// <summary>
        /// VM атрибута объекта
        /// </summary>
        public PgAtVM.PgAttributesVM AttributeVM
        {
            get { return _attributeVM; }
        }

        /// <summary>
        /// Список файлов в объекте
        /// </summary>
        public IEnumerable<PgAtM.PgAttributeFileM> Files
        { get { return _files; } }
        /// <summary>
        /// Список выделеных файлов
        /// </summary>
        public ObservableCollection<PgAtM.PgAttributeFileM> FilesSelected
        { get { return _filesSelected; } }

        public bool IsLoad
        {
            get { return _isLoad; }
            set { OnPropertyChanged(ref _isLoad, value, () => this.IsLoad); }
        }
        #endregion // Свойства

        #region Конструктор
        public PgAttributeFilesVM(PgAtVM.PgAttributesVM attributeVM)
        {
            _files = new MTObservableCollection<PgAtM.PgAttributeFileM>();
            _filesSelected = new ObservableCollection<PgAtM.PgAttributeFileM>();

            if (attributeVM.Table.FileInfo == null)
                throw new ArgumentNullException("attributeVM.Table.FileInfo");
            _attributeVM = attributeVM;

            _isDebug = AttributeVM.IsDebug;
            _connect = attributeVM.Connect;
            _isReadOnly = attributeVM.IsReadOnly;

        }
        #endregion // Конструктор

        #region Команды
        #region ReloadCommand
        /// <summary> 
        /// Загрузка атрибутов объекта из базы и их значения
        /// </summary>
        public ICommand ReloadCommand
        {
            get { return _reloadCommand ?? (_reloadCommand = new RelayCommand(Reload, CanReload)); }
        }
        public void Reload(object param = null)
        {
            if (!CanReload(param)) return;
            //new Thread(delegate()
            //    {
                    List<PgAtM.PgAttributeFileM> listFiles = new List<PgAtM.PgAttributeFileM>();

                    using (var SQLCmd = new SqlWork(_connect))
                    {
                        SQLCmd.sql = string.Format(@"
                                    SELECT ""{2}"", ""{4}"", dataupd, img_preview, file_name, is_photo
                                    FROM ""{0}"".""{1}""
                                    WHERE ""{4}"" = :id_obj
                                    ORDER BY file_name",
                                   AttributeVM.Table.SchemeName,
                                   AttributeVM.Table.FileInfo.TableName,
                                   AttributeVM.Table.FileInfo.FieldId,
                                   AttributeVM.Table.FileInfo.FieldFile,
                                   AttributeVM.Table.FileInfo.FieldIdObj);
                        var Params = new IParams[]
                                       {
                                           new Params(":id_obj", AttributeVM.AttributesListVM.PkAttribute.Value, (AttributeVM.AttributesListVM.PkAttribute.Field as PgM.PgFieldM).DbType)
                                       };
                        SQLCmd.ExecuteReader(Params);
                        bool hasPreview = false;
                        while (SQLCmd.CanRead())
                        {
                            var id = SQLCmd.GetInt32(AttributeVM.Table.FileInfo.FieldId);
                            var file = Files.FirstOrDefault(f => f.IdFile == id);
                            var fileName = SQLCmd.GetString("file_name");
                            if (file == null)
                            {

                                var preview = SQLCmd.GetBytes("img_preview");
                                hasPreview = (preview != null);

                                if (!hasPreview)
                                {
                                    preview = FilePreviewController.GetIcon(System.IO.Path.GetExtension(fileName));
                                }
                                var imgPreview = FilePreviewController.GetBitmapImage(preview);

                                file = new PgAtM.PgAttributeFileM(
                                                  source: this,
                                                  idFile: id,
                                                  imgPreview: imgPreview);
                            }
                            file.Date = SQLCmd.GetValue<DateTime>("dataupd");
                            file.FileName = fileName;
                            listFiles.Add(file);
                        }
                        ExtraFunctions.Sorts.SortList(_files, listFiles);
                    }
                //}).Start();
        }
        bool CanReload(object param = null)
        {
            return true;
        }
        #endregion // GetFilesCommand

        #region AddCommand
        public ICommand AddCommand
        {
            get
            {
                return _addCommand ?? (_addCommand
                    = new RelayCommand(this.Add, this.CanAdd));
            }
        }
        void Add(object param = null)
        {
            if (!CanAdd(param)) return;

            string[] fileNames = FuncOpenDialogToAddFilesExecution();
            //new Thread(delegate()
            //    {
                    for (int i = 0; i < fileNames.Length; i++)
                    {
                        string fileName = fileNames[i];
                        byte[] myData = GetFile(fileName);
                        byte[] imgPreview = null;

                        if (myData == null)
                            continue;
                        if (myData.Length > 5242880)
                        {
                            string sizeFile = ConvertSizeFile(myData.Length);
                            var result = System.Windows.MessageBox.Show("Файл превышает максимальный размер в 5 МБ (текущий: " + sizeFile + ")!\nХотите пропустить файл или отменить загрузку?\n\nДа  - пропустить файл.\nНет - прервать загрузку.", "Превышение размера файла!", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Warning);
                            if (result == System.Windows.MessageBoxResult.Yes)
                                continue;
                            else
                                break;
                        }
                        Bitmap preview = FilePreviewController.GetPreviewPhoto(myData, Path.GetExtension(fileName));
                        if (preview != null)
                            imgPreview = preview.ToByteArray();

                        bool isPhoto = (imgPreview != null);

                        using (var sqlCmd = new SqlWork(_connect))
                        {
                            string sql =
                                string.Format(
                                    @"
            INSERT INTO ""{0}"".""{1}"" ({2}, {3}, img_preview, file_name, is_photo) 
            VALUES (:id_obj, :file_blob, :img_preview, :file_name, :is_photo);",
                                    AttributeVM.Table.SchemeName,
                                    AttributeVM.Table.FileInfo.TableName,
                                    AttributeVM.Table.FileInfo.FieldIdObj,
                                    AttributeVM.Table.FileInfo.FieldFile);

                            sqlCmd.sql = sql;

                            var pk = AttributeVM.AttributesListVM.PkAttribute;
                            var pParams = new IParams[]
                                              {
                                                  new Params (":id_obj", pk.Value, (pk.Field as PgM.PgFieldM).DbType),
                                                  new Params (":file_blob", myData, NpgsqlTypes.NpgsqlDbType.Bytea),
                                                  new Params (":img_preview", imgPreview, NpgsqlTypes.NpgsqlDbType.Bytea),
                                                  new Params ( ":file_name",Path.GetFileName(fileName), NpgsqlTypes.NpgsqlDbType.Text),
                                                  new Params(":is_photo", isPhoto, NpgsqlTypes.NpgsqlDbType.Boolean)
                                              };
                            sqlCmd.ExecuteNonQuery(pParams);
                        }
                    }
                    Reload();
                //}).Start();
        }
        bool CanAdd(object param = null)
        {
            return !_isReadOnly;
        }
        #endregion AddCommand

        #region DeleteCommand
        public ICommand DeleteCommand
        {
            get
            {
                return _deleteCommand ?? (_deleteCommand
                    = new RelayCommand(this.Delete, this.CanDelete));
            }
        }
        void Delete(object param = null)
        {
            if (!CanDelete(param)) return;

            List<IParams> ListParams = new List<IParams>();
            List<string> ListSQL = new List<string>();
            for (int i = 0; i < FilesSelected.Count; i++)
            {
                ListParams.Add(new Params(":id_file" + i, FilesSelected[i].IdFile, DbType.Int32));
                ListSQL.Add(string.Format(
                                    @"
                                DELETE 
                                FROM ""{0}"".""{1}"" 
                                WHERE ""{2}"" = :id_file{3}
                                RETURNING true;",
                        AttributeVM.Table.SchemeName,
                        AttributeVM.Table.FileInfo.TableName,
                        AttributeVM.Table.FileInfo.FieldId,
                        i));
            }
            using (var sqlCmd = new SqlWork(_connect))
            {
                sqlCmd.sql = string.Join("\n", ListSQL.ToArray());

                sqlCmd.ExecuteNonQuery(ListParams);
            }
            Reload();
        }
        bool CanDelete(object param = null)
        {
            return !_isReadOnly && FilesSelected.Count > 0;
        }
        #endregion  DeleteCommand

        #region SaveCommand
        public ICommand SaveCommand
        {
            get
            {
                return _saveCommand ?? (_saveCommand
                    = new RelayCommand(this.Save, this.CanSave));
            }
        }
        void Save(object param = null)
        {
            if (!CanSave(param)) return;
            Dictionary<PgAtM.PgAttributeFileM, string> DicFiles = new Dictionary<PgAtM.PgAttributeFileM, string>();
            if (FilesSelected.Count == 1)
            {
                var dialogFile = new System.Windows.Forms.SaveFileDialog();
                dialogFile.FileName = FilesSelected[0].FileName;
                System.Windows.Forms.DialogResult result = dialogFile.ShowDialog();
                if (result != System.Windows.Forms.DialogResult.OK)
                    return;
                DicFiles.Add(FilesSelected[0], dialogFile.FileName);
            }
            else
            {
                var dialog = new System.Windows.Forms.FolderBrowserDialog();
                dialog.Description = "Укажите папку для сохранения файлов...";
                dialog.ShowNewFolderButton = true;
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result != System.Windows.Forms.DialogResult.OK)
                    return;
                for (int i = 0; i < FilesSelected.Count; i++)
                {
                    DicFiles.Add(FilesSelected[i], System.IO.Path.Combine(dialog.SelectedPath, FilesSelected[i].FileName));
                }
            }
            var thread = new Thread(
                delegate()
                {
                    IsLoad = true;
                    foreach (var item in DicFiles)
                    {
                        string pathFile = item.Value;
                        int idFile = item.Key.IdFile;
                        var table = AttributeVM.Table;
                        SaveFile(pathFile, idFile, table);
                    }
                    IsLoad = false;
                });
            thread.Start();
        }
        bool CanSave(object param = null)
        {
            return FilesSelected.Count > 0;
        }
        #endregion DeleteCommand

        #region PreviewCommand
        /// <summary>
        /// Открывает файл для предпросмотра
        /// </summary>
        public ICommand PreviewCommand
        {
            get { return _PreviewCommand ?? (_PreviewCommand = new RelayCommand(this.Preview, this.CanPreview)); }
        }
        /// <summary>
        /// Открывает файл для предпросмотра
        /// </summary>
        private void Preview(object obj = null)
        {
            if (!CanPreview(obj))
                return;
            CommandEventParameter commEvtParam = obj as CommandEventParameter;
            ListBox listBox = commEvtParam.EventSender as ListBox;
            if (listBox != null)
            {
                var value = listBox.SelectedValue as PgAtM.PgAttributeFileM;
                string pathTemp = Path.GetTempPath() + value.FileName;
                //var thread = new Thread(
                //    delegate()
                //    {
                IsLoad = true;
                SaveFile(pathTemp, value.IdFile, AttributeVM.Table);
                OpenProgram(pathTemp);
                IsLoad = false;
                //    });
                //thread.Start();
            }
        }
        private bool CanPreview(object obj = null)
        {
            return (obj is PgAtM.PgAttributeFileM || obj is CommandEventParameter); 
        }
        #endregion PreviewCommand
        #endregion Команды

        #region Методы
        private string[] FuncOpenDialogToAddFilesExecution()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Multiselect = true;
            if (dlg.ShowDialog() != true)
                return new string[0];

            return dlg.FileNames;
        }
        private byte[] GetFile(string fileName)
        {
            byte[] myData = null;
            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                myData = new byte[fs.Length];
                fs.Read(myData, 0, (int)fs.Length);
                fs.Close();
            }
            return myData;
        }

        private void SaveFile(string pathFile, int idFile, PgM.PgTableBaseM table)
        {
            FileStream FS;
            BinaryWriter BW;
            FS = new FileStream(pathFile, FileMode.OpenOrCreate, FileAccess.Write);
            BW = new BinaryWriter(FS);
            var Param = new IParams[] 
                { 
                    new Params(":id_file", idFile, DbType.Int32)
                };
            using (SqlWork sqlCmd = new SqlWork((table.Source as PgVM.PgDataRepositoryVM).Connect))
            {
                sqlCmd.sql = string.Format(@"SELECT ""{2}"" FROM ""{0}"".""{1}"" WHERE id = :id_file;",
                        table.SchemeName,
                        table.FileInfo.TableName,
                        table.FileInfo.FieldFile);

                sqlCmd.ExecuteReader(Param);

                if (sqlCmd.CanRead())
                {
                    BW.Write(sqlCmd.GetBytes(0));
                }
            }
            BW.Close();
            FS.Close();
        }

        /// <summary> Запуск програмы
        /// </summary>
        void OpenProgram(string pathFile)
        {
            try
            {
                var toLaunch = new Process
                {
                    StartInfo =
                    {
                        FileName = pathFile
                    }
                };
                toLaunch.Start();
            }
            catch (Exception ex)
            {
                // Ошибка при запуске приложения. Возможный выход - попросить пользователя самому запустить 
                MessageBox.Show("Не удалось запустить файл " + pathFile + ".\nОшибка: " + ex.Message);
            }
        }

        public static string ConvertSizeFile(double size)
        {
            string[] units = { "Б", "КБ", "МБ", "ГБ", "ТБ", "ПБ", "ЭБ", "ЗБ", "ЙБ" };

            int unit = 0;

            while (size >= 1024)
            {
                size /= 1024;
                ++unit;
            }
            if (size == 0)
                return String.Format("0 {0}", units[unit]);
            else
                return String.Format("{0:0.0} {1}", size, units[unit]);
        }
        #endregion // Методы
    }
}
