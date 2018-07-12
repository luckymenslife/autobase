using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using mvMapLib;
using System.Diagnostics;
using Interfaces;
using NpgsqlTypes;
using axVisUtils.Styles;
using System.Windows.Forms.VisualStyles;
using System.Resources;
using System.Collections.ObjectModel;
using System.Globalization;
using Rekod.DataAccess.SourceRastr.Model;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using Rekod.Services;
using REST = RESTLib.Model.REST;
using WFS = RESTLib.Model.WFS;
using AbsM = Rekod.DataAccess.AbstractSource.Model;
using CosM = Rekod.DataAccess.SourceCosmetic.Model;
using VMPM = Rekod.DataAccess.SourceVMP.Model;
using System.Linq.Expressions;

namespace Rekod.DBLayerForms
{
    public partial class LayerItemsView : UserControl, INotifyPropertyChanged
    {
        public enum enViewType
        {
            groups = 0,
            all = 1,
            visible = 2,
            selectable = 3
        }

        #region  Поля
        private int prevX = -5;
        private int prevY = -5;
        private bool dockChecked = false;
        private bool dockRight = false;

        private enViewType _viewType;
        private int _editableIdLayer;
        private AbsM.ILayerM _editableLayer;
        private int _visibleNodeIndex = 0;
        private ObservableCollection<AbsM.TableBaseM> _listLayersIsView;
        #endregion // Поля

        #region Анимация
        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                if (!dockChecked)
                {
                    dockRight = (Program.mainFrm1.layerItemsView1.Dock == DockStyle.Right);
                    dockChecked = true;
                }
                if (dockRight)
                {
                    return;
                }
                if (prevY == -5 && prevX == -5)
                {
                    prevY = e.Y;
                    prevX = e.X;
                    return;
                }

