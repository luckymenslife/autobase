using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;
using Rekod.Converters;
using Rekod.DataAccess.AbstractSource.Model;
using Rekod.DataAccess.SourceCosmetic.Model;
using Rekod.DataAccess.SourceCosmetic.ViewModel;

namespace Rekod.DataAccess.SourceCosmetic.View.ValidationRules
{
    class CosmeticPropertiesValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            BindingGroup bindingGroup = (BindingGroup)value;
            if (bindingGroup.Items.Count > 0)
            {
                CosmeticTableBaseM cosmLayer = (CosmeticTableBaseM)bindingGroup.Items[0];
                SourcePostgres.Model.PgStyleLayerM layerStyle = (SourcePostgres.Model.PgStyleLayerM)bindingGroup.Items[1];

                CosmeticDataRepositoryVM cosmRepo = cosmLayer.Source as CosmeticDataRepositoryVM;

                String cosmName = (String)bindingGroup.GetValue(cosmLayer, "Text");
                if (String.IsNullOrWhiteSpace(cosmName))
                {
                    return new ValidationResult(false, "Не задано название косметического слоя");
                }

                bool cosmNameExists =
                    (from TableBaseM cosmTable in cosmRepo.Tables where (cosmTable.Text == cosmName && cosmLayer != cosmTable) select cosmTable).Count() > 0;
                if (cosmNameExists)
                {
                    return new ValidationResult(false, "Слой с таким названием уже существует");
                }

                BooleanYesNoConverter boolYesNo = new BooleanYesNoConverter();
                bool useBounds = Convert.ToBoolean(boolYesNo.ConvertBack(bindingGroup.GetValue(layerStyle, "UseBounds"), null, null, null));

                object minScaleObj = bindingGroup.GetValue(layerStyle, "MinScale");
                object maxScaleObj = bindingGroup.GetValue(layerStyle, "MaxScale");

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
