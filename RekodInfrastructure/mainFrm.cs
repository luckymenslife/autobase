using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Rekod.Controllers;
using Npgsql;
using System.IO;
using mvMapLib;
using Rekod.Classes;
using System.Xml.Linq;
using System.Diagnostics;
using System.Drawing.Printing;
using Rekod.DBLayerForms;
using Rekod.ViewModel;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Rekod.PluginSettings;
using Rekod.FastReportClasses;
using Rekod.DataAccess.SourcePostgres.ViewModel;
using Rekod.Services;
using ExtraFunctions;
using System.Text;
using sw = System.Windows;
using System.Windows.Media.Imaging;
using System.Data;
using Interfaces;
using NpgsqlTypes;
using AbsM = Rekod.DataAccess.AbstractSource.Model;
using PgM = Rekod.DataAccess.SourcePostgres.Model;
using CosM = Rekod.DataAccess.SourceCosmetic.Model;
using Gdal = OSGeo.GDAL.Gdal;
using Ogr = OSGeo.OGR.Ogr;
using OSGeo.OGR;
using Rekod.DataAccess.SourceCosmetic.ViewModel;
using Interfaces.FastReport;

namespace Rekod
{
    public partial class mainFrm : UserControl, INotifyPropertyChanged
    {
        #region Поля
        public string selectLayer = "";
        public int idT = -1, idT_Copy = -1, tableId_Copy = -1;
        public object CopyObject = null;
        public object CopyTable = null;
        public int idT_Obj = -1;
        public layersManager layersManager1;
        public string projTo, projMap;
        private int _prjSrid;
        private Rekod.SelectedSetFrm frmHollowCopyObj;
        private PgFullTextSearchVM _pgFullTextSearchVM = null;
        private Rekod.MapLayersItemInfo.ViewModel.MapLayersItemInfoVM _mapLayersItemInfoVM = null;
        private Rekod.DataAccess.SourcePostgres.Modules.TableRegister.ViewModel.RTableRegistrationVM _pgTableRegistrationVM = null;
        private ObservableCollection<int> _selectObjectsIds;
        private mvLayer _selectLayer;
        private bool _newHollowAdding = false;
        private bool _newLineAdding = false;
        private ToolBar_VM tsp_layerItemsView1_VM;
        //private FormSaveLocationController saveLocationController;
        public Classes.ButtonManager bManager { get; private set; }
        private CmdPlugin.MainCallPlugin CMDPlugin;
        private ToolBar_VM tsp_FastStart;
        private ToolBar_VM tsp_Plugins;
        private bool _userScaleChange;
        #endregion Поля

        #region Конструкторы
        public mainFrm()
        {
            InitializeComponent();
            Program.mainFrm1 = this;
            Program.WorkSets = new UserSets.WorkSets_M();
            Program.ReportModel = new Report_M();
            bManager = new ButtonManager();
            Program.BManager = bManager;
            try
            {
                cti.ThreadProgress.ShowWait();

                axMapLIb1.SetSettingsParameter("ColorBgFastMode", "0xFFFFFFFF");
                axMapLIb1.SetSettingsParameter("MovingWithSelect", "1");
                axMapLIb1.setPaths(Application.StartupPath + "\\gdal\\data", Application.StartupPath + "\\gdal\\data");

                String mapFile = GetMapFile(Program.path_string + "\\map\\map_file", true);
                if (String.IsNullOrEmpty(mapFile))
                    Environment.Exit(0);
                else
                {
                    axMapLIb1.CloseMap();
                    axMapLIb1.LoadMap(mapFile, "id");
                    axMapLIb1.SRID = Program.srid;
                    axMapLIb1.statusbar = false;
                    Program.proj4Map = TransformGeometry.GetProj4(Convert.ToInt32(Program.srid));
                }

                Program.TablesManager = new Rekod.DataAccess.TableManager.ViewModel.TableManagerVM(axMapLIb1);

                Program.repository = new DataAccess.SourcePostgres.ViewModel.PgDataRepositoryVM(
                    Program.TablesManager, (NpgsqlConnectionStringBuilder)Program.connString);
                Program.repository.CheckRepository();
                Program.repository.ReloadPartInfo();
            }
            finally
            {
                cti.ThreadProgress.Close();
            }
            var cls = new classesOfMetods();
            cls.reloadInfo();
            _selectObjectsIds = new ObservableCollection<int>();
        }
        #endregion Конструкторы

        #region Свойства
        public bool CosmeticHollowAdding
        { get { return this._newHollowAdding; } }
        public bool CosmeticLineAdding
        { get { return this._newLineAdding; } }
        public ObservableCollection<int> SelectedObjectsIds
        {
            get { return _selectObjectsIds; }
        }
        public mvLayer SelectedLayer
        {
            get { return _selectLayer; }
        }
        #endregion Свойства

