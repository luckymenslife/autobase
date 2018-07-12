using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Threading;

namespace Rekod.Controllers
{
    /// <summary>
    /// Мультипоточная коллеция ObservableCollection
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MTObservableCollection<T> : ObservableCollection<T>
    {
        #region Поля
        bool _T_is_INotifyPropertyChanged;
        #endregion // Поля

        #region События
        public event PropertyChangedEventHandler ItemPropertyChanged;

        public override event NotifyCollectionChangedEventHandler CollectionChanged;
        #endregion // События

        #region Конструктор
        public MTObservableCollection()
            : base()
        {
            var type = typeof(T);
            var interfaces = type.GetInterfaces();

            _T_is_INotifyPropertyChanged = ChangeTypeInterface(typeof(T), typeof(INotifyPropertyChanged));

        }
        #endregion // Конструктор

        #region Методы
        private bool ChangeTypeInterface(Type t, Type interfaceType)
        {
            var tInterfaceType = t.GetInterfaces();
            for (int i = 0; i < tInterfaceType.Count(); i++)
            {
                if (tInterfaceType[i] == interfaceType)
                    return true;
            }
            return false;
        }

        #endregion // Методы

        #region Методы ObservableCollection<T>
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            var eh = CollectionChanged;
            if (eh != null)
            {
                Dispatcher dispatcher = (from NotifyCollectionChangedEventHandler nh in eh.GetInvocationList()
                                         let dpo = nh.Target as DispatcherObject
                                         where dpo != null
                                         select dpo.Dispatcher).FirstOrDefault();

                if (dispatcher != null && dispatcher.CheckAccess() == false)
                {
                    dispatcher.Invoke(DispatcherPriority.DataBind, (Action)(() => OnCollectionChanged(e)));
                }
                else
                {
                    foreach (NotifyCollectionChangedEventHandler nh in eh.GetInvocationList())
                    {
                        nh.Invoke(this, e);
                    }
                }
            }
        }

        protected override void ClearItems()
        {
            if (_T_is_INotifyPropertyChanged)
            {
                foreach (INotifyPropertyChanged item in this)
                {
                    item.PropertyChanged -= MTObservableCollection_ItemPropertyChanged;
                }
            }
            base.ClearItems();
        }
        protected override void InsertItem(int index, T item)
        {
            if (_T_is_INotifyPropertyChanged)
            {
                ((INotifyPropertyChanged)item).PropertyChanged += MTObservableCollection_ItemPropertyChanged;
            }
            base.InsertItem(index, item);
        }
        protected override void RemoveItem(int index)
        {
            if (_T_is_INotifyPropertyChanged)
            {
                ((INotifyPropertyChanged)this[index]).PropertyChanged -= MTObservableCollection_ItemPropertyChanged;
            }
            base.RemoveItem(index);
        }
        protected override void SetItem(int index, T item)
        {
            if (_T_is_INotifyPropertyChanged)
            {
                ((INotifyPropertyChanged)this[index]).PropertyChanged -= MTObservableCollection_ItemPropertyChanged;
                ((INotifyPropertyChanged)item).PropertyChanged += MTObservableCollection_ItemPropertyChanged;
            }
            base.SetItem(index, item);
        }
        #endregion // Методы ObservableCollection<T>

        #region Обработчики событий
        void MTObservableCollection_ItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (ItemPropertyChanged != null)
            {
                ItemPropertyChanged(sender, e);
            }
        }
        #endregion // Обработчик событий
    }
}