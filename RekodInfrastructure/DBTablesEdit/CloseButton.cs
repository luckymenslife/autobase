using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace Rekod
{
    class CloseButton:Button
    {
        UserControl controlForClose;
        DBTablesGroups form;
        public CloseButton(UserControl cfc, DBTablesGroups frm)
        {
            controlForClose = cfc;
            form = frm;
            this.Size = new Size(19, 19);
            this.BackgroundImage = Rekod.Properties.Resources.delete;
            this.BackgroundImageLayout = ImageLayout.Stretch;
            this.Anchor = (AnchorStyles.Top | AnchorStyles.Right);
        }
        public void CloseBox(EventArgs e)
        {
            OnClick(e);
        }
        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            if (controlForClose is ActionWithTables)
            {
                (controlForClose as ActionWithTables).table_edit.prev_selected = false;
                //(controlForClose as ActionWithTables).table_edit.act_tables = null;
            }
            form.CloseElemsAfter(controlForClose, true);
        }
    }
}