        #region Методы
        public void SetStart()
        {
            System.Environment.SetEnvironmentVariable
               (
                   "Path",
                   Application.StartupPath + "\\SQLite_libs" + ";" + System.Environment.GetEnvironmentVariable("Path")
               );
#if DEBUG
            if (Program.WithMap)
            {
                Program.WinMain.MiSources.Visibility = sw.Visibility.Visible;
                Program.WinMain.MiSrcShowPanel.IsChecked = true;
            }
            else
            {
                Program.WinMain.MiSrcShowPanel.IsChecked = false;
            }

#endif

            Rekod.MapFunctions.ProxyParameters.ProxyApplyChanges();

            layersManager1 = new layersManager(this);
            Program.RelationVisbleBdUser = new relation();

            {
                layerItemsView1.MakeGroups();
                layerItemsView1.AddNewLayersInGroups();
                axMapLIb1.ExternalSourceConnectAborted += new AxmvMapLib.IMapLIbEvents_ExternalSourceConnectAbortedEventHandler(axMapLIb1_ExternalSourceConnectAborted);
                axMapLIb1.OnMessage += new AxmvMapLib.IMapLIbEvents_OnMessageEventHandler(axMapLIb1_OnMessage);
                axMapLIb1.ExternalSourceAfterQueryError += new AxmvMapLib.IMapLIbEvents_ExternalSourceAfterQueryErrorEventHandler(axMapLIb1_ExternalSourceAfterQueryError);

                Program.TablesManager.VMPReposotory.ReloadInfo();
                layerItemsView1.GetStylesLayerVMP();
                layerItemsView1.SetRastrLayersVisible();
            }
            axMapLIb1.changeLabelCharset(CHARSET.DEFAULT);

            {
                bManager.PropertyChanged += bManager_PropertyChanged;
                bManager.ListToolBars_PropertyChanged += bManager_ListToolBars_PropertyChanged;
                bManager.SetNotEdited();

                tsp_layerItemsView1_VM = AddPanelToolBar(layerItemsView1, Program.WinMain.MiTbLayersList);

                AddPanelToolBar(Program.WinMain.TbModeEnabled, Program.WinMain.MiTbEnabledModes);
                AddPanelToolBar(Program.WinMain.TbSelectionOf, Program.WinMain.MiTbSelectObject);
                AddPanelToolBar(Program.WinMain.TbOperationsWithObjects, Program.WinMain.MiTbEditObject);
                AddPanelToolBar(Program.WinMain.TbMaps, Program.WinMain.MiTbMap);
                AddPanelToolBar(Program.WinMain.TbOperationsWithGeometry, Program.WinMain.MiTbEditGeom);
                AddPanelToolBar(Program.WinMain.TbEditableLayer, Program.WinMain.MiTbEditableLayer);
                AddPanelToolBar(Program.WinMain.TbFullTextSearch, Program.WinMain.MiTbSearch);
                //AddPanelToolBar(Program.WinMain.TbInfoText, Program.WinMain.MiTbInfo);
                //AddPanelToolBar(Program.WinMain.TbScale, Program.WinMain.MiTbScale);
                AddPanelToolBar(Program.WinMain.TbWorkSets, Program.WinMain.MiTbWorkSets);
                //AddPanelToolBar(Program.WinMain.TbCoordinates, Program.WinMain.MiTbCoords);
                tsp_FastStart = AddPanelToolBar(Program.WinMain.TbFastStart, Program.WinMain.MiTbFastStart);
                tsp_FastStart.ListButton.CollectionChanged += ListButton_CollectionChanged;
                tsp_Plugins = AddPanelToolBar(Program.WinMain.TbPlugins, Program.WinMain.MiTbPlugins);
            }

            Program.WinMain.TbtbHand.IsChecked = true;
            axMapLIb1.setBG(Program.bgMap);
            axMapLIb1.FastMode = true;
            axMapLIb1.toolbar = false;
            axMapLIb1.ShowInfoWindow = false;

            Program.WinMain.TbPlugins.Visibility = sw.Visibility.Collapsed;
            var p = new Plugins();
            p.FindPlugins(axMapLIb1, this);
            var work = new Rekod.Classes.WorkClass();

            CMDPlugin = new CmdPlugin.MainCallPlugin();
            CMDPlugin.StartPlugin(Program.SettingsXML.GetPlugin(CmdPlugin.MainCallPlugin.GUID), Program.app, work);

            if (Program.sscUser != null)
            {
                try
                {
                    sscSync.Controller.ProxySettings proxy = null;
                    if (Program.SettingsXML.LocalParameters.Proxy.Status != Repository.SettingsFile.eProxyStatus.Non)
                    {
                        proxy = new sscSync.Controller.ProxySettings()
                        {
                            Server = (new UriBuilder("http", Program.SettingsXML.LocalParameters.Proxy.IP, (int)Program.SettingsXML.LocalParameters.Proxy.Port)).Uri,
                            Login = Program.SettingsXML.LocalParameters.Proxy.Login,
                            Password = Program.SettingsXML.LocalParameters.Proxy.Password
                        };
                        Rekod.MapFunctions.ProxyParameters.ProxyApplyChanges();
                    }
                    Program.mapAdmin = new sscSync.Controller.MapAdmin(Program.sscUser, Program.app, Program.work, proxy);
                    Program.ssc = new Classes.SSC(
                           Program.app.sscUser.Login,
                           Program.app.sscUser.Password,
                           Program.app.sscUser.Server);
                }
                catch { }
            }
            else
            {
                Program.ssc = null;
            }
            //layerItemsView1.RefreshLayers();

            SetRights();

            ReloadDataMenu(Program.WinMain.MiData);

            this.Focus();
            {
                layerItemsView1.SetVisibleDefault();
            }
            var d = classesOfMetods.GetUserParams("id_map_extents");
            if (d.Length > 0)
                using (var sqlCmd = new SqlWork())
                {
                    sqlCmd.sql = sqlCmd.sql = "SELECT extent FROM " + Program.scheme + ".map_extents WHERE id = " + ExtraFunctions.Converts.To<int>(d[0]);
                    var gbb = new Rekod.SettingStartPosition.GetBboxBd();
                    string value = sqlCmd.ExecuteScalar<string>();
                    if (!string.IsNullOrEmpty(value))
                        gbb.SetToMapLib(gbb.GetBboxFromBd(value, Program.srid));
                }

            bool exists_proj = false;
            foreach (var item in Program.WinMain.TbcbSrid.Items)
            {
                if (item.ToString().Contains(axMapLIb1.SRID))
                {
                    exists_proj = true;
                    var item2 = item;
                    Program.WinMain.TbcbSrid.Items.Remove(item);
                    Program.WinMain.TbcbSrid.Items.Insert(0, item2);
                    break;
                }
            }
            if (!exists_proj)
            {
                Program.WinMain.TbcbSrid.Items.Insert(0, "(" + axMapLIb1.SRID + Rekod.Properties.Resources.mainFrm_mProjection);
            }
            // Выставление проекции карты если она есть
            projMap = projTo = TransformGeometry.GetProjWKT(int.Parse(axMapLIb1.SRID));
            PrjSrid = Convert.ToInt32(axMapLIb1.SRID);
            foreach (var item in Program.WinMain.TbcbSrid.Items)
            {
                if (item.ToString().Contains(axMapLIb1.SRID))
                {
                    Program.WinMain.TbcbSrid.SelectedItem = item;
                }
            }

            bManager.SetButtonsState();

            Program.WinMain.MiFitObject.IsChecked = Program.SettingsXML.LocalParameters.EnterTheScreen;
            Program.WinMain.MiHideVMP.IsChecked = Program.SettingsXML.LocalParameters.TurnOffVMPWhenRastr;
            Program.WinMain.MiShowObjectForm.IsChecked = Program.SettingsXML.LocalParameters.OpenAttrsAfterCreate;

            {
                layerItemsView1.PropertyChanged += layerItemsView1_PropertyChanged;
                layerItemsView1.ListLayersIsView.CollectionChanged += ListLayersIsView_CollectionChanged;
                layerItemsView1.UpdateListLayersIsView();
                ListLayersIsView_CollectionChanged(layerItemsView1.ListLayersIsView, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                this.layerItemsView1.RefreshLayers();

                Program.WinMain.TbcbEditableLayer.SelectionChanged -= TbcbEditableLayer_SelectionChanged;
                Program.WinMain.TbcbEditableLayer.SelectionChanged += TbcbEditableLayer_SelectionChanged;
            }
            {
                Program.WorkSets.PropertyChanged += WorkSets_PropertyChanged;
                Program.WorkSets.ListWorkSets_CollectionChanged += WorkSets_ListWorkSets_CollectionChanged;
                WorkSets_ListWorkSets_CollectionChanged(Program.WorkSets.ListWorkSets, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

                Program.ReportModel.ListReports.CollectionChanged += (s, arg) =>
                {
                    MenuReloadReports();
                };

                Program.WinMain.TbcbWorkSet.SelectionChanged -= TbcbWorkSet_SelectionChanged;
                Program.WinMain.TbcbWorkSet.SelectionChanged += TbcbWorkSet_SelectionChanged;
            }
            MenuReloadReports();
            layerItemsView1.UpdateListLayersIsView();
        }

        public int PrjSrid
        {
            get { return _prjSrid; }
            set
            {
                if (_prjSrid == value)
                {
                    return;
                }
                _prjSrid = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("PrjSrid"));
                }
            }
        }
        public void ApplyNewSettings()
        {
            Object MapBackColor = Program.GetAppSettingByName("MapBackColor");
            if (MapBackColor != null)
            {
                axMapLIb1.setBG((int)(layersManager.convToUInt((Color)(MapBackColor))));
            }
            axMapLIb1.mapRepaint();
            axMapLIb1.Update();
        }
        private void SetRights()
        {
            //Program.WinMain.MiAddMapAdminLayer.Visibility = sw.Visibility.Collapsed;
            if (!Program.user_info.admin)
            {
                Program.WinMain.MiManageTables.Visibility = sw.Visibility.Collapsed;
                Program.WinMain.MiUserRights.Visibility = sw.Visibility.Collapsed;
                Program.WinMain.MiHistory.Visibility = sw.Visibility.Collapsed;
                Program.WinMain.MiLocations.Visibility = sw.Visibility.Collapsed;
            }
            if (Program.sscUser == null || Program.user_info.admin)
            {
                Program.WinMain.MiAddMapAdminLayer.Visibility = sw.Visibility.Collapsed;
            }
        }
        private string GetSrid(string mapName, bool required)
        {
            if (!File.Exists(mapName + ".srid"))
                return OpenErrorSrid(required);

            string temp = "";
            var sr = new StreamReader(mapName + ".srid");
            while (sr.Peek() > -1)
            {
                temp = sr.ReadLine();
            }
            sr.Close();
            try
            {
                int.Parse(temp);
            }
            catch
            {
                cti.ThreadProgress.Close();
                MessageBox.Show(Rekod.Properties.Resources.mainFrm_errFileFomat, Rekod.Properties.Resources.error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return OpenErrorSrid(required);
            }
            if (temp == "")
            {
                return OpenErrorSrid(required);
            }
            return temp;
        }
        private string GetSrid(string mapSridFile, string pathString, bool required)
        {
            string temp = "";
            string fullSridPath = "";
            fullSridPath = pathString + mapSridFile;
            if (File.Exists(fullSridPath))
            {
                var sr = new StreamReader(fullSridPath);
                while (sr.Peek() > -1)
                {
                    temp = sr.ReadLine();
                }
                sr.Close();
                try
                {
                    int.Parse(temp);
                }
                catch
                {
                    cti.ThreadProgress.Close();
                    MessageBox.Show(Rekod.Properties.Resources.mainFrm_errFileFomat, Rekod.Properties.Resources.error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return OpenErrorSrid(required);
                }
                if (temp != "")
                {
                    return temp;
                }
            }
            return OpenErrorSrid(required);
        }
        private string OpenErrorSrid(bool required)
        {
            if (MessageBox.Show(required ? Rekod.Properties.Resources.mainFrm_errProjFile : Rekod.Properties.Resources.mainFrm_errProjFileChange,
                   Rekod.Properties.Resources.mainFrm_mapOpenError, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                string temp = "";
                var dlg = new OpenFileDialog();
                dlg.Filter = "SRID|*.srid";
                dlg.Title = Rekod.Properties.Resources.mainFrm_setProjFile;
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    var sr = new StreamReader(dlg.FileName);
                    while (sr.Peek() > -1)
                    {
                        temp = sr.ReadLine();
                    }
                    sr.Close();
                    if (temp != "")
                    {
                        return temp;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
        private String GetMapFile(string filePath, bool required)
        {
            string path = Path.GetFullPath(filePath + ".vmp");
            if (File.Exists(path))
            {
                string srid = GetSrid(filePath, required);
                if (srid != null)
                {
                    Program.srid = srid;
                    return filePath + ".vmp";
                }
                else
                    return String.Empty;
            }
            else
            {
                if (MessageBox.Show(required ? Rekod.Properties.Resources.mainFrm_errMFileNF : Rekod.Properties.Resources.mainFrm_errMFileNFChange,
                        Rekod.Properties.Resources.mainFrm_errMOpen, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    var dlg = new OpenFileDialog();
                    dlg.Filter = "VMP|*.vmp";
                    dlg.Title = Rekod.Properties.Resources.mainFrm_msgMFile;
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        string srid = GetSrid(Path.GetFileName(dlg.FileName), Path.GetDirectoryName(dlg.FileName), required);
                        if (srid != null)
                        {
                            Program.srid = srid;
                            return dlg.FileName;
                        }
                        else
                            return String.Empty;
                    }
                    else
                    {
                        throw new Exception(Rekod.Properties.Resources.mainFrm_FileNF);
                    }
                }
                else
                {
                    throw new Exception(Rekod.Properties.Resources.mainFrm_FileNF);
                }
            }
        }
        private void ReloadSelectedList(mvLayer layer)
        {
            //TODO: надо работать через порядковые номера
            this._selectLayer = layer;
            this._selectObjectsIds.Clear();
            var _ids = layer.getSelected();
            for (int i = 0; _ids.count > i; i++)
            {
                _selectObjectsIds.Add(_ids.getElem(i));
            }
            if (_selectObjectsIds.Count > 0)
                this.StatusInfo = String.Format(Rekod.Properties.Resources.mainFrm_SelectedCount, _selectObjectsIds.Count);
            else
                this.StatusInfo = "";
        }
        private void ReloadDataMenu(sw.Controls.MenuItem msp)
        {
            if (Program.user_info.admin == false && Program.WithMap == true)
            {
                if (Program.user_info.type_user == 1)
                {
                    Program.WinMain.MiTools.Visibility = sw.Visibility.Visible;
                    Program.WinMain.MiManageTables.Visibility = sw.Visibility.Visible;
                    Program.WinMain.MiUserRights.Visibility = sw.Visibility.Visible;
                    Program.WinMain.MiHistory.Visibility = sw.Visibility.Collapsed;
                    Program.WinMain.MiSettings.Visibility = sw.Visibility.Visible;
                    Program.WinMain.MiLocations.Visibility = sw.Visibility.Visible;
                }
                else
                {
                    Program.WinMain.MiTools.Visibility = sw.Visibility.Visible;
                    Program.WinMain.MiManageTables.Visibility = sw.Visibility.Collapsed;
                    Program.WinMain.MiUserRights.Visibility = sw.Visibility.Collapsed;
                    Program.WinMain.MiHistory.Visibility = sw.Visibility.Collapsed;
                    Program.WinMain.MiSettings.Visibility = sw.Visibility.Visible;
                    Program.WinMain.MiLocations.Visibility = sw.Visibility.Collapsed;
                }
            }
            /*if (Plugins.ListMainPlugins.Count == 0)//нет подключенных плагинов
                настройкиToolStripMenuItem.Visible = false;*/
            msp.Items.Clear();
            for (int i = 0; Program.tip_table.Count > i; i++)
            {
                if (Program.tip_table[i].idTipTable != 1)
                {
                    if (classesOfMetods.getTableOfType(Program.tip_table[i].idTipTable).Count != 0)
                    {
                        var windowNewMenu = new sw.Controls.MenuItem()
                        {
                            Header = Program.tip_table[i].nameTip,
                            Tag = Program.tip_table[i].idTipTable
                        };
                        msp.Items.Add(windowNewMenu);
                    }
                }
            }
            for (int i = 0; msp.Items.Count > i; i++)
            {
                if (((sw.Controls.MenuItem)msp.Items[i]).Tag == null)
                    continue;
                List<tablesInfo> tInfo;
                tInfo = classesOfMetods.getTableOfType(Convert.ToInt32(((sw.Controls.MenuItem)msp.Items[i]).Tag));
                foreach (tablesInfo t1 in tInfo)
                {
                    if (t1.hidden)
                        continue;
                    var menu = new sw.Controls.MenuItem()
                    {
                        Header = ReplaceFirstUnderline(t1.nameMap),
                        Tag = t1.idTable
                    };
                    menu.Click += OpenTable;
                    ((sw.Controls.MenuItem)msp.Items[i]).Items.Add(menu);
                }
            }
            msp.Items.Add(Program.WinMain.MiUpdMenu);
            msp.Items.Add(Program.WinMain.MiUpdDics);
        }
        private string ReplaceFirstUnderline(String text)
        {
            var chars = text.ToCharArray();
            List<char> charsNew = new List<char>();
            for (int i = 0; i < chars.Length; i++)
            {
                if (chars[i] == '_')
                {
                    charsNew.Add('_');
                }
                charsNew.Add(chars[i]);
            }
            return new String(charsNew.ToArray());
        }
        void OpenTable(object sender, sw.RoutedEventArgs e)
        {
            if (Convert.ToInt32(((sw.Controls.MenuItem)sender).Tag.ToString()) > -1)
            {
                tablesInfo ti = classesOfMetods.getTableInfo(Convert.ToInt32(((sw.Controls.MenuItem)sender).Tag.ToString()));
                if (ti != null)
                {
                    var idtable = Convert.ToInt32(((sw.Controls.MenuItem)sender).Tag.ToString());
                    var frm = new itemsTableGridForm(idtable)
                    {
                        Text = (Rekod.Properties.Resources.mainFrm_tData + ((sw.Controls.MenuItem)sender).Header + "\"")
                    };
                    frm.Show();
                    frm.Activate();
                }
                else
                {
                    MessageBox.Show(Rekod.Properties.Resources.mainFrm_errBDUnav, "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ReloadDataMenu(Program.WinMain.MiData);
                }
            }
        }
        private void ShowSquareArea(mvLayer layer, int ids)
        {
            var ll1 = axMapLIb1.getLayer(layer.NAME);
            if (ll1 == null)
                return;
            var o1 = ll1.getObject(ids);
            //mvVectorObject o1 = ll1.getObjectByNum(1);
            string rez = "";
            if (!ll1.External)
            {
                if (o1 != null && o1.VectorType == mvVecTypes.mvVecRegion)
                {
                    try
                    {
                        var sqlCmd = new SqlWork();
                        sqlCmd.sql = "SELECT st_area(st_geomfromtext('" + o1.getWKT() + "', " + Program.srid + ")) as val";
                        sqlCmd.Execute(false);
                        if (sqlCmd.CanRead())
                        {
                            rez = sqlCmd.GetValue<string>(0);
                            sqlCmd.Close();
                        }
                        else
                        {
                            sqlCmd.Close();
                        }
                        this.StatusInfo = Rekod.Properties.Resources.mainFrm_square + rez + Rekod.Properties.Resources.mainFrm_sqm;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
                else
                {
                    this.StatusInfo = "";
                }
            }
            else
            {
                this.StatusInfo = "";
            }
        }
        public void ReloadApp()
        {
            layersManager1 = new layersManager(this);
            {
                layerItemsView1.RefreshLayers();
                layerItemsView1.MakeGroups();
                layerItemsView1.AddNewLayersInGroups();
                layerItemsView1.SetPodlojkaLayersVisible();
                layerItemsView1.SetRastrLayersVisible();
            }

            Program.WinMain.TbtbHand.IsChecked = true;

            this.Focus();
            {
                int tabindex = layerItemsView1.tabControl1.SelectedIndex;
                layerItemsView1.textBox2.Text = "";
                //          layerItemsView1.UpdateTree();
                layerItemsView1.tabControl1.SelectedIndex = 2;
                System.Threading.Thread.Sleep(500);
                foreach (var ti in classesOfMetods.getTableOfType(1))
                {
                    layerItemsView1.layerSetVisible(false, null, ti.idTable);
                }
                layerItemsView1.SetVisibleDefault();
                layerItemsView1.RefreshLayers();
                layerItemsView1.tabControl1.SelectedIndex = tabindex;
            }
        }
        public void LoadFormTableData(int idT, int idT_Obj)
        {
            if (idT <= 0)
                return;
            this.idT = idT;
            this.idT_Obj = idT_Obj;
            //var frm = new axVisUtils.FormTableData(idT, idT_Obj);
            //frm.Show();

            Program.work.OpenForm.ShowAttributeObject(Program.app.getTableInfo(idT), idT_Obj, false, this.ParentForm, ActionDResult: null);
        }
        private string GetWktObj(tablesInfo ti, int idObj, string mapSrid)
        {
            string wkt_tamp = "";

            SqlWork sqlWork = new SqlWork();
            sqlWork.sql = String.Format("SELECT st_astext(st_transform({0}, {5})) FROM \"{1}\".\"{2}\" WHERE \"{3}\"={4};", ti.geomFieldName, ti.nameSheme,
                ti.nameDB, ti.pkField, idObj, Program.srid);
            sqlWork.Execute(false);
            if (sqlWork.CanRead())
            {
                wkt_tamp = sqlWork.GetValue<string>(0);
            }
            sqlWork.Close();
            return wkt_tamp;
        }
        private string GetWktObj(Interfaces.tablesInfo ti, int idObj)
        {
            using (SqlWork sqlWork = new SqlWork())
            {
                sqlWork.sql = String.Format("SELECT st_astext({0}) FROM \"{1}\".\"{2}\" WHERE \"{3}\"={4};", ti.geomFieldName, ti.nameSheme,
                    ti.nameDB, ti.pkField, idObj);

                return sqlWork.ExecuteScalar<string>();
            }
        }
        private string WktTransform(string wkt, int inSrid, int fromSrid)
        {
            if (inSrid != fromSrid)

                using (SQLiteWork sqlCmd = new SQLiteWork())
                {
                    sqlCmd.InstallSpatialite();
                    sqlCmd.Sql = String.Format("SELECT astext(transform(GeomFromText(\'{0}\', {1}), {2}));", wkt, inSrid, fromSrid);
                    return sqlCmd.ExecuteScalar<string>();
                }
            else
                return wkt;
        }
        private void OpenObjForm(tablesInfo ti, int idObj, string wkt, mvMapLib.mvLayer layer, object sender)
        {
            if (layer != null)
                layer.RemoveObjects();

            //axVisUtils.FormTableData frm = new axVisUtils.FormTableData(ti, idObj, false, wkt);
            //frm.Show();
            //frm.ActionResult = (DialogResult s) => classesOfMetods.reloadAllLayerData();

            Program.work.OpenForm.ShowAttributeObject(Program.app.getTableInfo(ti.idTable), idObj, false, this.ParentForm, wkt,
                    (DialogResult s) => classesOfMetods.reloadAllLayerData());

            TbtbHand_Checked(sender, null);
        }
        private void ClearLayerHollow(object sender, mvMapLib.mvLayer layer)
        {
            bManager.SetButtonsState();
            Program.WinMain.TbtbClippingPolygonBySpecifyingPoints.IsEnabled = true;
            _newHollowAdding = false;
            TbtbHand_Checked(sender, null);
            if (layer != null)
                layer.RemoveObjects();
            classesOfMetods.reloadAllLayerData();
        }
        public void AddMenu(sw.Controls.MenuItem swMenu)
        {
            Program.WinMain.MainMenu.Items.Add(swMenu);
        }
        public void AddPanelToolsPlugins(sw.Controls.Control swItem)
        {
            Program.WinMain.TbPlugins.Visibility = sw.Visibility.Visible;
            Program.WinMain.TbPlugins.Items.Add(swItem);
            tsp_Plugins.IsVisable = true;
        }

        private Geometry GetObjectGeometry(int idTable, int idObject)
        {
            var tableInfo = Program.app.getTableInfo(idTable);
            if (tableInfo != null)
            {
                var wkt = GetWktObj(tableInfo, idObject);
                if (!String.IsNullOrEmpty(wkt))
                    return Geometry.CreateFromWkt(wkt);
            }
            return null;
        }

        private string GetSqlGeomString(String wkt)
        {
            return String.Format("SELECT ST_geomfromtext('{0}', {1})", wkt, Program.srid);
        }
        private String GetSqlGeomString(tablesInfo ti, int idObj)
        {
            return String.Format("SELECT st_transform({0}, {5}) as the_geom FROM \"{1}\".\"{2}\" WHERE \"{3}\" = {4}",
                ti.geomFieldName,
                ti.nameSheme,
                ti.nameDB,
                ti.pkField,
                idObj,
                Program.srid);
        }
        private String GetSqlGeomString(tablesInfo ti, List<int> idObjs)
        {
            string sqlIn = "";
            foreach (var item in idObjs)
            {
                sqlIn += item + ",";
            }
            sqlIn = sqlIn.Substring(0, sqlIn.Length - 1);
            return String.Format("SELECT (ST_Dump(st_transform({0}, {5}))) as st_dump , \"{3}\" as pk FROM \"{1}\".\"{2}\" WHERE \"{3}\" in ({4})",
                ti.geomFieldName,
                ti.nameSheme,
                ti.nameDB,
                ti.pkField,
                sqlIn,
                Program.srid);
        }

        /// <summary>
        /// Вычисляет геометриюю после разрезания ее линией
        /// </summary>
        /// <param name="geometry">Исходная геометрия объекта</param>
        /// <param name="objSrid">Проекция объекта</param>
        /// <param name="lineWkt">Геометрия разрезающей линии</param>
        /// <returns>Список геометрий</returns>
        private List<GeomDumpInfo> DivisionObject(Geometry geometry, int objSrid, string lineWkt)
        {
            List<GeomDumpInfo> geomList = new List<GeomDumpInfo>();

            try
            {
                using (SqlWork sqlCmd = new SqlWork())
                {
                    if (geometry.GetGeometryType() == wkbGeometryType.wkbMultiPolygon)
                    {
                        sqlCmd.sql = String.Format(@"
SELECT st_astext(ST_multi((rez.st_dump::geometry_dump).geom)), (rez.st_dump::geometry_dump).path[1]
FROM (SELECT st_dump((SELECT st_polygonize(ST_Union(st_boundary(st_transform(ST_geomfromtext('{0}', {1}), {3})), line)) AS mpoly 
			FROM (SELECT ST_geomfromtext('{2}', {3}) AS line) AS b)
		) as st_dump, st_transform(ST_geomfromtext('{0}', {1}), {3}) as the_geom	
) as rez
WHERE ST_Within((rez.st_dump::geometry_dump).geom, ST_Buffer(rez.the_geom, 0.01))
ORDER BY (rez.st_dump::geometry_dump).path[1]::integer;", geometry.GetWkt(), objSrid, lineWkt, Program.srid);
                    }
                    else if (geometry.GetGeometryType() == wkbGeometryType.wkbPolygon)
                    {
                        sqlCmd.sql = String.Format(@"
SELECT st_astext((rez.st_dump::geometry_dump).geom), (rez.st_dump::geometry_dump).path[1]
FROM (SELECT st_dump((SELECT st_polygonize(ST_Union(st_boundary(st_transform(ST_geomfromtext('{0}', {1}), {3})), line)) AS mpoly 
			FROM (SELECT ST_geomfromtext('{2}', {3}) AS line) AS b)
		) as st_dump, st_transform(ST_geomfromtext('{0}', {1}), {3}) as the_geom
) as rez
WHERE ST_Within((rez.st_dump::geometry_dump).geom, ST_Buffer(rez.the_geom, 0.01))
ORDER BY (rez.st_dump::geometry_dump).path[1]::integer;", geometry.GetWkt(), objSrid, lineWkt, Program.srid);
                    }
                    else if (geometry.GetGeometryType() == wkbGeometryType.wkbMultiLineString)
                    {
                        sqlCmd.sql = String.Format(@"
SELECT ST_AsText(ST_multi((rez.st_dump::geometry_dump).geom)), (rez.st_dump::geometry_dump).path[1]
FROM (SELECT ST_Dump((ST_Difference(
                       ST_Transform(ST_GeomFromText('{0}', {1}), {2}),
                       ST_GeomFromText('{3}', {2})))) AS st_dump
) as rez
ORDER BY (rez.st_dump::geometry_dump).path[1]::integer;", geometry.GetWkt(), objSrid, Program.srid, lineWkt);
                    }
                    else if (geometry.GetGeometryType() == wkbGeometryType.wkbLineString)
                    {
                        sqlCmd.sql = String.Format(@"
SELECT ST_AsText((rez.st_dump::geometry_dump).geom), (rez.st_dump::geometry_dump).path[1]
FROM (SELECT ST_Dump((ST_Difference(
                       ST_Transform(ST_GeomFromText('{0}', {1}), {2}),
                       ST_GeomFromText('{3}', {2})))) AS st_dump
) as rez
ORDER BY (rez.st_dump::geometry_dump).path[1]::integer;", geometry.GetWkt(), objSrid, Program.srid, lineWkt);
                    }

                    sqlCmd.ExecuteReader();

                    while (sqlCmd.CanRead())
                    {
                        Geometry geom = Geometry.CreateFromWkt(sqlCmd.GetString(0));
                        geomList.Add(new GeomDumpInfo() { geom = geom.GetWkb(), pathId = sqlCmd.GetInt32(1) });
                    }
                    sqlCmd.Close();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }

            return geomList;
        }

        private void InsertUpdateCosmeticObjects(mvLayer layer, DataAccess.SourceCosmetic.Model.CosmeticTableBaseM table, List<GeomDumpInfo> obj)
        {
            string label = "";
            foreach (var item in obj)
            {
                Geometry geom = Geometry.CreateFromWkb(item.geom);

                if (item.pathId == 1)
                {
                    var tableObj = table.UpdateObject(item.idObj, geom.GetWkt());
                    if (tableObj != null)
                        label = (string)tableObj.GetAttributeValue("label");
                }
                else
                {
                    table.CreateObject(label, geom.GetWkt());
                }
            }
        }
        private void InsertUpdateObjects(tablesInfo ti, List<GeomDumpInfo> obj)
        {
            //Первый объект обновляет поле геометрии исходного объекта, остальные создают новые
            foreach (var item in obj)
            {
                List<IParams> prms = new List<IParams>();
                prms.Add(new Params("@binary", item.geom, DbType.Binary));
                if (item.pathId == 1)
                {
                    using (SqlWork sqlCmd = new SqlWork())
                    {
                        sqlCmd.sql = String.Format("UPDATE \"{0}\".\"{1}\" SET \"{2}\" = ST_transform(st_geomfromwkb(@binary, {4}), {7}) " +
                            "WHERE \"{5}\"={6}",
                            ti.nameSheme, ti.nameDB, ti.geomFieldName, sqlCmd.GetString(0), Program.srid, ti.pkField, item.idObj, ti.srid);
                        sqlCmd.ExecuteNonQuery(prms);
                        sqlCmd.Close();
                    }
                }
                else
                {
                    using (SqlWork sqlCmd = new SqlWork())
                    {
                        string insert_sql = GetInsertSql(ti, item.idObj);

                        sqlCmd.sql = String.Format(insert_sql,
                            "ST_transform(st_geomfromwkb(@binary, " + Program.srid + "), " + ti.srid + ")");
                        sqlCmd.ExecuteNonQuery(prms);
                        sqlCmd.Close();
                    }
                }
            }
        }
        private string GetInsertSql(tablesInfo ti, int idObj)
        {
            string sql_field = "";
            string sql_field_val = "";
            List<fieldInfo> fInfo = classesOfMetods.getFieldInfoTable(ti.idTable);
            for (int i = 0; fInfo.Count > i; i++)
            {
                if (fInfo[i].nameDB != ti.pkField)
                {
                    sql_field += "\"" + fInfo[i].nameDB + "\",";
                    if (fInfo[i].nameDB != ti.geomFieldName)
                    {
                        sql_field_val += "\"" + fInfo[i].nameDB + "\",";
                    }
                    else
                    {
                        sql_field_val += "{0},";
                    }
                }
            }
            sql_field = sql_field.Substring(0, sql_field.Length - 1);
            sql_field_val = sql_field_val.Substring(0, sql_field_val.Length - 1);

            sql_field = "INSERT INTO \"" + ti.nameSheme + "\".\"" + ti.nameDB + "\" (" + sql_field +
                ") SELECT " + sql_field_val + " FROM \"" + ti.nameSheme + "\".\"" + ti.nameDB +
                "\" WHERE \"" + ti.pkField + "\"=" + idObj.ToString();

            return sql_field;
        }
        private List<int> GetListIdFromSelectObjects(ObservableCollection<mvVectorObject> idObjs)
        {
            List<int> ids = new List<int>();
            foreach (var item in idObjs)
            {
                try
                {
                    ids.Add(Convert.ToInt32(item.fieldValue("id")));
                }
                catch (Exception ex)
                {
                    this.StatusInfo = ex.Message;
                }
            }
            return ids;
        }
        private void SeparationObject(ObservableCollection<int> idObjs)
        {
            var layerM = bManager.CurrentTable as AbsM.ILayerM;
            var tableInfo = bManager.CurrentTable as Interfaces.tablesInfo;
            if (tableInfo == null && layerM != null && layerM is PgM.PgTableBaseM)
                tableInfo = Program.app.getTableInfo((int)layerM.Id);

            Geometry geom;
            var programSrid = ExtraFunctions.Converts.To<int>(Program.srid);
            try
            {

                if (tableInfo != null)
                {
                    foreach (var item in idObjs)
                    {
                        List<GeomDumpInfo> obj = new List<GeomDumpInfo>();
                        int pathIdGeom = 1;
                        var wkt = GetWktObj(tableInfo, item);
                        geom = Geometry.CreateFromWkt(wkt);
                        geom.SetSrid((int)tableInfo.srid);
                        geom.Transform(programSrid);
                        int geomCount = geom.GetGeometryCount();
                        for (int i = 1; i < geomCount + 1 && geomCount > 1; i++)
                        {
                            Geometry temp = geom.GetGeometryRef(i - 1);
                            if (!geom.IsSimple() || geom.GetGeometryType().ToString().ToLower().Contains("multi"))
                            {
                                temp = TransformInMultiGeom(temp);
                            }
                            obj.Add(new GeomDumpInfo() { geom = temp.GetWkb(), pathId = pathIdGeom, idObj = item });
                            pathIdGeom++;
                        }
                        InsertUpdateObjects(classesOfMetods.getTableInfo(tableInfo.idTable), obj);
                    }
                    classesOfMetods.reloadLayerData((classesOfMetods.getTableInfo(tableInfo.idTable)));
                }
                else if (layerM != null)
                {
                    var layerML = axMapLIb1.getLayer(layerM.NameMap);
                    if (layerML != null && layerM.IsVisible && layerM.IsEditable)
                    {
                        foreach (var item in idObjs)
                        {
                            List<GeomDumpInfo> obj = new List<GeomDumpInfo>();
                            int pathIdGeom = 1;
                            var cobj = layerML.getObject(item);
                            if (cobj != null)
                            {
                                var wkt = cobj.getWKT();

                                geom = Geometry.CreateFromWkt(wkt);
                                geom.SetSrid(programSrid);
                                int geomCount = geom.GetGeometryCount();
                                for (int i = 1; i < geomCount + 1 && geomCount > 1; i++)
                                {
                                    Geometry temp = geom.GetGeometryRef(i - 1);
                                    //MessageBox.Show(wkt + "\n\r" + temp.GetWkt() + "\n\r" + geom.GetWkt());
                                    if (!geom.IsSimple() || geom.GetGeometryType().ToString().ToLower().Contains("multi"))
                                    {
                                        temp = TransformInMultiGeom(temp);
                                    }
                                    //MessageBox.Show(wkt + "\n\r" + temp.GetWkt());
                                    obj.Add(new GeomDumpInfo() { geom = temp.GetWkb(), pathId = pathIdGeom, idObj = item });
                                    pathIdGeom++;
                                }
                            }
                            else
                                throw new Exception(Properties.Resources.mainFrm_errNotFoundWorkObject);

                            InsertUpdateCosmeticObjects(layerML, layerM as CosM.CosmeticTableBaseM, obj);
                        }

                        classesOfMetods.reloadLayerData(layerML);
                    }
                    else
                    {
                        layerM.IsVisible = false;
                        throw new Exception(Properties.Resources.mainFrm_errWorkLayerNotEnabled);
                    }
                }
                else
                {
                    throw new Exception(Properties.Resources.mainFrm_errIncorrectLayerType);
                }

            }
            catch (Exception ex)
            {
                Classes.workLogFile.writeLogFile(ex, true, true);
            }
            //classesOfMetods.reloadLayer(ti.idTable);
        }
        internal void RemoveMenu(sw.Controls.MenuItem swMenu)
        {
            Program.WinMain.MainMenu.Items.Remove(swMenu);
        }
        internal void RemovePanelToolsPlugins(sw.Controls.Control swItem)
        {
            Program.WinMain.TbPlugins.Items.Remove(swItem);
        }
        public void SetPosition(double x, double y, double scale)
        {
            if (axMapLIb1.setScale(scale, true) == true)
            {
                axMapLIb1.setScrCenter(x, y);
                this.StatusInfo = Rekod.Properties.Resources.mainFrm_setPos;
                axMapLIb1.mapRepaint();
            }
            else
            {
                this.StatusInfo = Rekod.Properties.Resources.mainFrm_errSetPos;
            }
        }
        /// <summary>
        /// Регистрируем панель и привязка к меню
        /// </summary>
        /// <param name="bManager"></param>
        /// <param name="toolStrip"></param>
        private ToolBar_VM AddPanelToolBar(Control toolStrip, sw.Controls.MenuItem toolStripMenuItem)
        {
            Rekod.ViewModel.ToolBar_VM toolbar = bManager.AddPanel(toolStrip.Name);
            toolbar.IsVisable = toolStrip.Visible;
            toolStripMenuItem.Tag = toolbar;
            return toolbar;
        }
        private ToolBar_VM AddPanelToolBar(sw.Controls.Control toolStrip, sw.Controls.MenuItem toolStripMenuItem)
        {
            var toolbar = bManager.AddPanel(toolStrip.Name);
            toolbar.IsVisable = (toolStrip.Visibility == System.Windows.Visibility.Visible);
            toolStripMenuItem.Tag = toolbar;
            return toolbar;
        }
        private void MenuReloadReports()
        {
            Program.WinMain.MiReports.Items.Remove(Program.WinMain.MiReportsSeparator);
            Program.WinMain.MiReports.Items.Remove(Program.WinMain.MiReportsOpen);
            Program.WinMain.MiReports.Items.Clear();
            var listReports = Program.ReportModel.ListReports;
            foreach (var item in listReports)
            {
                switch (item.Type)
                {
                    case enTypeReport.All:
                        {
                            CreateItemReport(Program.WinMain.MiReports, item);
                        }
                        break;
                    default:
                        break;
                }
            }
            if (Program.user_info.admin)
            {
                if (Program.WinMain.MiReports.Items.Count > 0)
                {
                    Program.WinMain.MiReports.Items.Add(Program.WinMain.MiReportsSeparator);
                }
                Program.WinMain.MiReports.Items.Add(Program.WinMain.MiReportsOpen);
            }
            if (Program.WinMain.MiReports.Items.Count == 0)
            {
                Program.WinMain.MiReports.Visibility = sw.Visibility.Collapsed;
            }
        }
        private void CreateItemReport(sw.Controls.MenuItem menuItem, IReportItem_M item)
        {
            var subItem = new sw.Controls.MenuItem()
            {
                Header = item.Caption,
                Tag = item
            };
            subItem.Click += MiReportSubItem_Click;
            menuItem.Items.Add(subItem);
        }
        private void MiReportSubItem_Click(object sender, sw.RoutedEventArgs e)
        {
            var tsItemObject = sender as sw.Controls.MenuItem;
            var report = tsItemObject.Tag as ReportItem_M;
            try
            {
                Program.ReportModel.OpenReport(report, new FilterTable());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Rekod.Properties.Resources.mainFrm_errorWhenOpenReport, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void SearchFullText(String searchtext = null)
        {
            if (_pgFullTextSearchVM == null)
            {
                _pgFullTextSearchVM = new PgFullTextSearchVM(Program.repository);
                var pgFullTextSearchV =
                    new Rekod.DataAccess.SourcePostgres.View.PgFullTextSearch.PgFullTextSearchUV();
                WindowViewModelBase_VM.GetWindow(pgFullTextSearchV, _pgFullTextSearchVM, 350, axMapLIb1.Height, 250, 350, Program.WinMain);
                _pgFullTextSearchVM.AttachedWindow.WindowStyle = System.Windows.WindowStyle.SingleBorderWindow;
            }
            if (!_pgFullTextSearchVM.AttachedWindow.IsVisible)
            {
                Point pt = axMapLIb1.PointToScreen(new Point(0, 0));
                _pgFullTextSearchVM.AttachedWindow.Top = pt.Y;
                _pgFullTextSearchVM.AttachedWindow.Left = pt.X;
                _pgFullTextSearchVM.AttachedWindow.Height = axMapLIb1.Height;
                _pgFullTextSearchVM.AttachedWindow.Width = 350;
                _pgFullTextSearchVM.AttachedWindow.Show();
            }
            if (searchtext != null)
            {
                if (_pgFullTextSearchVM.CanFullTextSearch(searchtext))
                {
                    _pgFullTextSearchVM.FullTextSearch(searchtext);
                }
            }
            var searchUV =
                _pgFullTextSearchVM.AttachedWindow.Content as Rekod.DataAccess.SourcePostgres.View.PgFullTextSearch.PgFullTextSearchUV;
            if (searchUV != null)
            {
                searchUV.SearchBox.Focus();
            }
        }

        /// <summary>
        /// Показать форму для выбора объектов на карте, используемых для объединения, разрезания и т.п. геометрии
        /// </summary>
        /// <param name="selectedEvent"></param>
        /// <param name="formClosedEvent"></param>
        private void ShowFormForObjectSelecting(EventHandler selectedEvent, FormClosedEventHandler formClosedEvent)
        {
            if (frmHollowCopyObj == null)
            {
                frmHollowCopyObj = new SelectedSetFrm();

                frmHollowCopyObj.Location = new Point((int)Program.WinMain.Left + this.axMapLIb1.Size.Width / 2 - frmHollowCopyObj.Size.Width / 2,
                       (int)Program.WinMain.Top + this.Size.Height - frmHollowCopyObj.Size.Height);
                classesOfMetods.SetFormOwner(frmHollowCopyObj);

                frmHollowCopyObj.selectSetBtn.Click += new EventHandler(selectedEvent);
                frmHollowCopyObj.cancelBtn.Click += new EventHandler(cancelBtn_Click);
                frmHollowCopyObj.FormClosed += new FormClosedEventHandler(formClosedEvent);
                frmHollowCopyObj.Show();
            }
            else
            {
                frmHollowCopyObj.Location = new Point((int)Program.WinMain.Left + this.axMapLIb1.Size.Width / 2 - frmHollowCopyObj.Size.Width / 2,
                       (int)Program.WinMain.Top + this.Size.Height - frmHollowCopyObj.Size.Height);
                frmHollowCopyObj.Activate();
            }
            bManager.SetButtonsState();

            SaveSelectedTable();
        }

        /// <summary>
        /// Добавление геометрии объекта с помощью другого объекта
        /// </summary>
        /// <param name="wktObject">Объект, в который добавляется геометрия</param>
        /// <param name="sourceObjects">Объекты-источники</param>
        /// <param name="ti">Таблица объекта, в который добавляется геометрия</param>
        /// <param name="idObj">ID объекта, в который добавляется геометрия</param>
        private void JoinGeometryPg(string wktObject, ObservableCollection<mvVectorObject> sourceObjects, Interfaces.tablesInfo table, int idObj)
        {
            try
            {
                if (!wktObject.StartsWith("MULTI"))
                    throw new Exception(Properties.Resources.mainFrm_errGeometryShouldBeMulti);
                if (!wktObject.Contains("("))
                    throw new Exception(Properties.Resources.mainFrm_errObjectGeometryIsNotRecognized);
                if (table == null)
                    throw new Exception(Properties.Resources.mainFrm_errTableShouldNotBeEmpty);

                string wktSource = sourceObjects[0].getWKT();
                string objGeomType = wktObject.Substring(0, wktObject.IndexOf('(')).ToUpper().Trim(), // тип геометрии объекта, в который добавляем
                    sourceGeomType = wktSource.Substring(0, wktSource.IndexOf('(')).ToUpper().Trim(); // тип геометрии объекта-источника

                if (objGeomType == sourceGeomType || objGeomType == "MULTI" + sourceGeomType)
                {
                    wktObject = GetUnionsGeom(wktObject, sourceObjects);

                    var layerName = Program.RelationVisbleBdUser.GetNameInBd(table.idTable);
                    mvMapLib.mvLayer tempLayer = Program.mainFrm1.axMapLIb1.getLayer(layerName);
                    if (tempLayer != null)
                        tempLayer.RemoveObjects();

                    Action<DialogResult> actionCloseAttrForm = (DialogResult s) => classesOfMetods.reloadAllLayerData();
                    Program.work.OpenForm.ShowAttributeObject(table, idObj, false, this.ParentForm, wktObject,
                            actionCloseAttrForm);
                    // Обновление измененного слоя
                    if (tempLayer != null)
                    {
                        if (tempLayer.External)
                        {
                            tempLayer.ExternalFullReloadVisible();
                        }
                    }

                    bManager.SetButtonsState();
                }
                else
                {
                    throw new Exception(Properties.Resources.mainFrm_errGeometryTypesShouldMatch);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, Properties.Resources.mainFrm_errErrorInGeometryAdding, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        /// <summary>
        /// Добавление геометрии объекта с помощью другого объекта
        /// </summary>
        /// <param name="wktObject">Объект, в который добавляется геометрия</param>
        /// <param name="sourceObjects">Объекты-источники</param>
        /// <param name="ti">Таблица объекта, в который добавляется геометрия</param>
        /// <param name="idObj">ID объекта, в который добавляется геометрия</param>
        private void JoinGeometryAbsM(string wktObject, ObservableCollection<mvVectorObject> sourceObjects, AbsM.ILayerM table, int idObj)
        {
            try
            {
                if (!wktObject.Contains("("))
                    throw new Exception(Properties.Resources.mainFrm_errObjectGeometryIsNotRecognized);
                if (table == null)
                    throw new Exception(Properties.Resources.mainFrm_errTableShouldNotBeEmpty);

                string wktSource = sourceObjects[0].getWKT();
                string objGeomType = wktObject.Substring(0, wktObject.IndexOf('(')).ToUpper().Trim(), // тип геометрии объекта, в который добавляем
                    sourceGeomType = wktSource.Substring(0, wktSource.IndexOf('(')).ToUpper().Trim(); // тип геометрии объекта-источника

                if (objGeomType == sourceGeomType || objGeomType == "MULTI" + sourceGeomType)
                {
                    var tableM = table as AbsM.ILayerM;

                    wktObject = GetUnionsGeom(wktObject, sourceObjects);

                    tableM.Source.OpenObject(tableM, idObj, wktObject);

                    bManager.SetButtonsState();
                }
                else
                {
                    throw new Exception(Properties.Resources.mainFrm_errGeometryTypesShouldMatch);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, Properties.Resources.mainFrm_errErrorInGeometryAdding, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private string GetUnionsGeom(string objWkt, ObservableCollection<mvVectorObject> sourceObjects)
        {
            foreach (var item in sourceObjects)
            {

                using (SqlWork sqlCmd = new SqlWork(true))
                {
                    sqlCmd.sql = String.Format("SELECT st_astext(ST_Union(ST_geomfromtext('{0}', {1}),ST_geomfromtext('{2}', {1})));",
                        objWkt, Program.srid, item.getWKT());

                    objWkt = sqlCmd.ExecuteScalar<string>();
                }
            }
            return objWkt;
        }
        /// <summary>
        /// Обрезание полигона другими полигонами
        /// </summary>
        /// <param name="cuttingGeomList">Геометрии полигонов для пересечения</param>
        /// <param name="geometryFunc">Функция для редактирования геометрии</param>
        private void GeometryOperationWithPolygon(List<Geometry> cuttingGeomList, Func<Geometry, List<Geometry>, Geometry> geometryFunc)
        {
            if (CopyTable != null)
            {
                var layerM = CopyTable as AbsM.ILayerM;
                var tableInfo = CopyTable as Interfaces.tablesInfo;
                if (tableInfo == null && layerM != null && layerM is PgM.PgTableBaseM)
                    tableInfo = Program.app.getTableInfo((int)layerM.Id);

                try
                {
                    if (tableInfo != null)
                    {
                        var wkt = GetWktObj(tableInfo, (int)CopyObject);
                        if (!String.IsNullOrEmpty(wkt))
                        {
                            var geomObj = Geometry.CreateFromWkt(wkt);
                            var programSrid = ExtraFunctions.Converts.To<int>(Program.srid);
                            geomObj.SetSrid((int)tableInfo.srid);
                            geomObj.Transform(programSrid);

                            var geometryType = (int)geomObj.GetGeometryType();
                            var geomResult = new Geometry(geomObj.GetGeometryType());

                            geomObj = geometryFunc(geomObj, cuttingGeomList);

                            if (geomObj.GetGeometryType().ToString().Contains("Multi")
                                || geomObj.GetGeometryType() == wkbGeometryType.wkbGeometryCollection)
                            {
                                int geomCount = geomObj.GetGeometryCount();
                                for (int i = 1; i < geomCount + 1; i++)
                                {
                                    var type = (int)geomObj.GetGeometryRef(i - 1).GetGeometryType();
                                    if (type > 0 && type <= 6
                                        && geometryType % 3 == type % 3)
                                    {
                                        geomResult.AddGeometry(geomObj.GetGeometryRef(i - 1));
                                    }
                                }
                            }
                            else
                            {
                                var type = (int)geomObj.GetGeometryType();
                                if (type > 0 && type <= 6
                                    && geometryType % 3 == type % 3)
                                {
                                    geomResult.AddGeometry(geomObj);
                                }
                            }

                            Action<DialogResult> actionCloseAttrForm = (DialogResult s) => classesOfMetods.reloadAllLayerData();
                            Program.work.OpenForm.ShowAttributeObject(tableInfo, (int)CopyObject, false, this.ParentForm, geomResult.GetWkt(),
                                    actionCloseAttrForm);

                            string layerName = Program.RelationVisbleBdUser.GetNameInBd(tableInfo.idTable);
                            var layer = axMapLIb1.getLayer(layerName);
                            if (layer != null)
                            {
                                layer.editable = true;
                            }
                        }
                    }
                    else if (layerM != null)
                    {
                        var layerML = axMapLIb1.getLayer(layerM.NameMap);
                        if (layerM.IsVisible && layerML != null)
                        {
                            var obj = layerML.getObject((int)CopyObject);
                            if (obj != null)
                            {
                                var wkt = obj.getWKT();
                                if (!String.IsNullOrEmpty(wkt))
                                {
                                    var geomObj = Geometry.CreateFromWkt(wkt);
                                    var programSrid = ExtraFunctions.Converts.To<int>(Program.srid);
                                    geomObj.SetSrid(programSrid);

                                    geomObj = geometryFunc(geomObj, cuttingGeomList);

                                    layerM.Source.OpenObject(layerM, CopyObject, geomObj.GetWkt());

                                    layerM.IsEditable = true;
                                }
                            }
                            else
                                throw new Exception(Properties.Resources.mainFrm_errNotFoundWorkObject);
                        }
                        else
                        {
                            layerM.IsVisible = false;
                            throw new Exception(Properties.Resources.mainFrm_errWorkLayerNotEnabled);
                        }
                    }
                    else
                    {
                        throw new Exception(Properties.Resources.mainFrm_errIncorrectLayerType);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, Properties.Resources.mainFrm_errErrorInGeometryEditing, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// Обрезание полигона линией
        /// </summary>
        /// <param name="wktLine">Геометрия линии</param>
        private void CutPolygonWithLine(string wktLine)
        {
            List<int> objects = SelectedObjectsIds.ToList();

            if (CopyTable != null)
            {
                var layerM = CopyTable as AbsM.ILayerM;
                var tableInfo = CopyTable as Interfaces.tablesInfo;
                if (tableInfo == null && layerM != null && layerM is PgM.PgTableBaseM)
                    tableInfo = Program.app.getTableInfo((int)layerM.Id);

                try
                {
                    if (tableInfo != null)
                    {
                        var ti = classesOfMetods.getTableInfo(tableInfo.idTable);
                        foreach (var id in objects)
                        {
                            var wktObject = GetWktObj(tableInfo, id);
                            if (!String.IsNullOrEmpty(wktObject))
                            {
                                var multiobject = Geometry.CreateFromWkt(wktObject);
                                List<GeomDumpInfo> obj = DivisionObject(multiobject, (int)tableInfo.srid, wktLine);
                                foreach (var o in obj)
                                {
                                    o.idObj = id;
                                }

                                InsertUpdateObjects(ti, obj);
                            }
                        }

                        string layerName = Program.RelationVisbleBdUser.GetNameInBd(tableInfo.idTable);
                        var layer = axMapLIb1.getLayer(layerName);
                        if (layer != null)
                        {
                            layer.editable = true;
                        }
                        classesOfMetods.reloadLayerData(ti);
                    }
                    else if (layerM != null)
                    {
                        var layerML = axMapLIb1.getLayer(layerM.NameMap);
                        if (layerM.IsVisible && layerML != null)
                        {
                            int programSrid = Convert.ToInt32(Program.srid);
                            foreach (var id in objects)
                            {
                                var layerObj = layerML.getObject(id);
                                if (layerObj != null)
                                {
                                    var wktObject = layerObj.getWKT();
                                    if (!String.IsNullOrEmpty(wktObject))
                                    {
                                        var multiobject = Geometry.CreateFromWkt(wktObject);

                                        List<GeomDumpInfo> obj = DivisionObject(multiobject, programSrid, wktLine);
                                        foreach (var o in obj)
                                        {
                                            o.idObj = id;
                                        }

                                        InsertUpdateCosmeticObjects(layerML, layerM as CosM.CosmeticTableBaseM, obj);
                                    }
                                }
                            }

                            layerML.editable = true;
                            layerM.IsEditable = true;
                            layerM.Source.ReloadInfo();
                        }
                        else
                        {
                            layerM.IsVisible = false;
                            throw new Exception(Properties.Resources.mainFrm_errWorkLayerNotEnabled);
                        }
                    }
                    else
                    {
                        throw new Exception(Properties.Resources.mainFrm_errIncorrectLayerType);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, Properties.Resources.mainFrm_errErrorInGeometryEditing, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        public string StatusInfo
        {
            get { return axMapLIb1.StatusInfo; }
            set
            {
                if (value != axMapLIb1.StatusInfo && value != "")
                {
                    axMapLIb1.StatusInfo = value;
                }
                String statusInfoText = axMapLIb1.StatusInfo;
                if (statusInfoText != null)
                {
                    statusInfoText = statusInfoText.Replace("\n", " ");
                }
                Program.WinMain.TbtbInfoText.Text = statusInfoText;
            }
        }
        #endregion Методы

        #region Обработчики
        void ListLayersIsView_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var list = sender as ObservableCollection<AbsM.TableBaseM>;
            if (list != null)
            {
                var editableList =
                    from AbsM.TableBaseM tableBase in Program.mainFrm1.layerItemsView1.ListLayersIsView
                    where (tableBase is PgM.PgTableBaseM && classesOfMetods.getWriteTable(Convert.ToInt32(tableBase.Id)) == true ||
                           tableBase is CosM.CosmeticTableBaseM)
                    select tableBase;
                Program.WinMain.TbcbEditableLayer.SelectionChanged -= TbcbEditableLayer_SelectionChanged;
                Program.WinMain.TbcbEditableLayer.ItemsSource = null;
                Program.WinMain.TbcbEditableLayer.ItemsSource = editableList;
                Program.WinMain.TbcbEditableLayer.SelectedItem = layerItemsView1.EditableLayer;
                Program.WinMain.TbcbEditableLayer.SelectionChanged += TbcbEditableLayer_SelectionChanged;
            }
        }
        void WorkSets_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "CurrentWorkSet":
                    {
                        Program.WinMain.TbcbWorkSet.SelectionChanged -= TbcbWorkSet_SelectionChanged;
                        Program.WinMain.TbcbWorkSet.SelectedItem = Program.WorkSets.CurrentWorkSet;
                        Program.WinMain.TbcbWorkSet.SelectionChanged += TbcbWorkSet_SelectionChanged;
                    }
                    break;
                default:
                    break;
            }
        }
        void WorkSets_ListWorkSets_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Program.WinMain.TbcbWorkSet.SelectionChanged -= TbcbWorkSet_SelectionChanged;
            Program.WinMain.TbcbWorkSet.ItemsSource = Program.WorkSets.ListWorkSets;
            Program.WinMain.TbcbWorkSet.SelectedItem = Program.WorkSets.CurrentWorkSet;
            Program.WinMain.TbcbWorkSet.SelectionChanged += TbcbWorkSet_SelectionChanged;
        }
        void layerItemsView1_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "EditableLayer":
                    {
                        Program.WinMain.TbcbEditableLayer.SelectionChanged -= TbcbEditableLayer_SelectionChanged;
                        Program.WinMain.TbcbEditableLayer.SelectedItem = layerItemsView1.EditableLayer;
                        Program.WinMain.TbcbEditableLayer.SelectionChanged += TbcbEditableLayer_SelectionChanged;
                    }
                    break;
                default:
                    break;
            }
        }
        void axMapLIb1_ExternalSourceAfterQueryError(object sender, AxmvMapLib.IMapLIbEvents_ExternalSourceAfterQueryErrorEvent e)
        {
            Debug.WriteLine(e.message + " || " + e.query);
            Classes.workLogFile.writeLogFile(e.message + " || " + e.query + " || " + e.layer.NAME, false, true);
        }
        void axMapLIb1_OnMessage(object sender, AxmvMapLib.IMapLIbEvents_OnMessageEvent e)
        {
            Debug.WriteLine(e.msg);
        }
        void axMapLIb1_ExternalSourceConnectAborted(object sender, AxmvMapLib.IMapLIbEvents_ExternalSourceConnectAbortedEvent e)
        {
            bool continue_ask = true;
            while (continue_ask)
            {
                if (MessageBox.Show(Rekod.Properties.Resources.mainFrm_msgBDConFail,
                    Rekod.Properties.Resources.mainFrm_DBConFail, MessageBoxButtons.RetryCancel, MessageBoxIcon.Question) == DialogResult.Retry)
                {
                    Program.postgres.ResetConnection();
                    continue_ask = false;
                }
                else
                {
                    Application.Exit();
                    Process.GetCurrentProcess().Kill();
                    continue_ask = false;
                }
            }
        }
        [DebuggerStepThrough]
        public string WriteDouble(double num)
        {
            if (Math.Abs(num / 10000000) > 1)
                return num.ToString("0.000000");
            else
                return num.ToString("0.000000");
        }
        public void MoveToObjects(mvIntArray ids, mvLayer layer)
        {
            double minx = 0;
            double miny = 0;
            double maxx = 0;
            double maxy = 0;
            bool init = true;
            mvBbox bbox;
            if (ids != null)
            {
                for (int i = 0; ids.count > i; i++)
                {

                    mvVectorObject obj = layer.getObject(ids.getElem(i));
                    if (obj.points.count != 0)
                    {
                        double ax = obj.bbox.a.x;
                        double ay = obj.bbox.a.y;
                        double bx = obj.bbox.b.x;
                        double by = obj.bbox.b.y;

                        if (init)
                        {
                            minx = ax;
                            miny = ay;
                            maxx = bx;
                            maxy = by;
                            init = false;
                        }
                        else
                        {
                            minx = (minx > ax ? ax : minx);
                            miny = (miny > ay ? ay : miny);
                            maxx = (maxx < bx ? bx : maxx);
                            maxy = (maxy < by ? by : maxy);
                        }
                    }
                }
                //if (layer.SelectedCount > 0)
                //{
                bbox = new mvBbox();
                bbox.a.x = minx;
                bbox.a.y = miny;
                bbox.b.x = maxx;
                bbox.b.y = maxy;

                if (Convert.ToInt32(bbox.a.x) == Convert.ToInt32(bbox.b.x) || Convert.ToInt32(bbox.a.y) == Convert.ToInt32(bbox.b.y))
                {
                    mvMapLib.mvPointWorld gb = new mvMapLib.mvPointWorld();
                    gb.x = bbox.a.x;
                    gb.y = bbox.a.y;
                    this.SetPosition(gb.x, gb.y, Program.mainFrm1.axMapLIb1.ScaleZoom);
                }
                else
                {
                    mvBbox bbox_temp = AddPaddingToBbox(bbox, axMapLIb1.MapExtent);
                    axMapLIb1.SetExtent(bbox_temp);
                }
                axMapLIb1.mapRepaint();
                //}
            }
        }
        public void MoveToSelectedObjects(mvLayer layer)
        {
            if (layer == null) { MessageBox.Show(Rekod.Properties.Resources.mainFrm_infLayerN, Rekod.Properties.Resources.mainFrm_Info, MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            if (layer.Visible != true) { MessageBox.Show(Rekod.Properties.Resources.mainFrm_infLayerN, Rekod.Properties.Resources.mainFrm_Info, MessageBoxButtons.OK, MessageBoxIcon.Information); return; }


            string procString = "";
            try
            {
                procString = cti.ThreadProgress.Open("SetExtent");
                mvIntArray t = layer.getSelected();
                this.MoveToObjects(t, layer);
            }
            catch (Exception ex)
            {
                Classes.workLogFile.writeLogFile(ex, true, true);
            }
            finally
            {
                cti.ThreadProgress.Close(procString);
            }
        }
        public mvBbox AddPaddingToBbox(mvBbox bbox, mvBbox mapextent)
        {
            mapextent = CheckMvBbox(mapextent);
            bbox = CheckMvBbox(bbox);

            double dx = (bbox.b.x - bbox.a.x) * 0.05;
            double dy = (bbox.b.y - bbox.a.y) * 0.05;

            bbox.a.x = bbox.a.x - dx;
            bbox.a.y = bbox.a.y - dy;

            bbox.b.x = bbox.b.x + dx;
            bbox.b.y = bbox.b.y + dy;

            double mapWidth = mapextent.b.x - mapextent.a.x;
            double mapHeight = mapextent.b.y - mapextent.a.y;

            double boxWidth = bbox.b.x - bbox.a.x;
            double boxHeight = bbox.b.y - bbox.a.y;

            double relWidth = mapWidth / boxWidth;
            double relHeight = mapHeight / boxHeight;
            double relSize = relWidth > relHeight ? relHeight : relWidth;

            double dWidth = mapWidth / relSize - boxWidth;
            double dHeight = mapHeight / relSize - boxHeight;

            if (dWidth > 0)
            {
                bbox.a.x -= dWidth / 2;
                bbox.b.x -= dWidth / 2;
            }
            if (dHeight > 0)
            {
                bbox.a.y += dHeight / 2;
                bbox.b.y += dHeight / 2;
            }

            return bbox;
        }
        private mvBbox CheckMvBbox(mvBbox bbox)
        {
            if (bbox.a.x > bbox.b.x)
            {
                double xx = bbox.a.x;
                bbox.a.x = bbox.b.x;
                bbox.b.x = xx;
            }
            if (bbox.a.y > bbox.b.y)
            {
                double yy = bbox.a.y;
                bbox.a.y = bbox.b.y;
                bbox.b.y = yy;
            }
            return bbox;
        }
        public void SetExtentFromLayer(mvLayer layer)
        {
            if (layer == null) { MessageBox.Show(Rekod.Properties.Resources.mainFrm_infLayerN, Rekod.Properties.Resources.mainFrm_Info, MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            if (layer.Visible != true) { MessageBox.Show(Rekod.Properties.Resources.mainFrm_infLayerN, Rekod.Properties.Resources.mainFrm_Info, MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            string procString = "";
            try
            {
                procString = cti.ThreadProgress.Open("SetExtent");
                //axMapLIb1.MapExtent = layer.getBbox();
                var bbox = layer.getBbox();
                if (bbox.a.x == bbox.b.x && bbox.a.y == bbox.b.y)
                {
                    axMapLIb1.MoveTo(bbox.a);
                }

                axMapLIb1.MapExtent = AddPaddingToBbox(bbox, axMapLIb1.MapExtent);
                axMapLIb1.mapRepaint();
            }
            catch (Exception ex)
            {
                Classes.workLogFile.writeLogFile(ex, false, true);
                this.StatusInfo = ex.Message;
            }
            finally
            {
                cti.ThreadProgress.Close(procString);
            }
        }
        private void axMapLIb1_ObjectSelected(object sender, AxmvMapLib.IMapLIbEvents_ObjectSelectedEvent e)
        {
            if (e.objectNums.count == 0)
                return;

            var tInfo = new tablesInfo();

            tInfo = classesOfMetods.getTableInfo(Program.RelationVisbleBdUser.GetIdTable(e.layer.NAME));
            if (tInfo != null)
            {
                if (!string.IsNullOrWhiteSpace(tInfo.nameMap))
                {
                    var id = e.ids.getElem(e.ids.count - 1);

                    selectLayer = e.layer.NAME;
                    idT = tInfo.idTable;
                    idT_Obj = id;
                    ShowSquareArea(e.layer, id);
                }
                else
                {
                    idT = -1;
                }
            }
            ReloadSelectedList(e.layer);
        }
        private void axMapLIb1_ObjectUnselected(object sender, AxmvMapLib.IMapLIbEvents_ObjectUnselectedEvent e)
        {
            bManager.SetButtonsState();
            if (this._selectLayer != null && e.layer.NAME == this._selectLayer.NAME)
            {
                ReloadSelectedList(e.layer);
                this.StatusInfo = null;
            }
        }
        private void axMapLIb1_OnDblClick(object sender, EventArgs e)
        {
            if (axMapLIb1.CtlCursor == mvMapLib.Cursors.mlSelect)
            {
                var pressModifierKey = Control.ModifierKeys == Keys.Control || Control.ModifierKeys == Keys.Shift;
                if (!pressModifierKey && SelectedLayer != null && SelectedObjectsIds.Count == 1)
                {
                    int value = 0;
                    try
                    {
                        value = ExtraFunctions.Converts.To<int>(SelectedObjectsIds[0]);
                    }
                    catch (Exception)
                    {
                        return;
                    }
                    var idTable = Program.RelationVisbleBdUser.GetIdTable(SelectedLayer.NAME);
                    if (idTable > 0)
                    {
                        LoadFormTableData(idTable, value);
                    }
                    else
                    {
                        var table = Program.TablesManager.CosmeticRepository.FindTableByName(SelectedLayer.NAME);
                        if (table != null)
                        {
                            try
                            {
                                table.Source.OpenObject(table, value);
                            }
                            catch (Exception ex)
                            {
                                Classes.workLogFile.writeLogFile(ex, true, true);
                            }
                        }
                    }
                }
            }
        }
        private void axMapLIb1_ObjectAfterCreate(object sender, AxmvMapLib.IMapLIbEvents_ObjectAfterCreateEvent e)
        {
            if (_newHollowAdding)
            {
                if (e.layer.NAME == "TempLayerToCreateHollow")
                {
                    Program.WinMain.TbtbClippingPolygonBySpecifyingPoints.IsChecked = false;
                    _newHollowAdding = false;

                    try
                    {
                        var cuttingGeom = Geometry.CreateFromWkt(e.obj.getWKT());
                        var programSrid = ExtraFunctions.Converts.To<int>(Program.srid);
                        cuttingGeom.SetSrid(programSrid);

                        GeometryOperationWithPolygon(
                            new List<Geometry> { cuttingGeom },
                            delegate(Geometry main, List<Geometry> tools)
                            {
                                foreach (var tool in tools)
                                    main = main.Difference(tool);
                                return main;
                            });
                    }
                    catch (Exception exc)
                    {
                        MessageBox.Show(Properties.Resources.mainFrm_errErrorInGeometryEditing + ": " + exc.Message);
                    }

                    axMapLIb1.SetCursor(mvMapLib.Cursors.mlSelect);

                    if (axMapLIb1.getLayer("TempLayerToCreateHollow") != null)
                        axMapLIb1.getLayer("TempLayerToCreateHollow").deleteLayer();

                    bManager.SetButtonsState();

                    return;
                }
                else
                {
                    bManager.SetButtonsState();
                    _newHollowAdding = false;
                }
            }
            if (_newLineAdding)
            {
                if (e.layer.NAME == "TempLayerToCreateLine")
                {
                    Program.WinMain.TbtbClippingPolygonBySpecifyingLine.IsChecked = false;
                    _newLineAdding = false;

                    try
                    {
                        CutPolygonWithLine(e.obj.getWKT());
                    }
                    catch (Exception exc)
                    {
                        MessageBox.Show(Properties.Resources.mainFrm_errErrorInGeometryEditing + ": " + exc.Message);
                    }

                    this.axMapLIb1.SetCursor(mvMapLib.Cursors.mlSelect);

                    if (axMapLIb1.getLayer("TempLayerToCreateLine") != null)
                        axMapLIb1.getLayer("TempLayerToCreateLine").deleteLayer();

                    bManager.SetButtonsState();

                    var layer = axMapLIb1.getLayer(selectLayer);
                    if (layer != null)
                    {
                        layer.editable = true;
                    }
                    return;
                }
                else
                {
                    bManager.SetButtonsState();
                    _newLineAdding = false;
                }
            }
            if (true)
            {
                tablesInfo tInfo = classesOfMetods.getTableInfo(Program.RelationVisbleBdUser.GetIdTable(e.layer.NAME));
                if (tInfo == null)
                {
                    var cosmLayer = Program.TablesManager.CosmeticRepository.FindTableByName(e.layer.NAME) as DataAccess.SourceCosmetic.Model.CosmeticTableBaseM;
                    if (cosmLayer != null)
                    {
                        var tableObj = cosmLayer.CreateObject(null, e.obj.getWKT(), e.obj);
                        if (tableObj != null)
                        {
                            var layer = axMapLIb1.getLayer(cosmLayer.NameMap);
                            if (layer != null)
                            {
                                var layerObject = e.obj;
                                if (layerObject != null)
                                {
                                    switch (layerObject.VectorType)
                                    {
                                        case mvVecTypes.mvVecLine:
                                            if (cosmLayer.DefaultLineStyle != null)
                                                layerObject.style = cosmLayer.DefaultLineStyle.Value;
                                            break;
                                        case mvVecTypes.mvVecPoint:
                                            if (cosmLayer.DefaultDotStyle != null)
                                                layerObject.style = cosmLayer.DefaultDotStyle.Value;
                                            break;
                                        case mvVecTypes.mvVecRegion:
                                            if (cosmLayer.DefaultPolygonStyle != null)
                                                layerObject.style = cosmLayer.DefaultPolygonStyle.Value;
                                            break;
                                    }
                                }
                            }
                            if (Program.SettingsXML.LocalParameters.OpenAttrsAfterCreate)
                            {
                                try
                                {
                                    cosmLayer.Source.OpenObject(cosmLayer, tableObj.Id);
                                }
                                catch (Exception ex)
                                {
                                    Classes.workLogFile.writeLogFile(ex, true, true);
                                }
                            }
                        }
                    }
                }
                else
                {
                    int id_new = 0;
                    try
                    {
                        id_new = Convert.ToInt32(e.obj.fieldValue("id").ToString());
                        if (id_new < 0)
                        {
                            Rekod.Classes.workLogFile.writeLogFile(Rekod.Properties.Resources.mainFrm_errObjID, true, true);
                            if (axMapLIb1.getLayer(e.layer.NAME) != null)
                            {
                                if (axMapLIb1.getLayer(e.layer.NAME).External == true)
                                {
                                    axMapLIb1.getLayer(e.layer.NAME).ExternalFullReloadVisible();
                                    axMapLIb1.mapRepaint();
                                }
                            }
                            return;
                        }
                        this.StatusInfo = Rekod.Properties.Resources.mainFrm_IDrecieved + "(id=" + e.obj.fieldValue("id").ToString() + ")";
                    }
                    catch
                    {
                        Rekod.Classes.workLogFile.writeLogFile(Rekod.Properties.Resources.mainFrm_errObjID, true, true);
                        return;
                    }

                    if (Program.SettingsXML.LocalParameters.OpenAttrsAfterCreate)
                    {
                        Program.work.OpenForm.ShowAttributeObject(Program.app.getTableInfo(tInfo.idTable), id_new, false, this.ParentForm,
                                (DialogResult s) =>
                                {
                                    mvLayer mvltemp = axMapLIb1.getLayer(e.layer.NAME);
                                    if (mvltemp != null && mvltemp.External == true)
                                    {
                                        mvltemp.ExternalFullReloadVisible();
                                        axMapLIb1.mapRepaint();
                                        bManager.SetButtonsState();
                                    }
                                });
                    }

                    axMapLIb1.mapRepaint();
                }
            }
        }
        private void axMapLIb1_MouseDownEvent_1(object sender, AxmvMapLib.IMapLIbEvents_MouseDownEvent e)
        {
            if (e.button == TxMouseButton.mvMouseRight)
            {
                TbtbHand_Checked(sender, null);
            }
        }
        private void axMapLIb1_ObjectEdited(object sender, AxmvMapLib.IMapLIbEvents_ObjectEditedEvent e)
        {
            int id_table = 0;
            string NameLayer = Program.RelationVisbleBdUser.GetNameForUser(e.layer.NAME);

            tablesInfo tableInfo = classesOfMetods.getTableInfoOfNameMap(NameLayer);
            if (tableInfo != null)
            {
                id_table = tableInfo.idTable;
                if (!classesOfMetods.getWriteTable(id_table))
                {
                    if (axMapLIb1.getLayer(e.layer.NAME) != null)
                    {
                        if (axMapLIb1.getLayer(e.layer.NAME).External == true)
                        {
                            axMapLIb1.getLayer(e.layer.NAME).ExternalFullReloadVisible();
                            axMapLIb1.mapRepaint();
                            this.StatusInfo = Rekod.Properties.Resources.mainFrm_msgNoRights;
                        }
                    }
                    return;
                }
            }
        }
        private void tsLegenda_Click(object sender, EventArgs e)
        {
            axMapLIb1.showLegend();
        }
        void cancelBtn_Click(object sender, EventArgs e)
        {
            frmHollowCopyObj.Close();
        }
        #region Операции с геометриями
        private void selectSetBtn_Click(object sender, EventArgs e)
        {
            OnSelectObjects(Program.WinMain.TbtbClippingPolygonAnotherPolygon,
                delegate(Geometry main, List<Geometry> tools)
                {
                    foreach (var tool in tools)
                    {
                        main = main.Difference(tool);
                    }
                    Debug.WriteLine(main.GetWkt());
                    if (main.GetPointCount() == 0 && main.GetGeometryCount() == 0)
                    {
                        throw new Exception("Пустая геометрии");
                    }
                    else
                    {
                        return main;
                    }
                });
        }
        private void selectObjectsForIntersectionBtn_Click(object sender, EventArgs e)
        {
            OnSelectObjects(Program.WinMain.TbtbIntersectGeometry,
                delegate(Geometry main, List<Geometry> tools)
                {
                    foreach (var tool in tools)
                    {
                        main = main.Intersection(tool);
                    }
                    Debug.WriteLine(main.GetWkt());
                    if (main.GetPointCount() == 0 && main.GetGeometryCount() == 0)
                    {
                        throw new Exception("Пустая геометрии");
                    }
                    else
                    {
                        return main;
                    }
                });
        }
        private void selectObjectsForJoinIntersectionBtn_Click(object sender, EventArgs e)
        {
            OnSelectObjects(Program.WinMain.TbtbJoinIntersectGeometry,
                delegate(Geometry main, List<Geometry> tools)
                {
                    Geometry result = new Geometry(main.GetGeometryType());
                    foreach (var tool in tools)
                    {
                        Geometry geom = main.Intersection(tool);
                        if (main.NotStrictEqualityGeomType(geom))
                        {
                            result = result.Union(geom);
                        }
                    }
                    if (main.GetPointCount() == 0 && result.GetGeometryCount() == 0)
                    {
                        throw new Exception("Пустая геометрии");
                    }
                    else
                    {
                        return result;
                    }
                });
        }
        private void selectObjectsForSymDifferenceBtn_Click(object sender, EventArgs e)
        {
            OnSelectObjects(Program.WinMain.TbtbSymDifference,
                delegate(Geometry main, List<Geometry> tools)
                {
                    Geometry unionTool = new Geometry(main.GetGeometryType());
                    unionTool = TransformInMultiGeom(unionTool);
                    foreach (var tool in tools)
                    {
                        if (main.NotStrictEqualityGeomType(tool))
                        {
                            unionTool = unionTool.Union(tool);
                        }
                        
                    }
                    if (unionTool.GetPointCount() == 0 && unionTool.GetGeometryCount() == 0)
                    {
                        throw new Exception("Пустая геометрии");
                    }
                    else
                    {
                        return main.SymDifference(unionTool);;
                    }
                });
        }
        #endregion

        private void OnSelectObjects(System.Windows.Controls.Primitives.ToggleButton button, Func<Geometry, List<Geometry>, Geometry> geometryFunc)
        {
            if (button.IsChecked == true)
            {
                var programSrid = ExtraFunctions.Converts.To<int>(Program.srid);
                var list = SelectedGeometryList(programSrid);
                GeometryOperationWithPolygon(list, geometryFunc);

                tableId_Copy = -1;
                idT_Copy = -1;

                CopyTable = null;
                CopyObject = null;
            }
            frmHollowCopyObj.Close();
        }
        #region Ручной поворот выделенного объекта
        void RotateLeft_Click(object sender, EventArgs e)
        {
            axMapLIb1.RotateSelected(Convert.ToDouble(((ToolsFrm.RotateFrm)((Button)sender).Parent).numericUpDown1.Value));
        }
        void RotateRight_Click(object sender, EventArgs e)
        {
            axMapLIb1.RotateSelected((-1) * Convert.ToDouble(((ToolsFrm.RotateFrm)((Button)sender).Parent).numericUpDown1.Value));
        }
        #endregion
        private void axMapLIb1_OnKeyPress(object sender, AxmvMapLib.IMapLIbEvents_OnKeyPressEvent e)
        {
            if (e.key == (short)Keys.Delete)
            {
                bManager.DeleteGeometry(null);
            }
        }
        [DebuggerStepThrough]
        private void axMapLIb1_MouseMoveEvent(object sender, AxmvMapLib.IMapLIbEvents_MouseMoveEvent e)
        {
            var p = new mvPointWindow();
            p.x = e.winx;
            p.y = e.winy;
            var pw = axMapLIb1.win2Global(p);
            if (projTo != projMap)
            {
                System.Windows.Point[] point = new System.Windows.Point[1];
                point[0].X = pw.x;
                point[0].Y = pw.y;
                try
                {
                    point = OGRFramework.TransformGeometry.transform(point, "Point", projTo, projMap);
                }
                catch
                {
                    MessageBox.Show(Rekod.Properties.Resources.mainFrm_errProjUs, Rekod.Properties.Resources.mainFrm_errProj, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    foreach (var item in Program.WinMain.TbcbSrid.Items)
                    {
                        if (item.ToString().Contains(axMapLIb1.SRID))
                        {
                            Program.WinMain.TbcbSrid.SelectedItem = item;
                            break;
                        }
                    }
                }
                Program.WinMain.TbtbCoords.Text = "X: " + WriteDouble(point[0].X) + "  Y: " + WriteDouble(point[0].Y);
            }
            else
            {
                Program.WinMain.TbtbCoords.Text = "X: " + WriteDouble(pw.x) + "  Y: " + WriteDouble(pw.y);
            }
        }
        void bManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {

        }
        /// <summary>
        /// Обработка события изменения свойств элемента массива ListToolBars в bManager
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void bManager_ListToolBars_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var toolBar = sender as ToolBar_VM;
            if (toolBar == null)
                return;
            if (toolBar.Name == Program.WinMain.TbMaps.Name)
            {
                if (e.PropertyName == "IsVisable")
                {
                    Program.WinMain.TbMaps.Visibility = toolBar.IsVisable ? sw.Visibility.Visible : sw.Visibility.Collapsed;
                    Program.WinMain.MiTbMap.IsChecked = toolBar.IsVisable;
                }
            }
            else if (toolBar.Name == Program.WinMain.TbSelectionOf.Name)
            {
                if (e.PropertyName == "IsVisable")
                {
                    Program.WinMain.TbSelectionOf.Visibility = toolBar.IsVisable ? sw.Visibility.Visible : sw.Visibility.Collapsed;
                    Program.WinMain.MiTbSelectObject.IsChecked = toolBar.IsVisable;
                }
            }
            else if (toolBar.Name == Program.WinMain.TbOperationsWithObjects.Name)
            {
                if (e.PropertyName == "IsVisable")
                {
                    Program.WinMain.TbOperationsWithObjects.Visibility = toolBar.IsVisable ? sw.Visibility.Visible : sw.Visibility.Collapsed;
                    Program.WinMain.MiTbEditObject.IsChecked = toolBar.IsVisable;
                }
            }
            else if (toolBar.Name == Program.WinMain.TbOperationsWithGeometry.Name)
            {
                if (e.PropertyName == "IsVisable")
                {
                    Program.WinMain.TbOperationsWithGeometry.Visibility = toolBar.IsVisable ? sw.Visibility.Visible : sw.Visibility.Collapsed;
                    Program.WinMain.MiTbEditGeom.IsChecked = toolBar.IsVisable;
                }
            }
            else if (toolBar.Name == Program.WinMain.TbModeEnabled.Name)
            {
                if (e.PropertyName == "IsVisable")
                {
                    Program.WinMain.TbModeEnabled.Visibility = toolBar.IsVisable ? sw.Visibility.Visible : sw.Visibility.Collapsed;
                    Program.WinMain.MiTbEnabledModes.IsChecked = toolBar.IsVisable;
                }
            }
            else if (toolBar.Name == Program.WinMain.TbEditableLayer.Name)
            {
                if (e.PropertyName == "IsVisable")
                {
                    Program.WinMain.TbEditableLayer.Visibility = toolBar.IsVisable ? sw.Visibility.Visible : sw.Visibility.Collapsed;
                    Program.WinMain.MiTbEditableLayer.IsChecked = toolBar.IsVisable;
                }
            }
            else if (toolBar.Name == Program.WinMain.TbWorkSets.Name)
            {
                if (e.PropertyName == "IsVisable")
                {
                    Program.WinMain.TbWorkSets.Visibility = toolBar.IsVisable ? sw.Visibility.Visible : sw.Visibility.Collapsed;
                    Program.WinMain.MiTbWorkSets.IsChecked = toolBar.IsVisable;
                }
            }
            else if (toolBar.Name == Program.WinMain.TbFullTextSearch.Name)
            {
                if (e.PropertyName == "IsVisable")
                {
                    Program.WinMain.TbFullTextSearch.Visibility = toolBar.IsVisable ? sw.Visibility.Visible : sw.Visibility.Collapsed;
                    Program.WinMain.MiTbSearch.IsChecked = toolBar.IsVisable;
                }
            }
            //else if (toolBar.Name == Program.WinMain.TbInfoText.Name)
            //{
            //    if (e.PropertyName == "IsVisable")
            //    {
            //        Program.WinMain.TbInfoText.Visibility = toolBar.IsVisable ? sw.Visibility.Visible : sw.Visibility.Collapsed;
            //        Program.WinMain.MiTbInfo.IsChecked = toolBar.IsVisable;
            //    }
            //}
            //else if (toolBar.Name == Program.WinMain.TbScale.Name)
            //{
            //    if (e.PropertyName == "IsVisable")
            //    {
            //        Program.WinMain.TbScale.Visibility = toolBar.IsVisable ? sw.Visibility.Visible : sw.Visibility.Collapsed;
            //        Program.WinMain.MiTbScale.IsChecked = toolBar.IsVisable;
            //    }
            //}
            else if (toolBar.Name == layerItemsView1.Name)
            {
                if (e.PropertyName == "IsVisable")
                {
                    layerItemsView1.Visible = toolBar.IsVisable;
                    Program.WinMain.MiTbLayersList.IsChecked = toolBar.IsVisable;
                    Program.WinMain.TbtbLayerList.IsChecked = toolBar.IsVisable;
                }
            }
            else if (toolBar.Name == Program.WinMain.TbPlugins.Name)
            {
                if (e.PropertyName == "IsVisable")
                {
                    if (Program.WinMain.TbPlugins.Items.Count > 0)
                    {
                        Program.WinMain.TbPlugins.Visibility = toolBar.IsVisable ? sw.Visibility.Visible : sw.Visibility.Collapsed;
                        Program.WinMain.MiTbPlugins.IsChecked = toolBar.IsVisable;
                        Program.WinMain.MiTbPlugins.IsEnabled = true;
                    }
                    else
                    {
                        toolBar.IsVisable = false;
                        Program.WinMain.MiTbPlugins.IsEnabled = false;
                        Program.WinMain.MiTbPlugins.IsChecked = false;
                        Program.WinMain.TbPlugins.Visibility = sw.Visibility.Collapsed;
                    }
                }
            }
            else if (toolBar.Name == Program.WinMain.TbFastStart.Name)
            {
                if (e.PropertyName == "IsVisable")
                {
                    if (Program.WinMain.TbFastStart.Items.Count > 0)
                    {
                        Program.WinMain.TbFastStart.Visibility = toolBar.IsVisable ? sw.Visibility.Visible : sw.Visibility.Collapsed;
                        Program.WinMain.MiTbFastStart.IsChecked = toolBar.IsVisable;
                    }
                    else
                    {
                        toolBar.IsVisable = false;
                        Program.WinMain.TbFastStart.Visibility = toolBar.IsVisable ? sw.Visibility.Visible : sw.Visibility.Collapsed;
                        Program.WinMain.MiTbFastStart.IsChecked = toolBar.IsVisable;
                    }
                }
            }
            //else if (toolBar.Name == Program.WinMain.TbCoordinates.Name)
            //{
            //    if (e.PropertyName == "IsVisable")
            //    {
            //        Program.WinMain.TbCoordinates.Visibility = toolBar.IsVisable ? sw.Visibility.Visible : sw.Visibility.Collapsed;
            //        Program.WinMain.MiTbCoords.IsChecked = toolBar.IsVisable;
            //    }
            //}
        }
        private void ListButton_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // todo: надо переделать под универсальный класс (tspSpeedStart - не вариант)
            if (e.NewItems != null)
            {
                foreach (ToolButton_VM item in e.NewItems)
                {
                    sw.Controls.Button swButton = new sw.Controls.Button();
                    sw.Controls.Image swImage = new sw.Controls.Image();
                    swButton.Content = swImage;
                    swButton.Click += SwFastStartItem_Click;
                    swButton.Tag = item;
                    Program.WinMain.TbFastStart.Items.Add(swButton);
                    MemoryStream ms = new MemoryStream();  // no using here! BitmapImage will dispose the stream after loading
                    item.Image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    BitmapImage ix = new BitmapImage();
                    ix.BeginInit();
                    ix.CacheOption = BitmapCacheOption.OnLoad;
                    ix.StreamSource = ms;
                    ix.EndInit();
                    swImage.Source = ix;
                    swButton.ToolTip = item.Name;
                }
                tsp_FastStart.IsVisable = true;
                Program.WinMain.MiTbFastStart.IsEnabled = true;
            }
            if (e.OldItems != null)
            {
                foreach (ToolButton_VM item in e.OldItems)
                {
                    foreach (sw.Controls.Button buttonItem in Program.WinMain.TbFastStart.Items)
                    {
                        if (item.Name == buttonItem.Name)
                        {
                            buttonItem.Click -= SwFastStartItem_Click;
                            Program.WinMain.TbFastStart.Items.Remove(buttonItem);
                            break;
                        }
                    }
                }
                tsp_FastStart.IsVisable = false;
            }
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                Program.WinMain.TbFastStart.Items.Clear();
                Program.WinMain.MiTbFastStart.IsEnabled = false;
                tsp_FastStart.IsVisable = false;
            }
        }
        void SwFastStartItem_Click(object sender, sw.RoutedEventArgs e)
        {
            var button = sender as sw.Controls.Button;
            if (button == null)
                return;
            var tb = button.Tag as ToolButton_VM;
            tb.ClickCommand.Execute(null);
        }
        private void selectSetBtnJoin_Click(object sender, EventArgs e)
        {
            if (Program.WinMain.TbtbJoinGeometry.IsChecked == true)
            {
                if (CopyTable == null || _selectObjectsIds.Count < 1)
                    return;

                var programSrid = ExtraFunctions.Converts.To<int>(Program.srid);

                Geometry geomMain;


                var layerM = CopyTable as AbsM.ILayerM;
                var tableInfo = CopyTable as Interfaces.tablesInfo;
                if (tableInfo == null && layerM != null && layerM is PgM.PgTableBaseM)
                    tableInfo = Program.app.getTableInfo((int)layerM.Id);

                try
                {
                    if (tableInfo != null)
                    {
                        var wkt = GetWktObj(tableInfo, (int)CopyObject);

                        geomMain = Geometry.CreateFromWkt(wkt);
                        geomMain.SetSrid((int)tableInfo.srid);
                        geomMain.Transform(programSrid);
                    }
                    else if (layerM != null)
                    {
                        var layerML = axMapLIb1.getLayer(layerM.NameMap);
                        if (layerM.IsVisible && layerML != null)
                        {
                            var obj = layerML.getObject((int)CopyObject);
                            if (obj != null)
                            {
                                var wkt = obj.getWKT();

                                geomMain = Geometry.CreateFromWkt(wkt);
                                geomMain.SetSrid(programSrid);
                            }
                            else
                                throw new Exception(Properties.Resources.mainFrm_errNotFoundWorkObject);
                        }
                        else
                        {
                            layerM.IsVisible = false;
                            throw new Exception(Properties.Resources.mainFrm_errWorkLayerNotEnabled);
                        }
                    }
                    else
                    {
                        throw new Exception(Properties.Resources.mainFrm_errIncorrectLayerType);
                    }

                    bool isMultiGeom;
                    Geometry multiGeom;

                    if (layerM != null && layerM.GeomType == AbsM.EGeomType.Any)
                    {
                        isMultiGeom = true;
                    }
                    else
                    {
                        var geomMainType = geomMain.GetGeometryType();
                        switch (geomMainType)
                        {
                            case wkbGeometryType.wkbPoint:
                            case wkbGeometryType.wkbLineString:
                            case wkbGeometryType.wkbPolygon:
                                {
                                    isMultiGeom = false;
                                }
                                break;

                            case wkbGeometryType.wkbMultiPoint:
                            case wkbGeometryType.wkbMultiLineString:
                            case wkbGeometryType.wkbMultiPolygon:
                                {
                                    isMultiGeom = true;
                                }
                                break;

                            default:
                                {
                                    return;
                                }
                        }
                    }
                    multiGeom = TransformInMultiGeom(geomMain);
                    var multiGeomType = multiGeom.GetGeometryType();

                    var listGeom = SelectedGeometryList(programSrid, multiGeomType);

                    var wktUnion = multiGeom.GetWkt();
                    var geomUnion = multiGeom.Union(listGeom);
                    geomUnion.SetSrid(programSrid);

                    if (tableInfo != null)
                    {
                        var count = geomUnion.GetGeometryCount();
                        if (!isMultiGeom && count > 1)
                        {
                            //var strObjClipping = string.Join(", ", listClippingGeom);
                            //string s = "Объедененный объект содержит несколько геометрий, но такой объект в данном слое сохранить нельзя./nСохранить только первую геометрию в объекте?";
                            var res = MessageBox.Show(
                                Properties.Resources.mainFrm_errSaveWhenGeometryIsNotMulti,
                                Properties.Resources.mainFrm_errErrorInGeometryJoining,
                                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                            if (res != DialogResult.Yes)
                            {
                                return;
                            }
                            geomUnion = geomUnion.GetGeometryRef(0);
                        }
                        if (!isMultiGeom)
                        {
                            geomUnion = geomUnion.Simplify(0);
                        }

                        var layerName = Program.RelationVisbleBdUser.GetNameInBd(tableInfo.idTable);
                        mvMapLib.mvLayer tempLayer = Program.mainFrm1.axMapLIb1.getLayer(layerName);
                        if (tempLayer != null)
                            tempLayer.RemoveObjects();

                        Action<DialogResult> actionCloseAttrForm = (DialogResult s) => classesOfMetods.reloadAllLayerData();
                        Program.work.OpenForm.ShowAttributeObject(tableInfo, (int)CopyObject, false, this.ParentForm, geomUnion.GetWkt(),
                                actionCloseAttrForm);

                        // Обновление измененного слоя
                        if (tempLayer != null)
                        {
                            if (tempLayer.External)
                            {
                                tempLayer.ExternalFullReloadVisible();
                            }
                        }

                        bManager.SetButtonsState();
                    }
                    else if (layerM != null)
                    {
                        layerM.Source.OpenObject(layerM, CopyObject, geomUnion.GetWkt());
                        bManager.SetButtonsState();
                    }
                    //JoinGeometryPg(trans_wkt, _selectObjects, tableInfo, (int)CopyObject);
                    //JoinGeometryAbsM(trans_wkt, _selectObjects, layerM, (int)CopyObject);


                    tableId_Copy = -1;
                    idT_Copy = -1;

                    CopyTable = null;
                    CopyObject = null;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, Properties.Resources.mainFrm_errErrorInGeometryJoining, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            frmHollowCopyObj.Close();
        }

        private List<Geometry> SelectedGeometryList(int programSrid, wkbGeometryType multiGeomType)
        {
            var listGeom = new List<Geometry>();
            foreach (var item in _selectObjectsIds)
            {
                var wkt = SelectedLayer.getObject(item).getWKT();
                var itemGeom = Geometry.CreateFromWkt(wkt);
                itemGeom.SetSrid(programSrid);

                //var valid = itemGeom.IsValid();
                var valid = true;

                itemGeom = TransformInMultiGeom(itemGeom);
                var wktItem = itemGeom.GetWkt();
                var itemMultiGeomType = itemGeom.GetGeometryType();
                if (valid && multiGeomType == itemMultiGeomType)
                {
                    listGeom.Add(itemGeom);
                }
            }
            return listGeom;
        }

        private List<Geometry> SelectedGeometryList(int programSrid)
        {
            var listGeom = new List<Geometry>();
            foreach (var item in _selectObjectsIds)
            {
                var wkt = SelectedLayer.getObject(item).getWKT();
                var itemGeom = Geometry.CreateFromWkt(wkt);
                itemGeom.SetSrid(programSrid);
                listGeom.Add(itemGeom);
            }
            return listGeom;
        }

        private static Geometry TransformInMultiGeom(Geometry geomMain)
        {
            Geometry multiGeom;
            var geomMainType = geomMain.GetGeometryType();
            switch (geomMainType)
            {
                case wkbGeometryType.wkbPoint:
                    {
                        var geom = new Geometry(wkbGeometryType.wkbMultiPoint);
                        geom.AddGeometry(geomMain);
                        multiGeom = geom;
                    }
                    break;
                case wkbGeometryType.wkbLineString:
                    {
                        var geom = new Geometry(wkbGeometryType.wkbMultiLineString);
                        geom.AddGeometry(geomMain);
                        multiGeom = geom;
                    }
                    break;
                case wkbGeometryType.wkbPolygon:
                    {
                        var geom = new Geometry(wkbGeometryType.wkbMultiPolygon);
                        geom.AddGeometry(geomMain);
                        multiGeom = geom;
                    }
                    break;
                default:
                    {
                        multiGeom = geomMain.Clone();
                    }
                    break;
            }
            multiGeom.AssignSpatialReference(geomMain.GetSpatialReference());
            return multiGeom;
        }

        public void axMapLIb1_OnExtentChanged(object sender, AxmvMapLib.IMapLIbEvents_OnExtentChangedEvent e)
        {
            if (!this._userScaleChange)
            {
                Program.WinMain.TbtbScale.Text = ((Int64)axMapLIb1.ScaleZoom).ToString();
            }
            this._userScaleChange = false;
        }

        #region Обработчики тулбоксов из WinMain
        void TbcbWorkSet_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            int idSet = 0;
            if (e.AddedItems != null && e.AddedItems.Count != 0)
            {
                idSet = (int)(e.AddedItems[0] as Rekod.UserSets.WorkSetItem_S).Id;
                var workSet = Program.WorkSets.FindSet(idSet);
                Program.WorkSets.SwitchSet(workSet);
            }
        }
        public void TbtbScale_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            System.Windows.Controls.TextBox scaleBox = sender as System.Windows.Controls.TextBox;
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                try
                {
                    this._userScaleChange = true;
                    double x = 0, y = 0;
                    axMapLIb1.getScrCenter(out x, out y);
                    axMapLIb1.ScaleZoom = Int64.Parse(scaleBox.Text);
                    axMapLIb1.setScrCenter(x, y);
                    axMapLIb1.mapRepaint();
                }
                catch
                {
                    scaleBox.Text = ((Int64)axMapLIb1.ScaleZoom).ToString();
                }
                scaleBox.Select(scaleBox.Text.Length, 0);
            }
        }
        public void TbbApplySRID_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            String newSrid = Program.WinMain.TbtbCustomSRID.Text;
            string temp;
            if (newSrid != "")
            {
                try
                {
                    int srid = int.Parse(newSrid);
                    temp = TransformGeometry.GetProjWKT(srid);
                    if (temp == null)
                        MessageBox.Show(Rekod.Properties.Resources.mainFrm_errWSRID, Rekod.Properties.Resources.mainFrm_errProj, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    else
                    {
                        if (temp == "")
                            MessageBox.Show(Rekod.Properties.Resources.mainFrm_errSRIDNS, Rekod.Properties.Resources.mainFrm_errProj, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        else
                        {
                            projTo = temp;
                            PrjSrid = srid;
                        }
                    }
                }
                catch
                {
                    MessageBox.Show(Rekod.Properties.Resources.mainFrm_errSRIDINT, Rekod.Properties.Resources.mainFrm_errProj, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
        }
        public void TbcbSrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            System.Windows.Controls.ComboBox sridBox = sender as System.Windows.Controls.ComboBox;
            if (sridBox.Items.Count == 3)
            {
                if (sridBox.SelectedIndex == 2)
                {
                    Program.WinMain.TbbApplySRID.Visibility = Program.WinMain.TbtbCustomSRID.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    Program.WinMain.TbbApplySRID.Visibility = Program.WinMain.TbtbCustomSRID.Visibility = System.Windows.Visibility.Collapsed;
                }
            }
            else
            {
                if (sridBox.SelectedIndex == 3)
                {
                    Program.WinMain.TbbApplySRID.Visibility = Program.WinMain.TbtbCustomSRID.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    Program.WinMain.TbbApplySRID.Visibility = Program.WinMain.TbtbCustomSRID.Visibility = System.Windows.Visibility.Collapsed;
                }
            }
            if (sridBox.SelectedIndex >= 0)
            {
                if ((sridBox.Items.Count == 3 && sridBox.SelectedIndex < 2) || (sridBox.Items.Count == 4 && sridBox.SelectedIndex < 3))
                {
                    PrjSrid = int.Parse(sridBox.SelectedItem.ToString().Split(new char[] { '(', ')' })[1]);
                    Program.mainFrm1.projTo = TransformGeometry.GetProjWKT(PrjSrid);
                }
            }
        }
        public void TbbFullSearch_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SearchFullText(Program.WinMain.TbtbFullSearch.Text);
        }
        public void TbtbFullSearch_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == sw.Input.Key.Enter)
            {
                System.Windows.Controls.TextBox searchBox = sender as System.Windows.Controls.TextBox;
                SearchFullText(searchBox.Text);
            }
        }
        public void TbcbEditableLayer_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            AbsM.ILayerM selectedLayer = null;
            if (Program.WinMain.TbcbEditableLayer.SelectedItem != null)
            {
                selectedLayer = Program.WinMain.TbcbEditableLayer.SelectedItem as AbsM.ILayerM;
            }
            layerItemsView1.SetLayerIsEditable(selectedLayer);
            layerItemsView1.RefreshLayers();
            bManager.SetButtonsState();
        }
        public void TbbSnapToLine_CheckedChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            System.Windows.Controls.Primitives.ToggleButton snapToLineButton =
                sender as System.Windows.Controls.Primitives.ToggleButton;
            axMapLIb1.SnapLine = (bool)snapToLineButton.IsChecked;
            Program.WinMain.TbbSnapToLine.IsChecked = axMapLIb1.SnapLine;
            Program.WinMain.TbbSnapToPoint.IsChecked = axMapLIb1.Snap;
        }
        public void TbbSnapToPoint_CheckedChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            System.Windows.Controls.Primitives.ToggleButton snapToLineButton =
               sender as System.Windows.Controls.Primitives.ToggleButton;
            axMapLIb1.Snap = (bool)snapToLineButton.IsChecked;
            Program.WinMain.TbbSnapToLine.IsChecked = axMapLIb1.SnapLine;
            Program.WinMain.TbbSnapToPoint.IsChecked = axMapLIb1.Snap;
        }
        public void TbtbSelectingObject_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            axMapLIb1.SetCursor(mvMapLib.Cursors.mlSelect);
        }
        public void TbtbSelectingObjectsRectangle_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            axMapLIb1.SetCursor(mvMapLib.Cursors.mlSelectRegion);
        }
        public void TbtbSelectingObjectPolygon_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            axMapLIb1.SetCursor(mvMapLib.Cursors.mlSelectPoly);
        }
        public void TbbUnselectingObjects_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            for (int i = 0; i < Program.mainFrm1.axMapLIb1.LayersCount; i++)
            {
                mvMapLib.mvLayer layer = Program.mainFrm1.axMapLIb1.getLayerByNum(i);
                if (layer.SelectedCount > 0)
                {
                    layer.DeselectAll();
                }
            }
            Program.mainFrm1.axMapLIb1.mapRepaint();
            bManager.SetButtonsState();
        }
        public void TbtbObjectAddWithMap_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            layerItemsView1.addToolStripMenuItem_Click(sender, e);
        }
        public void TbbObjectAddWithWKT_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            layerItemsView1.addCoordToolStripMenuItem_Click(sender, e);
        }
        public void TbtbObjectAddRectange_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            layerItemsView1.addToolStripMenuItemRectangle_Click(sender, e);
        }
        public void TbbLayerObjectDelete_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            layerItemsView1.DeleteObjectFromLayer();
            bManager.SetButtonsState();
        }
        public void TbtbHand_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            axMapLIb1.SetCursor(mvMapLib.Cursors.mlPan);
            Program.WinMain.TbtbClippingPolygonAnotherPolygon.IsChecked = false;
            if (Program.WinMain.TbtbClippingPolygonBySpecifyingPoints.IsChecked == true)
            {
                if (axMapLIb1.getLayer("TempLayerToCreateHollow") != null)
                {
                    axMapLIb1.getLayer("TempLayerToCreateHollow").deleteLayer();
                }
            }
            if (Program.WinMain.TbtbClippingPolygonBySpecifyingLine.IsChecked == true)
            {
                if (axMapLIb1.getLayer("TempLayerToCreateLine") != null)
                {
                    axMapLIb1.getLayer("TempLayerToCreateLine").deleteLayer();
                }
            }
            Program.WinMain.TbtbClippingPolygonBySpecifyingPoints.IsChecked = false;
            Program.WinMain.TbtbClippingPolygonBySpecifyingLine.IsChecked = false;
            if (layerItemsView1._clickedNode != null)
            {
                if (layerItemsView1._clickedNode.Checked && layerItemsView1._clickedNode.Tag is tablesInfo)
                {
                    string lname = Program.RelationVisbleBdUser.GetNameInBd(((tablesInfo)layerItemsView1._clickedNode.Tag).idTable);
                    if (axMapLIb1.getLayer(lname) != null)
                    {
                        axMapLIb1.getLayer(lname).DeselectAll();
                        axMapLIb1.getLayer(lname).editable = true;
                    }
                }
            }
        }
        public void TbtbRuler_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            this.StatusInfo = Rekod.Properties.Resources.mainFrm_sqMeasure;
            axMapLIb1.SetCursor(mvMapLib.Cursors.mlDistance);
        }
        public void TbbZoomIn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            axMapLIb1.setScale(axMapLIb1.ScaleZoom / 1.5, true);
            axMapLIb1.mapRepaint();
        }
        public void TbbZoomOut_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            axMapLIb1.setScale(axMapLIb1.ScaleZoom * 1.5, true);
            axMapLIb1.mapRepaint();
        }
        public void TbtbZoomInRegion_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            axMapLIb1.SetCursor(mvMapLib.Cursors.mlZoomIn);
        }
        public void TbtbZoomOutRegion_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            axMapLIb1.SetCursor(mvMapLib.Cursors.mlZoomOut);
        }
        public void TbbZoomToObj_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (layerItemsView1._clickedNode != null && !(layerItemsView1._clickedNode.Tag is RastrInfo))
            {
                if (layerItemsView1._clickedNode.Tag is tablesInfo)
                {
                    tablesInfo ti = ((tablesInfo)layerItemsView1._clickedNode.Tag);
                    // есть этот слой среди включенных
                    var layer = Program.mainFrm1.axMapLIb1.getLayer(Program.RelationVisbleBdUser.GetNameInBd(ti.idTable));
                    MoveToSelectedObjects(layer);
                }
                else if (layerItemsView1._clickedNode.Tag is AbsM.ILayerM)
                {
                    var table = layerItemsView1._clickedNode.Tag as AbsM.ILayerM;
                    // есть этот слой среди включенных

                    var layer = Program.mainFrm1.axMapLIb1.getLayer(table.NameMap);
                    MoveToSelectedObjects(layer);
                }
            }
        }
        public void TbbZoomToLayer_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (layerItemsView1._clickedNode != null)
            {
                if (layerItemsView1._clickedNode.Tag is tablesInfo)
                {
                    tablesInfo ti = ((tablesInfo)layerItemsView1._clickedNode.Tag);
                    // есть этот слой среди включенных
                    var layer = Program.mainFrm1.axMapLIb1.getLayer(Program.RelationVisbleBdUser.GetNameInBd(ti.idTable));
                    SetExtentFromLayer(layer);
                }
                else if (layerItemsView1._clickedNode.Tag is AbsM.ILayerM)
                {
                    var table = layerItemsView1._clickedNode.Tag as AbsM.ILayerM;
                    // есть этот слой среди включенных
                    var layer = Program.mainFrm1.axMapLIb1.getLayer(table.NameMap);
                    SetExtentFromLayer(layer);
                }
                else if (layerItemsView1._clickedNode.Tag is RastrInfo)
                {
                    RastrInfo ri = ((RastrInfo)layerItemsView1._clickedNode.Tag);
                    if (!ri.IsExternal)
                    {

                        mvImageLayer imgLayer = axMapLIb1.getImageLayer(ri.RastrPath);
                        if (imgLayer != null)
                        {
                            try
                            {
                                axMapLIb1.MapExtent = imgLayer.getBbox();
                                axMapLIb1.mapRepaint();
                            }
                            catch (Exception ex)
                            {
                                Classes.workLogFile.writeLogFile(ex, true, true);
                            }
                        }
                    }
                }
            }
        }
        public void TbtbLayerList_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (tsp_layerItemsView1_VM != null)
            {
                tsp_layerItemsView1_VM.IsVisable = !tsp_layerItemsView1_VM.IsVisable;
            }
        }
        public void TbbWorkSets_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var view = new Rekod.UserSets.WorkSets_V();
            view.Owner = Program.WinMain;
            var viewModel = new Rekod.UserSets.WorkSets_VM(Program.WorkSets);
            view.DataContext = viewModel;
            viewModel.Reload();
            view.Show();
        }
        public void TbbUpdateListLayers_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            classesOfMetods.reloadAllLayerData();
            bManager.SetButtonsState();
        }
        public void TbtbMapPointInfo_CheckedChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_mapLayersItemInfoVM == null)
            {
                _mapLayersItemInfoVM = new MapLayersItemInfo.ViewModel.MapLayersItemInfoVM();
                var mapLayersItemInfoV = new Rekod.MapLayersItemInfo.View.MapLayersItemInfoV();
                WindowViewModelBase_VM.GetWindow(mapLayersItemInfoV, _mapLayersItemInfoVM, 800, 400, 250, 350, Program.WinMain);
                _mapLayersItemInfoVM.AttachedWindow.WindowStyle = System.Windows.WindowStyle.SingleBorderWindow;
                _mapLayersItemInfoVM.AttachedWindow.WindowStartupLocation = sw.WindowStartupLocation.CenterOwner;
            }
        }
        public void TbbRotateBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (idT_Obj == -1)
            {
                this.StatusInfo = Rekod.Properties.Resources.MainFrm_SelectedObjNot;
                return;
            }
            ToolsFrm.RotateFrm frm = new ToolsFrm.RotateFrm();
            frm.button1.Click += new EventHandler(RotateLeft_Click);
            frm.button2.Click += new EventHandler(RotateRight_Click);
            frm.ShowDialog();
        }
        public void TbtbRotateMouse_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            axMapLIb1.SetCursor(mvMapLib.Cursors.mlRotateObj);
        }
        public void TbtbMoveObj_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            axMapLIb1.SetCursor(mvMapLib.Cursors.mlMoveObj);
        }
        public void TbtbVertexEdit_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            axMapLIb1.SetCursor(mvMapLib.Cursors.mlVertexEdit);
        }
        public void TbtbJoinGeometry_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            Program.WinMain.TbtbJoinGeometry.IsChecked = true;
            ShowFormForObjectSelecting(selectSetBtnJoin_Click,
                new FormClosedEventHandler((o, arg) =>
                {
                    Program.WinMain.TbtbJoinGeometry.IsChecked = false;
                    frmHollowCopyObj = null;
                }));
        }

