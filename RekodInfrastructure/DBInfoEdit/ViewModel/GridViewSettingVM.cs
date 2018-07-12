using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Microsoft.Office.Interop.Excel;
using Rekod.Controllers;

namespace Rekod.DBInfoEdit.ViewModel
{
    class GridViewSettingVM : ViewModelBase
    {
        #region Sub

        public class Column : ViewModelBase
        {
            private bool _isChecked = true;
            private bool _isEnabled = true;
            public string Name { get; set; }
            public string Text { get; set; }

            public bool IsChecked
            {
                get { return _isChecked; }
                set
                {
                    if (!IsEnabled)
                    {
                        return;
                    }

                    _isChecked = value;
                    OnPropertyChanged(()=>this.IsChecked);
                    OnIsCheckChanged();
                }
            }

            public event EventHandler IsCheckChanged;

            private void OnIsCheckChanged()
            {
                if (IsCheckChanged != null)
                {
                    IsCheckChanged(this, new EventArgs());
                }
            }

            public bool IsEnabled
            {
                get { return _isEnabled; }
                set { _isEnabled = value; }
            }
        }

        private enum Direction
        {
            Up,
            Down
        } 

        #endregion //Sub

        #region Fields

        private GridViewSettingWindow _currentWindow;
        private ObservableCollection<Column> _columns;
        private RelayCommand _moveUpCommand;
        private RelayCommand _moveDownCommand;
        private RelayCommand _selectAllCommand;
        private RelayCommand _selectNoneCommand;
        private RelayCommand _acceptCommand;
        private RelayCommand _cancelCommand;
        private bool _checkAll;

        #endregion //Fields

        #region Constructors

        public GridViewSettingVM(IEnumerable<Column> columns)
        {
            IsCanceled = true;
            SelectedIndex = 0;

            _columns = new ObservableCollection<Column>(columns);
            foreach (var column in _columns)
            {
                column.IsCheckChanged += (sender, args) =>
                {
                    _checkAll = false;
                    OnPropertyChanged(()=>this.CheckAll);
                };
            }

            _currentWindow = new GridViewSettingWindow {DataContext = this};
            _currentWindow.ShowDialog();
        }

        #endregion //Constructors

        #region Properties

        public ObservableCollection<Column> ColumnList
        {
            get { return _columns; }
        }

        public bool IsCanceled { get; set; }

        public int SelectedIndex { get; set; }

        public bool CheckAll
        {
            get
            {
                return _checkAll;
            }
            set
            {
                foreach (var column in ColumnList)
                {
                    column.IsChecked = value;
                }
                _checkAll = value;
                OnPropertyChanged(() => this.CheckAll);
            }
        }

        #endregion //Properties

        #region Commands

        public RelayCommand MoveUpCommand
        {
            get { return _moveUpCommand ?? (_moveUpCommand = new RelayCommand((obj) => this.Move(Direction.Up), param => this.CanMove(Direction.Up))); }
        }

        public RelayCommand MoveDownCommand
        {
            get { return _moveDownCommand ?? (_moveDownCommand = new RelayCommand((obj) => this.Move(Direction.Down), param => this.CanMove(Direction.Down))); }
        }

        public RelayCommand AcceptCommand
        {
            get
            {
                return _acceptCommand
                    ?? (_acceptCommand = new RelayCommand(o =>
                    {
                        IsCanceled = false;
                        _currentWindow.Close();
                    }));
            }
        }

        public RelayCommand CancelCommand
        {
            get
            {
                return _cancelCommand 
                    ?? (_cancelCommand = new RelayCommand(o => _currentWindow.Close()));
            }
        }

        public RelayCommand SelectAllCommand
        {
            get
            {
                return _selectAllCommand
                    ?? (_selectAllCommand = new RelayCommand(o =>
                    {
                        foreach (var column in ColumnList)
                        {
                            column.IsChecked = true;
                        }
                    }));
            }
        }

        public RelayCommand SelectNoneCommand
        {
            get
            {
                return _selectNoneCommand
                    ?? (_selectNoneCommand = new RelayCommand(o =>
                    {
                        foreach (var column in ColumnList)
                        {
                            column.IsChecked = false;
                        }
                    }));
            }
        }

        #endregion //Commands

        #region Command methods
        private bool CanMove(Direction direction)
        {
            bool result;
            switch (direction)
            {
                case Direction.Up:
                    result = (SelectedIndex > 0);
                    break;
                case Direction.Down:
                    result = (SelectedIndex < ColumnList.Count - 1);
                    break;
                default:
                    result = false;
                    break;
            }
            return result;
        }

        private void Move(Direction direction)
        {
            int index = SelectedIndex;
            int newIndex = (direction == Direction.Up)
                ? index - 1
                : index + 1;

            var columnItem = ColumnList[index];

            ColumnList.Remove(columnItem);
            ColumnList.Insert(newIndex, columnItem);

            SelectedIndex = newIndex;

            OnPropertyChanged(()=>this.ColumnList);
        }

        #endregion //Command methods
    }
}
