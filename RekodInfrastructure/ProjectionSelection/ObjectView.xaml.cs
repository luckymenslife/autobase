using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Data;
using ProjectionSelection;
using System.Data.SQLite;
using Rekod.Repository;
using Rekod.Services;

namespace Rekod.ProjectionSelection
{
    /// <summary>
    /// Interaction logic for ObjectView.xaml
    /// </summary>
    public partial class ObjectView : Window
    {
        public ObjectView(Projection selectedProj = null)
        {
            InitializeComponent();

            if (selectedProj != null)
            {
                var dataTable = new DataTable();
                dataTable.Columns.Add("srid", typeof(int));
                dataTable.Columns.Add("auth_name", typeof(string));
                dataTable.Columns.Add("ref_sys_name", typeof(string));
                dataTable.Columns.Add("proj4text", typeof(string));
                dataTable.Columns.Add("Name", typeof(string));
                dataTable.Columns.Add("Location", typeof(string));
                dataTable.Columns.Add("sys_proj", typeof(bool));
                dataTable.Rows.Add(new object[] {
                selectedProj.Srid, selectedProj.Auth_name, selectedProj.Srtext, selectedProj.Proj4text, selectedProj.Name, selectedProj.Location, true});

                dataGrid1.ItemsSource = dataTable.DefaultView;
                richTextBox1.Document.Blocks.Clear();
                richTextBox1.Document.Blocks.Add(new Paragraph(new Run(selectedProj.Srtext)));
                richTextBox2.Document.Blocks.Clear();
                richTextBox2.Document.Blocks.Add(new Paragraph(new Run(selectedProj.Proj4text)));
            }
        }

        /// <summary>
        /// Получение текущей проекции
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void getProjBtn_Click(object sender, RoutedEventArgs e)
        {
            DataRow dr = ((DataRowView)dataGrid1.Items[0]).Row;
            int srid = 0;
            bool is_system = false;
            try
            {
                srid = Convert.ToInt32(dr["SRID"]);
                is_system = (bool)dr["sys_proj"];
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            string auth_name = dr["auth_name"].ToString();
            string srtext = new TextRange(richTextBox1.Document.ContentStart, richTextBox1.Document.ContentEnd).Text;
            string proj4text = new TextRange(richTextBox2.Document.ContentStart, richTextBox2.Document.ContentEnd).Text;

            if (srid <= 0)
            {
                MessageBox.Show("Некорректный SRID","Ошибка", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                return;
            }

            if (String.IsNullOrEmpty(srtext))
            {
                MessageBox.Show("Некорректная строка проекции", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                return;
            }

            if (String.IsNullOrEmpty(proj4text))
            {
                MessageBox.Show("Некорректное выражение проекции", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                return;
            }
            Projection proj = new Projection(srid, auth_name, srtext, proj4text, is_system);

            if (this.Owner != null)
            {
                MainWindow wnd = (MainWindow)this.Owner;
                if (wnd != null)
                {
                    wnd.SelectProject = proj;
                    this.Close();
                    wnd.Close();
                }
            }
            else
            {
                DialogResult = true;
                this.Close();
            }
        }

        /// <summary>
        /// Отмена и закрытие окна просмотра проекции
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            //Button btn = ((MainWindow)this.Owner).btnSearch;
            //btn.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, btn));
            this.Close();
        }

        /// <summary>
        /// Сохранение изменений проекции (только для пользовательских проекций)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveBtn_Click(object sender, RoutedEventArgs e)
        {
            DataRow dr = ((DataRowView)dataGrid1.Items[0]).Row;
            int srid = 0;
            bool is_system = false;
            try
            {
                srid = Convert.ToInt32(dr["SRID"]);
                is_system = (bool)dr["sys_proj"];
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            string name = dr["name"].ToString();
            string auth_name = dr["auth_name"].ToString();
            string srtext = new TextRange(richTextBox1.Document.ContentStart, richTextBox1.Document.ContentEnd).Text;
            string proj4text = new TextRange(richTextBox2.Document.ContentStart, richTextBox2.Document.ContentEnd).Text;

            if (srid <= 0)
            {
                MessageBox.Show("Некорректный SRID", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                return;
            }

            if (String.IsNullOrEmpty(srtext))
            {
                MessageBox.Show("Некорректная строка проекции", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                return;
            }

            if (String.IsNullOrEmpty(proj4text))
            {
                MessageBox.Show("Некорректное выражение проекции", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                return;
            }

            string sql = String.Format("UPDATE spatial_ref_sys SET auth_name = @param2, srtext = @param3, proj4text = @param4 WHERE srid = @param1;");

            SQLiteParameter param1 = new SQLiteParameter("@param1", System.Data.DbType.Int32);
            param1.Value = srid;
            SQLiteParameter param2 = new SQLiteParameter("@param2", System.Data.DbType.String);
            param2.Value = auth_name;
            SQLiteParameter param3 = new SQLiteParameter("@param3", System.Data.DbType.String);
            param3.Value = srtext;
            SQLiteParameter param4 = new SQLiteParameter("@param4", System.Data.DbType.String);
            param4.Value = proj4text;
            using (SQLiteCommand cmd = SQLiteWork.getCmd(sql))
            {
                cmd.Parameters.Add(param1);
                cmd.Parameters.Add(param2);
                cmd.Parameters.Add(param3);
                cmd.Parameters.Add(param4);     
                cmd.ExecuteNonQuery();
                SQLiteWork.freeCmd(cmd);
            }
            MessageBox.Show("Проекция успешно изменена", "Изменение проекции", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            MainWindow mainWindow = (MainWindow)this.Owner;
            if(mainWindow != null)
            {
                Button btn = mainWindow.btnSearch;
                btn.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, btn));
            }
        }
    }
}