        /// <summary>
        /// Запомнить выделенный слой и его объекты
        /// </summary>
        private void SaveSelectedTable()
        {
            idT_Copy = -1;
            tableId_Copy = -1;

            CopyObject = null;
            CopyTable = null;

            if (bManager.CurrentTable != null && SelectedObjectsIds.Count > 0)
            {
                var tableM = bManager.CurrentTable as AbsM.ILayerM;

                if (bManager.CurrentTable is Interfaces.tablesInfo)
                {
                    CopyTable = bManager.CurrentTable;
                }
                else if (tableM != null && tableM.IsVisible && tableM.IsEditable)
                {
                    CopyTable = bManager.CurrentTable;
                }
                else
                {
                    CopyTable = null;
                }

                if (CopyTable != null)
                {
                    CopyObject = ExtraFunctions.Converts.To<int>(SelectedObjectsIds[0]);
                }
            }
        }
        public void TbtbSeparatGeometry_Checked(object sender, sw.RoutedEventArgs e)
        {
            if (bManager.CurrentTable != null && SelectedObjectsIds.Count > 0)
            {
                SeparationObject(SelectedObjectsIds);
                bManager.SetButtonsState();
            }
            else
            {
                idT_Copy = -1;
                tableId_Copy = -1;
            }
            Program.WinMain.TbtbSeparatGeometry.IsChecked = false;
        }
        public void TbtbIntersectGeometry_Checked(object sender, sw.RoutedEventArgs e)
        {
            ShowFormForObjectSelecting(selectObjectsForIntersectionBtn_Click,
                new FormClosedEventHandler((o, arg) =>
                {
                    Program.WinMain.TbtbIntersectGeometry.IsChecked = false;
                    frmHollowCopyObj = null;
                }));
            Program.WinMain.TbtbIntersectGeometry.IsChecked = true;
        }
        public void TbtbJoinIntersectGeometry_Checked(object sender, sw.RoutedEventArgs e)
        {
            ShowFormForObjectSelecting(selectObjectsForJoinIntersectionBtn_Click,
                new FormClosedEventHandler((o, arg) =>
                {
                    Program.WinMain.TbtbJoinIntersectGeometry.IsChecked = false;
                    frmHollowCopyObj = null;
                }));
            Program.WinMain.TbtbJoinIntersectGeometry.IsChecked = true;
        }
        public void TbtbSymDifference_Checked(object sender, sw.RoutedEventArgs e)
        {
            Program.WinMain.TbtbSymDifference.IsChecked = true;
            ShowFormForObjectSelecting(selectObjectsForSymDifferenceBtn_Click,
                new FormClosedEventHandler((o, arg) =>
                {
                    Program.WinMain.TbtbSymDifference.IsChecked = false;
                    frmHollowCopyObj = null;
                }));
        }
        public void TbtbClippingPolygonAnotherPolygon_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            ShowFormForObjectSelecting(selectSetBtn_Click,
                new FormClosedEventHandler((o, arg) =>
                {
                    Program.WinMain.TbtbClippingPolygonAnotherPolygon.IsChecked = false;
                    frmHollowCopyObj = null;
                }));
            Program.WinMain.TbtbClippingPolygonAnotherPolygon.IsChecked = true;
        }
        public void TbtbClippingPolygonBySpecifyingPoints_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (Program.WinMain.TbtbClippingPolygonBySpecifyingPoints.IsChecked == true)
            {
                if (SelectedLayer != null)
                {
                    //MessageBox.Show("Создайте пустоту");
                    //ll.DeselectAll();
                    {
                        mvLayer ll1 = Program.app.mapLib.getLayer("TempLayerToCreateHollow");
                        if (ll1 == null)
                        {
                            var ff = new mvStringArray();
                            ff.count = 1;
                            ff.setElem(0, "id");

                            ll1 = Program.app.mapLib.CreateLayer("TempLayerToCreateHollow", ff);
                            var p1 = new mvPenObject
                            {
                                Color = 0x333333,
                                ctype = 2,
                                width = 2
                            };
                            var b1 = new mvBrushObject
                            {
                                bgcolor = 0xffff00,
                                fgcolor = 0x00ffff,
                                style = 0,
                                hatch = 2
                            };
                            var f1 = new mvFontObject
                            {
                                Color = 0xff0000,
                                fontname = "Map Symbols",
                                framecolor = 0xff0000,
                                size = 12
                            };
                            var s1 = new mvSymbolObject();
                            s1.shape = 35;
                            ll1.uniform = true;
                            ll1.SetUniformStyle(p1, b1, s1, f1);
                        }
                        else
                        {
                            ll1.RemoveObjects();
                        }
                        axMapLIb1.CtlCursor = mvMapLib.Cursors.mlAddPolygon;
                        SelectedLayer.editable = false;
                        ll1.editable = true;
                    }
                    _newHollowAdding = true;

                    SaveSelectedTable();
                }
            }
            else
            {
                _newHollowAdding = false;

                axMapLIb1.SetCursor(mvMapLib.Cursors.mlSelect);

                if (axMapLIb1.getLayer("TempLayerToCreateHollow") != null)
                    axMapLIb1.getLayer("TempLayerToCreateHollow").deleteLayer();

                bManager.SetButtonsState();
            }
        }
        public void TbtbClippingPolygonBySpecifyingLine_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (Program.WinMain.TbtbClippingPolygonBySpecifyingLine.IsChecked == true)
            {
                if (SelectedLayer != null)
                {
                    //MessageBox.Show("Создайте пустоту");
                    //ll.DeselectAll();
                    {
                        mvLayer ll1 = Program.app.mapLib.getLayer("TempLayerToCreateLine");
                        if (ll1 == null)
                        {
                            var ff = new mvStringArray();
                            ff.count = 1;
                            ff.setElem(0, "id");

                            ll1 = Program.app.mapLib.CreateLayer("TempLayerToCreateLine", ff);
                            var p1 = new mvPenObject
                            {
                                Color = 0x333333,
                                ctype = 2,
                                width = 2
                            };
                            var b1 = new mvBrushObject
                            {
                                bgcolor = 0xffff00,
                                fgcolor = 0x00ffff,
                                style = 0,
                                hatch = 2
                            };
                            var f1 = new mvFontObject
                            {
                                Color = 0xff0000,
                                fontname = "Map Symbols",
                                framecolor = 0xff0000,
                                size = 12
                            };
                            var s1 = new mvSymbolObject();
                            s1.shape = 35;
                            ll1.uniform = true;
                            ll1.SetUniformStyle(p1, b1, s1, f1);
                        }
                        else
                        {
                            ll1.RemoveObjects();
                        }
                        axMapLIb1.CtlCursor = mvMapLib.Cursors.mlAddPolyLine;
                        SelectedLayer.editable = false;
                        ll1.editable = true;
                    }
                    _newLineAdding = true;

                    SaveSelectedTable();
                }
            }
            else
            {
                _newLineAdding = false;

                this.axMapLIb1.SetCursor(mvMapLib.Cursors.mlSelect);

                if (axMapLIb1.getLayer("TempLayerToCreateLine") != null)
                    axMapLIb1.getLayer("TempLayerToCreateLine").deleteLayer();

                bManager.SetButtonsState();
            }
        }
        public void TbbLayerUp_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            layerItemsView1.PickLayerUp(Keys.Up);
            axMapLIb1.mapRepaint();
        }
        public void TbbLayerDown_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            layerItemsView1.PickLayerUp(Keys.Down);
            axMapLIb1.mapRepaint();
        }
        public void TbbWorkSetChangeStyleLayer_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Program.WorkSets.ChangeStyleLayer();
        }
        public void TbbWorkSetDeleteStyleLayer_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Program.WorkSets.DeleteStyleLayerSet();
        }
        public void TbbWorkSetSaveLocationLayers_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Program.WorkSets.SaveLocationLayers();
        }
        #endregion Обработчики тулбоксов из WinMain

        #region Обработчики меню из WinMain
        public void MiExportPrint_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ToolsFrm.PrintSettingsFrm frm = new ToolsFrm.PrintSettingsFrm();
            if (frm.ShowDialog() == DialogResult.OK)
            {
                SaveFileDialog dl = new SaveFileDialog();
                dl.Filter = "Bitmap Files | *.bmp";
                if (dl.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    try
                    {
                        Size s = frm.MyPrintSetting.GetSizeImage();
                        if (!axMapLIb1.ExportImage(s.Width, s.Height, dl.FileName))
                        {
                            try
                            {
                                File.Delete(dl.FileName);
                            }
                            catch { }
                            MessageBox.Show(Rekod.Properties.Resources.mainFrm_errImgSize, Rekod.Properties.Resources.message, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    catch
                    {
                        try
                        {
                            File.Delete(dl.FileName);
                        }
                        catch { }
                        MessageBox.Show(Rekod.Properties.Resources.mainFrm_errFrmImg, Rekod.Properties.Resources.message, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
        public void MiSaveImage_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Image img;
            try
            {
                img = (Image)axMapLIb1.Image.Clone();
            }
            catch (Exception ex)
            {
                Classes.workLogFile.writeLogFile(ex, true, true);
                return;
            }
            var dlg = new SaveFileDialog();
            dlg.AddExtension = true;
            dlg.CreatePrompt = false;
            dlg.Filter = Rekod.Properties.Resources.mainFrm_filter;
            dlg.RestoreDirectory = true;
            dlg.FilterIndex = 1;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    switch (dlg.FilterIndex)
                    {
                        case 1:
                            img.Save(dlg.FileName, System.Drawing.Imaging.ImageFormat.Bmp);
                            break;
                        case 2:
                            img.Save(dlg.FileName, System.Drawing.Imaging.ImageFormat.Jpeg);
                            break;
                        case 3:
                            img.Save(dlg.FileName, System.Drawing.Imaging.ImageFormat.Png);
                            break;
                    }
                }
                catch
                {
                    MessageBox.Show(@Rekod.Properties.Resources.mainFrm_msgPicCS);
                }
            }
            dlg.Dispose();
        }
        public void MiChooseVmpFile_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.Multiselect = false;
            dlg.Filter = "VMP|*.vmp";
            dlg.Title = Rekod.Properties.Resources.mainFrm_msgMFile;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                // выбран текущий файл карты
                if (dlg.FileName == axMapLIb1.MapFile)
                    return;

                // сохраняем текущий extent
                SettingStartPosition.GetBboxBd getBboxBd = new SettingStartPosition.GetBboxBd();
                var ext = getBboxBd.GetBboxInBd(axMapLIb1.MapExtent);

                string filePath = dlg.FileName.Remove(dlg.FileName.LastIndexOf(".vmp"));
                String mapFile = GetMapFile(filePath, false);
                if (String.IsNullOrEmpty(mapFile))
                    return; // нет файла карты или файла srid

                cti.ThreadProgress.ShowWait();

                List<mvLayer> layers = new List<mvLayer>();
                List<int> idList = new List<int>();
                // сохраняем видимость слоев
                for (int i = 0; i < axMapLIb1.LayersCount; i++)
                {
                    var layer = axMapLIb1.getLayerByNum(i);
                    layers.Add(layer);
                    int id = Program.RelationVisbleBdUser.GetIdTable(layer.NAME);
                    if (id != -1)
                        idList.Add(id);
                }
                foreach (var layer in layers)
                {
                    layer.deleteLayer();
                }

                // меняем подложку
                axMapLIb1.CloseMap();
                axMapLIb1.LoadMap(mapFile, "id");
                axMapLIb1.SRID = Program.srid;
                Program.proj4Map = TransformGeometry.GetProj4(Convert.ToInt32(Program.srid));
                projMap = TransformGeometry.GetProjWKT(int.Parse(Program.srid));

                axMapLIb1.MapExtent = getBboxBd.GetBboxFromBd(ext, Program.srid);

                var c = (NpgsqlConnectionStringBuilder)Program.connString;
                if (Program.postgres != null)
                {
                    var str = string.Format("host={0} port={1} user={2} password={3} dbname={4}",
                               c.Host,
                               c.Port,
                               c.UserName,
                               c["Password"],
                               c.Database);
                    Program.postgres.prepare(str, Convert.ToInt32(Program.srid));
                    Program.postgres.Connect();
                }

                // восстанавливаем положение слоев
                layerItemsView1.Hide();
                layerItemsView1.MakeGroups();
                layerItemsView1.AddNewLayersInGroups();
                ((Rekod.DataAccess.SourceVMP.ViewModel.VMPDataRepositoryVM)Program.TablesManager.VMPReposotory).ReloadFullInfo();
                layerItemsView1.GetStylesLayerVMP();
                layerItemsView1.SetRastrLayersVisible();
                foreach (var id in idList)
                    layerItemsView1.SetLayerVisible(id);
                layerItemsView1.UpdateListLayersIsView();
                layerItemsView1.Show();

                Program.TablesManager.CosmeticRepository.ReloadInfo();
                (Program.TablesManager.CosmeticRepository as CosmeticDataRepositoryVM).CreateLayer("Косметический слой");

                axMapLIb1.mapRepaint();

                cti.ThreadProgress.Close();
            }
        }
        public void MiRastrGeoreference_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DeepZoom.TestOverview.View.GeoreferencePreviewV previewV = new DeepZoom.TestOverview.View.GeoreferencePreviewV();
            Rekod.RasterGeoreferenceModule.ViewModel.GeoreferencePreviewVM previewVM = new Rekod.RasterGeoreferenceModule.ViewModel.GeoreferencePreviewVM(previewV.DeepZoomControl);
            previewV.DataContext = previewVM;
            previewV.Owner = Program.WinMain;
            previewV.Show();
        }
        public void MiPrint_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (Program.PreviewWindow == null)
            {
                Program.PreviewWindow = new PrintModule.View.PreviewWindow(axMapLIb1);
                Program.PreviewWindow.Owner = Program.WinMain;
            }
            else
            {
                (Program.PreviewWindow.DataContext as Rekod.PrintModule.ViewModel.PreviewWindowVM).SyncWithMapLib();
            }
            Program.PreviewWindow.Show();
        }
        public void MiExit_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Program.WinMain.Close();
        }
        public void MiManageTables_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (Program.WorkSets.CurrentWorkSet.IsDefault)
            {
                var frm = new DBTablesGroups(null);
                frm.ShowDialog();
            }
            else
            {
                MessageBox.Show(Rekod.Properties.Resources.mainFrm_msgStWS, Rekod.Properties.Resources.message, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        public void MiManageTablesRegistration_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_pgTableRegistrationVM == null)
            {
                _pgTableRegistrationVM = new DataAccess.SourcePostgres.Modules.TableRegister.ViewModel.RTableRegistrationVM();
                Rekod.DataAccess.SourcePostgres.Modules.TableRegister.View.RTableRegistrationV tableRegV =
                            new DataAccess.SourcePostgres.Modules.TableRegister.View.RTableRegistrationV();
                WindowViewModelBase_VM.GetWindow(tableRegV, _pgTableRegistrationVM, 800, 600, 800, 600, Program.WinMain);
            }
            _pgTableRegistrationVM.AttachedWindow.Show();
        }
        public void MiAddMapAdminLayer_Click(object sender, sw.RoutedEventArgs e)
        {
            if (Program.sscUser != null)
            {
                try
                {
                    sscSync.ViewModel.LayerVM layerVM = new sscSync.ViewModel.LayerVM(Program.sscUser);
                    sscSync.View.LayerView layerView = new sscSync.View.LayerView();
                    layerVM.PropertyChanged += (s, ee) =>
                    {
                        if (ee.PropertyName == "IsFinished" && layerVM.IsFinished)
                        {
                            layerView.DialogResult = true;
                            layerView.Close();
                        }
                    };
                    layerView.DataContext = layerVM;
                    if (layerView.ShowDialog() == true)
                    {
                        (new classesOfMetods()).reloadInfo();
                        layerItemsView1.RefreshLayers();
                    }
                }
                catch (Exception exc)
                {
                    MessageBox.Show(exc.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        public void MiRastrLayers_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Program.TablesManager.OpenConfigurator(DataAccess.AbstractSource.Model.ERepositoryType.Rastr);
        }
        public void MiBaseLayers_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Program.TablesManager.OpenConfigurator(DataAccess.AbstractSource.Model.ERepositoryType.VMP);
        }
        public void MiCosmeticLayers_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Program.TablesManager.OpenConfigurator(DataAccess.AbstractSource.Model.ERepositoryType.Cosmetic);
        }
        public void MiUserRights_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Program.repository.OpenAccessWindow();
        }
        public void MiHistory_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            PgHistoryVM histVM = new PgHistoryVM(Program.repository);
            Rekod.DataAccess.SourcePostgres.View.History.PgHistoryV histV = new DataAccess.SourcePostgres.View.History.PgHistoryV(histVM);
            histV.Owner = Program.WinMain;
            histV.Height = 600;
            histV.Width = 900;
            histV.Show();
        }
        public void MiLocations_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var frm = new SettingStartPosition.SettingsPosFrm();
            frm.Show();
        }
        public void MiSearch_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SearchFullText(null);
        }
        public void MiFastStart_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var cmdForm = CMDPlugin.SettingsForm;
            cmdForm.Show();
        }
        public void MiProgramPlugins_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            PluginSettings.PluginSettingsForm frm = new Rekod.PluginSettings.PluginSettingsForm();
            frm.ShowDialog();
        }
        public void MiSrcBaseLayers_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Program.TablesManager.OpenConfigurator(DataAccess.AbstractSource.Model.ERepositoryType.VMP);
        }
        public void MiSrcRastrLayers_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Program.TablesManager.OpenConfigurator(DataAccess.AbstractSource.Model.ERepositoryType.Rastr);
        }
        public void MiSrcPostgreSQL_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Program.TablesManager.OpenConfigurator(DataAccess.AbstractSource.Model.ERepositoryType.Postgres);
        }
        public void MiSrcShowPanel_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            sw.Controls.MenuItem menuItem = sender as sw.Controls.MenuItem;
            Program.WinMain.ManagerColumn.Width = menuItem.IsChecked ? new System.Windows.GridLength(350) : new System.Windows.GridLength(0);
            Program.WinMain.ToolBoxRow.Height = menuItem.IsChecked ? new System.Windows.GridLength(1, System.Windows.GridUnitType.Auto) : new System.Windows.GridLength(0);
            Program.IsV3 = menuItem.IsChecked;
        }
        public void MiManual_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var frm = new HelpFiles.HelpForm();
            frm.Show();
            frm.Activate();
        }
        public void MiLicences_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            (new HelpFiles.LicenseForm()).Show();
        }
        public void MiAbout_Click(object sender, System.Windows.RoutedEventArgs e)
        {

            HelpFiles.AboutBoxGS frm = new Rekod.HelpFiles.AboutBoxGS();
            frm.ShowDialog();

        }
        public void MiTb_CheckedChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            sw.Controls.MenuItem tbMenuItem = sender as sw.Controls.MenuItem;
            sw.Controls.ToolBar tbChangeVis = null;
            switch (tbMenuItem.Name)
            {
                case "MiTbMap": { tbChangeVis = Program.WinMain.TbMaps; break; }
                case "MiTbSelectObject": { tbChangeVis = Program.WinMain.TbSelectionOf; break; }
                case "MiTbEditObject": { tbChangeVis = Program.WinMain.TbOperationsWithObjects; break; }
                case "MiTbEditGeom": { tbChangeVis = Program.WinMain.TbOperationsWithGeometry; break; }
                case "MiTbLayersList": { tbChangeVis = Program.WinMain.TbLayersManager; break; }
                case "MiTbFastStart": { tbChangeVis = Program.WinMain.TbFastStart; break; }
                case "MiTbEditableLayer": { tbChangeVis = Program.WinMain.TbEditableLayer; break; }
                case "MiTbWorkSets": { tbChangeVis = Program.WinMain.TbWorkSets; break; }
                case "MiTbEnabledModes": { tbChangeVis = Program.WinMain.TbModeEnabled; break; }
                case "MiTbPlugins": { tbChangeVis = Program.WinMain.TbPlugins; break; }
                //case "MiTbCoords": { tbChangeVis = Program.WinMain.TbCoordinates; break; }
                case "MiTbSearch": { tbChangeVis = Program.WinMain.TbFullTextSearch; break; }
                //case "MiTbScale": { tbChangeVis = Program.WinMain.TbScale; break; }
                //case "MiTbInfo": { tbChangeVis = Program.WinMain.TbInfoText; break; }
            }
            if (tbChangeVis != null)
            {
                tbChangeVis.Visibility = tbMenuItem.IsChecked ? sw.Visibility.Visible : sw.Visibility.Collapsed;
            }
        }
        public void MiMapFitIn_CheckedChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            sw.Controls.MenuItem menuItem = sender as sw.Controls.MenuItem;
            Program.SettingsXML.LocalParameters.EnterTheScreen = menuItem.IsChecked;
            Program.SettingsXML.ApplyLocalParameters();
            Program.SettingsXML.ApplyLastChange();
        }
        public void MiShowObjectForm_CheckedChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            sw.Controls.MenuItem menuItem = sender as sw.Controls.MenuItem;
            Program.SettingsXML.LocalParameters.OpenAttrsAfterCreate = menuItem.IsChecked;
            Program.SettingsXML.ApplyLocalParameters();
            Program.SettingsXML.ApplyLastChange();
        }
        public void MiTbRestoreStandard_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            foreach (var item in bManager.ListToolBars)
            {
                item.IsVisable = true;
            }
        }
        public void MiTbRestorePrevious_CheckedChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            sw.Controls.MenuItem menuItem = sender as sw.Controls.MenuItem;
            Program.SettingsXML.ApplyLocalParameters();
            Program.SettingsXML.ApplyLastChange();
        }
        public void MiUpdMenu_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ReloadDataMenu(Program.WinMain.MiData);
        }
        public void MiUpdDics_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Program.CachedStyles.RelaodAllStyleTable();
        }
        public void MiReportsOpen_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                Program.ReportModel.OpenReportEditor(new FilterTable(), enTypeReport.All);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Rekod.Properties.Resources.mainFrm_errorReportManager, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion Обработчики меню из WinMain

        private void axMapLIb1_OnStatusInfoChanged(object sender, AxmvMapLib.IMapLIbEvents_OnStatusInfoChangedEvent e)
        {
            if (this.StatusInfo != e.str && e.str != null)
            {
                if (e.str == "Невозможно создать объект вне границ видемости слоя")
                {
                    this.StatusInfo = "Невозможно создать объект вне границ видимости слоя";
                }
                else
                {
                    this.StatusInfo = e.str;
                }
            }
        }
        #endregion Обработчики

        #region События
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion События
    }
}