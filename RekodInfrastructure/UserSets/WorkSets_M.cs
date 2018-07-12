using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Interfaces;
using Rekod.Controllers;
using Rekod.DataAccess.AbstractSource.ViewModel;
using Rekod.Services;
using AbsM = Rekod.DataAccess.AbstractSource.Model;
using PgM = Rekod.DataAccess.SourcePostgres.Model;

namespace Rekod.UserSets
{
    public class WorkSets_M : ViewModelBase
    {
        #region Поля
        private WorkSetItem_S _currentWorkSet;
        private WorkSetItem_S _defaultWorkSet;
        private MTObservableCollection<WorkSetItem_S> _listWorksets;

        #endregion // Поля

        #region Свойства
        public int IdUser
        {
            get { return Program.id_user; }
        }
        public WorkSetItem_S CurrentWorkSet
        {
            get { return _currentWorkSet; }
        }
        public WorkSetItem_S DefaultWorkSet
        {
            get { return _defaultWorkSet; }
        }
        public IEnumerable<WorkSetItem_S> ListWorkSets
        {
            get { return _listWorksets; }
        }
        #endregion // Свойства

        #region События
        public event NotifyCollectionChangedEventHandler ListWorkSets_CollectionChanged
        {
            add { _listWorksets.CollectionChanged += value; }
            remove { _listWorksets.CollectionChanged -= value; }
        }
        #endregion // События

        #region Конструктор
        public WorkSets_M()
        {
            _listWorksets = new MTObservableCollection<WorkSetItem_S>();
            _defaultWorkSet = new WorkSetItem_S(this, Rekod.Properties.Resources.WorkSets_M_DefaultWorkSetName);
            _currentWorkSet = _defaultWorkSet;
        }
        #endregion Конструктор

        #region Команды
        /// <summary>
        /// Обнавить список рабочих наборов
        /// </summary>
        public void Reload()
        {
            var lSets = new List<WorkSetItem_S>();
            lSets.Add(_defaultWorkSet); // Стандартный набор

            using (SqlWork sqlCmd = new SqlWork())
            {
                sqlCmd.sql = string.Format(@"
                                SELECT 
                                    id, 
                                    name, 
                                    owner_set, 
                                    show_set 
                                FROM 
                                    {0}.sets_styles 
                                WHERE
                                    show_set = TRUE
                                    OR owner_set = {1}
                                    OR {2}
                                ORDER BY owner_set, name;
                                ",
                            Program.scheme,
                            Program.user_info.id_user,
                            Program.user_info.admin);
                sqlCmd.ExecuteReader();
                while (sqlCmd.CanRead())
                {
                    var idSet = sqlCmd.GetInt32("id");

                    var set = FindSet(idSet);
                    if (set == null)
                    {
                        set = new WorkSetItem_S(
                            this,
                            idSet,
                            sqlCmd.GetString("name"),
                            sqlCmd.GetInt32("owner_set"));
                    }
                    else
                    {
                        string name = sqlCmd.GetString("name");
                        set.Name = name;
                    }
                    set.ShowSet = sqlCmd.GetBoolean("show_set");

                    lSets.Add(set);
                }
            }
            ExtraFunctions.Sorts.SortList(_listWorksets, lSets);
        }


        /// <summary>
        /// Добавить или изменить рабочий набор
        /// </summary>
        /// <param name="workSet">рабочий набор</param>
        public void Apply(WorkSetItem_S workSet)
        {
            if (!AccessChecked(workSet))
                throw new Exception(Rekod.Properties.Resources.WorkSets_M_ExceptionAccess);
            if (string.IsNullOrEmpty(workSet.Name))
            {
                throw new Exception(Rekod.Properties.Resources.WorkSets_M_ExceptionWorkSetName);
            }
            var listParams = new List<IParams>
                {
                    new Interfaces.Params(":id", workSet.Id, DbType.Int32),
                    new Interfaces.Params(":name", workSet.Name, DbType.String),
                    new Interfaces.Params(":owner_set", Program.user_info.id_user, DbType.Int32),
                    new Interfaces.Params(":show_set", workSet.ShowSet, DbType.Boolean)
                };
            using (SqlWork sqlCmd = new SqlWork())
            {
                if (workSet.IsNew)
                {
                    sqlCmd.sql = string.Format(@"
                                        INSERT INTO 
                                            {0}.""sets_styles"" 
                                        (
                                            name, owner_set, show_set
                                        ) VALUES (
                                            :name, :owner_set, :show_set
                                        )",
                                    Program.scheme);


                    if (!sqlCmd.ExecuteNonQuery(listParams))
                    {
                        throw new Exception(Rekod.Properties.Resources.WorkSets_M_ExceptionWorkSetAdd);
                    }
                }
                else
                {
                    sqlCmd.sql = string.Format(@"
                                        UPDATE 
                                            {0}.""sets_styles"" 
                                        SET
                                            name = :name, 
                                            show_set = :show_set
                                        WHERE
                                            id = :id
                                        ",
                                    Program.scheme);


                    if (!sqlCmd.ExecuteNonQuery(listParams))
                    {
                        throw new Exception(Rekod.Properties.Resources.WorkSets_M_ExceptionWorkSetEdit);
                    }
                }
            }
            Reload();
        }

