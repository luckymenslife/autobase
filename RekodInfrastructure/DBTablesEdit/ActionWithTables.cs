using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using Rekod.DBTablesEdit;

namespace Rekod
{
    public partial class ActionWithTables : UserControl
    {
        public TableEdit table_edit;
        private DBTablesGroups parent;
        public ActionWithTables(DBTablesGroups par, TableEdit t_edit)
        {
            InitializeComponent();
            parent = par;
            table_edit = t_edit;

            if (Program.app.sscUser != null)
            {
                btnGroups.Visible = false;
                btnIndexation.Location = btnCopy.Location;
                btnCopy.Location = btnGroups.Location;
            }
        }

        private void btnStyle_Click(object sender, EventArgs e)
        {
            table_edit.button4_Click(sender, e);
        }
        public void isLayerMap(bool isMap)
        {
            btnStyle.Enabled = isMap;
            btnGroups.Enabled = isMap;
            btnCaptions.Enabled = isMap;
        }

        private void btnStruct_Click(object sender, EventArgs e)
        {
            table_edit.button3_Click(sender, e);
        }

        private void btnProperties_Click(object sender, EventArgs e)
        {
            table_edit.listBox1_MouseDoubleClick(sender, null);
        }

        private void btnGroups_Click(object sender, EventArgs e)
        {
            table_edit.showGroups();
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            int select = table_edit.cbTableType.SelectedIndex;
            Rekod.DBTablesEdit.CreateCopyTable frm = new Rekod.DBTablesEdit.CreateCopyTable(table_edit.idT);
            if (frm.ShowDialog() == DialogResult.OK)
                table_edit.loadTables();
        }

        private void btnCaptions_Click(object sender, EventArgs e)
        {
            LabelControl sc = new LabelControl(parent, table_edit.idT);
            parent.CloseElemsAfter(this, false);
            parent.AddNewElem(sc, Rekod.Properties.Resources.AWT_EditLabel);
        }

        private void btnIndexation_Click(object sender, EventArgs e)
        {
            ObservableCollection<Rekod.DataAccess.AbstractSource.Model.IFieldM> iFields = new ObservableCollection<DataAccess.AbstractSource.Model.IFieldM>();
            var fields = classesOfMetods.getFieldInfoTable(table_edit.idT);
            var ti = classesOfMetods.getTableInfo(table_edit.idT);
            var table = new Rekod.DataAccess.SourcePostgres.Model.PgTableBaseM(
                Program.repository,
                ti.idTable,
                ti.srid,
                (DataAccess.AbstractSource.Model.ETableType)ti.type);
            foreach (var field in fields)
            {
                if (field.nameDB == ti.geomFieldName)
                    continue;
                iFields.Add(new Rekod.DataAccess.SourcePostgres.Model.PgFieldM(table, field.idField)
                {
                    Name = field.nameDB,
                    IsVisible = field.visible,
                    Text = field.nameMap,
                    Type = (DataAccess.AbstractSource.Model.EFieldType)field.type
                });
            }
            table.Fields = iFields;
            table.Name = ti.nameDB;
            table.Text = ti.nameMap;
            table.SchemeName = ti.nameSheme;
            table.PrimaryKey = ti.pkField;
            table.PrimaryKeyField = table.Fields.FirstOrDefault(w => w.Name == ti.pkField);

            Rekod.DataAccess.SourcePostgres.ViewModel.PgIndexVM datacontext = new DataAccess.SourcePostgres.ViewModel.PgIndexVM(table);
            Rekod.DataAccess.SourcePostgres.View.ConfigView.IndexView view = new DataAccess.SourcePostgres.View.ConfigView.IndexView();
            var window = Rekod.Controllers.WindowViewModelBase_VM.GetWindow(view, datacontext, 445, 550, 400, 455, null);
            //window.MaxHeight = 455;
            window.Title = Rekod.Properties.Resources.LocIndex;
            window.ShowDialog();
        }

    }
}
