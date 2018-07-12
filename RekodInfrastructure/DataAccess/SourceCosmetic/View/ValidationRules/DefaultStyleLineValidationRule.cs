using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;

namespace Rekod.DataAccess.SourceCosmetic.View.ValidationRules
{
    public class DefaultStyleLineValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            BindingGroup bindingGroup = (BindingGroup)value;
            if (bindingGroup.Items.Count > 0)
            {
                Object layerStyle = null;
                if (bindingGroup.Items[0] is SourcePostgres.Model.PgStyleLayerM)
                {
                    layerStyle = (SourcePostgres.Model.PgStyleLayerM)bindingGroup.Items[0];
                }
                else if (bindingGroup.Items[0] is SourcePostgres.Model.PgStyleObjectM)
                {
                    layerStyle = (SourcePostgres.Model.PgStyleObjectM)bindingGroup.Items[0];
                }

                if (layerStyle != null)
                {
                    object penWidthObj = bindingGroup.GetValue(layerStyle, "PenWidth");
                    int penWidth = -1;
                    try
                    {
                        penWidth = Convert.ToInt32(penWidthObj);
                        if (penWidth < 0)
                            throw new Exception("Толщина линии не может быть отрицательной");
                    }
                    catch (Exception ex)
                    {
                        String mes = "Ошибка в значении толщины линий: \n" + ex.Message;
                        return new ValidationResult(false, mes);
                    }

                    if (bindingGroup.GetValue(layerStyle, "PenType") == null)
                    {
                        return new ValidationResult(false, "Не выбран тип карандаша");
                    }
                }
                else
                {
                    return new ValidationResult(false, "Не задан объект стиля");
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