        /// <summary>
        /// Удалить рабочий набор
        /// </summary>
        /// <param name="workSet">Рабочий набор</param>
        public void Delete(WorkSetItem_S workSet)
        {
            if (!AccessChecked(workSet))
                throw new Exception(Rekod.Properties.Resources.WorkSets_M_ExceptionAccess);
            using (SqlWork sqlCmd = new SqlWork())
            {
                sqlCmd.sql = string.Format(@"
                                DELETE
                                FROM 
                                    {0}.sets_styles 
                                WHERE 
                                    id = {1}
                                ",
                            Program.scheme,
                            workSet.Id);
                sqlCmd.ExecuteNonQuery();
            }
            if (workSet.Id == CurrentWorkSet.Id)
                SwitchSet(DefaultWorkSet);
            Reload();
        }

        /// <summary>
        /// Загрузить рабочий набор
        /// </summary>
        /// <param name="workSet">Установить текущий набор</param>
        public void SwitchSet(WorkSetItem_S workSet)
        {
            if (workSet == null || workSet == CurrentWorkSet)
                return;
            OnPropertyChanged(ref _currentWorkSet, workSet, () => this.CurrentWorkSet);

            classesOfMetods cls = new classesOfMetods();
            cls.reloadInfo();
            Program.mainFrm1.ReloadApp();

        }
        #endregion Команды

        #region Открытые методы
        /// <summary>
        /// Проверить права на изменения рабочего набора
        /// </summary>
        /// <param name="workSet">Рабочий набор</param>
        /// <returns></returns>
        public bool AccessChecked(WorkSetItem_S workSet)
        {
            if (workSet == null || workSet.IsDefault)
            {
                return false;
            }
            return (workSet.IdUser == Program.user_info.id_user || Program.user_info.admin);
        }
        /// <summary>
        /// Находит набор по его Id
        /// </summary>
        /// <param name="idSet"></param>
        /// <returns></returns>
        public WorkSetItem_S FindSet(int idSet)
        {
            return ListWorkSets.FirstOrDefault(f => f.Id == idSet);
        }

        /// <summary>
        /// Есть ли набор с данным имени
        /// </summary>
        public bool FindNameMatch(WorkSetItem_S workSet)
        {
            return _listWorksets
                .FirstOrDefault(f => 
                    f.Name == workSet.Name 
                    && f.IdUser == workSet.IdUser
                    && f.Id != workSet.Id) != null;
        }

