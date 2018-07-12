using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RESTLib.Model.WFS;
using Interfaces;
using System.Windows.Forms;
using RESTLib.Model.REST;
using Rekod.Services;

namespace Rekod.DBTablesEdit
{
    /// <summary>
    /// Класс для работы с MapAdmin
    /// </summary>
    public static class SyncController
    {
        /// <summary>Регистрация слоя
        /// </summary>
        /// <param name="idTable">ID таблицы</param>
        /// <param name="group">Группа</param>
        public static void RegisterTable(int idTable, String nameGroup)
        {
            if (Program.mapAdmin != null && !String.IsNullOrEmpty(nameGroup))
            {
                try
                {
                    var tableInfo = getTableInfo(idTable);
                    Group groupInfo = Program.mapAdmin.SscGroups.Find(w => w.name == nameGroup);
                    if (tableInfo != null && groupInfo != null)
                    {
                        Program.mapAdmin.RegisterTable(Program.app.getTableInfo(idTable), groupInfo);
                        var group = Program.group_info_full.Find(w => w.name == groupInfo.name);
                        if (group.id!=0)
                        {
                            using (SqlWork sqlCmd = new SqlWork())
                            {
                                sqlCmd.sql = String.Format("SELECT sys_scheme.add_table_in_group({0}, {1});", idTable, group.id);
                                sqlCmd.ExecuteNonQuery();
                            }
                        }
                        Program.mapAdmin.ReloadInfo();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при регистрации в MapAdmin:" + Environment.NewLine + ex.Message, Properties.Resources.AET_CreatingTable, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>Перезагрузка слоя
        /// </summary>
        /// <param name="idTable">ID таблицы</param>
        public static void ReloadTable(int idTable)
        {
            if (Program.mapAdmin != null)
            {
                try
                {
                    var tableInfo = getTableInfo(idTable);

                    if (tableInfo != null)
                    {
                        var layer = getLayer(tableInfo);
                        if (layer != null)
                        {
                            Program.mapAdmin.ReloadLayer(layer);
                            Program.mapAdmin.ReloadInfo();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при загрузке слоя в MapAdmin:" + Environment.NewLine + ex.Message, Properties.Resources.AET_CreatingTable, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>Перезагрузка стиля таблицы
        /// </summary>
        /// <param name="idTable">ID таблицы</param>
        public static void ReloadStyle(int idTable)
        {
            if (Program.mapAdmin != null)
            {
                try
                {
                    var tableInfo = getTableInfo(idTable);

                    if (tableInfo != null)
                    {
                        var layer = getLayer(tableInfo);
                        if (layer != null)
                            Program.mapAdmin.ReloadStyle(layer);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при загрузке стиля в MapAdmin:" + Environment.NewLine + ex.Message, Properties.Resources.AET_CreatingTable, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private static Layer getLayer(Interfaces.tablesInfo tableInfo)
        {
            return Program.mapAdmin.SscLayers.FirstOrDefault(w =>
                w.lname == tableInfo.nameDB
                || w.lname == tableInfo.view_name);
        }

        private static Interfaces.tablesInfo getTableInfo(int idTable)
        {
            Program.app.reloadInfo();
            return Program.app.getTableInfo(idTable);
        }
        /// <summary>Удаления слоя
        /// </summary>
        /// <param name="idTable">ID таблицы</param>
        public static void DeleteTable(int idTable, bool deleteFromDB)
        {
            if (Program.mapAdmin != null)
            {
                try
                {
                    var tableInfo = Program.app.getTableInfo(idTable);

                    if (tableInfo != null)
                    {
                        var layer = getLayer(tableInfo);
                        if (layer != null)
                        {
                            Program.mapAdmin.DeleteLayer(layer, deleteFromDB);
                            Program.mapAdmin.ReloadInfo();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при удалении слоя в MapAdmin:" + Environment.NewLine + ex.Message, Properties.Resources.AET_CreatingTable, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// Имеет ли текущий пользователь право на изменение данного слоя
        /// </summary>
        /// <param name="layer">ID выбранной таблицы</param>
        /// <returns>True - если имеет право, False - иначе</returns>
        public static bool HasRight(int idTable)
        {
            if (Program.mapAdmin != null)
            {
                try
                {
                    var tableInfo = Program.app.getTableInfo(idTable);

                    if (tableInfo != null)
                    {
                        var layer = getLayer(tableInfo);
                        if (layer != null)
                        {
                            return Program.mapAdmin.HasRight(layer);
                        }
                    }
                }
                catch { }
            }

            return true;
        }
    }
}
