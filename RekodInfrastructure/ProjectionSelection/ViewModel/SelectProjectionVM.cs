using System;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;
using Rekod.Controllers;
using Rekod.Services;

namespace Rekod.ProjectionSelection
{
    public class SelectProjectionVM : ViewModelBase
    {
        #region Поля
        private ObservableCollection<Projection> _projList;
        private Projection _selectedProj;
        private SelectProjectionV _frmProj;
        #endregion Поля

        #region Свойства
        /// <summary>
        /// Коллекция проекций
        /// </summary>
        public ObservableCollection<Projection> ProjTable
        {
            get { return _projList; }
        }
        /// <summary>
        /// Выбранная проекция
        /// </summary>
        public Projection SelectedProj
        {
            get { return _selectedProj; }
            set { OnPropertyChanged(ref _selectedProj, value, () => SelectedProj); }
        }
        #endregion Свойства

        #region Конструктор
        public SelectProjectionVM(SelectProjectionV frmProj)
        {
            this._frmProj = frmProj;
            this._projList = new ObservableCollection<Projection>();

            GetProjData();
        }
        #endregion Конструктор

        #region Методы
        /// <summary>
        /// Получение информации обо всех проекциях
        /// </summary>
        public void GetProjData()
        {
            using (var sqlCmd = new SqlWork())
            {
                sqlCmd.sql = "SELECT srid, auth_name, auth_srid, srtext, proj4text FROM spatial_ref_sys;";
                sqlCmd.ExecuteReader();

                while (sqlCmd.CanRead())
                {
                    Projection proj = new Projection(
                        sqlCmd.GetInt32("srid"),
                        sqlCmd.GetString("auth_name"),
                        sqlCmd.GetInt32("auth_srid"),
                        sqlCmd.GetString("srtext"),
                        sqlCmd.GetString("proj4text"),
                        true);

                    _projList.Add(proj);
                }
            }
        }
        #endregion Методы

        #region Команды
        /// <summary>
        /// Очистить фильтр
        /// </summary>
        private ICommand _clearFilterCommand;
        public ICommand ClearFilterCommand
        {
            get { return _clearFilterCommand ?? (_clearFilterCommand = new RelayCommand(ClearFilter)); }
        }
        void ClearFilter(object parameter = null)
        {
            if (parameter is TextBox)
            {
                TextBox tbFilter = (TextBox)parameter;
                tbFilter.Text = "";
            }
        }
        
        /// <summary>
        /// Выбрать проекцию
        /// </summary>
        private ICommand _selectCommand;
        public ICommand SelectCommand
        {
            get { return _selectCommand ?? (_selectCommand = new RelayCommand(Select)); }
        }
        void Select(object parameter = null)
        {
            if (_selectedProj != null)
            {
                _frmProj.DialogResult = true;
                _frmProj.Close();
            }
        }

        /// <summary>
        /// Открыть окно информации о проекции
        /// </summary>
        private ICommand _openProjCommand;
        public ICommand OpenProjCommand
        {
            get { return _openProjCommand ?? (_openProjCommand = new RelayCommand(OpenProj)); }
        }
        void OpenProj(object parameter = null)
        {
            if (_selectedProj != null)
            {
                ObjectView objViewFrm = new ObjectView(_selectedProj);
                if (objViewFrm.ShowDialog() == true)
                {
                    Select();
                }
            }
        }
        #endregion Команды
    }
}
