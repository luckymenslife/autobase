using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AxmvMapLib;
using System.Collections;
using System.Runtime.InteropServices;
using System.Windows;
using System.Xml.Linq;
using System.Windows.Forms;
using System.Windows.Interop;
using Interfaces;
using Interfaces.UserControls;
using Npgsql;
using Rekod.PluginSettings;
using axVisUtils;
using Rekod.Services;
using Rekod.DataAccess.SourcePostgres.Model;
using Rekod.DataAccess.SourcePostgres.ViewModel;
using Rekod.DataAccess.SourcePostgres.View.History;
using MessageBox = System.Windows.Forms.MessageBox;

namespace Rekod.Classes
{
    class MainApp : Interfaces.IMainApp
    {
        public AxmvMapLib.AxMapLIb Map;
        public object AppSettingsObject { get { return Program.AppSettingsObject; } set { Program.AppSettingsObject = value; } }
        public MainApp(AxMapLIb map)
        {
            this.Map = map;
        }
        public AxMapLIb mapLib { get { return this.Map; } }
        public string path_string { get { return Program.path_string; } }
        public string setting_file { get { return ""; } }
        public string scheme { get { return Program.scheme; } }
        public string srid { get { return Program.srid; } }
        public List<Interfaces.user_right> tables_right
        {
            get
            {
                return Program.tables_right.Select(temp => new Interfaces.user_right
                                                               {
                                                                   id_table = temp.id_table,
                                                                   read = temp.read,
                                                                   write = temp.write
                                                               }).ToList();
            }
        }
        public List<Interfaces.tablesInfo> tables_info
        {
            get
            {
                return Program.tables_info.Select(temp => GetTableInfoClone(temp)).ToList();
            }
        }
        public List<Interfaces.fieldInfo> field_info
        {
            get
            {
                return Program.field_info.Select(temp => GetFieldInfoClone(temp)).ToList();
            }
        }
        public List<Interfaces.filtrTableInfo> filtr_table_info
        {
            get
            {
                return Program.filtr_table_info.Select(temp => new Interfaces.filtrTableInfo
                                                                   {
                                                                       idField = temp.idField,
                                                                       idFilter = temp.idFilter,
                                                                       idOperator = temp.idOperator,
                                                                       idTable = temp.idTable
                                                                   }).ToList();
            }
        }
        public List<Interfaces.tipTable> tip_table
        {
            get
            {
                return Program.tip_table.Select(temp => new Interfaces.tipTable
                                                            {
                                                                idTipTable = temp.idTipTable,
                                                                mapLayer = temp.mapLayer,
                                                                nameTip = temp.nameTip
                                                            }).ToList();
            }
        }
        public List<Interfaces.tipGeom> tip_geom
        {
            get
            {
                return Program.tip_geom.Select(temp => new Interfaces.tipGeom
                                                           {
                                                               idTipGeom = temp.idTipGeom,
                                                               nameDb = temp.nameDb,
                                                               nameGeom = temp.nameGeom
                                                           }).ToList();
            }
        }
        public List<Interfaces.tipData> tip_data
        {
            get
            {
                return Program.tip_data.Select(temp => new Interfaces.tipData
                                                           {
                                                               idTipData = temp.idTipData,
                                                               nameTipData = temp.nameTipData,
                                                               nameTipDataDB = temp.nameTipDataDB
                                                           }).ToList();
            }
        }
        public List<Interfaces.tipOperator> tip_operator
        {
            get
            {
                return Program.tip_operator.Select(temp => new Interfaces.tipOperator
                                                               {
                                                                   idTipOperator = temp.idTipOperator,
                                                                   namePered = temp.namePered,
                                                                   namePosle = temp.namePosle,
                                                                   nameTipOperator = temp.nameTipOperator
                                                               }).ToList();
            }
        }
        public List<Interfaces.photoInfo> photo_info
        {
            get
            {
                return Program.photo_info.Select(temp => new Interfaces.photoInfo
                                                             {
                                                                 idTable = temp.idTable,
                                                                 nameFieldID = temp.nameFieldID,
                                                                 namePhotoField = temp.namePhotoField,
                                                                 namePhotoFile = temp.namePhotoFile,
                                                                 namePhotoTable = temp.namePhotoTable
                                                             }).ToList();
            }
        }
        public List<string> schems { get { return Program.schems; } }
        public Interfaces.userInfo user_info
        {
            get
            {
                var c = ((NpgsqlConnectionStringBuilder)Program.connString);
                return new Interfaces.userInfo
                                {
                                    admin = Program.user_info.admin,
                                    dbString = c.Database,
                                    id_user = Program.user_info.id_user,
                                    ipString = c.Host,
                                    loginUser = Program.user_info.loginUser,
                                    nameUser = Program.user_info.nameUser,
                                    portString = c.Port.ToString(),
                                    pwdUser = c["Password"].ToString(),
                                    windowText = Program.user_info.windowText,
                                    type_user = Program.user_info.type_user
                                };
            }
        }
        public string ipString { get { return ((NpgsqlConnectionStringBuilder)Program.connString).Host; } }
        public string dbString { get { return ((NpgsqlConnectionStringBuilder)Program.connString).Database; } }
        public string portString { get { return ((NpgsqlConnectionStringBuilder)Program.connString).Port.ToString(); } }
        public sscUserInfo sscUser
        {
            get { return Program.sscUser; }
            set { Program.sscUser = value; }
        }
        public Dictionary<string, string[]> userParams { get { return Program.UserParams; } }

