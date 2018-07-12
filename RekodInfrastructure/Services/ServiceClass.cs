using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AbsM = Rekod.DataAccess.AbstractSource.Model;
using AbsVM = Rekod.DataAccess.AbstractSource.ViewModel;
using System.Windows.Input;
using System.Windows.Data;
using System.ComponentModel;
using Rekod.Controllers;
using System.Windows;
using Rekod.Behaviors;
using System.Windows.Controls;

namespace Rekod.Services
{
    public static class ServiceClass
    {
        #region Команды
        #region FilterTablesCommand
        private static ICommand _filterTablesCommand;
        /// <summary>
        /// Команда для фильтрования таблиц
        /// </summary>
        public static ICommand FilterTablesCommand
        {
            get { return _filterTablesCommand ?? (_filterTablesCommand = new RelayCommand(FilterTables, CanFilterTables)); }
        }
        
        /// <summary>
        /// Фильтр таблиц
        /// </summary>
        public static void FilterTables(object parameter = null)
        {
            if (parameter is Rekod.Behaviors.CommandEventParameter)
            {
                Rekod.Behaviors.CommandEventParameter commEventParameter =
                    parameter as Rekod.Behaviors.CommandEventParameter;
                if (commEventParameter.CommandParameter != null && commEventParameter.EventSender is System.Windows.Controls.TextBox)
                {
                    System.Windows.Controls.TextBox textBox =
                        commEventParameter.EventSender as System.Windows.Controls.TextBox;

                    List<String> propertyNames = new List<string>() { "Text" }; 
                    if(commEventParameter.ExtraParameter != null)
                    {
                        propertyNames.Clear();
                        propertyNames.AddRange(commEventParameter.ExtraParameter.ToString().Split(new[] { '+' }));
                    }
                    ICollectionView defView = CollectionViewSource.GetDefaultView(commEventParameter.CommandParameter);
                    if (defView != null)
                    {
                        defView.Filter = delegate(object o)
                        {
                            bool contains = false;
                            foreach (String propertyName in propertyNames)
                            {
                                String text = "";
                                try
                                {
                                    text = o.GetType().GetProperty(propertyName).GetValue(o, null).ToString();
                                }
                                catch
                                {
                                    contains = false;
                                }
                                contains |= text.ToUpper().Contains(textBox.Text.ToUpper()); 
                            }
                            return contains; 
                        };
                    }
                }
            }
        }
        /// <summary>
        /// Можно ли отфильтровать таблицы
        /// </summary>
        public static bool CanFilterTables(object parameter = null)
        {
            return true;
        }
        #endregion // FilterTablesCommand 
        
        #region FilterTreeCommand
        private static ICommand _filterTreeCommand;
        /// <summary>
        /// Фильтр дерева
        /// </summary>
        public static void FilterTree(object parameter = null)
        {
            if (parameter is Rekod.Behaviors.CommandEventParameter)
            {
                Rekod.Behaviors.CommandEventParameter commEventParameter =
                    parameter as Rekod.Behaviors.CommandEventParameter;
                if (commEventParameter.CommandParameter != null && commEventParameter.EventSender is System.Windows.Controls.TextBox)
                {
                    System.Windows.Controls.TextBox textBox =
                        commEventParameter.EventSender as System.Windows.Controls.TextBox;

                    string subList = "";
                    List<String> propertyNames = new List<string>() { "Text" };
                    if (commEventParameter.ExtraParameter != null)
                    {
                        // через "/" указывается свойство, в котором хранится список, который нужно отсортировать
                        string strParams = commEventParameter.ExtraParameter.ToString();
                        if (strParams.Contains('/'))
                        {
                            subList = strParams.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries).Last();
                            strParams = strParams.Remove(strParams.IndexOf('/'));
                        }
                        else return;

                        propertyNames.Clear();
                        propertyNames.AddRange(strParams.Split(new[] { '+' }));
                    }
                    ICollectionView defView = CollectionViewSource.GetDefaultView(commEventParameter.CommandParameter);
                    defView.Filter = delegate(object o)
                    {
                        bool contains = false;
                        foreach (String propertyName in propertyNames)
                        {
                            if (o.GetType().GetProperty(subList) == null)
                            {
                                String text = o.GetType().GetProperty(propertyName).GetValue(o, null).ToString();
                                contains |= text.ToUpper().Contains(textBox.Text.ToUpper());
                            }
                            else contains |= true;
                        }
                        return contains;
                    };
                    foreach (var i in defView)
                    {
                        var property = i.GetType().GetProperty(subList);
                        if (property != null)
                        {
                            var list = property.GetValue(i, null);
                            var defList = CollectionViewSource.GetDefaultView(list);
                            if (defList != null)
                            {
                                defList.Filter = delegate(object o)
                                {
                                    bool contains = false;
                                    foreach (String propertyName in propertyNames)
                                    {
                                        String text = o.GetType().GetProperty(propertyName).GetValue(o, null).ToString();
                                        contains |= text.ToUpper().Contains(textBox.Text.ToUpper());
                                    }
                                    return contains;
                                };
                            }
                        }
                    }

                }
            }
        }
        public static ICommand FilterTreeCommand
        {
            get { return _filterTreeCommand ?? (_filterTreeCommand = new RelayCommand(FilterTree, CanFilterTables)); }
        } 
        #endregion FilterTreeCommand

