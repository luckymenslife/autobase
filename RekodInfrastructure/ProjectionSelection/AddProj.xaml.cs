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
using ProjectionSelection;
using mvMapLib;
using System.Data;
using System.Text.RegularExpressions;
using System.Data.SQLite;
using Rekod.Repository;
using Rekod.Services;

namespace Rekod.ProjectionSelection
{
    /// <summary>
    /// Interaction logic for AddProj.xaml
    /// </summary>
    public partial class AddProj : Window
    {
        public bool needTest = false;
        public double xCoord = 0;
        public double yCoord = 0;
        public string projKey = null;

        public AddProj()
        {
            InitializeComponent();
        }


        /// <summary>
        /// Делает попытку создания проекции
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void createButton_Click(object sender, RoutedEventArgs e)
        {
            int srid = 0;
            try
            {
                srid = Convert.ToInt32(Srid.Text);
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
            }
            string auth_name = Author.Text;
            string srtext = ProjString.Text;
            string proj4text = ProjExpr.Text;

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

            SQLiteParameter param1 = new SQLiteParameter("@param1", System.Data.DbType.Int32);
            param1.Value = srid;
            string sql = string.Format("SELECT EXISTS(SELECT * FROM spatial_ref_sys WHERE srid = @param1);");
            using (SQLiteCommand cmd = SQLiteWork.getCmd(sql))
            {
                cmd.Parameters.Add(param1);
                SQLiteDataReader dr = cmd.ExecuteReader();
                dr.Read();
                bool recordExists = dr.GetBoolean(0);
                dr.Close();

                if (recordExists)
                {
                    MessageBox.Show("Объект с таким уже SRID существует!");
                    return;
                }

                sql = String.Format("INSERT INTO spatial_ref_sys VALUES(@param1, @param2, @param1, @param3, @param4, 0);", srid, auth_name, srtext, proj4text);
                cmd.CommandText = sql;
                cmd.CommandType = System.Data.CommandType.Text;

                SQLiteParameter param2 = new SQLiteParameter("@param2", System.Data.DbType.String);
                param2.Value = auth_name;
                cmd.Parameters.Add(param2);
                SQLiteParameter param3 = new SQLiteParameter("@param3", System.Data.DbType.String);
                param3.Value = srtext;
                cmd.Parameters.Add(param3);
                SQLiteParameter param4 = new SQLiteParameter("@param4", System.Data.DbType.String);
                param4.Value = proj4text;
                cmd.Parameters.Add(param4);

                cmd.ExecuteNonQuery();
                SQLiteWork.freeCmd(cmd);
            }
            MessageBox.Show("Проекция успешно создана", "Создание проекции", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
            this.Close();

            Button btn = ((MainWindow)this.Owner).btnSearch;
            btn.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, btn));
        }

        /// <summary>
        /// Кнопка "Отмена"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Проверяет корректность строки srtext, введенной в текстбокс ProjString, пытаясь создать проекцию
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void testButton_Click(object sender, RoutedEventArgs e)
        {
            Dictionary<String, String> srtextFromName = new Dictionary<string,string>();
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

                sql = "SELECT srid, srtext FROM spatial_ref_sys;";
                cmd.CommandText = sql;
                SQLiteDataAdapter da = new SQLiteDataAdapter(cmd);

                DataTable dataTable = new DataTable();
                da.Fill(dataTable);
                dataTable.Columns.Add("Name");


                foreach (DataRow drow in dataTable.Rows)
                {
                    string pattern = "\\[(.*?)\\/";
                    string name = Regex.Match(drow["srtext"].ToString(), pattern).Value;
                    if (name == null || name.Length<2)
                    {
                        pattern = "\\[(.*?)\\,";
                        name = Regex.Match(drow["srtext"].ToString(), pattern).Value;
                    }
                    if (name.Length < 2) name = "aa";
                    name = name.Remove(0, 1);
                    name = name.Remove(name.Length - 1, 1);

                    srtextFromName.Add(drow["srid"].ToString() + '/' + name, drow["srtext"].ToString());
                }

                SQLiteWork.freeCmd(cmd);
            }
            AddProjTestDialog wnd = new AddProjTestDialog();
            wnd.CBprojSelect.ItemsSource = srtextFromName.Keys;
            wnd.Owner = this;
            wnd.ShowDialog();

            if (needTest)
            {
                needTest = false;
                Point[] p = new Point[1];
                p[0].X = xCoord;
                p[0].Y = yCoord;
                string projFrom = null;

                if (projKey == null)
                {
                    MessageBox.Show("Сначала выберите проекцию", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                    return;
                }
                srtextFromName.TryGetValue(projKey, out projFrom);

                string projTo = ProjString.Text;

                try
                {
                    p = OGRFramework.TransformGeometry.transform(p, "Point", projTo, projFrom);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Данная проекция не может быть применена!", "Ошибка проекции", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }
                MessageBox.Show("Преобразование удалось!\nКоордината X = " + p[0].X +"\nКоордината Y = " + p[0].Y, "Тест проекции", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
            }
        }
    }
}