        public void reloadInfo()
        {
            (new classesOfMetods()).reloadInfo();
        }

        public Interfaces.user_right getTableRight(int idTable)
        {
            var temp = classesOfMetods.getTableRight(idTable);

            return new Interfaces.user_right
            {
                id_table = temp.id_table,
                read = temp.read,
                write = temp.write
            };
        }
        public Interfaces.tablesInfo getTableInfo(int idTable)
        {
            var temp = classesOfMetods.getTableInfo(idTable);
            if (temp == null) return null;
            if (temp.idTable != idTable) return null;
            return GetTableInfoClone(temp);
        }
        public Interfaces.tablesInfo getTableInfoOfNameMap(string nameMap)
        {
            var temp = classesOfMetods.getTableInfoOfNameMap(nameMap);
            if (temp == null) return null;
            if (temp.nameMap != nameMap) return null;
            return GetTableInfoClone(temp);
        }
        public Interfaces.tablesInfo getTableInfoOfNameDB(string nameDb)
        {
            var temp = classesOfMetods.getTableInfoOfNameDB(nameDb);
            if (temp == null) return null;
            if (temp.nameDB != nameDb) return null;
            return GetTableInfoClone(temp);
        }
        public Interfaces.tablesInfo getTableInfoOfNameDB(string nameDb, string schema)
        {
            var temp = classesOfMetods.getTableInfoOfNameDB(nameDb);
            if (temp == null || temp.nameDB != nameDb) return null;
            return GetTableInfoClone(temp);
        }
        public List<Interfaces.tablesInfo> getTableOfType(int idType)
        {
            var tempr = classesOfMetods.getTableOfType(idType);
            return tempr.Select(GetTableInfoClone).ToList();
            //return classesOfMetods.getTableOfType(id_type);
        }
        public Interfaces.fieldInfo getFieldInfo(int idField)
        {
            var temp = classesOfMetods.getFieldInfo(idField);
            return GetFieldInfoClone(temp);

        }

