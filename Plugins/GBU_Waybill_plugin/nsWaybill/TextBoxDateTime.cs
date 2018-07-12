using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace GBU_Waybill_plugin.nsWaybill.DTTexBox
{
    /// <summary>
    /// Компонент для привязки данных типа DateTime для текстового ввода и представления
    /// </summary>
    class TextBoxDateTime : TextBox, IValueConverter
    {
        private static string convert_mask = "dd.MM.yyyy HH:mm";
        private static string visual_mask = "__.__.____ __:__";
        public TextBoxDateTime()
        {
            Binding bind = new Binding();
            bind.ElementName = base.Name;
            bind.Converter = this;
            bind.Path = new PropertyPath("Text");
            this.SetBinding(TextBox.TextProperty, bind);
        }


        /// <summary>
        /// Из исходного в отображаемый
        /// </summary>

        object IValueConverter.Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return "";
            return ((DateTime)value).ToString(convert_mask);
        }

        /// <summary>
        /// Из отображаемого формата в его "родной" формат
        /// </summary>
        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return null;
            try
            {
                return DateTime.ParseExact((string)value, convert_mask, null);
            }
            catch
            {
                return null;
            }
        }

    }
}
