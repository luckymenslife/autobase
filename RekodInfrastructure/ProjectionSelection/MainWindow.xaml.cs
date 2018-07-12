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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data;
using System.Text.RegularExpressions;
using Rekod.ProjectionSelection;
using System.Data.SQLite;
using Rekod.Services;

namespace ProjectionSelection
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Projection _proj;

        public MainWindow()
        {
            InitializeComponent();
            GetProjData();
        }

        /// <summary>
        /// Получение информации обо всех проекциях
        /// </summary>
        public void GetProjData()
        {
            string sql = "SELECT EXISTS(SELECT * FROM spatial_ref_sys)";
            using (SQLiteCommand cmd = SQLiteWork.getCmd(sql))
            {
                SQLiteDataReader dr = cmd.ExecuteReader();
                dr.Read();
                bool tabexists = dr.GetBoolean(0);
                dr.Close();

                if (!tabexists)
                {
                    MessageBox.Show("Таблица проекций не найдена!");
                    return;
                }

                sql = "SELECT * FROM spatial_ref_sys;";
                cmd.CommandText = sql;
                SQLiteDataAdapter da = new SQLiteDataAdapter(cmd);

                DataTable dataTable = new DataTable();
                da.Fill(dataTable);
                dataTable.Columns.Add("Name");
                dataTable.Columns.Add("Location");

                foreach (DataRow drow in dataTable.Rows)
                {
                    string pattern = "\"(.*?)\\/";
                    string name = Regex.Match(drow["ref_sys_name"].ToString(), pattern).Value;
                    if (name == null || name.Length<2)
                    {
                        pattern = "\"(.*?)\\,";
                        name = Regex.Match(drow["ref_sys_name"].ToString(), pattern).Value;
                    }
                    if (name.Length < 2) name = "aa";
                    name = name.Remove(0, 1);
                    name = name.Remove(name.Length - 1, 1);
                    name = name.Replace("\"", String.Empty);
                    drow["Name"] = name;

                    pattern = "\\/(.*?)\\,";
                    name = Regex.Match(drow["ref_sys_name"].ToString(), pattern).Value;
                    name = name.Replace("3 / ", String.Empty);
                    if (name == null || name.Length < 6)
                    {
                        pattern = "DATUM\\[(.*?)\\,";
                        name = Regex.Match(drow["ref_sys_name"].ToString(), pattern).Value;
                        if (name.Length < 2) name = "aaaaaaa";
                        name = name.Remove(0, 4);
                    }

                    name = name.Remove(0, 2);
                    name = name.Remove(name.Length - 1);
                    name = name.Replace("\"", String.Empty);

                    drow["Location"] = name;
                }

                dataGrid1.DataContext = dataTable;
                label5.Content = "Число записей - " + dataTable.Rows.Count.ToString();
                SQLiteWork.freeCmd(cmd);
            }
        }

        /// <summary>
        /// Кнопка поиска(обновления, если поля фильтра пусты)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            string search_param = textbox1.Text;
            //string name = textbox2.Text;
            //string location = textbox3.Text;
            string search_string = "SELECT * FROM spatial_ref_sys";

            SQLiteParameter param1 = new SQLiteParameter("@param1", System.Data.DbType.String);
            param1.Value = search_param;
            search_string += " WHERE CAST(srid as varchar2) LIKE '%'||@param1||'%' OR ref_sys_name LIKE '%'||@param1||'%'";


            using (SQLiteCommand cmd = SQLiteWork.getCmd(search_string))
            {
                cmd.Parameters.Add(param1);
                SQLiteDataAdapter da = new SQLiteDataAdapter(cmd);

                DataTable dataTable = new DataTable();
                da.Fill(dataTable);
                dataTable.Columns.Add("Name");
                dataTable.Columns.Add("Location");

                foreach (DataRow drow in dataTable.Rows)
                {
                    string pattern = "\\[(.*)\\/";
                    string name = Regex.Match(drow["ref_sys_name"].ToString(), pattern).Value;
                    if (name == null || name.Length < 2)
                    {
                        pattern = "\\[(.*?)\\,";
                        name = Regex.Match(drow["ref_sys_name"].ToString(), pattern).Value;
                    }
                    if (name.Length < 2) name = "aa";
                    name = name.Remove(0, 1);
                    name = name.Remove(name.Length - 1, 1);
                    name = name.Replace("\"", String.Empty);
                    drow["Name"] = name;

                    pattern = "\\/(.*?)\\,";
                    name = Regex.Match(drow["ref_sys_name"].ToString(), pattern).Value;
                    name = name.Replace("3 / ", String.Empty);
                    if (name == null || name.Length < 6)
                    {
                        pattern = "DATUM\\[(.*?)\\,";
                        name = Regex.Match(drow["ref_sys_name"].ToString(), pattern).Value;
                        if (name.Length < 2) name = "aaaaaaa";
                        name = name.Remove(0, 4);
                    }

                    name = name.Remove(0, 2);
                    name = name.Remove(name.Length - 1);
                    name = name.Replace("\"", String.Empty);

                    drow["Location"] = name;
                }

                label5.Content = "Число записей - " + dataTable.Rows.Count.ToString();
                dataGrid1.DataContext = dataTable;
                SQLiteWork.freeCmd(cmd);
            }
        }

        /// <summary>
        /// Получение аттрибутной информации по проекции
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //string str = dataGrid1.SelectedItem;
            //string srid = ((DataRowView)dataGrid1.SelectedCells[0].Item).Row.ItemArray[0].ToString();
            DataRowView drw = (DataRowView)dataGrid1.SelectedCells[0].Item;
            ObjectView wnd = new ObjectView();
            wnd.dataGrid1.ItemsSource= dataGrid1.SelectedItems;
            wnd.richTextBox1.Document.Blocks.Clear();
            wnd.richTextBox1.Document.Blocks.Add(new Paragraph(new Run(drw["ref_sys_name"].ToString())));
            wnd.richTextBox2.Document.Blocks.Clear();
            wnd.richTextBox2.Document.Blocks.Add(new Paragraph(new Run(drw["proj4text"].ToString())));
            wnd.Owner = this;
            bool sys_proj =  (bool)drw["sys_proj"];

            if (!sys_proj)
            {
                wnd.dataGrid1.IsReadOnly = false;
                wnd.richTextBox1.IsReadOnly = false;
                wnd.richTextBox2.IsReadOnly = false;
                wnd.dataGrid1.CanUserAddRows = false;
                wnd.saveBtn.Visibility = System.Windows.Visibility.Visible;
                wnd.dataGrid1.Columns[0].IsReadOnly = true;
                wnd.dataGrid1.Columns[2].IsReadOnly = true;
                wnd.dataGrid1.Columns[3].IsReadOnly = true;
                wnd.dataGrid1.Columns[4].IsReadOnly = true;
            }

            wnd.ShowDialog();

            //string search_string = "SELECT * FROM spatial_ref_sys WHERE srid = " + srid;
        }
        /// <summary>
        /// Кнопка "Добавить" (проекцию)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            AddProj wnd = new AddProj();
            wnd.Owner = this;
            wnd.ShowDialog();
        }


        /// <summary>
        /// Кнопка "Удаление проекции"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult res = MessageBox.Show("Вы действительно хотите удалить выбранную проекцию?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
            if (res != MessageBoxResult.Yes)
            {
                return;
            }

            if (dataGrid1.SelectedCells.Count == 0)
            {
                MessageBox.Show("Выберите проекцию, которую хотите удалить", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                return;
            }
            DataRow drw = ((DataRowView)dataGrid1.SelectedCells[0].Item).Row;
            bool is_system = (bool)drw.ItemArray[5];
            if (is_system)
            {
                MessageBox.Show("Нельзя удалить системную проекцию", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                return;
            }
            int srid = Int32.Parse(drw.ItemArray[0].ToString());
            if (srid <= 0)
            {
                MessageBox.Show("Некорректный SRID", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
            }

            string sql = String.Format("SELECT EXISTS(SELECT * FROM spatial_ref_sys WHERE srid = @param1);");
            SQLiteParameter param1 = new SQLiteParameter("@param1", System.Data.DbType.Int32);
            param1.Value = srid;
            using (SQLiteCommand cmd = SQLiteWork.getCmd(sql))
            {
                cmd.Parameters.Add(param1);
                SQLiteDataReader dr = cmd.ExecuteReader();
                dr.Read();
                bool recordExists = dr.GetBoolean(0);
                dr.Close();

                if (!recordExists)
                {
                    MessageBox.Show("Объекта с таким SRID не существует! Попробуйте обновить список проекций.");
                    return;
                }

                sql = String.Format("DELETE FROM spatial_ref_sys WHERE srid = @param1;");
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
                SQLiteWork.freeCmd(cmd);
            }
            MessageBox.Show("Проекция успешно удалена", "Удаление проекции", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
            refreshBtn_Click(null,null);
        }
        /// <summary>
        /// Выбирает проекцию по нажатию кнопки "Выбрать"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void selectBtn_Click(object sender, RoutedEventArgs e)
        {
            if (dataGrid1.SelectedCells.Count == 0)
            {
                MessageBox.Show("Сначала выберите проекцию", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                return;
            }

            DataRow dr = ((DataRowView)dataGrid1.SelectedCells[0].Item).Row;
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
            string ref_sys_name = dr["ref_sys_name"].ToString();
            string proj4text = dr["proj4text"].ToString();

            if (srid <= 0)
            {
                MessageBox.Show("Некорректный SRID", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                return;
            }

            if (String.IsNullOrEmpty(ref_sys_name))
            {
                MessageBox.Show("Некорректная строка проекции", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                return;
            }

            if (String.IsNullOrEmpty(proj4text))
            {
                MessageBox.Show("Некорректное выражение проекции", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                return;
            }
            _proj = new Projection(srid, auth_name, ref_sys_name, proj4text, is_system);
            this.DialogResult = true;
            this.Close();
        }

        /// <summary>
        /// Очищает фильтры и показывает список всех проекций
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void refreshBtn_Click(object sender, RoutedEventArgs e)
        {
            textbox1.Text = String.Empty;
            btnSearch_Click(null, null);
        }

        /// <summary>
        /// Делает кнопку "Удалить" неактивной, если удаление невозможно
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGrid1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dataGrid1.SelectedItem == null)
            {
                btnDelete.IsEnabled = false;
                selectBtn.IsEnabled = false;
                return;
            }

            selectBtn.IsEnabled = true;
            bool is_system = (bool)((DataRowView)dataGrid1.SelectedCells[0].Item).Row["sys_proj"];
            if (is_system)
            {
                btnDelete.IsEnabled = false;
            }
            else
            {
                btnDelete.IsEnabled = true;
            }
        }
        
        private void textbox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btnSearch_Click(null, null);
            }
        }


        public Projection SelectProject 
        {
            get { return _proj; }
            set { _proj = value; }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_proj != null)
            {
                this.DialogResult = true;
            }
        }
    }
}
