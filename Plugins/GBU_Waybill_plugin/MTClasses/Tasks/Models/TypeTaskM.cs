using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace GBU_Waybill_plugin.MTClasses.Tasks.Models
{
    public class TypeTaskM : SelectableClass
    {
        #region Поля
        private int _id;
        private string _name;
        #endregion

        #region Конструктор
        public TypeTaskM(int id, string name)
        {
            _id = id;
            _name = name;
        }
        #endregion

        #region Свойства
        public int Id
        {
            get { return _id; }
        }
        public string Name
        {
            get { return _name; }
        }
        #endregion
    }
    public class TypeTaskWithCarsM : TypeTaskM, IDisposable
    {
        #region Поля
        ObservableCollection<CarM> _cars = new ObservableCollection<CarM>();
        #endregion

        public TypeTaskWithCarsM(int id, string name)
            : base(id, name)
        {
        }

        #region Свойства
        public ObservableCollection<CarM> Cars
        {
            get { return _cars; }
        }
        public string CarsCount
        {
            get
            {
                int count = Cars.Count(w => w.Selected);
                if (count == 0)
                    return "";
                else
                    return count.ToString();
            }
        }
        #endregion

        void car_selected_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnPropertyChanged("CarsCount");
        }
        public void RemoveHandlerCar()
        {
            foreach (var item in Cars)
            {
                item.PropertyChanged -= car_selected_PropertyChanged;
            }
        }
        public void AddHandlerCar()
        {
            foreach (var item in Cars)
            {
                item.PropertyChanged += car_selected_PropertyChanged;
            }
        }

        public void Dispose()
        {
            RemoveHandlerCar();
        }
    }
}