        public List<Interfaces.fieldInfo> getFieldInfoTable(int idTable)
        {
            var tempr = classesOfMetods.getFieldInfoTable(idTable);
            return tempr.Select(GetFieldInfoClone).ToList();
        }
        public Interfaces.filtrTableInfo getFiltrInfo(int idFilter)
        {
            var temp = classesOfMetods.getFiltrInfo(idFilter);

            return new Interfaces.filtrTableInfo
                        {
                            idField = temp.idField,
                            idFilter = temp.idFilter,
                            idOperator = temp.idOperator,
                            idTable = temp.idTable
                        };
        }
        public List<Interfaces.filtrTableInfo> getFiltrTableInfo(int idTable)
        {
            var tempr = classesOfMetods.getFiltrTableInfo(idTable);
            return tempr.Select(temp => new Interfaces.filtrTableInfo
                                            {
                                                idField = temp.idField,
                                                idFilter = temp.idFilter,
                                                idOperator = temp.idOperator,
                                                idTable = temp.idTable
                                            }).ToList();
        }
        public Interfaces.tipTable getTipTable(int idTip)
        {
            var temp = classesOfMetods.getTipTable(idTip);

            var t = new Interfaces.tipTable
                        {
                            idTipTable = temp.idTipTable,
                            mapLayer = temp.mapLayer,
                            nameTip = temp.nameTip
                        };


            return t;
        }
        public Interfaces.tipGeom getTipGeom(int idTip)
        {
            tipGeom temp = classesOfMetods.getTipGeom(idTip);

            var t = new Interfaces.tipGeom
            {
                idTipGeom = temp.idTipGeom,
                nameDb = temp.nameDb,
                nameGeom = temp.nameGeom
            };


            return t;
        }
        public Interfaces.tipData getTipField(int idTip)
        {
            tipData temp = classesOfMetods.getTipField(idTip);
            var t = new Interfaces.tipData
                        {
                            idTipData = temp.idTipData,
                            nameTipData = temp.nameTipData,
                            nameTipDataDB = temp.nameTipDataDB
                        };


            return t;
        }
        public Interfaces.tipOperator getTipOperator(int idTip)
        {
            var temp = classesOfMetods.getTipOperator(idTip);
            return new Interfaces.tipOperator
                                           {
                                               idTipOperator = temp.idTipOperator,
                                               namePered = temp.namePered,
                                               namePosle = temp.namePosle,
                                               nameTipOperator = temp.nameTipOperator
                                           };
        }
        public Interfaces.photoInfo getPhotoInfo(int idTable)
        {
            photoInfo tempp = classesOfMetods.getPhotoInfo(idTable);
            if (tempp.idTable == idTable)
            {
                return new Interfaces.photoInfo()
                {
                    idTable = tempp.idTable,
                    nameFieldID = tempp.nameFieldID,
                    namePhotoField = tempp.namePhotoField,
                    namePhotoFile = tempp.namePhotoFile,
                    namePhotoTable = tempp.namePhotoTable
                };
            }
            return null;
        }
        public bool getReadTable(int idTable)
        {
            return classesOfMetods.getOpenTable(idTable);
        }
        public bool getWriteTable(int idTable)
        {
            return classesOfMetods.getWriteTable(idTable);
        }
        public Interfaces.IRelation relation
        {
            get
            {
                return Program.RelationVisbleBdUser;
            }
        }

        public Interfaces.ISQLCommand SqlWork(bool enableException = false)
        {
            return new SqlWork(enableException);
        }

        public void SetVisableLayer(int idTable, bool visable)
        {
            Program.mainFrm1.layerItemsView1.layerSetVisible(visable, "", idTable);
        }

        public string ConnectionString
        {
            get { return Program.connString.ToString(); }
        }

        private Interfaces.fieldInfo GetFieldInfoClone(fieldInfo value)
        {
            return new Interfaces.fieldInfo
                        {
                            idField = value.idField,
                            idTable = value.idTable,
                            nameDB = value.nameDB,
                            nameMap = value.nameMap,
                            nameLable = value.nameLable,
                            TypeField = (TypeField)value.type,

                            is_reference = value.is_reference,
                            is_interval = value.is_interval,
                            is_style = value.is_style,

                            ref_table = value.ref_table,
                            ref_field = value.ref_field,
                            ref_field_end = value.ref_field_end,
                            ref_field_name = value.ref_field_name,

                            read_only = value.read_only,
                            visible = value.visible,
                            Order = value.Order
                        };
        }
        private Interfaces.tablesInfo GetTableInfoClone(tablesInfo value)
        {
            var table = new Interfaces.tablesInfo
                        {
                            angleColumn = value.angleColumn,
                            geomFieldName = value.geomFieldName,
                            idTable = value.idTable,
                            imageColumn = value.imageColumn,
                            lableFieldName = value.lableFieldName,
                            map_style = value.map_style,
                            maxScale = value.maxScale,
                            minScale = value.minScale,
                            nameDB = value.nameDB,
                            nameMap = value.nameMap,
                            nameSheme = value.nameSheme,
                            photo = value.photo,
                            pkField = value.pkField,
                            read_only = value.read_only,
                            sourceLayer = value.sourceLayer,
                            srid = (int?)value.srid,
                            TypeTable = (TypeTable)value.type,
                            TypeGeom = (TypeGeometry)classesOfMetods.GetIntGeomType(value.GeomType_GC),
                            useBounds = value.useBounds,
                            sql_view_string = value.sql_view_string,
                            view_name = value.view_name
                        };
            table.ListField = getFieldInfoTable(table.idTable);
            table.PhotoInfo = getPhotoInfo(table.idTable);
            return table;
        }
    }

