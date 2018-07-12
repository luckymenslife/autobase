using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;

namespace Rekod.DataAccess.SourceCosmetic.View.ValidationRules
{
    public class DefaultStyleValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            BindingGroup bindingGroup = (BindingGroup)value;
            if (bindingGroup.Items.Count > 0)
            {
                SourcePostgres.Model.PgStyleLayerM layerStyle = (SourcePostgres.Model.PgStyleLayerM)bindingGroup.Items[0];
                
                String fontName = (String)bindingGroup.GetValue(layerStyle, "FontName");
                if (String.IsNullOrWhiteSpace(fontName))
                {
                    return new ValidationResult(false, "Не задан шрифт точечных объектов");
                }

                object penWidthObj = bindingGroup.GetValue(layerStyle, "PenWidth");
                int penWidth = -1;
                try
                {
                    penWidth = Convert.ToInt32(penWidthObj);
                }
                catch (Exception ex)
                {
                    String mes = "Ошибка в значении толщины линий: \n" + ex.Message;
                    return new ValidationResult(false, mes);
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
