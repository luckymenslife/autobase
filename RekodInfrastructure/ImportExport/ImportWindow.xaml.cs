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

namespace Rekod.ImportExport
{
    /// <summary>
    /// Interaction logic for ImportWindow.xaml
    /// </summary>
    public partial class ImportWindow : Window
    {
        Importer importer;
        private DispatcherTimer colsTim;
        private DataTable importDT;
        private tablesInfo _tablesInfo;
        private readonly string progressKey;
        public ImportWindow(FileInfo inputFile, tablesInfo dbTable = null)
        {
            InitializeComponent();
            _tablesInfo = dbTable;
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
                case ".kml":
                case ".mif":
                case ".xml":
                case ".gxt":
                case ".dxf":
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
                showError(Rekod.Properties.Resources.ImportWindow_NotSupportExtension);
                return;
            }
            importer.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(importer_ProgressChanged);
            importer.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(importer_RunWorkerCompleted);
            progressKey = "importProgress" + DateTime.Now.Ticks.ToString();
            settingsSP.Children.Clear();
            try
            {
                if (importer.SettingsPanel != null)
                    settingsSP.Children.Add(importer.SettingsPanel);
                importer.Init(inputFile, dbTable);
            }
            catch (Exception ex)
            {
                showError(ex.Message);
                loadBtn.IsEnabled = false;
                return;
            }
            //dbTableCB.ItemsSource = Program.tables_info;
            //dbTableCB.Focus();
            if (dbTable != null)
            {
                //dbTableCB.SelectedItem = dbTable;
                dbTableCB.Text = dbTable.nameMap;
                //dbTableCB.IsEnabled = false;
            }
            preview();
        }

        void loadCols(object sender, EventArgs e)
        {
            colsTim.Stop();
            var list = FindChild<ComboBox>(importGrid, "headerCB");
            var checkBoxList = FindChild<CheckBox>(importGrid, "LoadField");
            string ext = null;
            if (inputFileTb.Text.Contains('.'))
                ext = inputFileTb.Text.Substring(inputFileTb.Text.LastIndexOf('.'));
            for (int i = 1; i < list.Count & i < importDT.Columns.Count; i++)
            {
                try
                {
                    for (int j = 0; j < list[i].Items.Count; j++)
                    {
                        if (((Interfaces.fieldInfo)list[i].Items[j]).nameDB.ToLower().StartsWith(importDT.Columns[i].ColumnName.ToLower())
                            || (ext == ".xls" || ext == ".xlsx") &&
                            ((Interfaces.fieldInfo)list[i].Items[j]).nameMap.ToLower().StartsWith(importDT.Columns[i].ColumnName.ToLower()))
                        {
                            list[i + 1].SelectedItem = list[i].Items[j];
                            checkBoxList[i + 1].IsChecked = true;
                            break;
                        }
                    }
                }
                catch { continue; }
            }
        }

