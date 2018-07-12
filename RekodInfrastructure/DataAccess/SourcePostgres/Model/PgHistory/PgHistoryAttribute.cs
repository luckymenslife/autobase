using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using AbsVM = Rekod.DataAccess.AbstractSource.ViewModel;
using PgM = Rekod.DataAccess.SourcePostgres.Model;
using PgVM = Rekod.DataAccess.SourcePostgres.ViewModel;
using Rekod.Controllers;
using mvMapLib; 

namespace Rekod.DataAccess.SourcePostgres.Model.PgHistory
{
    public class PgHistoryAttribute: ViewModelBase
    {
        #region Поля
        private bool _showBeforeGeom;
        private bool _showAfterGeom;
        private bool _showActualGeom;
        private bool _restoreValues = true;
        #endregion Поля

        #region Конструкторы
        public PgHistoryAttribute(PgHistoryEvent parentEvent)
        {
            ParentEvent = parentEvent;
        }
        #endregion Конструкторы

        #region Свойства
        public PgM.PgFieldM Field
        {
            get;
            set;
        }
        public object BeforeValue
        {
            get;
            set;
        }
        public object AfterValue
        {
            get;
            set;
        }
        public object ActualValue
        {
            get;
            set;
        }
        public bool IsGeomField
        {
            get;
            set;
        }
        public bool AfterValueHasChanged
        {
            get
            {
                PgM.PgAttributes.PgAttributeM attrBefore = BeforeValue as PgM.PgAttributes.PgAttributeM;
                PgM.PgAttributes.PgAttributeM attrAfter = AfterValue as PgM.PgAttributes.PgAttributeM;
                return AttributesHaveDifferentValues(attrBefore, attrAfter);
            }
        }
        public bool ActualValueHasChanged
        {
            get
            {
                PgM.PgAttributes.PgAttributeM attrAfter = AfterValue as PgM.PgAttributes.PgAttributeM;
                PgM.PgAttributes.PgAttributeM attrActual = ActualValue as PgM.PgAttributes.PgAttributeM;
                return AttributesHaveDifferentValues(attrAfter, attrActual);
            }
        }
        public bool HasChanges
        {
            get { return (ActualValueHasChanged || AfterValueHasChanged); }
        }
        public bool HasBeforeValue
        {
            get
            {
                if (BeforeValue != null && (BeforeValue as PgM.PgAttributes.PgAttributeM).Value != null)
                {
                    return true;
                }
                else 
                {
                    return false; 
                }
            }
        }
        public bool HasAfterValue
        {
            get
            {
                if (AfterValue != null && (AfterValue as PgM.PgAttributes.PgAttributeM).Value != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public bool HasActualValue
        {
            get
            {
                if (ActualValue != null && (ActualValue as PgM.PgAttributes.PgAttributeM).Value != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public bool ShowBeforeGeom
        {
            get { return _showBeforeGeom; }
            set 
            {
                _showBeforeGeom = value;
                ParentEvent.ParentHistoryDate.ParentPgHistoryVM.SwitchGeometry(
                    HistoryAttributeValueType.Before, value?(BeforeValue as PgM.PgAttributes.PgAttributeM).Value.ToString():null); 
            }
        }
        public bool ShowAfterGeom
        {
            get 
            {
                return _showAfterGeom; 
            }
            set
            {
                _showAfterGeom = value;
                ParentEvent.ParentHistoryDate.ParentPgHistoryVM.SwitchGeometry(
                    HistoryAttributeValueType.After, value ? (AfterValue as PgM.PgAttributes.PgAttributeM).Value.ToString() : null);
            }
        }
        public bool ShowActualGeom
        {
            get { return _showActualGeom; }
            set
            {
                _showActualGeom = value;
                ParentEvent.ParentHistoryDate.ParentPgHistoryVM.SwitchGeometry(
                    HistoryAttributeValueType.Actual, value ? (ActualValue as PgM.PgAttributes.PgAttributeM).Value.ToString() : null);
            }
        }
        public PgHistoryEvent ParentEvent
        {
            get;
            private set; 
        }
        /// <summary>
        /// Нужно ли восстанавливать эти значения
        /// </summary>
        public bool RestoreValues
        {
            get { return _restoreValues; }
            set 
            {
                if (_restoreValues == value)
                {
                    return;
                }
                OnPropertyChanged(ref _restoreValues, value, () => this.RestoreValues); 
            }
        }
        #endregion Свойства

        #region Методы
        public bool AttributesHaveDifferentValues(PgM.PgAttributes.PgAttributeM attrLeft, PgM.PgAttributes.PgAttributeM attrRight)
        {
            bool attrLeftIsNull = AttributeIsNullOrEmpty(attrLeft);
            bool attrRightIsNull = AttributeIsNullOrEmpty(attrRight);

            if (attrLeftIsNull && attrRightIsNull)
            {
                return false;
            }
            else if (attrRightIsNull != attrLeftIsNull)
            {
                return true;
            }
            else 
            {
                if (attrLeft.Value.ToString() == attrRight.Value.ToString())
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
        public bool AttributeIsNullOrEmpty(PgM.PgAttributes.PgAttributeM attr)
        {
            return (attr == null || attr.Value == null || attr.Value == DBNull.Value || String.IsNullOrEmpty(attr.Value.ToString()));
        }
        #endregion Методы

        #region Внутренние типы
        public enum HistoryAttributeValueType
        {
            Before = 0, After = 1, Actual = 2
        }
        #endregion Внутренние типы
    }
}