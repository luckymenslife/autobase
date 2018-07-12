using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;

namespace Rekod.DataAccess.SourceCosmetic.View.ValidationRules
{
    public class DefaultStylePolygonValidationRule : ValidationRule
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
                    if (bindingGroup.GetValue(layerStyle, "BrushStyle") == null
                        || bindingGroup.GetValue(layerStyle, "BrushHatch") == null)
                    {
                        return new ValidationResult(false, "Не выбран тип кисти стиля полигона");
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