    class WorkClass : Interfaces.IWorkClass
    {
        public WorkClass()
        {
        }
        #region Члены IWorkClass

        IOpenForms _openForm;
        public IOpenForms OpenForm { get { return _openForm = (_openForm) ?? new OpenForm(); } }
        public Interfaces.FastReport.IReport_M FastReport { get { return Program.ReportModel; } }

        public bool AddSpoofingAttributesOfObject(int idTable, Func<int, int?, referInfo, IUserControlMain> func)
        {
            var table = Program.app.getTableInfo(idTable);
            if (table != null)
            {
                var pluginFunc = new Model.PluginModel.PluginFunc<Func<int, int?, referInfo, IUserControlMain>>(idTable, func);
                Plugins.ListSpoofingAttributesOfObject.Add(pluginFunc);
                return true;
            }
            else
                return false;
        }

        public bool AddSpoofingTableOfObjects(int idTable, Func<int, bool, Form, int?, int?, IUserControlMain> func)
        {
            var table = Program.app.getTableInfo(idTable);
            if (table != null)
            {
                var pluginFunc = new Model.PluginModel.PluginFunc<Func<int, bool, Form, int?, int?, IUserControlMain>>(idTable, func);
                Plugins.ListSpoofingTableOfObjects.Add(pluginFunc);
                return true;
            }
            else
                return false;
        }

        public void RemoveSpoofingAttributesOfObject(int idTable)
        {
            Plugins.ListSpoofingAttributesOfObject.RemoveAll(f => f.IdTable == idTable);
        }

        public void RemoveSpoofingTableOfObjects(int idTable)
        {
            Plugins.ListSpoofingTableOfObjects.RemoveAll(f => f.IdTable == idTable);
        }
        Interfaces.Forms.ImainFrm _mForm = null;
        public Interfaces.Forms.ImainFrm MainForm
        {
            get { return _mForm = (_mForm) ?? new DoubleMainForm(); }
        }

        private class DoubleMainForm : Interfaces.Forms.ImainFrm
        {
            public void PanelTools(System.Windows.Controls.Control swItem)
            {
                Program.mainFrm1.AddPanelToolsPlugins(swItem);
            }
            public void RemovePanelTools(System.Windows.Controls.Control swItem)
            {
                Program.mainFrm1.RemovePanelToolsPlugins(swItem);
            }
            public void Menu(System.Windows.Controls.MenuItem swMenu)
            {
                Program.mainFrm1.AddMenu(swMenu);
            }
            public void RemoveMenu(System.Windows.Controls.MenuItem swMenu)
            {
                Program.mainFrm1.RemoveMenu(swMenu);
            }
        }


        public void AddMenuInTable(int idTable, Func<ToolStripMenuItem> menuItem)
        {
            Plugins.ListAddMenuInTable.Add(new MenuItem() { IdTable = idTable, ToolStripMenuItem = menuItem });

        }

        public void RemoveMenuInTable(Func<ToolStripMenuItem> menuItem)
        {
            Plugins.ListAddMenuInTable.RemoveAll(f => f.ToolStripMenuItem == menuItem);
        }

