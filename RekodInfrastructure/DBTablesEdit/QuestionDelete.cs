using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Rekod
{
    public partial class QuestionDelete : Form
    {
        private QuestionDelete(string caption__,string message__,string check_message__)
        {
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            InitializeComponent();
            pictureBox1.Image = SystemIcons.Question.ToBitmap();
            this.Text = caption__;
            label1.Text = message__;
            checkBox1.Text = check_message__;
            this.Size = new Size((label1.Width <= 280) ? this.Size.Width : label1.Width + 60,
                (label1.Height <= 22) ? this.Size.Height : this.Size.Height + label1.Height - 22);
        }
        private QuestionDelete(string caption__, string message__, string check_message__,Icon icon)
        {
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            InitializeComponent();
            pictureBox1.Image = icon.ToBitmap();
            this.Text = caption__;
            label1.Text = message__;
            checkBox1.Text = check_message__;
            this.Size = new Size((label1.Width <= 280) ? this.Size.Width : label1.Width + 60,
                (label1.Height <= 22) ? this.Size.Height : this.Size.Height + label1.Height - 22);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = checkBox1.Checked ? DialogResult.Yes : DialogResult.No;
            this.Dispose();
        }
        static public DialogResult ShowDialog(string caption_, string message_, string check_message_)
        {
            // Call the private constructor so the users only need to call this
            // function, which is similar to MessageBox.Show.
            // Returns a standard DialogResult.
            using (QuestionDelete dialog = new QuestionDelete(caption_, message_, check_message_))
            {
                DialogResult result = dialog.ShowDialog();
                return result;
            }
        }
        static public DialogResult ShowDialog(string caption_, string message_, string check_message_,Icon icon)
        {
            // Call the private constructor so the users only need to call this
            // function, which is similar to MessageBox.Show.
            // Returns a standard DialogResult.
            using (QuestionDelete dialog = new QuestionDelete(caption_, message_, check_message_,icon))
            {
                DialogResult result = dialog.ShowDialog();
                return result;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Dispose();
        }
    }
}