        #region FilterSourceTablesCommand
        private static String _filterText;
        private static Predicate<object> _filterPredicate = delegate(object o)
        {
            bool isNotHidden = !(o as AbsM.TableBaseM).IsHidden;
            bool textIncludes = (o as AbsM.TableBaseM).Text.ToUpper().Contains(_filterText.ToUpper());
            return isNotHidden && textIncludes;
        };
        private static ICommand _filterSourceTablesCommand;
        /// <summary>
        /// Команда для фильтрации слоев в источниках и в группах источников
        /// </summary>
        public static ICommand FilterSourceTablesCommand
        {
            get { return _filterSourceTablesCommand ?? (_filterSourceTablesCommand = new RelayCommand(FilterSourceTables, CanFilterSourceTables)); }
        }
        /// <summary>
        /// Фильтрация слоев в источниках и в группах источников
        /// </summary>
        public static void FilterSourceTables(object parameter = null)
        {
            CommandEventParameter commEvtParam = parameter as CommandEventParameter;
            List<object> commParams = commEvtParam.CommandParameter as List<object>;
            Rekod.DataAccess.TableManager.ViewModel.TableManagerVM
                tableManager = commParams[0] as Rekod.DataAccess.TableManager.ViewModel.TableManagerVM;            
            _filterText = commParams[1].ToString();
            if (tableManager != null)
            {
                foreach (AbsVM.DataRepositoryVM repo in tableManager.DataRepositories)
                {
                    ICollectionView defView = CollectionViewSource.GetDefaultView(repo.Layers);
                    defView.Filter = _filterPredicate;
                    foreach (AbsM.GroupM group in repo.Groups)
                    {
                        defView = CollectionViewSource.GetDefaultView(group.Tables);
                        defView.Filter = _filterPredicate;
                    }
                }
                ICollectionView defVisView = CollectionViewSource.GetDefaultView(tableManager.VisibleLayersGroup[0].Tables);
                defVisView.Filter = _filterPredicate;
            }
        }
        /// <summary>
        /// Можно ли отфильтровать в источниках и в группах источников
        /// </summary>
        public static bool CanFilterSourceTables(object parameter = null)
        {
            return true;
        }
        #endregion // FilterSourceTablesCommand

