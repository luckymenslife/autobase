using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AbsVM = Rekod.DataAccess.AbstractSource.ViewModel;
using System.Collections.ObjectModel;
using PgVM = Rekod.DataAccess.SourcePostgres.ViewModel;
using Rekod.Controllers;

namespace Rekod.DataAccess.SourcePostgres.Model.PgHistory
{
    public class PgHistoryEvent: ViewModelBase
    {
        #region Поля
        private ObservableCollection<PgHistoryAttribute> _attributes;
        private Boolean _showGeometryBefore = true;
        private Boolean? _restoreAllValues = null;
        #endregion Поля

        #region Конструкторы
        public PgHistoryEvent(PgHistoryDate parentHistoryDate)
        {
            ParentHistoryDate = parentHistoryDate;
            PropertyChanged += PgHistoryEvent_PropertyChanged;
            Attributes.CollectionChanged += Attributes_CollectionChanged;
        }
        #endregion Конструкторы

        #region Коллекции
        public ObservableCollection<PgHistoryAttribute> Attributes
        {
            get { return _attributes ?? (_attributes = new ObservableCollection<PgHistoryAttribute>()); }
        }
        #endregion Коллекции

        #region Свойства
        public PgHistoryDate ParentHistoryDate
        {
            get;
            private set;
        }
        public String UserName
        {
            get; 
            set; 
        }
        public int IdHistoryObject
        {
            get;
            set;
        }
        public PgHistoryTypeOperation TypeOperation
        {
            get;
            set; 
        }
        public int IdHistoryTable
        {
            get;
            set; 
        }
        public int IdTable
        {
            get;
            set;
        }
        public String TableName
        {
            get;
            set; 
        }
        public int IdObject
        {
            get;
            set;
        }
        public DateTime ChangeTime
        {
            get;
            set; 
        }
        public Boolean ShowGeometryBefore
        {
            get { return _showGeometryBefore; }
            set { OnPropertyChanged(ref _showGeometryBefore, value, () => this.ShowGeometryBefore); }
        }
        public PgVM.PgAttributes.PgAttributesListVM AfterValuesListVM
        {
            get;
            set; 
        }
        public Boolean? RestoreAllValues
        {
            get { return _restoreAllValues; }
            set 
            {
                if (_restoreAllValues == value)
                {
                    return;
                }
                OnPropertyChanged(ref _restoreAllValues, value, () => this.RestoreAllValues); 
            }
        }
        #endregion Свойства

        #region Методы
        public override string ToString()
        {              
            return String.Format("{0} {1} <{2}> ({3})",
                String.Format("{0:HH:mm:ss}", ChangeTime),
                UserName,
                TableName,
                IdObject);
        }
        public void UpdateRestoreAllValues()
        {
            if (Attributes.Count == 0)
            {
                RestoreAllValues = false;
            }
            else
            {
                int trueCount = 0;
                foreach (var attr in Attributes)
                {
                    if (attr.RestoreValues)
                    {
                        trueCount++;
                    }
                }
                if (trueCount == 0)
                {
                    RestoreAllValues = false;
                }
                else if (Attributes.Count == trueCount)
                {
                    RestoreAllValues = true;
                }
                else {
                    RestoreAllValues = null;
                }
            }
        }
        #endregion Методы

        #region Обработчики
        void PgHistoryEvent_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "RestoreAllValues")
            {
                if (RestoreAllValues != null)
                {
                    bool restoreAllValue = (bool)RestoreAllValues; 
                    foreach (var attr in Attributes)
                    {
                        attr.RestoreValues = restoreAllValue; 
                    }
                }
            }
        }
        void Attributes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (var attrObject in e.NewItems)
                {
                    PgHistoryAttribute attr = attrObject as PgHistoryAttribute;
                    attr.PropertyChanged += Attribute_PropertyChanged;
                }
            }
            if (e.OldItems != null)
            {
                foreach (var attrObject in e.OldItems)
                {
                    PgHistoryAttribute attr = attrObject as PgHistoryAttribute;
                    attr.PropertyChanged -= Attribute_PropertyChanged;
                }
            }
            UpdateRestoreAllValues();
        }
        void Attribute_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "RestoreValues")
            {
                UpdateRestoreAllValues();
            }
        }
        #endregion Обработчики
    }
    public enum PgHistoryTypeOperation
    {
        Insert = 1, Update = 2, Delete = 3
    }
}