        internal void SaveLocationLayers()
        {
            if (MessageBox.Show(Rekod.Properties.Resources.LIV_SavePositionLayer, Rekod.Properties.Resources.LIV_SavePositionLayerHeader,
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                var layers =
                   from AbsM.TableBaseM tableBase
                       in Program.mainFrm1.layerItemsView1.ListLayersIsView
                   where tableBase is PgM.PgTableBaseM
                   select tableBase.Id;


                if (!CurrentWorkSet.IsDefault)
                {
                    // Пользовательский рабочий набор
                    using (SqlWork sqlCmd = new SqlWork())
                    {
                        sqlCmd.sql = "DELETE FROM sys_scheme.table_order_set WHERE id_set = " + CurrentWorkSet.Id;
                        sqlCmd.ExecuteNonQuery();
                        sqlCmd.Close();
                    }
                    using (SqlWork sqlCmd = new SqlWork())
                    {
                        int index = 0;
                        foreach (var item in layers)
                        {
                            sqlCmd.sql = string.Format(@"
                                        INSERT INTO 
                                            sys_scheme.table_order_set
                                        (
                                            id_set, 
                                            id_table, 
                                            order_num
                                        ) VALUES (
                                            {0}, 
                                            {1}, 
                                            {2}
                                        );
                                    ",
                                    CurrentWorkSet.Id,
                                    item,
                                    index);
                            sqlCmd.ExecuteNonQuery();
                            index++;
                        }
                    }
                }
                else
                {
                    // Стандартный рабочий набор
                    using (SqlWork sqlCmd = new SqlWork())
                    {
                        sqlCmd.sql = "UPDATE " + Program.scheme + ".table_info SET default_visibl = false, order_num = 0;";
                        sqlCmd.ExecuteNonQuery();
                        sqlCmd.Close();
                    }
                    using (SqlWork sqlCmd = new SqlWork())
                    {
                        int index = 0;
                        foreach (var item in layers)
                        {
                            sqlCmd.sql = String.Format("UPDATE {0}.table_info SET order_num = {1}, default_visibl = true WHERE id = {2};",
                                Program.scheme, index, item);
                            sqlCmd.ExecuteNonQuery();
                            index++;
                        }
                    }
                }
            }
        }
        internal void ChangeStyleLayer()
        {
            bool existsStyle = false;
            AbsM.TableBaseM pgTable =
                (from AbsM.TableBaseM tableBase
                     in Program.mainFrm1.layerItemsView1.ListLayersIsView
                 where (tableBase is PgM.PgTableBaseM && Convert.ToInt32(tableBase.Id) == Program.mainFrm1.layerItemsView1.EditableIdLayer)
                 select tableBase).FirstOrDefault();
            if (pgTable != null)
            {
                tablesInfo ti = classesOfMetods.getTableInfo(Convert.ToInt32(pgTable.Id));
                existsStyle = isExistsUserStyle(ti.idTable);
                UserSets.LayerStyleFrm frm = new UserSets.LayerStyleFrm(ti.idTable, existsStyle);
                frm.sc.setStyles(LoadUserStyle(ti.idTable, existsStyle));
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    var style = frm.sc.getStyles();
                    bool bgcolor_white = !frm.sc.checkBox1.Enabled || frm.sc.checkBox1.Checked;
                    if (!existsStyle)
                    {
                        InsertUserStyle(style, bgcolor_white, ti.idTable);
                    }
                    UpdateUserStyle(style, bgcolor_white, ti.idTable);
                    SaveLableExpration(ti.idTable, frm.ls.LableFieldRezult, frm.ls.check_showLabel.Checked);
                    if (frm.ls.lFont != null)
                    {
                        if (frm.ls.lFont.save && !String.IsNullOrEmpty(frm.ls.lFont.SQL))
                        {
                            SaveLableStyle(frm.ls.lFont.SQL);
                        }
                    }
                    classesOfMetods rls = new classesOfMetods();
                    rls.reloadInfo();
                    classesOfMetods.reloadLayer(ti.idTable);
                }
            }
        }

