using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;
using Rekod.Converters;

namespace Rekod.DataAccess.SourcePostgres.View.ValidationRules
{
    public class CaptionStyleValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            BindingGroup bindingGroup = (BindingGroup)value;
            if (bindingGroup.Items.Count > 0)
            {
                SourcePostgres.Model.PgStyleLableM labelStyle = (SourcePostgres.Model.PgStyleLableM)(bindingGroup.Items[0]);

                String fontName = (String)bindingGroup.GetValue(labelStyle, "LabelFontName");
                if (String.IsNullOrWhiteSpace(fontName))
                {
                    return new ValidationResult(false, "Не задан шрифт подписи");
                }

                bool useBounds = Convert.ToBoolean(bindingGroup.GetValue(labelStyle, "LabelUseBounds"));

                object minScaleObj = bindingGroup.GetValue(labelStyle, "LabelMinScale");
                object maxScaleObj = bindingGroup.GetValue(labelStyle, "LabelMaxScale");

                int minScale = -1;
                int maxScale = -1;

                try
                {
                    minScale = Convert.ToInt32(minScaleObj);
                }
                catch (Exception ex)
                {
                    String mes = "Ошибка в значении нижней границы масштаба: \n" + ex.Message;
                    return new ValidationResult(false, mes);
                }
                try
                {
                    maxScale = Convert.ToInt32(maxScaleObj);
                }
                catch (Exception ex)
                {
                    String mes = "Ошибка в значении верхней границы масштаба: \n" + ex.Message;
                    return new ValidationResult(false, mes);
                }
                if (useBounds)
                {
                    if (minScale < 0)
                    {
                        return new ValidationResult(false, "Минимальное значение не может быть отрицательным числом");
                    }
                    if (maxScale < 0)
                    {
                        return new ValidationResult(false, "Максимальное значение не может быть отрицательным числом");
                    }
                    if (minScale > maxScale)
                    {
                        return new ValidationResult(false, "Минимальное значение не может быть больше максимального");
                    }
                }

                return ValidationResult.ValidResult;
            }
            else
            {
                return new ValidationResult(false, "Не выбран объект для отображения");
            }
        }        
    }
}
