using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rekod.Controllers;


namespace Rekod.RasterGeoreferenceModule.Model
{
    public class LinkedPointM: ViewModelBase
    {
        #region Поля
        private int _rastrX;
        private int _rastrY;
        private double _mapX;
        private double _mapY;
        private bool _isSelected;
        private bool _isActive;
        private string _name;
        #endregion Поля
        
        #region Конструкторы
        public LinkedPointM()
        {
        }
        public LinkedPointM(string name, int rastrX, int rastrY, double mapX, double mapY)
        {
            _name = name;
            _rastrX = rastrX;
            _rastrY = rastrY;
            _mapX = mapX;
            _mapY = mapY;
        }
        #endregion Конструкторы

        #region Свойства
        public String Name
        {
            get { return _name; }
            set { OnPropertyChanged(ref _name, value, () => this.Name); }
        }
        public int RastrX
        {
            get { return _rastrX; }
            set { OnPropertyChanged(ref _rastrX, value, () => this.RastrX); }
        }
        public int RastrY
        {
            get { return _rastrY; }
            set { OnPropertyChanged(ref _rastrY, value, () => this.RastrY); }
        }
        public double MapX
        {
            get { return _mapX; }
            set { OnPropertyChanged(ref _mapX, value, () => this.MapX); }
        }
        public double MapY
        {
            get { return _mapY; }
            set { OnPropertyChanged(ref _mapY, value, () => this.MapY); }
        }
        public bool IsSelected
        {
            get { return _isSelected; }
            set { OnPropertyChanged(ref _isSelected, value, () => this.IsSelected); }
        }
        public bool IsActive
        {
            get { return _isActive; }
            set { OnPropertyChanged(ref _isActive, value, () => this.IsActive); }
        }
        #endregion Свойства
    }
}