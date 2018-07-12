using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq.Expressions;

namespace Rekod.Controllers
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Members
        // Пример использования: OnPropertyChanged(ref _defaultSet, value, () => this.DefaultSet);
        internal void OnPropertyChanged<T>(ref T Value, T newValue, Expression<Func<T>> action)
        {
            if (Value == null && newValue == null)
                return;
            if (Value != null && Value.Equals(newValue))
                return;
            Value = newValue;
            OnPropertyChanged(GetPropertyName(action));
        }
        public void OnPropertyChanged<T>(Expression<Func<T>> action)
        {
            var propertyName = GetPropertyName(action);
            OnPropertyChanged(propertyName);
        }

        private static string GetPropertyName<T>(Expression<Func<T>> action)
        {
            var expression = (MemberExpression)action.Body;
            var propertyName = expression.Member.Name;
            return propertyName;
        }
        public virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
    }
}