        public void AddMenuForObject(int idTable, Func<ToolStripMenuItem> menuItem, bool inTable = true, bool inAttribute = true)
        {
            if (inTable)
                Plugins.ListAddMenuInTable.Add(new MenuItem()
                                                    {
                                                        IdTable = idTable,
                                                        ToolStripMenuItem = menuItem
                                                    });
            if (inAttribute)
                Plugins.ListAddMenuInAttribute.Add(new MenuItem()
                                                    {
                                                        IdTable = idTable,
                                                        ToolStripMenuItem = menuItem
                                                    });


        }
        public void RemoveMenuForObject(Func<ToolStripMenuItem> menuItem)
        {
            Plugins.ListAddMenuInTable.RemoveAll(f => f.ToolStripMenuItem == menuItem);
            Plugins.ListAddMenuInAttribute.RemoveAll(f => f.ToolStripMenuItem == menuItem);
        }

        public void ExportToExcel(List<object[]> data, List<int> Types)
        {
            try
            {
                Rekod.Classes.ExportExcelManager.ExportToExcel(data, Types);
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                if (ex.ErrorCode == -2147221164)
                {
                    MessageBox.Show("Необходимо установить MS Excel!", "Сообщение", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                Classes.workLogFile.writeLogFile(ex.Message, true, true);
            }
        }

        public void SaveSettings()
        {
            Program.SettingsXML.ApplyLastChange();
        }
        public void SaveSettings(string guid, XElement settings)
        {
            Program.SettingsXML.ApplyPlugin(guid, settings);
        }
        #endregion


        public void SetProcessesControl(IUserControlMain userControl)
        {
            Plugins.ProcessesControl = userControl;
        }
    }
    class OpenForm : Interfaces.IOpenForms
    {
        [DllImport("user32.dll")]

        private static extern int SetWindowLong(HandleRef hWnd, int nIndex, int dwNewLong);

        /// <summary>
        /// sets the owner of a System.Windows.Forms.Form to a System.Windows.Window
        /// </summary>
        /// <param name="form"></param>
        /// <param name="owner"></param>
        public static void SetOwner(System.Windows.Forms.Form form, System.Windows.Window owner)
        {
            WindowInteropHelper helper = new WindowInteropHelper(owner);
            SetWindowLong(new HandleRef(form, form.Handle), -8, helper.Handle.ToInt32());
        }
        public void OpenTableObject(Interfaces.tablesInfo table, Form owner)
        {
            var form = new itemsTableGridForm(table.idTable) { Owner = owner };
            form.Show();
        }
        public void OpenTableObject(Interfaces.tablesInfo table, Form owner, Window wpfowner)
        {
            var form = new itemsTableGridForm(table.idTable, setOwner: wpfowner==null);
            if (owner != null)
                form.Owner = owner;
            else if (wpfowner != null)
                SetOwner(form, wpfowner);
            form.Show();
        }
        public void OpenAttributeObject(Interfaces.tablesInfo table, int idObject, bool isNew, referInfo refValue = null, Form owner = null)
        {
            var form = new axVisUtils.FormTableData(table, idObject, isNew, "", refValue) { Owner = owner };
            form.FormClosed += (o, e) =>
                {
                    if (form.Owner != null)
                    {
                        if (form.Owner.WindowState == FormWindowState.Minimized)
                            form.Owner.WindowState = FormWindowState.Normal;
                        form.Owner.Activate();
                    }
                };
            if (!form.CancelOpenForm)
                form.ShowDialog();
        }
        public void ShowAttributeObject(Interfaces.tablesInfo table, int idObject, bool isNew, Form owner, Action<DialogResult> ActionDResult = null)
        {
            ShowAttributeObject(table, idObject, isNew, owner, "", ActionDResult, "", -1);
        }
        public void ShowAttributeObject(Interfaces.tablesInfo table, int idObject, bool isNew, Window wpfowner)
        {
            ShowAttributeObject(table, idObject, isNew, null, "", null, "", -1, wpfowner);
        }

        public void ShowAttributeObject(Interfaces.tablesInfo table, int idObject, bool isNew, Form owner, String wkt,
            Action<DialogResult> ActionDResult = null, String SearchField = "", int SearchId = -1)
        {
            ShowAttributeObject(table, idObject, isNew, owner, wkt, ActionDResult, SearchField, SearchId, null);
        }
        public void ShowAttributeObject(Interfaces.tablesInfo table, int idObject, bool isNew, Form owner, String wkt, Action<DialogResult> ActionDResult = null,
             String SearchField = "", int SearchId = -1, Window wpfowner=null)
        {
            //if (!isNew && !classesOfMetods.ExistsOpeningObject(table.idTable, idObject))
            //{
            //    MessageBox.Show(Rekod.Properties.Resources.InterfaceClass_NoExistsObj, Rekod.Properties.Resources.error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    return;
            //}

            List<string> list = classesOfMetods.GetTableNotRight(table.idTable);
            if (list.Count > 0)
            {
                string mess = Rekod.Properties.Resources.InterfaceClass_FindLinkError + "\n\r";
                foreach (var item in list)
                {
                    mess += item + "\n\r";
                }
                MessageBox.Show(mess, Rekod.Properties.Resources.error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            referInfo RefValue = null;
            if (SearchField != "")
            {
                RefValue = new referInfo() { nameField = SearchField, idObj = SearchId };
            }
            var form = new axVisUtils.FormTableData(table, idObject, isNew, wkt, RefValue, wpfowner==null);

            if (owner != null)
                form.Owner = owner;
            else if (wpfowner != null)
                SetOwner(form, wpfowner);

            if (!form.CancelOpenForm)
            {
                form.Show();
                form.ActionResult = ActionDResult;
            }
        }

        public void HistoryFrm(int idTable, int idObj, Form owner)
        {
            PgTableBaseM pgTable = Program.repository.FindTable(idTable) as PgTableBaseM;
            PgHistoryVM pgHistVM = new PgHistoryVM(Program.repository, table: pgTable, idObject: idObj);
            PgHistoryV pgHistV = new PgHistoryV(pgHistVM);
            pgHistV.Owner = Program.WinMain;
            pgHistV.Height = 600;
            pgHistV.Width = 900;
            pgHistV.ShowDialog();
        }

        public void HistoryFrm(int idTable, Form owner)
        {
            PgTableBaseM pgTable = Program.repository.FindTable(idTable) as PgTableBaseM;
            PgHistoryVM pgHistVM = new PgHistoryVM(Program.repository, table: pgTable);
            PgHistoryV pgHistV = new PgHistoryV(pgHistVM);
            pgHistV.Owner = Program.WinMain;
            pgHistV.Height = 600;
            pgHistV.Width = 900;
            pgHistV.ShowDialog();
        }

        public void HistoryFrm(string userName, Form owner)
        {
            PgUserM pgUser = Program.repository.Users.First(w => w.Login == userName);
            PgHistoryVM pgHistVM = new PgHistoryVM(Program.repository, user: pgUser);
            PgHistoryV pgHistV = new PgHistoryV(pgHistVM);
            pgHistV.Owner = Program.WinMain;
            pgHistV.Height = 600;
            pgHistV.Width = 900;
            pgHistV.ShowDialog();
        }

        public void SetFormOwner(Form frm)
        {
            classesOfMetods.SetFormOwner(frm);
        }


        public int? OpenTableObject(Interfaces.tablesInfo table, int? idSelected, bool isSelected)
        {
            var form = new Rekod.itemsTableGridForm(table.idTable, idSelected, isSelected);
            if (isSelected && form.ShowDialog() == DialogResult.OK)
                return form.idObj;
            else 
                return (int?)null;
        }

        public DialogResult ShowDialogAttributeObject(Interfaces.tablesInfo table, int idObject, bool isNew, Form owner)
        {
            var form = new axVisUtils.FormTableData(table, idObject, isNew, "") { Owner = owner };
            if (!form.CancelOpenForm)
                return form.ShowDialog();
            return DialogResult.Cancel;
        }


        public string ProcOpen(string prefix = "")
        {
            return cti.ThreadProgress.Open(prefix);
        }

        public void ProcClose(string key)
        {
            cti.ThreadProgress.Close(key);
        }

        public void SetText(string txt)
        {
            cti.ThreadProgress.SetText(txt);
        }
    }
}