        #region CopyToClipboardCommand
        private static ICommand _copyToClipboardCommand;
        /// <summary>
        /// Команда для копирования переданного параметра в буфер обмена
        /// </summary>
        public static ICommand CopyToClipboardCommand
        {
            get { return _copyToClipboardCommand ?? (_copyToClipboardCommand = new RelayCommand(CopyToClipboard, CanCopyToClipboard)); }
        }
        /// <summary>
        /// Копирование параметра в буфер обмена
        /// </summary>
        public static void CopyToClipboard(object parameter = null)
        {
            Clipboard.SetDataObject(parameter);
        }
        /// <summary>
        /// Можно ли скопировать параметра в буфер обмена
        /// </summary>
        public static bool CanCopyToClipboard(object parameter = null)
        {
            return true;
        }
        #endregion // CopyToClipboardCommand

        #region FontDialogCommand
        private static ICommand _fontDialogCommand;
        /// <summary>
        /// Команда для открытия и выбора параметров шрифта
        /// </summary>
        public static ICommand FontDialogCommand
        {
            get { return _fontDialogCommand ?? (_fontDialogCommand = new RelayCommand(FontDialog, CanFontDialog)); }
        }
        /// <summary>
        /// Открытие параметров шрифта
        /// </summary>
        public static void FontDialog(object parameter = null)
        {
            List<object> commParams = null; 
            if (parameter is List<object>)
            {
                commParams = parameter as List<object>;
            }
            else if(parameter is CommandEventParameter)
            {
                CommandEventParameter commEvtParam = parameter as CommandEventParameter;
                commParams = commEvtParam.CommandParameter as List<object>;
            }
                        
            TextBox TextBoxFontName = commParams[0] as TextBox;
            TextBox TextBoxFontSize = commParams[1] as TextBox;
            using (System.Windows.Forms.FontDialog fontDialog = new System.Windows.Forms.FontDialog())
            {
                System.Drawing.FontStyle fs = System.Drawing.FontStyle.Regular;
                if (!String.IsNullOrEmpty(TextBoxFontName.Text) && Convert.ToInt32(TextBoxFontSize.Text) > 0)
                {
                    System.Drawing.Font font = new System.Drawing.Font(TextBoxFontName.Text, Convert.ToInt32(TextBoxFontSize.Text), fs);
                    fontDialog.Font = font;
                }
                if (fontDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    TextBoxFontName.Text = fontDialog.Font.Name;
                    TextBoxFontSize.Text = ((int)(fontDialog.Font.Size)).ToString();
                    TextBoxFontName.Focus();
                    TextBoxFontSize.Focus();
                }
            }
        }
        /// <summary>
        /// Можно ли открыть параметры шрифта
        /// </summary>
        public static bool CanFontDialog(object parameter = null)
        {
            return true;
        }
        #endregion // FontDialogCommand