        private void preview()
        {
            cti.ThreadProgress.ShowWait();
            hideError();
            hideSuccess();
            if (importer == null)
            {
                showError(Rekod.Properties.Resources.ImportWindow_NotSupportExtension);
                cti.ThreadProgress.Close();
                return;
            }
            //if (dbTableCB.SelectedItem == null)
            if (_tablesInfo == null)
            {
                showError(Rekod.Properties.Resources.ImportWindow_NotSelectTableBD);
                cti.ThreadProgress.Close();
                return;
            }
            //importGrid.DataContext = Program.app.getTableInfo(((tablesInfo)dbTableCB.SelectedItem).idTable).ListField;
            var lf = Program.app.getTableInfo(_tablesInfo.idTable).ListField;
            lf.Remove(lf.Find(w => w.nameMap == _tablesInfo.geomFieldName));
            lf.Remove(lf.Find(w => w.nameDB == _tablesInfo.pkField));

            importGrid.DataContext = lf.ToArray();
            try
            {
                importDT = importer.GetPreviewTable();
                importGrid.ItemsSource = importDT.DefaultView;
                countsTB.Text = string.Format(Rekod.Properties.Resources.ImportWindow_Status, importDT.Rows.Count, importer.RowsCount);

                loadBtn.IsDefault = true;
                loadBtn.Focus();
            }
            catch (Exception ex)
            {
                countsTB.Text = string.Format(Rekod.Properties.Resources.ImportWindow_Status, 0, 0);
                showError(ex.Message);
                loadBtn.IsEnabled = false;
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
                loadBtn.IsDefault = false;
                importDT = null;
                importGrid.DataContext = null;
                importGrid.ItemsSource = null;
                countsTB.Text = string.Format(Rekod.Properties.Resources.ImportWindow_Status, 0,0);
            }
            else
            {
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
                List<StackPanel> stackPanelsList = FindChild<StackPanel>(importGrid, "HeaderStackPanel");

                for (int i = 1; i < stackPanelsList.Count; i++)
                {
                    StackPanel stackPanel = stackPanelsList[i];
                    CheckBox doLoadBox = stackPanel.Children[0] as CheckBox;
                    ComboBox selectedFieldBox = stackPanel.Children[2] as ComboBox;
                    if ((bool)doLoadBox.IsChecked && selectedFieldBox.SelectedItem != null)
                    {
                        fields.Add(new FieldMatch() { Dest = (Interfaces.fieldInfo)selectedFieldBox.SelectedItem, Src = importDT.Columns[i - 1].Caption });
                    }
                }
                loadBtn.IsEnabled = false;
                importer.Load(fields);
            }
            catch (Exception ex)
            {
                cti.ThreadProgress.Close(progressKey);
                showError(ex.Message, false);
                loadBtn.IsEnabled = true;
            }
        }

        void importer_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if (e.Result == null)
                showSuccess();
            else showError(e.Result.ToString(), false);
            cti.ThreadProgress.Close(progressKey);
            loadBtn.IsEnabled = true;
        }

        void importer_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            //if (((int)e.UserState + 1) % 100 == 0)
            {
                cti.ThreadProgress.SetText(string.Format(Rekod.Properties.Resources.ImportWindow_StatusProcess, (int)e.UserState + 1, importer.RowsCount));
            }
        }

        private void dbTableCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            loadBtn.IsDefault = false;
        }

        private void headerCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            checkComboBoxes();
        }
        private void LoadField_CheckedChanged(object sender, RoutedEventArgs e)
        {
            checkComboBoxes();
        }
        private void checkComboBoxes()
        {
            List<StackPanel> stackPanelsList = FindChild<StackPanel>(importGrid, "HeaderStackPanel");
            List<ComboBox> selectedFieldBoxesList = new List<ComboBox>();
            List<CheckBox> doLoadBoxesList = new List<CheckBox>();

            for (int i = 1; i < stackPanelsList.Count; i++)
            {
                StackPanel stackPanel = stackPanelsList[i];
                CheckBox doLoadBox = stackPanel.Children[0] as CheckBox;
                ComboBox selectedFieldBox = stackPanel.Children[2] as ComboBox;
                selectedFieldBoxesList.Add(selectedFieldBox);
                doLoadBoxesList.Add(doLoadBox);
                selectedFieldBox.BorderBrush = Brushes.LightGray;
            }

            for (int i = 0; i < selectedFieldBoxesList.Count; i++)
                for (int j = i; j < selectedFieldBoxesList.Count; j++)
                {

                    if (selectedFieldBoxesList[i] != selectedFieldBoxesList[j]
                            && selectedFieldBoxesList[i].SelectedItem != null
                            && selectedFieldBoxesList[j].SelectedItem != null
                            && selectedFieldBoxesList[i].SelectedItem == selectedFieldBoxesList[j].SelectedItem
                            && (bool)doLoadBoxesList[i].IsChecked
                            && (bool)doLoadBoxesList[j].IsChecked)
                    {
                        selectedFieldBoxesList[i].BorderBrush = Brushes.Red;
                        selectedFieldBoxesList[j].BorderBrush = Brushes.Red;
                    }
                }
        }

        private void importGrid_AutoGeneratedColumns(object sender, EventArgs e)
        {
            foreach (var column in importGrid.Columns)
            {
                column.Width = new DataGridLength(1, DataGridLengthUnitType.Star);
            }
        }


    }
}