        internal void DeleteStyleLayerSet()
        {
            AbsM.TableBaseM pgTable = 
                Program.mainFrm1.layerItemsView1.ListLayersIsView.FirstOrDefault
                        (f => (f is PgM.PgTableBaseM) && (Convert.ToInt32(f.Id) == Program.mainFrm1.layerItemsView1.EditableIdLayer));
            if(pgTable != null)
            {
                tablesInfo ti = classesOfMetods.getTableInfo(Convert.ToInt32(pgTable.Id));
                if (MessageBox.Show(Rekod.Properties.Resources.LIV_DeleteStyleLayer, Rekod.Properties.Resources.LIV_DeleteStyleLayerHeader,
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    using (SqlWork sqlCmd = new SqlWork())
                    {
                        sqlCmd.sql = "DELETE FROM sys_scheme.table_info_sets WHERE id_set = " + CurrentWorkSet.Id + " AND id_table = " + ti.idTable;
                        sqlCmd.ExecuteNonQuery();
                    }
                    classesOfMetods rls = new classesOfMetods();
                    rls.reloadInfo();
                    classesOfMetods.reloadLayer(ti.idTable);
                }
            }
        }
        #endregion Открытые методы

        #region Методы
        private bool isExistsUserStyle(int idT)
        {
            bool exists = false;
            using (SqlWork sqlCmd = new SqlWork())
            {
                sqlCmd.sql = "SELECT 1 FROM " + Program.scheme + ".table_info_sets WHERE id_table = " + idT.ToString() + " AND id_set = " + Program.WorkSets.CurrentWorkSet.Id;
                sqlCmd.ExecuteReader();
                if (sqlCmd.CanRead())
                {
                    exists = true;
                }
                sqlCmd.Close();
            }
            return exists;
        }
        private axVisUtils.Styles.objStylesM LoadUserStyle(int id_table, bool exists)
        {
            axVisUtils.Styles.objStylesM s1 = new axVisUtils.Styles.objStylesM();

            string sql = "";
            if (exists)
            {
                sql = "SELECT fontname, fontcolor, fontframecolor, fontsize,";
                sql += " symbol,";
                sql += " pencolor, pentype, penwidth,";
                sql += " brushbgcolor, brushfgcolor, brushstyle, brushhatch";
                sql += " FROM " + Program.scheme + ".\"table_info_sets\" WHERE id_table = " + id_table.ToString() + " AND id_set = " + Program.WorkSets.CurrentWorkSet.Id;
            }
            else
            {
                sql = "SELECT fontname, fontcolor, fontframecolor, fontsize,";
                sql += " symbol,";
                sql += " pencolor, pentype, penwidth,";
                sql += " brushbgcolor, brushfgcolor, brushstyle, brushhatch";
                sql += " FROM " + Program.scheme + ".\"table_info\" WHERE id = " + id_table.ToString();
            }
            using (SqlWork sqlCmd = new SqlWork())
            {
                sqlCmd.sql = sql;
                sqlCmd.Execute(false);
                while (sqlCmd.CanRead())
                {
                    s1.FontStyle.FontName = sqlCmd.GetString(0);
                    s1.FontStyle.Color = sqlCmd.GetValue<uint>(1);
                    s1.FontStyle.FrameColor = sqlCmd.GetValue<uint>(2);
                    s1.FontStyle.Size = sqlCmd.GetInt32(3);

                    s1.SymbolStyle.Shape = sqlCmd.GetValue<uint>(4);

                    s1.PenStyle.Color = sqlCmd.GetValue<uint>(5);
                    s1.PenStyle.Type = sqlCmd.GetValue<ushort>(6);
                    s1.PenStyle.Width = sqlCmd.GetValue<uint>(7);

                    s1.BrushStyle.bgColor = sqlCmd.GetValue<uint>(8);
                    s1.BrushStyle.fgColor = sqlCmd.GetValue<uint>(9);
                    s1.BrushStyle.Style = sqlCmd.GetValue<ushort>(10);
                    s1.BrushStyle.Hatch = sqlCmd.GetValue<ushort>(11);
                }
                sqlCmd.Close();
            }
            return s1;
        }
        private void InsertUserStyle(axVisUtils.Styles.objStylesM style, bool bg, int idT)
        {
            string bgColor = "";
            if (!bg)
            {
                bgColor = "4294967295";
            }
            else
            {
                bgColor = style.BrushStyle.bgColor.ToString();
            }

            SqlWork sqlCmd = new SqlWork();
            sqlCmd.sql = @"INSERT INTO sys_scheme.table_info_sets(
            id_table, id_set, scheme_name, name_db, name_map, lablefiled, 
            map_style, geom_field, style_field, geom_type, type, default_style, 
            fontname, fontcolor, fontframecolor, fontsize, symbol, pencolor, 
            pentype, penwidth, brushbgcolor, brushfgcolor, brushstyle, brushhatch, 
            read_only, photo, id_style, pk_fileld, is_style, source_layer, 
            image_column, angle_column, use_bounds, min_scale, max_scale, 
            id_group, default_visibl, view_name, order_num, sql_view_string, 
            masterdb_history_id, connection_string, remote_lgn, remote_pwd, 
            fixed_history_id, range_colors, range_column, precision_point, 
            type_color, min_color, min_val, max_color, max_val, use_min_val, 
            null_color, use_null_color, hidden, use_max_val, label_showframe, 
            label_framecolor, label_parallel, label_overlap, label_usebounds, 
            label_minscale, label_maxscale, label_offset, label_graphicunits, 
            label_fontname, label_fontcolor, label_fontsize, label_fontstrikeout, 
            label_fontitalic, label_fontunderline, label_fontbold, label_uselabelstyle, 
            label_showlabel, min_object_size, ref_table, graphic_units)
            SELECT id, " + Program.WorkSets.CurrentWorkSet.Id + @", scheme_name, name_db, name_map, lablefiled, 
            map_style, geom_field, style_field, geom_type, type, true, 
            fontname, fontcolor, fontframecolor, fontsize, symbol, pencolor, 
            pentype, penwidth, brushbgcolor, brushfgcolor, brushstyle, brushhatch, 
            read_only, photo, id_style, pk_fileld, is_style, source_layer, 
            image_column, angle_column, use_bounds, min_scale, max_scale, 
            id_group, default_visibl, view_name, order_num, sql_view_string, 
            masterdb_history_id, connection_string, remote_lgn, remote_pwd, 
            fixed_history_id, false, range_column, precision_point, 
            type_color, min_color, min_val, max_color, max_val, use_min_val, 
            null_color, use_null_color, hidden, use_max_val, label_showframe, 
            label_framecolor, label_parallel, label_overlap, label_usebounds, 
            label_minscale, label_maxscale, label_offset, label_graphicunits, 
            label_fontname, label_fontcolor, label_fontsize, label_fontstrikeout, 
            label_fontitalic, label_fontunderline, label_fontbold, label_uselabelstyle, 
            label_showlabel, min_object_size, ref_table, graphic_units
            FROM sys_scheme.table_info
            WHERE id = " + idT.ToString() + ";";
            sqlCmd.Execute(true);
            sqlCmd.Close();
        }
        private void UpdateUserStyle(axVisUtils.Styles.objStylesM style, bool bg, int idT)
        {
            string bgColor = "";
            if (!bg)
            {
                bgColor = "4294967295";
            }
            else
            {
                bgColor = style.BrushStyle.bgColor.ToString();
            }

            SqlWork sqlCmd = new SqlWork();
            sqlCmd.sql = "UPDATE " + Program.scheme + ".table_info_sets SET fontname = '" + style.FontStyle.FontName + "', " +
                "fontcolor = " + style.FontStyle.Color.ToString() + ", " +
                "fontframecolor = " + style.FontStyle.FrameColor.ToString() + ", " +
                "fontsize = " + style.FontStyle.Size.ToString() + ", " +
                "symbol = " + style.SymbolStyle.Shape.ToString() + ", " +
                "pencolor = " + style.PenStyle.Color.ToString() + ", " +
                "pentype = " + style.PenStyle.Type.ToString() + ", " +
                "penwidth = " + style.PenStyle.Width.ToString() + ", " +
                "brushbgcolor = " + bgColor + ", " +
                "brushfgcolor = " + style.BrushStyle.fgColor.ToString() + ", " +
                "brushstyle = " + style.BrushStyle.Style.ToString() + ", " +
                "brushhatch = " + style.BrushStyle.Hatch.ToString() + " WHERE id_set = " + Program.WorkSets.CurrentWorkSet.Id + " AND id_table = " + idT.ToString();
            sqlCmd.Execute(true);
            sqlCmd.Close();
        }
        private void SaveLableExpration(int id_table, string rezult, bool show_lable)
        {
            using (SqlWork sqlCmd = new SqlWork())
            {
                Params par = new Params();
                par.paramName = "@labelexpr";
                par.type = DbType.String;
                par.value = rezult;

                if (!Program.WorkSets.CurrentWorkSet.IsDefault)
                    sqlCmd.sql = String.Format("UPDATE sys_scheme.table_info_sets SET lablefiled=@labelexpr, label_showlabel={1} WHERE id_table={0} AND id_set = {2}",
                        id_table, show_lable, Program.WorkSets.CurrentWorkSet.Id);
                else
                    sqlCmd.sql = String.Format("UPDATE sys_scheme.table_info SET lablefiled=@labelexpr, label_showlabel={1} WHERE id={0}", id_table, show_lable);

                sqlCmd.ExecuteNonQuery(new IParams[] { par });
                sqlCmd.Close();
            }
        }
        private void SaveLableStyle(string sql)
        {
            using (SqlWork sqlCmd = new SqlWork())
            {
                sqlCmd.sql = sql;
                sqlCmd.ExecuteNonQuery();
                sqlCmd.Close();
            }
        }
        #endregion Методы
    }
}