        #region FilterItemsCommand
        private static ICommand _filterItemsCommand;
        /// <summary>
        /// Команда для фильтрации
        /// </summary>
        public static ICommand FilterItemsCommand
        {
            get { return _filterItemsCommand ?? (_filterItemsCommand = new RelayCommand(FilterItems, CanFilterItems)); }
        }
        /// <summary>
        /// Фильтрация
        /// </summary>
        public static void FilterItems(object parameter = null)
        {
            CommandEventParameter commEvtParam = parameter as CommandEventParameter;
            TextBox textBox = commEvtParam.EventSender as TextBox;
            List<object> commParams = commEvtParam.CommandParameter as List<object>;
            ItemsControl itemsControl = commParams[0] as ItemsControl;
            String filterParams = commParams[1] as String;
            Dictionary<int, string[]> filterLevels = GetFilterLevelsDictionary(filterParams);
            FilterItemsControl(itemsControl, 1, filterLevels, textBox.Text);
        }
        public static void FilterItemsControl(ItemsControl itemscontrol, int level, Dictionary<int, string[]> levelsparams, String filtertext)
        {
            bool filterThisLevel = levelsparams.ContainsKey(level);
            bool filterHigherLevel = (from int l in levelsparams.Keys where l > level select l).Count() > 0;

            if (filterThisLevel)
            {
                ICollectionView iColView = itemscontrol.Items as ICollectionView;
                iColView.Filter = (param) =>
                {
                    Boolean result = true;
                    foreach (String propName in levelsparams[level])
                    {
                        if (propName.Contains('='))
                        {
                            String findValue = propName.Split(new[] { '=' })[1];
                            String findName = propName.Split(new[] { '=' })[0];
                            object propValue = GetPropertyValue(findName, param);
                            result &= (propValue.ToString().ToUpper() == findValue.ToUpper());
                        }
                        else
                        {
                            object propValue = GetPropertyValue(propName, param);
                            result &= propValue.ToString().ToUpper().Contains(filtertext.ToUpper());
                        }
                    }
                    return result;
                };
            }

            if (filterHigherLevel)
            {
                foreach (object item in itemscontrol.Items)
                {
                    ItemsControl nextLevelItemsControl = itemscontrol.ItemContainerGenerator.ContainerFromItem(item) as ItemsControl;
                    FilterItemsControl(nextLevelItemsControl, level + 1, levelsparams, filtertext);
                }
            }
        }
        public static object GetPropertyValue(String propertyPath, object param)
        {
            String[] pathSplit = propertyPath.Split(new []{'.'});
            object currentObject = param;
            foreach (String pathSegment in pathSplit)
            {
                Type t = currentObject.GetType();
                currentObject = t.GetProperty(pathSegment).GetValue(currentObject, null);
            }
            return currentObject;
        }
        public static Dictionary<int, string[]> GetFilterLevelsDictionary(String filterparams)
        {
            Dictionary<int, string[]> filterLevels = new Dictionary<int, string[]>();
            String[] levelsParams = filterparams.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (String levelParams in levelsParams)
            {
                String[] paramsArray = levelParams.Split(new[] { ':' });
                String[] propsArray = paramsArray[1].Split(new[] { '+' });
                int level = Convert.ToInt32(paramsArray[0].Trim());
                filterLevels.Add(level, propsArray);
            }
            return filterLevels;
        }
        /// <summary>
        /// Можно ли отфильтровать
        /// </summary>
        public static bool CanFilterItems(object parameter = null)
        {
            return true;
        }
        #endregion FilterItemsCommand

        #region FilterComboboxCommand
        private static ICommand _filterComboboxCommand;
        /// <summary>
        /// Фильтр дерева
        /// </summary>
        public static void FilterCombobox(object parameter = null)
        {
            if (parameter is Rekod.Behaviors.CommandEventParameter)
            {
                Rekod.Behaviors.CommandEventParameter commEventParameter =
                    parameter as Rekod.Behaviors.CommandEventParameter;
                if (commEventParameter.CommandParameter != null && commEventParameter.CommandParameter is ComboBox)
                {
                    ComboBox cb = commEventParameter.CommandParameter as ComboBox;
                    ICollectionView defView = CollectionViewSource.GetDefaultView(cb.ItemsSource);

                    String filter = String.Empty;
                    if (cb.SelectedIndex == -1
                        && commEventParameter.EventSender is TextBox)
                    {
                        filter = (commEventParameter.EventSender as TextBox).Text;
                    }
                    else
                    {
                        if (defView != null)
                            defView.Filter = null;
                        return;
                    }

                    List<String> propertyNames = new List<string>();
                    if (commEventParameter.ExtraParameter != null)
                    {
                        propertyNames.Clear();
                        propertyNames.AddRange(commEventParameter.ExtraParameter.ToString().Split(new[] { '+' }));
                    }
                    else 
                        return;

                    if (defView != null)
                    {
                        defView.Filter = delegate(object o)
                        {
                            bool contains = false;
                            foreach (String propertyName in propertyNames)
                            {
                                String text = o.GetType().GetProperty(propertyName).GetValue(o, null).ToString();
                                contains |= text.ToUpper().Contains(filter.ToUpper());
                            }
                            return contains;
                        };
                    }
                }
            }
        }
        public static ICommand FilterComboboxCommand
        {
            get { return _filterComboboxCommand ?? (_filterComboboxCommand = new RelayCommand(FilterCombobox, CanFilterTables)); }
        }
        #endregion FilterComboboxCommand

        #endregion Команды
    }
}