                Point pt = new Point(this.Location.X + e.X - prevX, this.Location.Y + e.Y - prevY);
                this.Location = pt;
            }
        }
        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            dockChecked = false;
            prevY = -5;
            prevX = -5;
        }
        private void panel1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (Program.mainFrm1.layerItemsView1.Dock == DockStyle.Right)
            {
                Program.mainFrm1.layerItemsView1.BringToFront();
                Program.mainFrm1.layerItemsView1.Dock = DockStyle.None;
            }
            else
            {
                Program.mainFrm1.layerItemsView1.SendToBack();
                Program.mainFrm1.layerItemsView1.Dock = DockStyle.Right;
            }
        }
        private void panel2_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                if (!dockChecked)
                {
                    dockRight = (Program.mainFrm1.layerItemsView1.Dock == DockStyle.Right);
                    dockChecked = true;
                }
                if (dockRight)
                {
                    return;
                }
                if (prevY == -5 && prevX == -5)
                {
                    prevY = e.Y;
                    prevX = e.X;
                    return;
                }

                int newWidth = this.Width + e.X - prevX;
                int newHeight = this.Height + e.Y - prevY;
                if (newWidth > 205 && newHeight > 75)
                {
                    this.Width = newWidth;
                    this.Height = newHeight;
                }

                prevX = e.X;
                prevY = e.Y;
            }
        }
        private void panel2_MouseUp(object sender, MouseEventArgs e)
        {
            dockChecked = false;
            prevY = -5;
            prevX = -5;
        }
        private void panel2_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (Program.mainFrm1.layerItemsView1.Dock == DockStyle.Right)
            {
                Program.mainFrm1.layerItemsView1.BringToFront();
                Program.mainFrm1.layerItemsView1.Dock = DockStyle.None;
            }
            else
            {
                Program.mainFrm1.layerItemsView1.SendToBack();
                Program.mainFrm1.layerItemsView1.Dock = DockStyle.Right;
            }
        }

        private bool minimized = false;
        private Size exSize;
        private Point exLocation;

        private void pictureBox2_MouseEnter(object sender, EventArgs e)
        {
            pictureBox2.Image = Rekod.Properties.Resources.obnovit;
            pictureBox2.BackgroundImage = Rekod.Properties.Resources.pri_vibore_instrumenta2;
        }

        private void pictureBox2_MouseLeave(object sender, EventArgs e)
        {
            pictureBox2.Image = Rekod.Properties.Resources.обновить;
            pictureBox2.BackgroundImage = null;
        }

        private void pictureBox1_MouseEnter(object sender, EventArgs e)
        {
            pictureBox1.BackgroundImage = Rekod.Properties.Resources.pri_vibore_instrumenta2;
        }
        private void pictureBox1_MouseLeave(object sender, EventArgs e)
        {
            pictureBox1.BackgroundImage = null;
        }
        private void pictureBox2_Click(object sender, EventArgs e)
        {
            cti.ThreadProgress.ShowWait();
            classesOfMetods cls = new classesOfMetods();
            cls.reloadInfo();
            tabControl1_SelectedIndexChanged(sender, e);
            Program.repository.ReloadPartInfo();
            cti.ThreadProgress.Close();
        }

        public void RefreshLayers()
        {
            SuspendLayout();
            cti.ThreadProgress.ShowWait();
            tabControl1_SelectedIndexChanged(null, null);
            textBox2.ForeColor = Color.Silver;
            defineToolBoxVisibility();
            cti.ThreadProgress.Close();
            ResumeLayout();
        }
        #endregion

        public List<GroupInfo> GroupInfoList = new List<GroupInfo>();

        public Dictionary<int, bool> GroupExpanded = new Dictionary<int, bool>();
        public Dictionary<int, bool> AllExpanded = new Dictionary<int, bool>();
        /// <summary>
        /// Список слоев PG отображаемых на карте
        /// </summary>
        public ObservableCollection<AbsM.TableBaseM> ListLayersIsView
        {
            get { return _listLayersIsView; }
        }

        private Dictionary<string, StylesVM> _dicStyles;
        private Dictionary<TreeNode, TreeNode> _layersOrder = new Dictionary<TreeNode, TreeNode>();
        private mvLayer _mvLayer;

        public int EditableIdLayer
        {
            get { return _editableIdLayer; }
            private set { OnPropertyChanged(ref _editableIdLayer, value, () => this.EditableIdLayer); }
        }
        public AbsM.ILayerM EditableLayer
        {
            get { return _editableLayer; }
            private set { OnPropertyChanged(ref _editableLayer, value, () => this.EditableLayer); }
        }


        public LayerItemsView()
        {
            _listLayersIsView = new ObservableCollection<AbsM.TableBaseM>();
            _viewType = enViewType.groups;
            _dicStyles = new Dictionary<string, StylesVM>();
            InitializeComponent();
        }
        internal void OnPropertyChanged<T>(ref T Value, T newValue, Expression<Func<T>> action)
        {
            if (Value == null && newValue == null)
                return;
            if (Value != null && Value.Equals(newValue))
                return;
            Value = newValue;
            OnPropertyChanged(GetPropertyName(action));
        }
        private static string GetPropertyName<T>(Expression<Func<T>> action)
        {
            var expression = (MemberExpression)action.Body;
            var propertyName = expression.Member.Name;
            return propertyName;
        }
        public virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #region Основная логика менеджера
        public TreeNode _clickedNode = null;
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_viewType == enViewType.visible)
            {
                for (int i = 0; i < treeView1.Nodes.Count; i++)
                {
                    if (treeView1.Nodes[i].IsVisible)
                    {
                        _visibleNodeIndex = i;
                        break;
                    }
                }
            }
            TreeNode _tempClickedNode = _clickedNode;
            _clickedNode = null;
            treeView1.SelectedNode = null;
            makeToolBoxInvisible(false, false);

            _primaryNodesList.Clear();
            treeView1.SuspendLayout();

            _viewType = (enViewType)tabControl1.SelectedIndex;

            switch (_viewType)
            {
                case enViewType.groups:
                    {
                        treeView1.Visible = false;
                        AddNewLayersInGroups();
                        SetPodlojkaLayersVisible();
                        SetRastrLayersVisible();
                        SetCosmeticLayersVisible();
                        SetGroupsExpanded();
                        setBackColorSelectableLayers();
                        fillLayersOrder();
                        treeView1.Visible = true;
                        break;
                    }
                case enViewType.all:
                    {
                        treeView1.Visible = false;
                        AddNewLayersAll();
                        SetPodlojkaLayersVisible();
                        sortPodlojkaLayers();
                        SetRastrLayersVisible();
                        SetCosmeticLayersVisible();
                        SetAllExpanded();
                        setBackColorSelectableLayers();
                        fillLayersOrder();
                        treeView1.Visible = true;
                        break;
                    }
                case enViewType.visible:
                    {
                        treeView1.Visible = false;
                        addNewLayersVisible(false);
                        sortInMapOrder();
                        setBackColorSelectableLayers();
                        treeView1.Visible = true;
                        break;
                    }
                case enViewType.selectable:
                    {
                        treeView1.Visible = false;
                        addNewLayersVisible(true);
                        sortInMapOrder();
                        setBackColorSelectableLayers();
                        treeView1.Visible = true;
                        break;
                    }
            }
            UpdateTree();

            if (treeView1.SelectedNode == null && _tempClickedNode != null && treeView1.Nodes.Find(_tempClickedNode.Name, true).Count() == 0 && _tempClickedNode.Tag is tablesInfo)
            {
                String nameInBd = Program.RelationVisbleBdUser.GetNameInBd(((tablesInfo)(_tempClickedNode.Tag)).idTable);
                mvMapLib.mvLayer ll = Program.mainFrm1.axMapLIb1.getLayer(nameInBd);
                if (ll != null)
                    ll.selectable = ll.editable = false;
                _tempClickedNode = null;
            }

            treeView1.ResumeLayout();
            if (_viewType != enViewType.selectable)
            {
                _clickedNode = _tempClickedNode;
                setBackColorSelectableLayers();
                defineToolBoxVisibility();
            }
            else
            {
                defineToolBoxVisibility();
                _clickedNode = _tempClickedNode;
            }
            UpdateListLayersIsView();

            if (treeView1.SelectedNode == null && _clickedNode != null)
            {
                treeView1.SelectedNode = _clickedNode;
                if (treeView1.SelectedNode != null)
                    treeView1.SelectedNode.EnsureVisible();
            }

            Program.mainFrm1.bManager.SetButtonsState();
        }
        private bool layerIsVisible(String nameMap, int? tablId)
        {
            try
            {
                String layerName = nameMap;
                if (tablId != null)
                {
                    layerName = Program.RelationVisbleBdUser.GetNameInBd((int)tablId);
                }
                if (!string.IsNullOrEmpty(layerName))
                {
                    _mvLayer = Program.mainFrm1.axMapLIb1.getLayer(layerName);
                    if (_mvLayer != null)
                    {
                        return _mvLayer.Visible;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            { }
            return false;
        }
        /// <summary>
        /// Поменять свойство видимости слоя на visable
        /// </summary>
        /// <param name="visable"></param>
        /// <param name="nameMap"></param>
        /// <param name="tablId"></param>
        /// <returns>Выполнималь ли операция или нет</returns>
        public void layerSetVisible(bool visable, String nameMap = null, int? tablId = null)
        {
            try
            {
                String layerName = nameMap;
                if (tablId != null)
                {
                    tablesInfo ti = classesOfMetods.getTableInfo((int)tablId);
                    if (ti != null)
                        layerName = ti.nameMap;
                }
                TreeNode[] coll = treeView1.Nodes.Find(layerName, true);

                for (int i = 0; i < coll.Length; i++)
                {
                    if (coll[i].Tag is tablesInfo)
                    {
                        //coll[i].Checked = true;
                        SetCheckedVisible(coll[i], visable);
                        break;
                    }
                }
            }
            catch (Exception)
            {
            }
        }
        public void MakeGroups()
        {
            GroupInfoList.Clear();
            if (Program.ssc != null)
                Program.ssc.ClearId();

            if (Program.ssc != null)
                foreach (var item in Program.group_info)
                {
                    var gi = new GroupInfo(item.id, item.name, item.descript);

                    //var sscGroup = Program.ssc.GetGroupByName(gi.NameGroup);
                    var sscGroup = Program.mapAdmin.SscData.GroupFromLayer.FirstOrDefault(w=>w.name==gi.NameGroup);
                    if (sscGroup == null)
                        continue;
                    sscGroup.id = gi.Id;


                    var nodePg = GetTreeNodeGroup(gi);
                    gi.IdNode = treeView1.Nodes.Add(nodePg);

                    GroupInfoList.Add(gi);
                }
            else
            {
                foreach (var item in Program.group_info)
                {
                    var gi = new GroupInfo(item.id, item.name, item.descript);
                    var nodePg = GetTreeNodeGroup(gi);
                    gi.IdNode = treeView1.Nodes.Add(nodePg);
                    GroupInfoList.Add(gi);
                }
            }
            GroupInfo gip = new GroupInfo(-1, Rekod.Properties.Resources.LIV_Vmp_layer, Rekod.Properties.Resources.LIV_Vmp_layer);
            var nodeVMP = GetTreeNodeGroup(gip);
            gip.IdNode = treeView1.Nodes.Add(nodeVMP);
            GroupInfoList.Add(gip);

            GroupInfo gir = new GroupInfo(-2, Rekod.Properties.Resources.LIV_Rastr_layer, Rekod.Properties.Resources.LIV_Rastr_layer);
            var nodeRastr = GetTreeNodeGroup(gir);
            gir.IdNode = treeView1.Nodes.Add(nodeRastr);
            GroupInfoList.Add(gir);

            GroupInfo gic = new GroupInfo(-4, Rekod.Properties.Resources.LocSourceCosmetic, Rekod.Properties.Resources.LocSourceCosmetic);
            var nodeCosmetic = GetTreeNodeGroup(gic);
            gir.IdNode = treeView1.Nodes.Add(nodeCosmetic);
            GroupInfoList.Add(gic);
        }
        public void AddNewLayersInGroups()
        {
            treeView1.Nodes.Clear();

            GC.Collect();
            MakeGroups();

            List<tablesInfo> tInfoList = classesOfMetods.getTableOfType(1);

            var groupsAndGrOrder = (from c in Program.tablegroups_info orderby c.IdGroup ascending select c.IdGroup).Distinct();
            foreach (int tagi in groupsAndGrOrder)
            {
                GroupInfo gi = GroupInfoList.Find(w => w.Id == tagi);

                var tableGroupsIdOrder = (from c in Program.tablegroups_info where c.IdGroup == tagi orderby c.OrderNum ascending select c.IdTable);
                if (gi == null) continue;

                //Rekod.Classes.GroupSSCInfo giSSC = null;
                //if (Program.ssc != null)
                //{
                //    giSSC = Program.ssc.GetGroupByGroupId(gi.Id);
                //}
                List<tablesInfo> tablesInGroupsAndOrder = (from ti in tInfoList where tableGroupsIdOrder.Contains(ti.idTable) select ti).ToList();
                foreach (int idT in tableGroupsIdOrder)
                {
                    bool Found = false;
                    int i = 0;
                    if (Program.ssc != null)
                    {
                        while (!Found && i < tablesInGroupsAndOrder.Count)
                        {
                            tablesInfo ti = (tablesInfo)(tablesInGroupsAndOrder[i]);
                            if (ti.idTable == idT)
                            {
                                tablesInGroupsAndOrder.Remove(ti);
                                tablesInGroupsAndOrder.Add(ti);
                                Found = true;
                                break;
                            }
                            i++;
                        }
                    }
                    else
                    {
                        while (!Found && i < tablesInGroupsAndOrder.Count)
                        {
                            tablesInfo ti = (tablesInfo)(tablesInGroupsAndOrder[i]);
                            if (ti.idTable == idT)
                            {
                                tablesInGroupsAndOrder.Remove(ti);
                                tablesInGroupsAndOrder.Add(ti);
                                Found = true;
                                break;
                            }
                            i++;
                        }
                    }
                }
                if (gi == null) continue;
                foreach (tablesInfo ti in tablesInGroupsAndOrder)
                {
                    var node = GetTreeNodePg(ti, layerIsVisible(ti.nameMap, ti.idTable));
                    treeView1.Nodes[gi.IdNode].Nodes.Add(node);
                }
            }
        }
        public void AddNewLayersAll()
        {
            treeView1.Nodes.Clear();
            GC.Collect();

            GroupInfo gia = new GroupInfo(-3, Rekod.Properties.Resources.LIV_DB_layer, Rekod.Properties.Resources.LIV_DB_layer);
            var nodePg = GetTreeNodeGroup(gia);
            gia.IdNode = treeView1.Nodes.Add(nodePg);
            GroupInfoList.Add(gia);

            GroupInfo gip = new GroupInfo(-1, Rekod.Properties.Resources.LIV_Vmp_layer, Rekod.Properties.Resources.LIV_Vmp_layer);
            var nodeVMP = GetTreeNodeGroup(gip);
            gip.IdNode = treeView1.Nodes.Add(nodeVMP);
            GroupInfoList.Add(gip);

            GroupInfo gir = new GroupInfo(-2, Rekod.Properties.Resources.LIV_Rastr_layer, Rekod.Properties.Resources.LIV_Rastr_layer);
            var nodeRasrt = GetTreeNodeGroup(gir);
            gir.IdNode = treeView1.Nodes.Add(nodeRasrt);
            GroupInfoList.Add(gir);

            GroupInfo gic = new GroupInfo(-4, Rekod.Properties.Resources.LocSourceCosmetic, Rekod.Properties.Resources.LocSourceCosmetic);
            var nodeCosmetic = GetTreeNodeGroup(gic);
            gir.IdNode = treeView1.Nodes.Add(nodeCosmetic);
            GroupInfoList.Add(gic);

            List<tablesInfo> tInfoList = classesOfMetods.getTableOfType(1);
            TreeNode par = treeView1.Nodes[gia.IdNode];
            tInfoList.Sort((IComparer<tablesInfo>)(new tablesInfo()));

            foreach (tablesInfo ti in tInfoList)
            {
                if (!ti.hidden)
                {
                    var node = GetTreeNodePg(ti, layerIsVisible(ti.nameMap, ti.idTable));
                    par.Nodes.Add(node);
                }
            }
        }
        private void addNewLayersVisible(bool checkSelectable)
        {
            treeView1.Nodes.Clear();
            GC.Collect();

            var tInfoList = classesOfMetods.getTableOfType(1);
            foreach (tablesInfo ti in tInfoList)
            {
                if (!ti.hidden && layerIsVisible(ti.nameMap, ti.idTable))
                {
                    var visable = false;
                    if (checkSelectable)
                    {
                        String layerName = Program.RelationVisbleBdUser.GetNameInBd(ti.idTable);
                        visable = getLayerIsSelectable(layerName);
                    }
                    else
                    {
                        visable = true;
                    }
                    var node = GetTreeNodePg(ti, visable);
                    treeView1.Nodes.Add(node);
                    if (!checkSelectable)
                    {
                        foreach (var item in ti.StyleVM.ListStyles)
                        {
                            var noteStyle = GetTreeNodeStyle(item);
                            node.Nodes.Add(noteStyle);
                        }
                        node.Expand();
                    }
                }
            }

            foreach (AbsM.ILayerM layer in Program.TablesManager.VMPReposotory.Tables)
            {
                if (layer.IsVisible)
                {
                    bool visable = !checkSelectable || layer.IsSelectable;

                    var node = GetTreeNodeVMP(layer, visable);
                    treeView1.Nodes.Add(node);
                    if (!checkSelectable)
                    {
                        if (_dicStyles.ContainsKey(layer.Name))
                        {
                            var styles = _dicStyles[layer.Name];

                            foreach (var item in styles.ListStyles)
                            {
                                var noteStyle = GetTreeNodeStyle(item);
                                node.Nodes.Add(noteStyle);
                            }
                            node.Expand();
                        }
                    }
                }
            }

            foreach (CosM.CosmeticTableBaseM table in Program.TablesManager.CosmeticRepository.Tables)
            {
                TreeNode node = null;
                bool visable = false;
                try
                {
                    var ll = Program.mainFrm1.axMapLIb1.getLayer(table.NameMap);
                    visable = (ll != null && ll.Visible);
                    if (visable)
                    {
                        if (checkSelectable)
                        {
                            visable = getLayerIsSelectable(table.NameMap);
                        }
                        node = GetTreeNodeCosmetic(table, visable);
                        treeView1.Nodes.Add(node);
                    }
                }
                catch (Exception)
                {

                }
            }

            if (!checkSelectable)
            {
                layersManagerFRMRastr rastrManager = new layersManagerFRMRastr();
                foreach (RastrInfo rInfo in rastrManager.GetRastrLayers())
                {
                    try
                    {
                        mvMapLib.mvImageLayer ll = Program.mainFrm1.axMapLIb1.getImageLayer(rInfo.RastrPath);
                        if (ll != null)
                        {
                            if (ll.Visible)
                            {
                                var node = GetTreeNodeRastr(rInfo, true);
                                treeView1.Nodes.Add(node);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // Записать в лог
                    }
                }
            }
        }
        private bool getLayerIsSelectable(String layerName)
        {
            mvMapLib.mvLayer layer = Program.mainFrm1.axMapLIb1.getLayer(layerName);
            if (layer != null)
            {
                return layer.selectable;
            }
            else
            {
                return false;
            }
        }

        public TreeNode GetTreeNodeGroup(GroupInfo group)
        {
            int imageIndex = 2;

            return new TreeNode(group.NameGroup)
                        {
                            Name = group.NameGroup,
                            ImageIndex = imageIndex,
                            SelectedImageIndex = imageIndex,
                            Tag = group,
                            Checked = false
                        };


        }
        public TreeNode GetTreeNodePg(tablesInfo table, bool checkedNode)
        {
            //      type:
            //      1 point
            //      2 line 
            //      3 polygon

            int imageIndex = 0;
            switch (classesOfMetods.GetIntGeomType(table.GeomType_GC))
            {
                case 1: // point
                    {
                        if (classesOfMetods.getWriteTable(table.idTable))
                        {
                            imageIndex = 30;
                        }
                        else
                        {
                            imageIndex = 27;
                        }
                        break;
                    }
                case 2: // line
                    {
                        if (classesOfMetods.getWriteTable(table.idTable))
                        {
                            imageIndex = 29;
                        }
                        else
                        {
                            imageIndex = 26;
                        }
                        break;
                    }
                case 3: // polygon
                    {
                        if (classesOfMetods.getWriteTable(table.idTable))
                        {
                            imageIndex = 31;
                        }
                        else
                        {
                            imageIndex = 28;
                        }
                        break;
                    }
                default:
                    {
                        throw new Exception(String.Format("Таблица \"{0}\" имеет недопустимы тип геометрии!", table.nameMap));
                    }
            }

            var node = new TreeNode(table.nameMap)
                        {
                            Name = table.nameMap,
                            ImageIndex = imageIndex,
                            SelectedImageIndex = imageIndex,
                            Tag = table,
                            Checked = checkedNode
                        };

            return node;
        }
        public TreeNode GetTreeNodeRastr(RastrInfo rInfo, bool checkedNode)
        {
            int imageIndex = 33;

            return new TreeNode(rInfo.RastrName)
                        {
                            Name = rInfo.RastrName,
                            ImageIndex = imageIndex,
                            SelectedImageIndex = imageIndex,
                            Tag = rInfo,
                            Checked = checkedNode
                        };
        }
        public TreeNode GetTreeNodeVMP(AbsM.ILayerM layer, bool checkedNode)
        {
            int imageIndex = 32;

            return new TreeNode(layer.Text)
            {
                Name = layer.Name,
                ImageIndex = imageIndex,
                SelectedImageIndex = imageIndex,
                Tag = layer,
                Checked = checkedNode
            };
        }
        public TreeNode GetTreeNodeCosmetic(AbsM.ILayerM layer, bool checkedNode)
        {
            int imageIndex = 32;

            return new TreeNode(layer.Text)
            {
                Name = layer.Name,
                ImageIndex = imageIndex,
                SelectedImageIndex = imageIndex,
                Tag = layer,
                Checked = checkedNode
            };
        }
        public TreeNode GetTreeNodeCosmetic(CosM.CosmeticTableBaseM table, bool checkedNode)
        {
            int imageIndex = 32;

            return new TreeNode(table.Text)
            {
                Name = table.Name,
                ImageIndex = imageIndex,
                SelectedImageIndex = imageIndex,
                Tag = table,
                Checked = checkedNode
            };
        }
        public TreeNode GetTreeNodeStyle(StylesM style)
        {
            return new TreeNode(style.Name)
            {
                Name = style.Name,
                Tag = style,
                Checked = true
            };
        }

        public void SetVisibleDefault()
        {
            SqlWork sqlCmd = new SqlWork();
            if (Program.WorkSets.CurrentWorkSet.IsDefault)
                sqlCmd.sql = string.Format(@"SELECT ti.id FROM {0}.table_info ti, {0}.table_right tr, {0}.user_db ub 
WHERE 
ub.login like '{1}' AND
tr.id_user = ub.id AND
ti.id = tr.id_table AND
ti.hidden = false AND
tr.read_data = true AND
ti.default_visibl = true 
ORDER BY order_num desc;", Program.scheme, Program.user_info.loginUser);
            else
                sqlCmd.sql = string.Format(@"SELECT id_table FROM sys_scheme.table_order_set WHERE id_set = " + Program.WorkSets.CurrentWorkSet.Id + " ORDER BY order_num desc;");

            sqlCmd.Execute(false);
            List<int> idList = new List<int>();
            while (sqlCmd.CanRead())
            {
                idList.Add(sqlCmd.GetInt32(0));
            }
            sqlCmd.Close();

            foreach (int id in idList)
            {
                tablesInfo ti = classesOfMetods.getTableInfo(id);
                if (ti == null || ti.hidden)
                {
                    continue;
                }
                mvMapLib.mvLayer ll =
                        Program.mainFrm1.axMapLIb1.getLayer(Program.RelationVisbleBdUser.GetNameInBd(ti.idTable));
                if (ll != null)
                {
                    ll.Visible = true;
                }
                else
                {
                    Program.mainFrm1.layersManager1.loadLayerFromSource(ti.idTable);
                    ll = Program.mainFrm1.axMapLIb1.getLayer(Program.RelationVisbleBdUser.GetNameInBd(ti.idTable));
                    if (ll != null)
                    {
                        ll.Visible = true;
                    }
                }
                Program.mainFrm1.axMapLIb1.mapRepaint();
            }
        }
        public void SetPodlojkaLayersVisible()
        {
            try
            {
                TreeNode parentNode = null;
                TreeNode[] tempNodes = treeView1.Nodes.Find(Rekod.Properties.Resources.LIV_Vmp_layer, false);
                Debug.WriteLine("tempNodes.Count() = " + tempNodes.Count());
                foreach (TreeNode tempTn in tempNodes)
                {
                    Debug.WriteLineIf(tempTn.Tag is GroupInfo, "tempTn.Tag is GroupInfo");
                    if (tempTn.Tag is GroupInfo && ((GroupInfo)(tempTn.Tag)).Id == -1)
                    {
                        parentNode = tempTn;
                        break;
                    }
                }

                if (Program.TablesManager != null)
                {
                    foreach (AbsM.ILayerM layer in Program.TablesManager.VMPReposotory.Tables)
                    {
                        Debug.WriteLine("1layer = " + layer.Name + "parentNode = " + parentNode);
                        var node = GetTreeNodeVMP(layer, Program.mainFrm1.axMapLIb1.getLayer(layer.Name).Visible);
                        parentNode.Nodes.Add(node);
                        Debug.WriteLine("2layer = " + layer.Name);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format("Exception: \nMessage: {0}\nSource = {1}", ex.Message, ex.Source));
            }
        }
        public void SetRastrLayersVisible()
        {
            TreeNode parentNode = null;
            TreeNode[] tempNodes = treeView1.Nodes.Find(Rekod.Properties.Resources.LIV_Rastr_layer, false);
            foreach (TreeNode tempTn in tempNodes)
            {
                if (tempTn.Tag is GroupInfo && ((GroupInfo)(tempTn.Tag)).Id == -2)
                {
                    parentNode = tempTn;
                    break;
                }
            }
            layersManagerFRMRastr rastManager = new layersManagerFRMRastr();
            GroupInfo gti = GroupInfoList.Find(w => w.Id == -2);
            foreach (RastrInfo rInfo in rastManager.GetRastrLayers())
            {
                TreeNode node = null;
                try
                {
                    var ll = Program.mainFrm1.axMapLIb1.getImageLayer(rInfo.RastrPath);

                    bool visable = (ll != null && ll.Visible);
                    node = GetTreeNodeRastr(rInfo, visable);

                }
                catch (Exception)
                {
                    node = GetTreeNodeRastr(rInfo, false);
                }
                parentNode.Nodes.Add(node);
            }
        }
        private void SetCosmeticLayersVisible()
        {
            if (Program.TablesManager == null)
            {
                return;
            }
            TreeNode parentNode = null;
            TreeNode[] tempNodes = treeView1.Nodes.Find(Rekod.Properties.Resources.LocSourceCosmetic, false);
            foreach (TreeNode tempTn in tempNodes)
            {
                if (tempTn.Tag is GroupInfo && ((GroupInfo)(tempTn.Tag)).Id == -4)
                {
                    parentNode = tempTn;
                    break;
                }
            }
            foreach (CosM.CosmeticTableBaseM table in Program.TablesManager.CosmeticRepository.Tables)
            {
                TreeNode node = null;
                try
                {
                    var ll = Program.mainFrm1.axMapLIb1.getLayer(table.Name);

                    bool visable = (ll != null && ll.Visible);
                    node = GetTreeNodeCosmetic(table, visable);

                }
                catch (Exception)
                {
                    node = GetTreeNodeCosmetic(table, false);
                }
                parentNode.Nodes.Add(node);
            }
        }
        private void SetGroupsExpanded()
        {
            foreach (TreeNode tn in treeView1.Nodes)
            {
                if (tn.Tag is GroupInfo)
                {
                    GroupInfo gi = (GroupInfo)(tn.Tag);
                    if (GroupExpanded.ContainsKey(gi.Id))
                    {
                        if (GroupExpanded[gi.Id])
                        {
                            tn.Expand();
                        }
                        else
                        {
                            tn.Collapse();
                        }
                    }
                }
            }
        }
        private void SetAllExpanded()
        {
            foreach (TreeNode tn in treeView1.Nodes)
            {
                if (tn.Tag is GroupInfo)
                {
                    GroupInfo gi = (GroupInfo)(tn.Tag);
                    if (AllExpanded.ContainsKey(gi.Id))
                    {
                        if (AllExpanded[gi.Id])
                        {
                            tn.Expand();
                        }
                        else
                        {
                            tn.Collapse();
                        }
                    }
                }
            }
        }
        private void SetVisibleAllLocalLayer(bool isChecked)
        {
            if (Program.SettingsXML.LocalParameters.TurnOffVMPWhenRastr)
            {
                bool? vmpsVisible = null;
                bool lastRastrIsOff = Program.mainFrm1.axMapLIb1.ImageLayerCount == 0 && !isChecked;
                if (lastRastrIsOff)
                {
                    vmpsVisible = true;
                }
                bool firstRastrIsOn = Program.mainFrm1.axMapLIb1.ImageLayerCount == 1 && isChecked;
                if (firstRastrIsOn)
                {
                    vmpsVisible = false;
                }

                if (vmpsVisible != null)
                {
                    foreach (TreeNode tn in treeView1.Nodes)
                    {
                        foreach (TreeNode tnn in tn.Nodes)
                        {
                            if (tnn.Tag is VMPM.VMPTableBaseModel)
                            {
                                tnn.Checked = vmpsVisible.Value;
                                SetCheckedVisible(tnn, vmpsVisible.Value);
                            }
                        }
                    }
                }
            }
        }
        void SetCheckedVisible(TreeNode node, bool isChecked)
        {
            this.OnInvalidated(new InvalidateEventArgs(new Rectangle()));

            if (node.Tag is tablesInfo)
            {
                #region node.Tag is tablesInfo
                String nameInBd = Program.RelationVisbleBdUser.GetNameInBd(((tablesInfo)(node.Tag)).idTable);
                mvMapLib.mvLayer ll =
                       Program.mainFrm1.axMapLIb1.getLayer(nameInBd);
                if (isChecked)
                {
                    if (ll != null)
                    {
                        ll.Visible = isChecked;
                    }
                    else
                    {
                        cti.ThreadProgress.ShowWait();
                        try
                        {
                            Program.mainFrm1.layersManager1.loadLayerFromSource(((tablesInfo)(node.Tag)).idTable);
                            nameInBd = Program.RelationVisbleBdUser.GetNameInBd(((tablesInfo)(node.Tag)).idTable);
                            ll =
                                Program.mainFrm1.axMapLIb1.getLayer(nameInBd);
                            if (ll != null)
                            {
                                ll.Visible = isChecked;
                            }
                            Program.mainFrm1.axMapLIb1.mapRepaint();

                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(Rekod.Properties.Resources.LIV_ErrorLoadLayer + ":\n" + ex.Message);
                        }
                        finally
                        {
                            cti.ThreadProgress.Close();
                        }
                    }

                }
                else if (ll != null)
                {
                    ll.deleteLayer();
                    Program.mainFrm1.axMapLIb1.mapRepaint();
                }
                ll = Program.mainFrm1.axMapLIb1.getLayer(
                           Program.RelationVisbleBdUser.GetNameInBd(((tablesInfo)(node.Tag)).idTable));
                if ((ll == null) == !isChecked)
                {
                    TreeNode[] coll = treeView1.Nodes.Find(((tablesInfo)(node.Tag)).nameMap, true);
                    for (int i = 0; i < coll.Length; i++)
                    {
                        if (coll[i].Tag is tablesInfo)
                        {
                            tablesInfo cur = (tablesInfo)(coll[i].Tag);
                            tablesInfo ti = (tablesInfo)(node.Tag);
                            if (ti.idTable == cur.idTable)
                            {
                                coll[i].Checked = isChecked;
                            }
                        }
                    }
                    //if (!isChecked && _prevNode != null && _prevNode.Tag != null && node.Tag.Equals(_prevNode.Tag))
                    //{
                    //    node.BackColor = treeView1.BackColor;
                    //    for (int i = 0; i < coll.Length; i++)
                    //    {
                    //        if (coll[i].Tag is tablesInfo)
                    //        {
                    //            coll[i].BackColor = treeView1.BackColor;
                    //        }
                    //    }
                    //    _prevNode = null;
                    //    _prevSelectedLayerName = "";
                    //}
                }
                //EditableIdLayer = ((tablesInfo)(node.Tag)).idTable;
                #endregion node.Tag is tablesInfo
            }
            else if (node.Tag is AbsM.ILayerM)
            {
                #region node.Tag is AbsM.ILayerM

                var table = node.Tag as AbsM.ILayerM;
                table.IsVisible = isChecked;

                #endregion node.Tag is AbsM.ILayerM
            }
            else if (node.Tag is RastrInfo)
            {
                #region node.Tag is RastrInfo
                RastrInfo rInfo = (RastrInfo)(node.Tag);
                if (isChecked)
                {
                    cti.ThreadProgress.ShowWait();
                    mvMapLib.mvImageLayer ll = Program.mainFrm1.axMapLIb1.getImageLayer(rInfo.RastrPath);
                    if (ll == null)
                    {
                        bool isLoad = false;
                        if (rInfo.IsExternal)
                        {
                            bool exists = false;
                            try
                            {
                                if (Path.GetExtension(rInfo.RastrPath).TrimEnd().ToLower() == ".xml")
                                {
                                    XmlReader xml;
                                    if (rInfo.Content != null)
                                    {
                                        var xmlString = (string)rInfo.Content;
                                        xml = XmlReader.Create(new System.IO.StringReader(xmlString));
                                    }
                                    else
                                        xml = GetXmlReader(rInfo.RastrPath);

                                    var rastrXML = (IRastrXml)this.ConverXmlToObject<WMSRastrM>(xml)
                                                ?? (IRastrXml)this.ConverXmlToObject<TMSRastrM>(xml)
                                                ?? (IRastrXml)this.ConverXmlToObject<TWMSRastrM>(xml);
                                    if (rastrXML != null)
                                    {
                                        if (rastrXML.IsValid())
                                        {
                                            switch (rastrXML.Type)
                                            {
                                                case ERastrXmlType.WMS:
                                                    {
                                                        var wms_rastr = rastrXML as WMSRastrM;
                                                        string layerNameInServer = this.GetWmsLayersFromXml(wms_rastr);
                                                        string serverURL = wms_rastr.ServerUrl;
                                                        string layerInSystem = rInfo.RastrPath;
                                                        string styles = this.GetWmsLayerStylesFromXml(wms_rastr);
                                                        exists = Program.mainFrm1.axMapLIb1.AddImageLayerWMS(layerNameInServer, serverURL, layerInSystem, styles);
                                                    }
                                                    break;
                                                case ERastrXmlType.TMS:
                                                    {
                                                        var tms_rastr = rastrXML as TMSRastrM;
                                                        mvBbox bbox = new mvBbox();
                                                        bbox.a.x = tms_rastr.TMSExtent.a_x;
                                                        bbox.a.y = tms_rastr.TMSExtent.a_y;
                                                        bbox.b.x = tms_rastr.TMSExtent.b_x;
                                                        bbox.b.y = tms_rastr.TMSExtent.b_y;

                                                        exists = Program.mainFrm1.axMapLIb1.AddImageLayerTMS(
                                                            tms_rastr.Url,
                                                            tms_rastr.LayerName,
                                                            rInfo.RastrPath,
                                                            tms_rastr.MinZoom,
                                                            tms_rastr.MaxZoom,
                                                            bbox,
                                                            tms_rastr.Proj,
                                                            tms_rastr.TileSize,
                                                            this.CheckForlder(tms_rastr.CacheFolder),
                                                            Program.proj4Map);
                                                    }
                                                    break;
                                                case ERastrXmlType.TWMS:
                                                    {
                                                        var twms_rastr = rastrXML as TWMSRastrM;
                                                        mvBbox bbox = new mvBbox();
                                                        bbox.a.x = twms_rastr.TWMSExtent.a_x;
                                                        bbox.a.y = twms_rastr.TWMSExtent.a_y;
                                                        bbox.b.x = twms_rastr.TWMSExtent.b_x;
                                                        bbox.b.y = twms_rastr.TWMSExtent.b_y;

                                                        exists = Program.mainFrm1.axMapLIb1.AddImageLayerTWMS(
                                                                string.Join(",", twms_rastr.Layers.Select(f => f.LayerName).ToArray()),
                                                                rInfo.RastrPath,
                                                                twms_rastr.Url,
                                                                twms_rastr.ZoomCount,
                                                                Convert.ToInt32(Program.srid),
                                                                bbox,
                                                                string.Join(",", twms_rastr.Layers.Select(f => f.StyleName).ToArray()),
                                                                twms_rastr.TileSize,
                                                                this.CheckForlder(twms_rastr.CacheFolder));
                                                    }
                                                    break;
                                                default:
                                                    {
                                                        Program.mainFrm1.axMapLIb1.LoadExternalImageLayer(rInfo.RastrPath, true);
                                                    }
                                                    break;
                                            }
                                        }
                                        else
                                        {
                                            throw new Exception("Не валидный ресурс файл");
                                        }
                                    }
                                    else
                                    {
                                        exists = Program.mainFrm1.axMapLIb1.LoadExternalImageLayer(rInfo.RastrPath, true);

                                    }
                                }
                                else if (Path.GetExtension(rInfo.RastrPath).TrimEnd().ToLower() == ".gxml")
                                {
                                    Program.mainFrm1.axMapLIb1.LoadExternalImageLayer(rInfo.RastrPath, false);
                                }
                                else if (Path.GetExtension(rInfo.RastrPath).TrimEnd().ToLower() == ".rwms")
                                {
                                    XmlReader xml;
                                    xml = GetXmlReader(rInfo.RastrPath);
                                    var rastrXML = this.ConverXmlToObject<WMSRastrM>(xml);
                                    if (rastrXML != null)
                                    {
                                        if (rastrXML.IsValid())
                                        {
                                            string layerNameInServer = this.GetWmsLayersFromXml(rastrXML);
                                            string serverURL = rastrXML.ServerUrl;
                                            string layerInSystem = rInfo.RastrPath;
                                            string styles = this.GetWmsLayerStylesFromXml(rastrXML);
                                            exists = Program.mainFrm1.axMapLIb1.AddImageLayerWMS(layerNameInServer, serverURL, layerInSystem, styles);
                                        }
                                    }
                                }
                                else if (Path.GetExtension(rInfo.RastrPath).TrimEnd().ToLower() == ".rtwms")
                                {
                                    XmlReader xml;
                                    xml = GetXmlReader(rInfo.RastrPath);
                                    var rastrXML = this.ConverXmlToObject<TWMSRastrM>(xml);
                                    if (rastrXML != null)
                                    {
                                        if (rastrXML.IsValid())
                                        {
                                            mvBbox bbox = new mvBbox();
                                            bbox.a.x = rastrXML.TWMSExtent.a_x;
                                            bbox.a.y = rastrXML.TWMSExtent.a_y;
                                            bbox.b.x = rastrXML.TWMSExtent.b_x;
                                            bbox.b.y = rastrXML.TWMSExtent.b_y;

                                            exists = Program.mainFrm1.axMapLIb1.AddImageLayerTWMS(
                                                    string.Join(",", rastrXML.Layers.Select(f => f.LayerName).ToArray()),
                                                    rInfo.RastrPath,
                                                    rastrXML.Url,
                                                    rastrXML.ZoomCount,
                                                    Convert.ToInt32(Program.srid),
                                                    bbox,
                                                    string.Join(",", rastrXML.Layers.Select(f => f.StyleName).ToArray()),
                                                    rastrXML.TileSize,
                                                    this.CheckForlder(rastrXML.CacheFolder));
                                        }
                                    }
                                }
                                else if (Path.GetExtension(rInfo.RastrPath).TrimEnd().ToLower() == ".rtms")
                                {
                                    XmlReader xml;
                                    xml = GetXmlReader(rInfo.RastrPath);
                                    var rastrXML = this.ConverXmlToObject<TMSRastrM>(xml);
                                    if (rastrXML != null)
                                    {
                                        if (rastrXML.IsValid())
                                        {
                                            mvBbox bbox = new mvBbox();
                                            bbox.a.x = rastrXML.TMSExtent.a_x;
                                            bbox.a.y = rastrXML.TMSExtent.a_y;
                                            bbox.b.x = rastrXML.TMSExtent.b_x;
                                            bbox.b.y = rastrXML.TMSExtent.b_y;

                                            exists = Program.mainFrm1.axMapLIb1.AddImageLayerTMS(
                                                rastrXML.Url,
                                                rastrXML.LayerName,
                                                rInfo.RastrPath,
                                                rastrXML.MinZoom,
                                                rastrXML.MaxZoom,
                                                bbox,
                                                rastrXML.Proj,
                                                rastrXML.TileSize,
                                                this.CheckForlder(rastrXML.CacheFolder),
                                                Program.proj4Map);
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Classes.workLogFile.writeLogFile(ex, false, true);
                            }
                            isLoad = exists;
                        }
                        else
                        {
                            if (Path.GetExtension(rInfo.RastrPath).TrimEnd().ToLower() == ".jpg" ||
                                Path.GetExtension(rInfo.RastrPath).TrimEnd().ToLower() == ".jpeg")
                            {
                                try
                                {
                                    Program.mainFrm1.axMapLIb1.LoadImageLayer(rInfo.RastrPath);
                                }
                                catch (Exception ex)
                                {
                                    try
                                    {
                                        Classes.workLogFile.writeLogFile(ex, false, true);
                                        Program.mainFrm1.axMapLIb1.LoadLayer(rInfo.RastrPath, true);

                                    }
                                    catch (Exception ex2)
                                    {
                                        Classes.workLogFile.writeLogFile(ex2, false, true);
                                    }
                                }
                            }
                            else
                            {
                                switch (rInfo.MethodUse)
                                {
                                    case 0:
                                        Program.mainFrm1.axMapLIb1.LoadLayer(rInfo.RastrPath, true);
                                        break;
                                    case 1:
                                        Program.mainFrm1.axMapLIb1.LoadExternalImageLayer(rInfo.RastrPath, rInfo.BuildPyramids);
                                        break;
                                    default:
                                        Program.mainFrm1.axMapLIb1.LoadLayer(rInfo.RastrPath, true);
                                        break;
                                }
                            }
                        }

                        ll = Program.mainFrm1.axMapLIb1.getImageLayer(rInfo.RastrPath);
                        if (ll != null)
                        {
                            if (!rInfo.IsExternal)
                            {
                                ll.bgcolor = (uint)Program.rasterBgMap;
                            }
                            ll.Visible = isChecked;
                            ll.usebounds = rInfo.UseBounds;
                            ll.MinScale = (uint)rInfo.MinScale;
                            ll.MaxScale = (uint)rInfo.MaxScale;

                            // Перемещаем слой в самый верх в списке видимых растровых слоев
                            if (ll.Visible)
                            {
                                for (int i = 0; i < Program.mainFrm1.axMapLIb1.ImageLayerCount; i++)
                                {
                                    ll.MoveUp();
                                }
                            }

                            SetVisibleAllLocalLayer(isChecked);
                        }
                        else
                        {

                            if (isChecked)
                            {
                                cti.ThreadProgress.Close();
                                MessageBox.Show(Rekod.Properties.Resources.LIV_ErrorSetVisible);
                                node.Checked = false;
                            }
                        }
                    }
                    else
                    {
                        ll.Visible = true;
                    }
                    cti.ThreadProgress.Close();
                }
                else
                {
                    cti.ThreadProgress.ShowWait();
                    try
                    {
                        mvMapLib.mvImageLayer ll;
                        bool isExternal = rInfo.IsExternal;
                        if (isExternal || rInfo.MethodUse == 1)
                        {
                            mvMapLib.mvExternalImageLayer ll2 = Program.mainFrm1.axMapLIb1.GetExternalImageLayer(rInfo.RastrPath);
                            if (ll2 != null)
                                Program.mainFrm1.axMapLIb1.DeleteExternalImageLayer(rInfo.RastrPath);
                        }
                        else
                        {
                            ll = Program.mainFrm1.axMapLIb1.getImageLayer(rInfo.RastrPath);
                            if (ll != null)
                                Program.mainFrm1.axMapLIb1.deleteImageLayer(ll);
                        }

                        ll = Program.mainFrm1.axMapLIb1.getImageLayer(rInfo.RastrPath);
                        if (ll != null)
                        {
                            Program.mainFrm1.axMapLIb1.deleteImageLayer(ll);
                        }
                        SetVisibleAllLocalLayer(isChecked);
                    }
                    catch (Exception ex)
                    {
                        Classes.workLogFile.writeLogFile(ex, false, true);
                    }
                    finally
                    {
                        cti.ThreadProgress.Close();
                    }
                }
                #endregion node.Tag is RastrInfo
            }
            if (_viewType == enViewType.visible && isChecked)
            {
                RefreshLayers();
            }
            Program.mainFrm1.axMapLIb1.mapRepaint();
            if (_viewType == enViewType.visible)
            {
                defineToolBoxVisibility();
            }
            UpdateListLayersIsView();
        }

        private string CheckForlder(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            return folderPath;
        }

        /// <summary>
        /// Считывает XML файл
        /// </summary>
        /// <param name="filepath">Пусть к файлу</param>
        /// <returns></returns>
        /// <exception cref="Exception"/>
        private XmlReader GetXmlReader(string filepath)
        {
            using (FileStream fs = new FileStream(filepath, FileMode.Open))
            {
                var xml = XElement.Load(fs);
                return xml.CreateReader();
            }
        }
        private T ConverXmlToObject<T>(XmlReader xml)
        {
            var serializer = new XmlSerializer(typeof(T));
            if (serializer.CanDeserialize(xml))
                return (T)serializer.Deserialize(xml);
            else
                return default(T);

        }

        private string GetWmsLayerStylesFromXml(WMSRastrM wms_rastr)
        {
            string temp = "";
            foreach (var item in wms_rastr.Layers)
            {
                if (item.StyleName != null && !string.IsNullOrEmpty(item.StyleName))
                {
                    temp += item.StyleName + ",";
                }
                else
                {
                    temp += ",";
                }
            }
            temp = temp.Substring(0, temp.Length - 1);
            return temp;
        }

        private string GetWmsLayersFromXml(WMSRastrM wms_rastr)
        {
            string temp = "";
            foreach (var item in wms_rastr.Layers)
            {
                temp += item.LayerName + ",";
            }
            temp = temp.Substring(0, temp.Length - 1);
            return temp;
        }

        public static void SaveXML_Test(string filepath)
        {
            WMSRastrM temp = new WMSRastrM();
            temp.ServerUrl = "http://okr.rekod.ru/geoserver";
            temp.Layers = new ObservableCollection<WMSLayerM>();
            temp.Layers.Add(new WMSLayerM() { LayerName = "cku:mo_np_a", StyleName = "for_zemli_pol11_a_style" });
            temp.Layers.Add(new WMSLayerM() { LayerName = "cku:bu_ga_ga", StyleName = "" });

            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(WMSRastrM));
                TextWriter writer = new StreamWriter(filepath, false);
                serializer.Serialize(writer, temp);
                writer.Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error in method ConfigSaveInXml: " + ex.Message);
            }
        }
        void SetCheckedSelectable(TreeNode node, bool isChecked)
        {
            if (node.Tag is tablesInfo)
            {
                mvMapLib.mvLayer ll =
                       Program.mainFrm1.axMapLIb1.getLayer(
                           Program.RelationVisbleBdUser.GetNameInBd(((tablesInfo)(node.Tag)).idTable));
                if (ll != null)
                {
                    ll.selectable = isChecked;
                }
            }
            else if (node.Tag is VMPM.VMPTableBaseModel)
            {
                var table = node.Tag as VMPM.VMPTableBaseModel;
                table.IsSelectable = isChecked;
            }
            else if (node.Tag is CosM.CosmeticTableBaseM)
            {
                CosM.CosmeticTableBaseM cosmTable = node.Tag as CosM.CosmeticTableBaseM;
                mvLayer layer = Program.mainFrm1.axMapLIb1.getLayer(cosmTable.NameMap);
                if (layer != null)
                {
                    layer.selectable = isChecked;
                }
            }
            Program.mainFrm1.axMapLIb1.mapRepaint();
        }
        private void treeView1_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (e.Action == TreeViewAction.Unknown) return;
            if (_viewType != enViewType.selectable)
            {
                SetCheckedVisible(e.Node, e.Node.Checked);

                String layerName = null;
                if (e.Node.Tag is tablesInfo)
                {
                    layerName = Program.RelationVisbleBdUser.GetNameInBd((e.Node.Tag as tablesInfo).idTable);
                }
                else if (e.Node.Tag is AbsM.ILayerM)
                {
                    layerName = e.Node.Name;
                }
                mvMapLib.mvLayer layer = Program.mainFrm1.axMapLIb1.getLayer(layerName);
                if (layer != null && !layer.selectable)
                {
                    foreach (AbsM.ILayerM item in Program.TablesManager.VMPReposotory.Tables)
                    {
                        item.IsSelectable = false;
                    }
                    foreach (AbsM.ILayerM item in Program.TablesManager.CosmeticRepository.Tables)
                    {
                        item.IsSelectable = false;
                    }

                    for (int i = 0; i < Program.mainFrm1.axMapLIb1.LayersCount; i++)
                    {
                        layer = Program.mainFrm1.axMapLIb1.getLayerByNum(i);
                        if (layer.selectable)
                        {
                            layer.selectable = false;
                        }
                    }
                }

                layerName = null;
                if (e.Node.Tag is tablesInfo)
                {
                    layerName = Program.RelationVisbleBdUser.GetNameInBd((e.Node.Tag as tablesInfo).idTable);
                    layer = Program.mainFrm1.axMapLIb1.getLayer(layerName);
                    if (layer != null)
                    {
                        layer.selectable = true;
                        layer.editable = true;

                        var tableInfo = e.Node.Tag as tablesInfo;
                        if (tableInfo != null)
                        {
                            EditableLayer = Program.repository.FindTable(tableInfo.idTable) as AbsM.ILayerM;
                            EditableIdLayer = tableInfo.idTable;
                        }
                    }
                    else
                    {
                        EditableLayer = null;
                    }
                }
                else if (e.Node.Tag is AbsM.ILayerM)
                {
                    var layerCos = e.Node.Tag as AbsM.ILayerM;
                    layerCos.IsSelectable = true;
                    if (layerCos.IsReadOnly == false)
                        layerCos.IsEditable = true;
                    else
                        layerCos.IsEditable = false;
                }

            }
            else
            {
                TreeNode _tempClickedNode = _clickedNode;
                SetCheckedSelectable(e.Node, e.Node.Checked);
                _clickedNode = null;
                treeView1.Visible = false;
                addNewLayersVisible(true);
                sortInMapOrder();
                setBackColorSelectableLayers();
                treeView1.Visible = true;
                _clickedNode = _tempClickedNode;

                EditableLayer = null;
            }
            Program.mainFrm1.bManager.SetButtonsState();
            UpdateListLayersIsView();
        }


        /// <summary>
        /// Поиск слоя и попытка сделать его видимым
        /// </summary>
        /// <param name="id">ID таблицы</param>
        public void SetLayerVisible(int id)
        {
            var prevViewType = _viewType;
            tabControl1.SelectedIndex = (int)enViewType.all;
            try
            {
                foreach (TreeNode tn in treeView1.Nodes)
                {
                    String layerName;
                    if (tn.Tag is tablesInfo)
                    {
                        tablesInfo ti = (tablesInfo)(tn.Tag);
                        layerName = Program.RelationVisbleBdUser.GetNameInBd(ti.idTable);
                        if (ti.idTable == id)
                        {
                            tn.Checked = true;
                            treeView1_AfterCheck(treeView1, new TreeViewEventArgs(tn, TreeViewAction.ByMouse));
                            return;
                        }
                    }
                    else if (tn.Tag is GroupInfo)
                    {
                        foreach (TreeNode tnGroup in tn.Nodes)
                        {
                            if (tnGroup.Tag is tablesInfo)
                            {
                                tablesInfo ti = (tablesInfo)(tnGroup.Tag);
                                layerName = Program.RelationVisbleBdUser.GetNameInBd(ti.idTable);
                                if (ti.idTable == id)
                                {
                                    tnGroup.Checked = true;
                                    treeView1_AfterCheck(treeView1, new TreeViewEventArgs(tnGroup, TreeViewAction.ByMouse));
                                    return;
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                tabControl1.SelectedIndex = (int)prevViewType;
            }
        }

        private void treeView1_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Tag is tablesInfo)
            {
                tablesInfo ti = (tablesInfo)(e.Node.Tag);
                try
                {
                    Program.mainFrm1.layersManager1.openTableGrid(ti.idTable);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(Rekod.Properties.Resources.LIV_NotOpenTable + ":\n" + ex.Message);
                }
            }
            else if (e.Node.Tag is AbsM.ITableBaseM)
            {
                var table = e.Node.Tag as AbsM.ITableBaseM;
                try
                {
                    table.Source.OpenTable(table);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(Rekod.Properties.Resources.LIV_ErrorOpenTable + ": " + ex.Message);
                }
            }
            else if (e.Node.Tag is StylesM)
            {
                var style = (StylesM)e.Node.Tag;
                if (style.Tag != null)
                {
                    var ti = (tablesInfo)(style.Tag);
                    Program.mainFrm1.layersManager1.openTableGrid(ti.idTable);
                }
            }
        }
        private void treeView1_MouseDown(object sender, MouseEventArgs e)
        {
            var tvLayer = sender as TreeView;
            var prevClickedNode = _clickedNode;
            if (_viewType != enViewType.selectable)
            {
                _clickedNode = tvLayer.GetNodeAt(e.X, e.Y);
                if (_clickedNode != null && _clickedNode.Tag is StylesM)
                {
                    _clickedNode = _clickedNode.Parent;
                    return;
                }

                tvLayer.SelectedNode = _clickedNode;

                if (e.Button == MouseButtons.Left)
                {
                    if (_clickedNode != null && _clickedNode.Tag is GroupInfo)
                    {
                        GroupInfo gi = (GroupInfo)(_clickedNode.Tag);
                        _clickedNode.Toggle();
                        _clickedNode = prevClickedNode;
                    }
                }
                else if (e.Button == MouseButtons.Right)
                {
                    if (_clickedNode != null)
                    {
                        bool hasRight = false;
                        if (Program.WorkSets.CurrentWorkSet.Id != -1)
                            hasRight = Program.WorkSets.CurrentWorkSet.IdUser == Program.id_user || Program.user_info.admin;
                        ToolStripMenuItemSetExtent.Visible = _clickedNode.Checked;
                        if (_clickedNode.Tag is tablesInfo)
                        {
                            tablesInfo ti = _clickedNode.Tag as tablesInfo;
                            tsAddLineOnMap.Visible = false;
                            tsAddPointOnMap.Visible = false;
                            tsAddPolygonOnMap.Visible = false;
                            tsShowSettings.Visible = false;

                            switch (classesOfMetods.GetIntGeomType(ti.GeomType_GC))
                            {
                                case 1:
                                    {
                                        tsAddPointOnMap.Visible = true;
                                        break;
                                    }
                                case 2:
                                    {
                                        tsAddLineOnMap.Visible = true;
                                        break;
                                    }
                                case 3:
                                    {
                                        tsAddPolygonOnMap.Visible = true;
                                        break;
                                    }
                            }

                            addCoordToolStripMenuItem.Visible = true;
                            contextMenuStrip1.Show(tvLayer, new Point(e.X, e.Y));
                        }
                        else if (_clickedNode.Tag is VMPM.VMPTableBaseModel)
                        {
                            tsShowSettings.Visible = true;
                            addCoordToolStripMenuItem.Visible = false;
                            tsAddLineOnMap.Visible = false;
                            tsAddPointOnMap.Visible = false;
                            tsAddPolygonOnMap.Visible = false;
                            contextMenuStrip1.Show(tvLayer, new Point(e.X, e.Y));
                        }
                        else if (_clickedNode.Tag is CosM.CosmeticTableBaseM)
                        {
                            tsShowSettings.Visible = true;
                            addCoordToolStripMenuItem.Visible = false;
                            tsAddLineOnMap.Visible = true;
                            tsAddPointOnMap.Visible = true;
                            tsAddPolygonOnMap.Visible = true;
                            contextMenuStrip1.Show(tvLayer, new Point(e.X, e.Y));
                        }
                        else if (_clickedNode.Tag is RastrInfo)
                        {
                            var ras = _clickedNode.Tag as RastrInfo;
                            if (ras.Content == null)
                                tsShowSettings.Visible = true;
                            else
                                tsShowSettings.Visible = false;

                            addCoordToolStripMenuItem.Visible = false;
                            tsAddLineOnMap.Visible = false;
                            tsAddPointOnMap.Visible = false;
                            tsAddPolygonOnMap.Visible = false;
                            openTableToolStripMenuItem.Visible = false;

                            string path = ras.RastrPath.ToLower();
                            bool isgxml = path.EndsWith(".gxml");
                            bool isrwms = path.EndsWith(".rwms");
                            bool isrtms = path.EndsWith(".rtms");
                            bool isrtwms = path.EndsWith(".rtwms");
                            bool isxml = path.EndsWith(".xml");
                            ToolStripMenuItemSetExtent.Visible = _clickedNode.Checked && !isgxml && !isrwms && !isrtms && !isrtwms && !isxml && (ras.MethodUse != 1);

                            contextMenuStrip1.Show(tvLayer, new Point(e.X, e.Y));
                        }
                    }
                }
            }
            setBackColorSelectableLayers();
        }
        private void treeView1_AfterCollapseExpand(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Tag is GroupInfo)
            {
                switch (_viewType)
                {
                    case enViewType.groups:
                        {
                            GroupInfo gi = (GroupInfo)(e.Node.Tag);
                            if (!GroupExpanded.ContainsKey(gi.Id))
                            {
                                GroupExpanded.Add(gi.Id, e.Node.IsExpanded);
                            }
                            else
                            {
                                GroupExpanded[gi.Id] = e.Node.IsExpanded;
                            }
                        }
                        break;
                    case enViewType.all:
                        {
                            GroupInfo gi = (GroupInfo)(e.Node.Tag);
                            if (!AllExpanded.ContainsKey(gi.Id))
                            {
                                AllExpanded.Add(gi.Id, e.Node.IsExpanded);
                            }
                            else
                            {
                                AllExpanded[gi.Id] = e.Node.IsExpanded;
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
        }
        private void treeView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && _viewType == enViewType.visible)
            {
                PickLayerUp(e.KeyCode);
            }
        }
        private void treeView1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Control)
                Program.mainFrm1.axMapLIb1.mapRepaint();
        }
        public void PickLayerUp(Keys KeyCode)
        {
            List<String> keyLayers = new List<string>();
            for (int i = 0; i < Program.mainFrm1.axMapLIb1.LayersCount; i++)
            {
                mvLayer ll = Program.mainFrm1.axMapLIb1.getLayerByNum(i);
                try
                {
                    String layerName = ll.NAME;
                    if (ll != null && layerName != null)
                    {
                        keyLayers.Add(ll.NAME);
                    }
                }
                catch (Exception ex)
                {
                    // Записать в лог
                }
            }
            if (treeView1.SelectedNode != null)
            {
                TreeNode tn_Current = treeView1.SelectedNode;
                int index_current = treeView1.Nodes.IndexOf(tn_Current);
                TreeNode tn_Next = null;
                bool isUp;
                switch (KeyCode)
                {
                    case Keys.Up:
                        isUp = true;
                        break;
                    case Keys.Down:
                        isUp = false; ;
                        break;
                    default:
                        {
                            return;
                        }
                }
                tn_Next = GetNextNode(tn_Current, isUp: isUp);
                if (tn_Next == null)
                {
                    return;
                }
                if (SetOrderlayers(tn_Current, tn_Next))
                    SetOrderNode(tn_Current, isUp: isUp);

                Program.WinMain.TbbLayerUp.IsEnabled = (GetNextNode(tn_Current, true) != null);
                Program.WinMain.TbbLayerDown.IsEnabled = (GetNextNode(tn_Current, false) != null);


                //if (!(tn_Current.Tag is RastrInfo))
                //{
                //    Program.mainFrm1.tsLayerDown.Enabled = (GetNextNode(tn_Current, false) != null);
                //    Program.mainFrm1.tsLayerUp.Enabled = (GetNextNode(tn_Current, true) != null);
                //}
                //else
                //{
                //    Program.mainFrm1.tsLayerDown.Enabled = false;
                //    Program.mainFrm1.tsLayerUp.Enabled = false;
                //}
                UpdateListLayersIsView();
            }
        }
        /// <summary>
        /// Поучает следующий стоящий или предыдущий элемент дерева
        /// </summary>
        /// <param name="tn_Current">Текущий элемент TreeNode</param>
        /// <param name="isUp">Поднять или опустить</param>
        /// <returns>Возвращает TreeNode, если такого нет возвращает Null</returns>
        private TreeNode GetNextNode(TreeNode tn_Current, bool isUp)
        {
            if (tn_Current == null) return null;

            int index_current = treeView1.Nodes.IndexOf(tn_Current);
            bool isRastr = tn_Current.Tag is RastrInfo;

            int index_Next = index_current;
            if (((isUp) ? --index_Next : ++index_Next) >= 0 && index_Next < treeView1.Nodes.Count)
            {
                TreeNode tn_Next = treeView1.Nodes[index_Next];
                if (isRastr ^ tn_Next.Tag is RastrInfo)
                    return null;
                else
                    return tn_Next;
            }
            return null;
        }
        /// <summary>
        /// Сортирует список слоев в MVActiveX
        /// </summary>
        /// <param name="tn_Current">Текущий элемент TreeNode</param>
        /// <param name="tn_Next">Заменяемый элемент TreeNode</param>
        /// <returns>Возвращает результат сортировки</returns>
        private bool SetOrderlayers(TreeNode tn_Current, TreeNode tn_Next)
        {
            List<String> keyLayers = new List<string>();
            for (int i = 0; i < Program.mainFrm1.axMapLIb1.LayersCount; i++)
                try
                {
                    mvLayer ll = Program.mainFrm1.axMapLIb1.getLayerByNum(i);
                    if (ll != null && ll.NAME != null)
                        keyLayers.Add(ll.NAME);
                }
                catch (Exception ex)
                { }
            for (int i = 0; i < Program.mainFrm1.axMapLIb1.ImageLayerCount; i++)
                try
                {
                    mvImageLayer ll = Program.mainFrm1.axMapLIb1.getImageLayerByNum(i);
                    if (ll != null && ll.filename != null)
                        keyLayers.Add(ll.filename);
                }
                catch (Exception ex)
                { }

            String layerName_current;
            String layerName_Next;
            int i_current;
            int i_Next;
            if (tn_Current.Tag is RastrInfo || tn_Next.Tag is RastrInfo)
            {
                if (tn_Current.Tag is RastrInfo && tn_Next.Tag is RastrInfo)
                {
                    layerName_current = ((RastrInfo)tn_Current.Tag).RastrPath;
                    layerName_Next = ((RastrInfo)tn_Next.Tag).RastrPath;
                    mvImageLayer iL_current = Program.mainFrm1.axMapLIb1.getImageLayer(layerName_current);
                    mvImageLayer iL_next = Program.mainFrm1.axMapLIb1.getImageLayer(layerName_Next);
                    if (iL_current != null && iL_next != null)
                    {
                        i_current = keyLayers.LastIndexOf(((RastrInfo)tn_Current.Tag).RastrPath);
                        i_Next = keyLayers.LastIndexOf(((RastrInfo)tn_Next.Tag).RastrPath);

                        if (i_current < i_Next)
                        {
                            for (int i = i_Next; i > i_current; i--)
                                iL_next.MoveUp();
                            for (int i = i_current; i < i_Next - 1; i++)
                                iL_current.MoveDown();
                        }
                        else
                        {
                            for (int i = i_current; i > i_Next; i--)
                                iL_current.MoveUp();
                            for (int i = i_Next; i < i_current - 1; i++)
                                iL_next.MoveDown();
                        }
                        return true;
                    }
                    else
                        throw new Exception(Rekod.Properties.Resources.LIV_ErrorDataForSort);
                }
                else
                    throw new Exception(Rekod.Properties.Resources.LIV_ErrorDataForSort);
            }
            else
            {
                if (tn_Current.Tag is tablesInfo)
                    layerName_current = Program.RelationVisbleBdUser.GetNameInBd(((tablesInfo)tn_Current.Tag).idTable);
                else
                    layerName_current = tn_Current.Name;

                if (tn_Next.Tag is tablesInfo)
                    layerName_Next = Program.RelationVisbleBdUser.GetNameInBd(((tablesInfo)tn_Next.Tag).idTable);
                else
                    layerName_Next = tn_Next.Name;
                mvLayer ll_current = Program.mainFrm1.axMapLIb1.getLayer(layerName_current);
                mvLayer ll_next = Program.mainFrm1.axMapLIb1.getLayer(layerName_Next);
                if (ll_current != null && ll_next != null)
                {
                    i_current = keyLayers.LastIndexOf(ll_current.NAME);
                    i_Next = keyLayers.LastIndexOf(ll_next.NAME);

                    if (i_current < i_Next)
                    {
                        for (int i = i_Next; i > i_current; i--)
                            ll_next.MoveUp();
                        for (int i = i_current; i < i_Next - 1; i++)
                            ll_current.MoveDown();
                    }
                    else
                    {
                        for (int i = i_current; i > i_Next; i--)
                            ll_current.MoveUp();
                        for (int i = i_Next; i < i_current - 1; i++)
                            ll_next.MoveDown();
                    }
                    return true;

                }
            }
            return false;
        }
        /// <summary>
        /// Сортирует список слоев в TreeView
        /// </summary>
        /// <param name="tn_Current">Текущий элемент TreeNode</param>
        /// <param name="isUp">Поднять или опустить</param>
        private void SetOrderNode(TreeNode tn_Current, bool isUp)
        {
            int index_current = treeView1.Nodes.IndexOf(tn_Current);
            treeView1.BeginUpdate();
            treeView1.Nodes.Remove(tn_Current);
            treeView1.Nodes.Insert(((isUp) ? index_current - 1 : index_current + 1), tn_Current);
            treeView1.SelectedNode = tn_Current;
            treeView1.EndUpdate();
        }
        private void defineToolBoxVisibility()
        {
            if (_clickedNode != null)
            {
                if (_clickedNode.Tag is tablesInfo)
                {
                    makeToolBoxInvisible(true, _viewType == enViewType.visible);
                }
                else if (_clickedNode.Tag is AbsM.ILayerM)
                {
                    makeToolBoxInvisible(false, _viewType == enViewType.visible);
                }
                else if (_clickedNode.Tag is RastrInfo)
                {
                    makeToolBoxInvisible(false, _viewType == enViewType.visible);
                }
            }
        }
        private void makeToolBoxInvisible(bool visable, bool isOrderLayer = false)
        {
            tablesInfo ti = null;

            {
                if (_clickedNode != null && _clickedNode.Tag is tablesInfo)
                    ti = ((tablesInfo)_clickedNode.Tag);
                Program.mainFrm1.bManager.SetButtonsState();
                if (isOrderLayer == true)
                {
                    Program.WinMain.TbbLayerUp.IsEnabled = (GetNextNode(_clickedNode, true) != null) && _clickedNode.Checked;
                    Program.WinMain.TbbLayerDown.IsEnabled = (GetNextNode(_clickedNode, false) != null) && _clickedNode.Checked;
                }
                else
                {
                    Program.WinMain.TbbLayerUp.IsEnabled = false;
                    Program.WinMain.TbbLayerDown.IsEnabled = false;
                }
            }
        }
        private void setBackColorSelectableLayers()
        {
            foreach (TreeNode tn in treeView1.Nodes)
            {
                if (tn.Tag is GroupInfo)
                {
                    foreach (TreeNode stn in tn.Nodes)
                    {
                        if (stn.Tag is tablesInfo)
                        {
                            tablesInfo ti = stn.Tag as tablesInfo;
                            String layerName = Program.RelationVisbleBdUser.GetNameInBd(ti.idTable);
                            if (getLayerIsSelectable(layerName))
                            {
                                stn.BackColor = Color.Gray;
                            }
                            else
                            {
                                stn.BackColor = treeView1.BackColor;
                            }
                        }
                        else if (stn.Tag is VMPM.VMPTableBaseModel)
                        {
                            var table = stn.Tag as VMPM.VMPTableBaseModel;
                            if (table.IsSelectable)
                            {
                                stn.BackColor = Color.Gray;
                            }
                            else
                            {
                                stn.BackColor = treeView1.BackColor;
                            }
                        }
                        else if (stn.Tag is RastrInfo)
                        {
                            if (!isSelectedNode(stn))
                            {
                                stn.BackColor = treeView1.BackColor;
                            }
                        }
                        else if (stn.Tag is CosM.CosmeticTableBaseM)
                        {
                            if (getLayerIsSelectable(stn.Name))
                            {
                                stn.BackColor = Color.Gray;
                            }
                            else
                            {
                                stn.BackColor = treeView1.BackColor;
                            }
                        }
                        if (isSelectedNode(stn))
                        {
                            stn.BackColor = SystemColors.Highlight;
                        }
                    }
                }
                if (tn.Tag is tablesInfo)
                {
                    tablesInfo ti = tn.Tag as tablesInfo;
                    String layerName = Program.RelationVisbleBdUser.GetNameInBd(ti.idTable);
                    if (getLayerIsSelectable(layerName))
                    {
                        tn.BackColor = Color.Gray;
                    }
                    else
                    {
                        tn.BackColor = treeView1.BackColor;
                    }
                    if (isSelectedNode(tn))
                    {
                        tn.BackColor = SystemColors.Highlight;
                    }
                }
                else if (tn.Tag is VMPM.VMPTableBaseModel)
                {
                    var tableVMP = tn.Tag as VMPM.VMPTableBaseModel;
                    if (tableVMP.IsSelectable)
                    {
                        tn.BackColor = Color.Gray;
                    }
                    else
                    {
                        tn.BackColor = treeView1.BackColor;
                    }
                    if (isSelectedNode(tn))
                    {
                        tn.BackColor = SystemColors.Highlight;
                    }
                }
                else if (tn.Tag is RastrInfo)
                {
                    if (!isSelectedNode(tn))
                    {
                        tn.BackColor = treeView1.BackColor;
                    }
                    else
                    {
                        tn.BackColor = SystemColors.Highlight;
                    }
                }
                else if (tn.Tag is CosM.CosmeticTableBaseM)
                {
                    if (getLayerIsSelectable(tn.Name))
                    {
                        tn.BackColor = Color.Gray;
                    }
                    else
                    {
                        tn.BackColor = treeView1.BackColor;
                    }
                }
            }
        }
        private bool isSelectedNode(TreeNode tn)
        {
            bool isSelectedNode = false;
            if (_clickedNode == null)
            {
                return false;
            }
            else
            {
                if (tn.Tag is tablesInfo && _clickedNode.Tag is tablesInfo)
                {
                    isSelectedNode = ((tn.Tag as tablesInfo).idTable == (_clickedNode.Tag as tablesInfo).idTable);
                    if (isSelectedNode && (treeView1.SelectedNode == null || treeView1.SelectedNode.Tag != tn.Tag))
                    {
                        if (_viewType == enViewType.selectable)
                            treeView1.SelectedNode = tn;
                        _clickedNode = tn;
                    }
                    return isSelectedNode;
                }
                else if (tn.Tag is VMPM.VMPTableBaseModel && _clickedNode.Tag is VMPM.VMPTableBaseModel)
                {
                    isSelectedNode = (tn.Tag == _clickedNode.Tag);
                    if (isSelectedNode && treeView1.SelectedNode != tn)
                    {
                        if (_viewType == enViewType.selectable)
                            treeView1.SelectedNode = tn;
                        _clickedNode = tn;
                    }
                    return isSelectedNode;
                }
                else if (tn.Tag is RastrInfo && _clickedNode.Tag is RastrInfo)
                {
                    isSelectedNode = (tn.Name == _clickedNode.Name && ((tn.Tag as RastrInfo).RastrPath == (_clickedNode.Tag as RastrInfo).RastrPath));
                    if (isSelectedNode && treeView1.SelectedNode != tn)
                    {
                        if (_viewType == enViewType.selectable)
                            treeView1.SelectedNode = tn;
                        _clickedNode = tn;
                    }
                    return isSelectedNode;
                }
                else if (tn.Tag is CosM.CosmeticTableBaseM && _clickedNode.Tag is CosM.CosmeticTableBaseM)
                {
                    isSelectedNode = ((tn.Tag as CosM.CosmeticTableBaseM).Id == (_clickedNode.Tag as CosM.CosmeticTableBaseM).Id);
                    if (isSelectedNode && (treeView1.SelectedNode == null || treeView1.SelectedNode.Tag != tn.Tag))
                    {
                        if (_viewType == enViewType.selectable)
                            treeView1.SelectedNode = tn;
                        _clickedNode = tn;
                    }
                    return isSelectedNode;
                }
                else
                {
                    return false;
                }
            }
        }

        public void DeleteObjectFromLayer()
        {
            if (_clickedNode == null)
                return;
            var layer = _clickedNode.Tag as AbsM.ILayerM;
            if (_clickedNode.Tag is tablesInfo)
            {
                tablesInfo ti = (tablesInfo)(_clickedNode.Tag);
                String layerName = Program.RelationVisbleBdUser.GetNameInBd(ti.idTable);

                mvLayer ll = Program.mainFrm1.axMapLIb1.getLayer(layerName);
                if (ll == null)
                {
                    MessageBox.Show(Rekod.Properties.Resources.LIV_CanNotGetLayer);
                    return;
                }
                if (ll.SelectedCount == 0)
                {
                    MessageBox.Show(Rekod.Properties.Resources.LIV_NotSelectedObj);
                    return;
                }
                else
                {
                    int curObjSelected = Program.mainFrm1.idT_Obj;
                    DialogResult dr;
                    if (ll.SelectedCount == 1)
                    {
                        dr = MessageBox.Show(Rekod.Properties.Resources.LIV_DeleteSelectedRow,
                                        Rekod.Properties.Resources.ucTable_DeleteRowsHeader, MessageBoxButtons.YesNo);
                    }
                    else
                    {
                        dr = MessageBox.Show(Rekod.Properties.Resources.ucTable_DeleteRows,
                                        Rekod.Properties.Resources.ucTable_DeleteRowsHeader, MessageBoxButtons.YesNo);
                    }
                    if (dr == DialogResult.Yes)
                    {
                        mvIntArray ids = ll.getSelected();
                        ll.DeleteArray(ids);
                        Program.mainFrm1.axMapLIb1.mapRepaint();
                        if (ll.SelectedCount == 1)
                        {
                            Program.mainFrm1.StatusInfo = Rekod.Properties.Resources.LIV_ObjectDelete;
                        }
                        else
                        {
                            Program.mainFrm1.StatusInfo = Rekod.Properties.Resources.LIV_ObjectsDelete;
                        }

                    }
                }
            }
            else if (layer != null && layer.IsEditable == true)
            {
                mvLayer ll = Program.mainFrm1.axMapLIb1.getLayer(layer.NameMap);
                if (ll == null)
                {
                    MessageBox.Show(Rekod.Properties.Resources.LIV_CanNotGetLayer);
                    return;
                }
                if (ll.SelectedCount == 0)
                {
                    MessageBox.Show(Rekod.Properties.Resources.LIV_NotSelectedObj);
                    return;
                }
                else
                {
                    int curObjSelected = Program.mainFrm1.idT_Obj;
                    DialogResult dr;
                    if (ll.SelectedCount == 1)
                    {
                        dr = MessageBox.Show(Rekod.Properties.Resources.LIV_DeleteSelectedRow,
                                        Rekod.Properties.Resources.ucTable_DeleteRowsHeader, MessageBoxButtons.YesNo);
                    }
                    else
                    {
                        dr = MessageBox.Show(Rekod.Properties.Resources.ucTable_DeleteRows,
                                        Rekod.Properties.Resources.ucTable_DeleteRowsHeader, MessageBoxButtons.YesNo);
                    }
                    if (dr == DialogResult.Yes)
                    {
                        mvIntArray ids = ll.getSelected();
                        ll.DeleteArray(ids);
                        Program.mainFrm1.axMapLIb1.mapRepaint();
                        if (ll.SelectedCount == 1)
                        {
                            Program.mainFrm1.StatusInfo = Rekod.Properties.Resources.LIV_ObjectDelete;
                        }
                        else
                        {
                            Program.mainFrm1.StatusInfo = Rekod.Properties.Resources.LIV_ObjectsDelete;
                        }

                    }
                }
            }
            else
            {
                MessageBox.Show(Rekod.Properties.Resources.ucTable_NoRowsSeleted);
            }
        }
        private void fillLayersOrder()
        {
            _layersOrder.Clear();
            foreach (TreeNode tn in treeView1.Nodes)
            {
                if (tn.Tag is GroupInfo)
                {
                    foreach (TreeNode gtn in tn.Nodes)
                    {
                        _layersOrder.Add(gtn, tn);
                    }
                }
                else
                {
                    _layersOrder.Add(tn, null);
                }
            }
        }
        private void sortInMapOrder()
        {
            switch (_viewType)
            {
                case enViewType.visible:
                case enViewType.selectable:
                    {
                        // Название слоя на карте, вершина соответствующая этому слою 
                        Dictionary<String, TreeNode> layerOrder = new Dictionary<string, TreeNode>();
                        for (int i = 0; i < Program.mainFrm1.axMapLIb1.LayersCount; i++)
                        {
                            mvLayer ll = Program.mainFrm1.axMapLIb1.getLayerByNum(i);
                            try
                            {
                                String layerName = ll.NAME;
                                if (ll != null && ll.Visible && layerName != null)
                                {
                                    layerOrder.Add(layerName, null);
                                }
                            }
                            catch (Exception)
                            {

                            }
                        }
                        layersManagerFRMRastr rastrManager = new layersManagerFRMRastr();
                        var rInfoList = rastrManager.GetRastrLayers();
                        for (int i = 0; i < Program.mainFrm1.axMapLIb1.ImageLayerCount; i++)
                        {
                            var ll = Program.mainFrm1.axMapLIb1.getImageLayerByNum(i);
                            try
                            {
                                var rInfo = rInfoList.FirstOrDefault(w => w.RastrPath == ll.filename);
                                if (ll != null && ll.Visible && rInfo != null && rInfo.RastrName != null)
                                {
                                    layerOrder.Add(rInfo.RastrName, null);
                                }
                            }
                            catch (Exception)
                            {

                            }
                        }

                        foreach (TreeNode tn in treeView1.Nodes)
                        {
                            String layerName;
                            if (tn.Tag is tablesInfo)
                            {
                                tablesInfo ti = (tablesInfo)(tn.Tag);
                                layerName = Program.RelationVisbleBdUser.GetNameInBd(ti.idTable);
                            }
                            else
                            {
                                layerName = tn.Name;
                            }
                            layerOrder[layerName] = tn;
                        }

                        List<TreeNode> nodes = layerOrder.Values.ToList();
                        for (int i = 0; i < nodes.Count; i++)
                        {
                            if (nodes[i] != null && treeView1.Nodes.Contains(nodes[i]))
                            {
                                treeView1.Nodes.Remove(nodes[i]);
                                treeView1.Nodes.Add(nodes[i]);

                            }
                        }
                        if (_visibleNodeIndex != -1 && treeView1.Nodes.Count > 0 && tabControl1.SelectedIndex == 2 && treeView1.SelectedNode == null)
                        {
                            if (treeView1.Nodes.Count > _visibleNodeIndex)
                            {
                                treeView1.Nodes[_visibleNodeIndex].EnsureVisible();
                            }
                        }

                    } break;
                default:
                    break;
            }
        }
        public void UpdateListLayersIsView()
        {
            // Название слоя на карте, вершина соответствующая этому слою 
            List<AbsM.TableBaseM> listLayer = new List<AbsM.TableBaseM>();
            Dictionary<String, TreeNode> layerOrder = new Dictionary<string, TreeNode>();
            for (int i = 0; i < Program.mainFrm1.axMapLIb1.LayersCount; i++)
            {
                mvLayer ll = Program.mainFrm1.axMapLIb1.getLayerByNum(i);
                try
                {
                    String layerName = ll.NAME;
                    if (ll != null && ll.Visible && layerName != null && Program.RelationVisbleBdUser != null)
                    {
                        int idTable = Program.RelationVisbleBdUser.GetIdTable(layerName);
                        if (idTable != -1)
                        {
                            AbsM.TableBaseM pgTable = Program.repository.FindTable(idTable) as AbsM.TableBaseM;
                            if (pgTable != null)
                                listLayer.Add(pgTable);
                        }
                        else
                        {
                            var tables = Program.TablesManager.CosmeticRepository.Tables;

                            AbsM.TableBaseM cosLayer =
                                Program.TablesManager.CosmeticRepository.Tables.FirstOrDefault(t => (t as AbsM.TableBaseM).NameMap == layerName) as AbsM.TableBaseM;
                            if (cosLayer != null)
                            {
                                listLayer.Add(cosLayer);
                            }
                        }
                    }
                }
                catch (Exception)
                {

                }
            }
            ExtraFunctions.Sorts.SortList(ListLayersIsView, listLayer);
        }
        private void sortInLayersOrder()
        {
            List<TreeNode> nodes = _layersOrder.Keys.ToList();
            for (int i = 0; i < nodes.Count; i++)
            {
                TreeNode par = _layersOrder[nodes[i]];
                if (par != null && par.Nodes.Contains(nodes[i]))
                {
                    par.Nodes.Remove(nodes[i]);
                    par.Nodes.Add(nodes[i]);
                }
                else if (par == null && treeView1.Nodes.Contains(nodes[i]))
                {
                    treeView1.Nodes.Remove(nodes[i]);
                    treeView1.Nodes.Add(nodes[i]);
                }
            }
        }
        private void sortPodlojkaLayers()
        {
            TreeNode parentNode = null;
            TreeNode[] tempNodes = treeView1.Nodes.Find(Rekod.Properties.Resources.LIV_Vmp_layer, false);
            foreach (TreeNode tempTn in tempNodes)
            {
                if (tempTn.Tag is GroupInfo && ((GroupInfo)(tempTn.Tag)).Id == -1)
                {
                    parentNode = tempTn;
                    break;
                }
            }
            Dictionary<String, TreeNode> tmp = new Dictionary<string, TreeNode>();
            foreach (TreeNode tn in parentNode.Nodes)
            {
                tmp.Add(tn.Name, tn);
            }
            List<String> tmList = tmp.Keys.ToList();
            tmList.Sort();
            foreach (String str in tmList)
            {
                parentNode.Nodes.Remove(tmp[str]);
                parentNode.Nodes.Add(tmp[str]);
            }
        }
        private void treeView1_BeforeSelect(object sender, TreeViewCancelEventArgs e)
        {
            if (_viewType != enViewType.selectable && !(e.Node.Tag is GroupInfo) && e.Action != TreeViewAction.ByKeyboard /* && _canSetSelectableOneItem*/)
            {
                _clickedNode = e.Node;
                if (_clickedNode.Tag is StylesM)
                {
                    treeView1.SelectedNode = e.Node.Parent;
                    return;
                }

                EditableLayer = null;

                object tag = e.Node.Tag;
                String layerName = null;
                if (tag is tablesInfo)
                {
                    tablesInfo tInfo = tag as tablesInfo;
                    layerName = Program.RelationVisbleBdUser.GetNameInBd(tInfo.idTable);
                }
                else if (tag is AbsM.ILayerM)
                {
                    var table = tag as AbsM.ILayerM;
                    layerName = table.NameMap;
                }

                foreach (AbsM.ILayerM item in Program.TablesManager.CosmeticRepository.Tables)
                {
                    item.IsSelectable = false;
                    item.IsEditable = false;
                }
                foreach (AbsM.ILayerM item in Program.TablesManager.VMPReposotory.Tables)
                {
                    item.IsSelectable = false;
                    item.IsEditable = false;
                }
                mvMapLib.mvLayer layer = Program.mainFrm1.axMapLIb1.getLayer(layerName);
                if (layer == null || layer != null && !layer.selectable)
                {
                    for (int i = 0; i < Program.mainFrm1.axMapLIb1.LayersCount; i++)
                    {
                        layer = Program.mainFrm1.axMapLIb1.getLayerByNum(i);
                        if (layer.selectable)
                        {
                            layer.selectable = false;
                            layer.editable = false;
                        }

                    }
                }
                if (tag is tablesInfo)
                {
                    int id = -1;
                    tablesInfo tInfo = tag as tablesInfo;
                    layerName = Program.RelationVisbleBdUser.GetNameInBd(tInfo.idTable);
                    id = tInfo.idTable;
                    layer = Program.mainFrm1.axMapLIb1.getLayer(layerName);
                    if (layer != null)
                    {
                        layer.editable = true;
                        if (!layer.selectable)
                        {
                            layer.selectable = true;
                        }

                        EditableLayer = Program.repository.FindTable(id) as AbsM.ILayerM;
                        if (EditableIdLayer != id)
                        {
                            EditableIdLayer = id;
                        }
                        OnPropertyChanged("EditableIdLayer");
                    }
                }
                else if (tag is AbsM.ILayerM)
                {
                    var table = tag as AbsM.ILayerM;
                    EditableIdLayer = 0;
                    table.IsSelectable = true;
                    if (table.IsReadOnly == false)
                        table.IsEditable = true;
                    else
                        table.IsEditable = false;
                    EditableLayer = table;
                }
            }
        }
        public void SetLayerIsEditable(AbsM.ILayerM layerToSet)
        {
            if (layerToSet != _editableLayer)
            {
                if (layerToSet is DataAccess.SourcePostgres.Model.PgTableBaseM)
                {
                    tablesInfo tInfo = classesOfMetods.getTableInfo(Convert.ToInt32(layerToSet.Id));
                    var layerName = Program.RelationVisbleBdUser.GetNameInBd(tInfo.idTable);
                    layerToSet.NameMap = layerName;
                    var mvLayer = Program.mainFrm1.axMapLIb1.getLayer(layerName);

                    if (mvLayer != null)
                    {
                        for (int i = 0; i < Program.mainFrm1.axMapLIb1.LayersCount; i++)
                        {
                            var layer = Program.mainFrm1.axMapLIb1.getLayerByNum(i);
                            if (layer.editable)
                            {
                                layer.editable = false;
                            }
                            if (layer.selectable)
                            {
                                layer.selectable = false;
                            }
                        }

                        mvLayer.editable = true;
                        if (!mvLayer.selectable)
                        {
                            mvLayer.selectable = true;
                        }
                        if (EditableIdLayer != tInfo.idTable)
                        {
                            layerToSet.IsVisible = true;
                            layerToSet.IsSelectable = true;
                            layerToSet.IsEditable = true;

                            EditableLayer = layerToSet;
                            EditableIdLayer = tInfo.idTable;
                        }
                    }
                    _clickedNode = GetTreeNodePg(tInfo, true);
                }
                else
                {
                    layerToSet.IsVisible = true;
                    layerToSet.IsSelectable = true;
                    layerToSet.IsEditable = true;

                    if (layerToSet.IsEditable)
                    {
                        _clickedNode = GetTreeNodeCosmetic(layerToSet, true);
                        treeView1.SelectedNode = _clickedNode;
                        EditableLayer = layerToSet;
                    }
                }
            }
        }
        public void SetLayerIsEditable(int idTable)
        {
            if (idTable <= 0 || EditableIdLayer == idTable)
                return;

            SetLayerVisible(idTable);
            tablesInfo tInfo = classesOfMetods.getTableInfo(idTable);
            var layerName = Program.RelationVisbleBdUser.GetNameInBd(idTable);
            var layerTable = Program.mainFrm1.axMapLIb1.getLayer(layerName);
            if (layerTable != null)
            {
                layerTable.editable = true;
                if (!layerTable.selectable)
                {
                    layerTable.selectable = true;
                }
                if (EditableIdLayer != tInfo.idTable)
                {
                    EditableIdLayer = tInfo.idTable;
                }
            }
            EditableIdLayer = idTable;
            EditableLayer = Program.repository.FindTable(idTable) as AbsM.ILayerM;
            _clickedNode = GetTreeNodePg(tInfo, true);
        }
        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            setBackColorSelectableLayers();
            defineToolBoxVisibility();
        }
        #endregion
        #region Обработка нажатий пунктов меню
        public void openTableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_clickedNode == null)
            {
                return;
            }
            if (_clickedNode.Tag is tablesInfo)
            {
                tablesInfo ti = (tablesInfo)(_clickedNode.Tag);
                try
                {
                    Program.mainFrm1.layersManager1.openTableGrid(ti.idTable);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(Rekod.Properties.Resources.LIV_NotOpenTable + ":\n" + ex.Message);
                }
            }
            else if (_clickedNode.Tag is AbsM.ITableBaseM)
            {
                var tableVMP = _clickedNode.Tag as AbsM.ITableBaseM;
                try
                {
                    tableVMP.Source.OpenTable(tableVMP);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(Rekod.Properties.Resources.LIV_ErrorOpenTable + ": " + ex.Message);
                }
            }
        }


        public void addToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Program.mainFrm1.CosmeticHollowAdding || Program.mainFrm1.CosmeticLineAdding)
                return;

            if (_clickedNode != null && _clickedNode.Tag is tablesInfo)
            {
                if (!_clickedNode.Checked)
                    _clickedNode.Checked = true;

                tablesInfo ti = (tablesInfo)(_clickedNode.Tag);
                //String layerName = Program.RelationVisbleBdUser.GetNameInBd(ti.idTable);
                //mvMapLib.mvLayer layer = Program.mainFrm1.axMapLIb1.getLayer(layerName);
                layerSetVisible(true, "", ti.idTable);

                tablesInfo tInfo = _clickedNode.Tag as tablesInfo;
                var layerName = Program.RelationVisbleBdUser.GetNameInBd(tInfo.idTable);
                var layer = Program.mainFrm1.axMapLIb1.getLayer(layerName);
                if (layer != null)
                {
                    layer.editable = true;
                }

                switch (classesOfMetods.GetIntGeomType(ti.GeomType_GC))
                {
                    case 1:
                        {
                            Program.mainFrm1.axMapLIb1.SetCursor(mvMapLib.Cursors.mlAddDot);
                            break;
                        }
                    case 2:
                        {
                            Program.mainFrm1.axMapLIb1.SetCursor(mvMapLib.Cursors.mlAddPolyLine);
                            break;
                        }
                    case 3:
                        {
                            Program.mainFrm1.axMapLIb1.SetCursor(mvMapLib.Cursors.mlAddPolygon);
                            break;
                        }
                }
            }
            else if (_clickedNode != null && _clickedNode.Tag is CosM.CosmeticTableBaseM)
            {
                if (sender == tsAddPointOnMap)
                { Program.mainFrm1.axMapLIb1.SetCursor(mvMapLib.Cursors.mlAddDot); }
                else if (sender == tsAddLineOnMap)
                { Program.mainFrm1.axMapLIb1.SetCursor(mvMapLib.Cursors.mlAddPolyLine); }
                else if (sender == tsAddPolygonOnMap)
                { Program.mainFrm1.axMapLIb1.SetCursor(mvMapLib.Cursors.mlAddPolygon); }
            }
        }
        public void addToolStripMenuItemRectangle_Click(object sender, EventArgs e)
        {
            if (_clickedNode != null && _clickedNode.Tag is CosM.CosmeticTableBaseM)
            {
                CosM.CosmeticTableBaseM cosmLayer = _clickedNode.Tag as CosM.CosmeticTableBaseM;
                var layer = Program.mainFrm1.axMapLIb1.getLayer(cosmLayer.NameMap);
                if (layer != null)
                {
                    layer.selectable = true;
                    layer.editable = true;
                    Program.mainFrm1.axMapLIb1.SetCursor(mvMapLib.Cursors.mlRectangle);
                }
            }
            else
            {
                if (existEditLayerNotFormDB()) return;
                if (_clickedNode != null && _clickedNode.Tag is tablesInfo)
                {
                    if (!_clickedNode.Checked) _clickedNode.Checked = true;

                    tablesInfo ti = (tablesInfo)(_clickedNode.Tag);
                    //String layerName = Program.RelationVisbleBdUser.GetNameInBd(ti.idTable);
                    //mvMapLib.mvLayer layer = Program.mainFrm1.axMapLIb1.getLayer(layerName);
                    layerSetVisible(true, "", ti.idTable);

                    tablesInfo tInfo = _clickedNode.Tag as tablesInfo;
                    var layerName = Program.RelationVisbleBdUser.GetNameInBd(tInfo.idTable);
                    var layer = Program.mainFrm1.axMapLIb1.getLayer(layerName);
                    if (layer != null)
                    {
                        layer.selectable = true;
                        layer.editable = true;
                    }

                    switch (classesOfMetods.GetIntGeomType(ti.GeomType_GC))
                    {
                        case 3:
                            {
                                Program.mainFrm1.axMapLIb1.SetCursor(mvMapLib.Cursors.mlRectangle);
                                break;
                            }
                    }
                }
            }
        }
        private bool existEditLayerNotFormDB()
        {
            for (int i = 0; i < Program.mainFrm1.axMapLIb1.LayersCount; i++)
            {
                var t = Program.mainFrm1.axMapLIb1.getLayerByNum(i);
                if (t.editable)
                {
                    if (Program.mainFrm1.axMapLIb1.getLayerByNum(i).NAME == Program.RelationVisbleBdUser.GetNameForUser(
                        Program.mainFrm1.axMapLIb1.getLayerByNum(i).NAME)
                        )
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public void addCoordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_clickedNode != null && _clickedNode.Tag is tablesInfo)
            {
                if (!_clickedNode.Checked)
                    _clickedNode.Checked = true;

                tablesInfo ti = (tablesInfo)(_clickedNode.Tag);

                String layerName = Program.RelationVisbleBdUser.GetNameInBd(ti.idTable);
                mvMapLib.mvLayer layer = Program.mainFrm1.axMapLIb1.getLayer(layerName);
                if (layer == null)
                {
                    Program.mainFrm1.layersManager1.loadLayerFromSource(((tablesInfo)(_clickedNode.Tag)).idTable);
                }
                layerName = Program.RelationVisbleBdUser.GetNameInBd(ti.idTable);
                layer = Program.mainFrm1.axMapLIb1.getLayer(layerName);

                layerName = null;
                layer = null;
                if (_clickedNode.Tag is tablesInfo)
                {
                    layerName = Program.RelationVisbleBdUser.GetNameInBd((_clickedNode.Tag as tablesInfo).idTable);
                    layer = Program.mainFrm1.axMapLIb1.getLayer(layerName);
                    if (layer != null)
                    {
                        layer.selectable = true;
                        layer.editable = true;
                    }
                }
                //else if (_clickedNode.Tag is VMPM.VMPTableBaseModel)
                //{
                //    var tableVMP = _clickedNode.Tag as VMPM.VMPTableBaseModel;
                //    tableVMP.IsSelectable = true;
                //    tableVMP.IsEditable = true;
                //}
                setBackColorSelectableLayers();


                if (!classesOfMetods.getWriteTable(ti.idTable))
                {
                    MessageBox.Show(Rekod.Properties.Resources.LIV_NotRightLayer, Rekod.Properties.Resources.InformationMessage_Header,
                        MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    return;
                }
                switch (classesOfMetods.GetIntGeomType(ti.GeomType_GC))
                {
                    case 1:
                        {
                            Program.mainFrm1.layersManager1.addPoint(ti.idTable);
                        } break;
                    case 2:
                        {
                            Program.mainFrm1.layersManager1.addLine(ti.idTable);
                        } break;
                    case 3:
                        {
                            Program.mainFrm1.layersManager1.addPolygon(ti.idTable);
                        } break;
                }
            }
        }
        #endregion
        #region Пользовательская прорисовка узлов дерева
        // offset of window style value
        public const int GWL_STYLE = -16;
        // window style constants for scrollbars
        public const int WS_VSCROLL = 0x00200000;
        public const int WS_HSCROLL = 0x00100000;
        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DebuggerStepThrough]
        private void treeView1_DrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            if (e.Node.Tag is GroupInfo)
            {
                GroupInfo gi = (GroupInfo)(e.Node.Tag);
                Font font = new Font(treeView1.Font.OriginalFontName, treeView1.Font.Size, FontStyle.Regular,
                                     GraphicsUnit.Point);

                int fontsize = (int)(treeView1.Font.Size);
                int delta = e.Bounds.Height - fontsize;
                int indent = 5;
                if (!e.Node.IsExpanded)
                {
                    Image im = (Image)(Rekod.Properties.Resources.plus);
                    e.Graphics.DrawImage(im, e.Bounds.X + indent, e.Bounds.Y + delta / 2, e.Bounds.Height - delta,
                                         e.Bounds.Height - delta);
                }
                else
                {
                    Image im = (Image)(Rekod.Properties.Resources.minus);
                    e.Graphics.FillRectangle(new SolidBrush(treeView1.BackColor), e.Bounds.X + indent, e.Bounds.Y + delta / 2, e.Bounds.Height - delta,
                                        e.Bounds.Height - delta);
                    e.Graphics.DrawImage(im, e.Bounds.X + indent, e.Bounds.Y + delta / 2, e.Bounds.Height - delta,
                                        e.Bounds.Height - delta);
                }

                {
                    // Рисование значка принадлежности группы
                    // -1: подложка
                    // -2: растровые слои
                    if (gi.Id == -1)
                    {
                        Image nim = (Image)(imageList1.Images[0]);
                        e.Graphics.DrawImage(nim, e.Bounds.X + 2 * indent + e.Bounds.Height - delta - 2, e.Bounds.Y + delta / 2 - 2,
                                             e.Bounds.Height - delta + 4,
                                             e.Bounds.Height - delta + 4);
                    }
                    else if (gi.Id == -2)
                    {
                        Image nim = (Image)(imageList1.Images[1]);
                        e.Graphics.DrawImage(nim, e.Bounds.X + 2 * indent + e.Bounds.Height - delta - 1, e.Bounds.Y + delta / 2,
                                             e.Bounds.Height - delta + 2,
                                             e.Bounds.Height - delta);
                    }
                    else
                    {
                        Image nim = (Image)(imageList1.Images[2]);
                        e.Graphics.DrawImage(nim, e.Bounds.X + 2 * indent + e.Bounds.Height - delta, e.Bounds.Y + delta / 2,
                                             e.Bounds.Height - delta,
                                             e.Bounds.Height - delta);
                    }
                }

                String str = e.Node.Text; // +" (" + e.Node.Nodes.Count.ToString() + ")";
                e.Graphics.DrawString(str, font, Brushes.Black, e.Bounds.X + 3 * indent + 2 * (e.Bounds.Height - delta),
                                      e.Bounds.Y + delta / 2 - 3);

                {
                    // Рисование количества внутренних узлов
                    int wndStyle = GetWindowLong(treeView1.Handle, GWL_STYLE);
                    bool vsVisible = (wndStyle & WS_VSCROLL) != 0;
                    String nstr = "[" + e.Node.Nodes.Count.ToString() + "]";
                    SizeF size = e.Graphics.MeasureString(nstr, font);
                    int rightLoc = e.Bounds.X + treeView1.Width - 8 - (int)(size.Width) - (vsVisible ? 15 : 0);
                    if ((e.Graphics.MeasureString(str, font).Width + e.Bounds.X) <= rightLoc)
                    {
                        e.Graphics.DrawString(nstr, font, Brushes.Black, rightLoc, e.Bounds.Y + delta / 2 - 3);
                    }
                }

                int heightDelta = 2;
                Rectangle rect = new Rectangle(e.Bounds.Location.X + 1, e.Bounds.Location.Y + heightDelta / 2,
                                               e.Bounds.Width - 2, e.Bounds.Height - heightDelta);
                e.Graphics.DrawRectangle(new Pen(Color.FromArgb(80, 80, 80)), rect);

                //e.Graphics.DrawString(e.Node.Text + e.Node.Nodes.Count.ToString(),  myTreeView.Font, brush, e.Bounds);
                //e.Graphics.DrawRectangle(pen, e.Bounds);
            }
            else if (e.Node.Tag is StylesM)
            {
                StylesM style = (StylesM)(e.Node.Tag);
                Font font = new Font(treeView1.Font.OriginalFontName, treeView1.Font.Size, FontStyle.Regular,
                                     GraphicsUnit.Point);

                int fontsize = (int)(treeView1.Font.Size);
                int delta = e.Bounds.Height - fontsize;
                int indent = 40;
                int left = indent;
                int imagePadding = 1;

                if (style.Preview != null)
                {
                    var k = (float)(e.Bounds.Height - imagePadding * 2) / (float)style.Preview.Height;
                    var w = (int)(((float)style.Preview.Width * (float)(treeView1.ItemHeight - imagePadding * 2)) / (float)style.Preview.Height);
                    e.Graphics.DrawImage(style.Preview,
                                            e.Bounds.X + left,
                                            e.Bounds.Y + imagePadding,
                                            w,
                                            treeView1.ItemHeight - imagePadding * 2);
                    left += w;
                }
                left += 5;

                String str = e.Node.Text; // +" (" + e.Node.Nodes.Count.ToString() + ")";
                e.Graphics.DrawString(str, font, Brushes.Black, e.Bounds.X + left,
                                      e.Bounds.Y + e.Bounds.Height / 2 - fontsize / 2);

            }
            else
            {
                e.DrawDefault = true;
                //int heightDelta = 2;
                //Rectangle rect = new Rectangle(e.Bounds.Location.X + 5, e.Bounds.Location.Y + heightDelta / 2,
                //                               e.Bounds.Width - 10, e.Bounds.Height - heightDelta);
                //e.Graphics.DrawRectangle(new Pen(Color.FromArgb(180, 50, 20)), rect);
            }
        }
        #endregion
        #region Поиск слоя по условию
        private Dictionary<TreeNode, TreeNode> _primaryNodesList = new Dictionary<TreeNode, TreeNode>();
        private Dictionary<TreeNode, TreeNode> _secondaryNodesList = new Dictionary<TreeNode, TreeNode>();
        private bool IsFind()
        {
            return treeView1.Nodes.Count != 0;
        }
        private void SetColorFindTextBox()
        {
            if (textBox2.Text != "")
            {
                textBox2.BackColor = Color.FromArgb(168, 236, 221);
                if (IsFind())
                {
                    textBox2.BackColor = Color.FromArgb(168, 236, 221);

                }
                else
                {
                    textBox2.BackColor = Color.FromArgb(250, 177, 175);
                }
                pictureBox1.Visible = true;
            }
            else
            {
                textBox2.BackColor = Color.FromArgb(230, 230, 230);
                pictureBox1.Visible = false;
            }
        }
        public void UpdateTree()
        {
            if (textBox2.Text == Rekod.Properties.Resources.LIV_Find)
            {
                textBox2.BackColor = Color.FromArgb(230, 230, 230);
                pictureBox1.Visible = false;
                return;
            }

            List<TreeNode> emptyGroupNodes = new List<TreeNode>();
            int count = 0;
            if (_primaryNodesList.Count == 0)
            {
                count = treeView1.Nodes.Count;
                for (int i = 0; i < count; i++)
                {
                    TreeNode mtn = treeView1.Nodes[i];
                    if (mtn != null && mtn.Tag is GroupInfo)
                    {
                        int inncount = mtn.GetNodeCount(false);
                        for (int j = 0; j < inncount; j++)
                        {
                            TreeNode itn = mtn.Nodes[j];
                            if (!itn.Text.ToUpper().Contains(textBox2.Text.ToUpper()))
                            {
                                if (itn != itn.Parent)
                                {
                                    _primaryNodesList.Add(itn, itn.Parent);
                                }
                                else
                                {
                                    _primaryNodesList.Add(itn, null);
                                }
                                mtn.Nodes.Remove(itn);
                                j--;
                                inncount--;
                            }
                        }
                        if (mtn.Nodes.Count == 0)
                            emptyGroupNodes.Add(mtn);
                    }
                    else if (mtn != null && mtn.Tag != null)
                    {
                        if (!mtn.Text.ToUpper().Contains(textBox2.Text.ToUpper()))
                        {
                            if (mtn != mtn.Parent)
                            {
                                _primaryNodesList.Add(mtn, mtn.Parent);
                            }
                            else
                            {
                                _primaryNodesList.Add(mtn, null);
                            }
                            treeView1.Nodes.Remove(mtn);
                            count--;
                            i--;
                        }
                    }
                }
            }
            else
            {
                count = treeView1.Nodes.Count;
                for (int i = 0; i < count; i++)
                {
                    TreeNode mtn = treeView1.Nodes[i];
                    if (mtn != null && mtn.Tag is GroupInfo)
                    {
                        int inncount = mtn.GetNodeCount(false);
                        for (int j = 0; j < inncount; j++)
                        {
                            TreeNode itn = mtn.Nodes[j];
                            if (!itn.Text.ToUpper().Contains(textBox2.Text.ToUpper()))
                            {
                                if (itn != itn.Parent)
                                {
                                    _secondaryNodesList.Add(itn, itn.Parent);
                                }
                                else
                                {
                                    _secondaryNodesList.Add(itn, null);
                                }
                                mtn.Nodes.Remove(itn);
                                j--;
                                inncount--;
                            }
                        }
                        if (mtn.Nodes.Count == 0)
                            emptyGroupNodes.Add(mtn);
                    }
                    else if (mtn != null && mtn.Tag != null)
                    {
                        if (!mtn.Text.ToUpper().Contains(textBox2.Text.ToUpper()))
                        {
                            if (mtn != mtn.Parent)
                            {
                                _secondaryNodesList.Add(mtn, mtn.Parent);
                            }
                            else
                            {
                                _secondaryNodesList.Add(mtn, null);
                            }
                            treeView1.Nodes.Remove(mtn);
                            count--;
                            i--;
                        }
                    }
                }

                count = _primaryNodesList.Count;
                for (int i = 0; i < count; i++)
                {
                    if (_primaryNodesList.ElementAt(i).Key.Name.ToUpper().Contains(textBox2.Text.ToUpper()))
                    {
                        TreeNode parent = _primaryNodesList.ElementAt(i).Value;
                        TreeNode child = _primaryNodesList.ElementAt(i).Key;
                        if (parent != child && parent != null)
                        {
                            parent.Nodes.Add(child);
                        }
                        else
                        {
                            treeView1.Nodes.Add(child);
                        }
                        _primaryNodesList.Remove(child);
                        count--;
                        i--;
                    }
                }
                for (int i = 0; i < _secondaryNodesList.Count; i++)
                {
                    _primaryNodesList.Add(_secondaryNodesList.ElementAt(i).Key, _secondaryNodesList.ElementAt(i).Value);
                }
                _secondaryNodesList.Clear();
            }

            foreach (var node in emptyGroupNodes)
            {
                treeView1.Nodes.Remove(node);
            }

            if (_viewType == enViewType.visible || _viewType == enViewType.selectable)
            {
                sortInMapOrder();
            }
            else
            {
                sortInLayersOrder();
            }
            SetColorFindTextBox();
            treeView1.Invalidate();
        }
        private void textBox2_Enter(object sender, EventArgs e)
        {
            if (textBox2.Text == Rekod.Properties.Resources.LIV_Find)
            {
                textBox2.Text = "";
            }
            textBox2.ForeColor = Color.Black;
        }
        public void textBox2_Leave(object sender, EventArgs e)
        {
            if (textBox2.Text == "")
            {
                textBox2.Text = Rekod.Properties.Resources.LIV_Find;
                textBox2.ForeColor = Color.Silver;
            }
        }
        private void textBox2_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                tabControl1_SelectedIndexChanged(sender, e);
                setBackColorSelectableLayers();
            }
        }
        private void textBox2_MouseClick(object sender, MouseEventArgs e)
        {
            if (textBox2.Text == Rekod.Properties.Resources.LIV_Find)
            {
                textBox2.Text = "";
            }
            textBox2.ForeColor = Color.Black;
        }
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            textBox2.Text = "";
            tabControl1_SelectedIndexChanged(sender, e);
            textBox2.Text = Rekod.Properties.Resources.LIV_Find;
            if (textBox2.Focused)
            {
                textBox2.Text = "";
            }
            else
            {
                textBox2.Text = Rekod.Properties.Resources.LIV_Find;
                textBox2.ForeColor = Color.Silver;
            }
        }
        #endregion

        public List<int> GetOrderLayers()
        {
            List<int> temp = new List<int>();
            for (int i = 0; i < Program.mainFrm1.axMapLIb1.LayersCount; i++)
            {
                if (Program.mainFrm1.axMapLIb1.getLayerByNum(i).External)
                {
                    if (Program.mainFrm1.axMapLIb1.getLayerByNum(i).Visible)
                    {
                        int idt = -1;
                        idt = Program.RelationVisbleBdUser.GetIdTable(Program.mainFrm1.axMapLIb1.getLayerByNum(i).NAME);
                        if (idt >= 0)
                            temp.Add(idt);
                    }
                }
            }
            return temp;
        }

        internal void GetStylesLayerVMP()
        {
            _dicStyles.Clear();
            foreach (AbsM.ILayerM layer in Program.TablesManager.VMPReposotory.Tables)
            {
                mvLayer ll = Program.mainFrm1.axMapLIb1.getLayer(layer.NameMap);
                var styles = new StylesVMP_VM(ll);
                styles.GetListStyles();
                _dicStyles.Add(ll.NAME, styles);
            }
        }



        private void treeView1_BeforeCollapse(object sender, TreeViewCancelEventArgs e)
        {
            if (_viewType == enViewType.visible)
            {
                if (e.Node.Tag is tablesInfo || e.Node.Tag is AbsM.ITableBaseM)
                    e.Cancel = true;
            }
        }

        private void ToolStripMenuItemSetExtent_Click(object sender, EventArgs e)
        {
            Program.mainFrm1.TbbZoomToLayer_Click(sender, null);
        }

        private void tsShowSettings_Click(object sender, EventArgs e)
        {
            if (_clickedNode != null)
            {
                var table = _clickedNode.Tag as AbsM.ITableBaseM;
                if (_clickedNode.Tag is RastrInfo)
                {
                    var rast = _clickedNode.Tag as RastrInfo;
                    table = Program.TablesManager.RastrRepository.FindTableByName(rast.RastrName);
                }
                if (table != null)
                {
                    table.Source.OpenTableSettings(table);
                }
            }
        }
    }

    public class GroupInfo
    {
        public GroupInfo(int id, String nameGroup, String description)
        {
            Id = id;
            NameGroup = nameGroup;
            Description = description;
        }
        public int Id;
        public String NameGroup
        {
            get;
            set;
        }
        public String Description;
        public int IdNode;
        public object tag;
        public override string ToString()
        {
            return NameGroup;
        }
    }

    public class RastrInfo
    {
        public RastrInfo(String rastrName, String rastrPath, bool isExternal, bool buildPyramids, int methodUse)
        {
            RastrName = rastrName;
            RastrPath = rastrPath;
            IsExternal = isExternal;
            this.BuildPyramids = buildPyramids;
            this.MethodUse = methodUse;
        }
        public RastrInfo(String rastrName, String rastrPath, bool isExternal, bool buildPyramids, int methodUse, bool usebounds, int minscale, int maxscale)
        {
            RastrName = rastrName;
            RastrPath = rastrPath;
            IsExternal = isExternal;
            this.BuildPyramids = buildPyramids;
            this.MethodUse = methodUse;
            this.UseBounds = usebounds;
            this.MinScale = minscale;
            this.MaxScale = maxscale;
        }
        public String RastrPath;
        public String RastrName;
        public Boolean IsExternal;
        public bool BuildPyramids;
        public int MethodUse;
        public object Content;
        public bool UseBounds;
        public int MinScale;
        public int MaxScale;
    }
    public static class Styles
    {
        public static Font Podlojka_font = new Font("Times New Roman", 12, FontStyle.Italic, GraphicsUnit.Point);
        public static Color Podlojka_color = Color.MidnightBlue;

        public static Font Group_font = new Font("Microsoft Sans Serif", 10, FontStyle.Italic, GraphicsUnit.Point);
        public static Color Group_color = Color.Indigo;

        public static Font CommonLayer_font = new Font("Times New Roman", 12, FontStyle.Italic, GraphicsUnit.Point);
        public static Color CommonLayer_color = Color.Black;
    }
}