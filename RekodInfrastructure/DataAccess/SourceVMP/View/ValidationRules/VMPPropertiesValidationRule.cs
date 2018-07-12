using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Globalization;
using System.Windows.Data;
using VmpM = Rekod.DataAccess.SourceVMP.Model;
using VmpVM = Rekod.DataAccess.SourceVMP.ViewModel;
using Rekod.Converters;

namespace Rekod.DataAccess.SourceVMP.View.ValidationRules
{
    class VMPPropertiesValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            BindingGroup bindingGroup = (BindingGroup)value;
            if (bindingGroup.Items.Count > 0)
            {
                VmpM.VMPTableBaseModel vmpLayer = (VmpM.VMPTableBaseModel)(bindingGroup.Items[0]);
                VmpVM.VMPDataRepositoryVM vmpRepo = vmpLayer.Source as VmpVM.VMPDataRepositoryVM;

                BooleanYesNoConverter boolYesNo = new BooleanYesNoConverter();
                bool useBounds = Convert.ToBoolean(boolYesNo.ConvertBack(bindingGroup.GetValue(vmpLayer, "UseBounds"), null, null, null));

                object minScaleObj = bindingGroup.GetValue(vmpLayer, "MinScale");
                object maxScaleObj = bindingGroup.GetValue(vmpLayer, "MaxScale");

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
    }
}