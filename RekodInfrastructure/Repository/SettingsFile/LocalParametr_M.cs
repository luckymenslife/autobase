using Rekod.Controllers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Rekod.Repository.SettingsFile
{
    public class LocalParameters_M: ViewModelBase
    {
        #region Статические поля
        public static string Guid = "AE5F77D8-4738-4B74-9A96-9DAF22CBDFEA";
        #endregion Статические поля

        #region Поля
        private bool _enterTheScreen;
        private bool _showCoordinatesPanel;
        private bool _turnOffVMPWhenRastr;
        private bool _openAttrsAfterCreate;
        private CultureInfo _localeProgram;
        private ProxyParameters_M _proxy;
        #endregion Поля

        #region Конструкторы
        public LocalParameters_M()
        {
            PropertyChanged += LocalParameters_M_PropertyChanged;
        }
        #endregion Конструкторы

        #region Свойства
        public bool EnterTheScreen
        {
            get { return _enterTheScreen; }
            set { OnPropertyChanged(ref _enterTheScreen, value, () => this.EnterTheScreen); }
        }
        public bool ShowCoordinatesPanel
        {
            get { return _showCoordinatesPanel; }
            set { OnPropertyChanged(ref _showCoordinatesPanel, value, () => this.ShowCoordinatesPanel); }
        }
        public bool TurnOffVMPWhenRastr
        {
            get { return _turnOffVMPWhenRastr; }
            set { OnPropertyChanged(ref _turnOffVMPWhenRastr, value, () => this.TurnOffVMPWhenRastr); }
        }
        public bool OpenAttrsAfterCreate
        {
            get { return _openAttrsAfterCreate; }
            set { OnPropertyChanged(ref _openAttrsAfterCreate, value, () => this.OpenAttrsAfterCreate); }
        }
        public CultureInfo LocaleProgram
        {
            get { return _localeProgram; }
            set { OnPropertyChanged(ref _localeProgram, value, () => this.LocaleProgram); }
        }
        public ProxyParameters_M Proxy
        {
            get { return _proxy; }
            set { OnPropertyChanged(ref _proxy, value, () => this.Proxy); }
        }
        #endregion Свойства

        #region Обработчики
        void LocalParameters_M_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (Program.SettingsXML != null)
            {
                Program.SettingsXML.ApplyLocalParameters();
            }
        }
        #endregion Обработчики
    }
}