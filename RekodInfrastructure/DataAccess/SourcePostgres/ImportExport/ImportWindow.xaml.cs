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
using System.IO;
using System.Globalization;
using System.Windows.Threading;
using System.Data;
using Npgsql;
using PgM = Rekod.DataAccess.SourcePostgres.Model;
using PgVM = Rekod.DataAccess.SourcePostgres.ViewModel;

namespace Rekod.DataAccess.SourcePostgres.ImportExport
{
    /// <summary>
    /// Interaction logic for ImportWindow.xaml
    /// </summary>
    public partial class ImportWindow : Window
    {
        Importer importer;
        private DispatcherTimer colsTim;
        private DataTable importDT;
        private readonly string progressKey;
        private PgM.PgTableBaseM _pgTable;
        NpgsqlConnectionStringBuilder _connect; 

        public ImportWindow(FileInfo inputFile, PgM.PgTableBaseM pgTable)
        {
            InitializeComponent();
            _pgTable = pgTable;
            _connect = (pgTable.Source as PgVM.PgDataRepositoryVM).Connect; 

            colsTim = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 0, 0, 200), IsEnabled = false };
            colsTim.Tick += new EventHandler(loadCols);
            inputFileTb.Text = inputFile.FullName;
            switch (inputFile.Extension.ToLower())
            {
                case ".dbf":
                    importer = new Importers.DBFImporter();
                    break;
                case ".xls":
                case ".xlsx":
                    importer = new Importers.ExcelImporter();
                    break;
                case ".shp":
                case ".tab":
                case ".bna":
                case ".csv":
                case ".geojson":
                case ".gml":
                case ".gmt":
                case ".itf":
                case ".sqlite":
                    importer = new Importers.SHPImporter();
                    break;
                default:
                    importer = null;
                    break;
            }
            hideError();
            hideSuccess();
            if (importer == null)
            {
                showError("Выбранный тип файла загрузчиком не поддерживается");
                return;
            }
            importer.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(importer_ProgressChanged);
            importer.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(importer_RunWorkerCompleted);
            progressKey = "importProgress" + DateTime.Now.Ticks.ToString();
            settingsSP.Children.Clear();
            try
            {
                cti.ThreadProgress.ShowWait();
                if (importer.SettingsPanel != null)
                    settingsSP.Children.Add(importer.SettingsPanel);
                importer.Init(inputFile, _pgTable);
            }
            catch (Exception ex)
            {
                showError(ex.Message);
                return;
            }
            finally
            {
                cti.ThreadProgress.Close();
            }
            if (_pgTable != null)
            {
                dbTableCB.Text = _pgTable.Name;
            }
        }
        void loadCols(object sender, EventArgs e)
        {
            colsTim.Stop();
            var list = FindChild<ComboBox>(importGrid, "headerCB");
            for (int i = 1; i < list.Count; i++)
                if (i > list[i].Items.Count)
                    break;
                else
                    list[i].SelectedIndex = i - 1;
        }
        private void preview(object sender, RoutedEventArgs e)
        {
            cti.ThreadProgress.ShowWait();
            hideError();
            hideSuccess();
            if (importer == null)
            {
                showError("Выбранный тип файла загрузчиком не поддерживается");
                cti.ThreadProgress.Close();
                return;
            }
            if (_pgTable == null)
            {
                showError("Не выбрана таблицa базы данных");
                cti.ThreadProgress.Close();
                return;
            }

            List<PgM.PgFieldM> fields = new List<PgM.PgFieldM>();
            foreach (PgM.PgFieldM field in _pgTable.Fields)
            {
                if (field.Type != AbstractSource.Model.EFieldType.Geometry)
                {
                    fields.Add(field); 
                }
            }
            importGrid.DataContext = fields; 
            try
            {   
                importDT = importer.GetPreviewTable();
                countsTB.Text = "Загрyжено строк для предпросмотра: 0  Строк всего: 0";
                importGrid.ItemsSource = importDT.DefaultView;
                countsTB.Text = string.Format("Загрyжено строк для предпросмотра: {0}  Строк всего: {1}", importDT.Rows.Count, importer.RowsCount);

                previewBtn.IsDefault = false;
                loadBtn.IsDefault = true;
                loadBtn.Focus();
            }
            catch (Exception ex)
            {
                showError(ex.Message);
                return;
            }
            finally
            {
                cti.ThreadProgress.Close();
            }
            colsTim.Start();
        }

        #region служебные
        public static List<T> FindChild<T>(DependencyObject parent, string childName) where T : DependencyObject
        {
            // Confirm parent and childName are valid. 
            if (parent == null) return null;

            List<T> foundChild = new List<T>();

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child == null)
                    continue;
                // If the child is not of the request child type child
                T childType = child as T;
                if (childType == null)
                {
                    // recursively drill down the tree
                    foundChild.AddRange(FindChild<T>(child, childName));
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    // If the child's name is set for search
                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        // if the child's name is of the request name
                        foundChild.Add((T)child);
                    }
                    else if (frameworkElement != null)
                        // recursively drill down the tree
                        foundChild.AddRange(FindChild<T>(child, childName));
                }
                else
                {
                    // child element found.
                    foundChild.Add((T)child);
                    break;
                }
            }
            return foundChild;
        }
        private void showError(string message, bool deleteData = true)
        {
            errorTb.Text = message;
            errorTb.Visibility = Visibility.Visible;
            if (deleteData)
            {
                previewBtn.IsDefault = true;
                loadBtn.IsDefault = false;
                importDT = null;
                importGrid.DataContext = null;
                importGrid.ItemsSource = null;
                countsTB.Text = "Загрyжено строк для предпросмотра: 0  Строк всего: 0";
            }
            else
            {
                previewBtn.IsDefault = false;
                loadBtn.IsDefault = true;
            }
        }
        private void hideError()
        {
            errorTb.Visibility = Visibility.Collapsed;
        }
        private void showSuccess()
        {
            successTb.Visibility = Visibility.Visible;
            loadBtn.IsDefault = false;
            previewBtn.IsDefault = true;
            previewBtn.Focus();
        }
        private void hideSuccess()
        {
            successTb.Visibility = Visibility.Collapsed;
        }
        #endregion

        private void load(object sender, RoutedEventArgs e)
        {
            cti.ThreadProgress.ShowWait(progressKey);
            hideError();
            hideSuccess();
            try
            {
                var fields = new List<FieldMatch>();
                var list = FindChild<ComboBox>(importGrid, "headerCB");
                for (int i = 1; i < list.Count; i++)
                {
                    var cb = list[i];
                    if (cb.SelectedItem != null)
                        fields.Add(new FieldMatch() { Dest = (PgM.PgFieldM)cb.SelectedItem, Src = importDT.Columns[i - 1].Caption });
                }
                loadBtn.IsEnabled = previewBtn.IsEnabled = false;
                importer.Load(fields);
            }
            catch (Exception ex)
            {
                cti.ThreadProgress.Close(progressKey);
                showError(ex.Message, false);
                loadBtn.IsEnabled = previewBtn.IsEnabled = true;
            }
        }
        void importer_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if (e.Result == null)
                showSuccess();
            else showError(e.Result.ToString(), false);
            cti.ThreadProgress.Close(progressKey);
            loadBtn.IsEnabled = previewBtn.IsEnabled = true;
        }
        void importer_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            cti.ThreadProgress.SetText(string.Format("Обработано объектов {0} из {1}", (int)e.UserState + 1, importer.RowsCount));
        }
        private void dbTableCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            loadBtn.IsDefault = false;
            previewBtn.IsDefault = true;
        }
        private void headerCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox senderCb = sender as ComboBox;
            List<ComboBox> headers = FindChild<ComboBox>(importGrid, "headerCB");
            for (int i = 1; i < headers.Count; i++)
                headers[i].BorderBrush = Brushes.LightGray;
            for (int i = 1; i < headers.Count; i++)
                for (int j = i + 1; j < headers.Count; j++)
                    if (headers[i].SelectedItem != null && headers[j].SelectedItem != null &&
                        headers[i].SelectedItem == headers[j].SelectedItem)
                    {
                        headers[i].BorderBrush = Brushes.Red;
                        headers[j].BorderBrush = Brushes.Red;
                    }
        }
    }
}