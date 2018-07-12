using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rekod.DataAccess.AbstractSource.ViewModel;
using System.Threading;
using Rekod.Controllers;

namespace Rekod.DataAccess.SourcePostgres.Model
{
    public class PgExtentM : ViewModelBase
    {
        #region Поля
        private int _id;
        private String _name;
        private double _bottomLeftX;
        private double _bottomLeftY;
        private double _topRightX;
        private double _topRightY;
        #endregion Поля

        #region Конструкторы
        public PgExtentM(int id, String name, String extent)
        {
            _id = id;
            _name = name;
            SetExtent(extent); 
        }
        #endregion Конструкторы

        #region Свойства
        public int Id
        {
            get
            {
                return _id; 
            }
            set
            {
                OnPropertyChanged(ref _id, value, () => this.Id);
            }
        }

        public String Name
        {
            get
            {
                return _name;
            }
            set
            {
                OnPropertyChanged(ref _name, value, () => this.Name);
            }
        }

        public Double BottomLeftX
        {
            get
            {
                return _bottomLeftX;
            }
            set
            {
                OnPropertyChanged(ref _bottomLeftX, value, () => this.BottomLeftX);
            }
        }

        public Double BottomLeftY
        {
            get
            {
                return _bottomLeftY;
            }
            set 
            {
                OnPropertyChanged(ref _bottomLeftY, value, () => this.BottomLeftY);
            }
        }

        public Double TopRightX
        {
            get
            {
                return _topRightX; 
            }
            set
            {
                OnPropertyChanged(ref _topRightX, value, () => this.TopRightX);
            }
        }

        public Double TopRightY
        {
            get
            {
                return _topRightY;
            }
            set
            {
                OnPropertyChanged(ref _topRightY, value, () => this.TopRightY);
            }
        }
        #endregion Свойства

        #region Методы
        public void SetExtent(String extent)
        {
            String[] coords = extent.Split(new[] { ',' });
            String sep = Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            BottomLeftX = Convert.ToDouble(coords[0].Replace(".", sep));
            BottomLeftY = Convert.ToDouble(coords[1].Replace(".", sep));
            TopRightX = Convert.ToDouble(coords[2].Replace(".", sep));
            TopRightY = Convert.ToDouble(coords[3].Replace(".", sep));
        }
        #endregion Методы
    }
}