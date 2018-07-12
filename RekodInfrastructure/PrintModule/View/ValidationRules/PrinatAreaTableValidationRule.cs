using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;

namespace Rekod.PrintModule.View.ValidationRules
{
    public class PrinatAreaTableValidationRule: ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            BindingGroup bindingGroup = (BindingGroup)value;
            if (bindingGroup.Items.Count > 0)
            {
                Rekod.PrintModule.ViewModel.PreviewWindowVM previewWindowVM = bindingGroup.Items[0] as Rekod.PrintModule.ViewModel.PreviewWindowVM;
                object horCountValue = bindingGroup.GetValue(previewWindowVM, "HorizontalCount");
                object verCountValue = bindingGroup.GetValue(previewWindowVM, "VerticalCount");

                int hCount;
                int vCount; 
                bool hCountValid = Int32.TryParse(horCountValue.ToString(), out hCount);
                bool vCountValid = Int32.TryParse(verCountValue.ToString(), out vCount);

                if (!hCountValid || !vCountValid)
                {
                    return new ValidationResult(false, "Введите корректные значения"); 
                }
                if (hCount < 1 || vCount < 1)
                {
                    return new ValidationResult(false, "Значения должны быть больше нуля"); 
                }
                //PgM.PgTableBaseM tableM = obj as PgM.PgTableBaseM;

                //String schemeName = (String)bindingGroup.GetValue(tableM, "SchemeName");
                //String baseName = (String)bindingGroup.GetValue(tableM, "Name");
                //String systemName = (String)bindingGroup.GetValue(tableM, "Text");
                //AbsM.EGeomType geomType = (AbsM.EGeomType)((EnumWrapper)(bindingGroup.GetValue(tableM, "GeomType"))).Value;

                //if (String.IsNullOrEmpty(schemeName))
                //{
                //    return new ValidationResult(false, "Схема не должна быть пустой");
                //}
                //if (String.IsNullOrEmpty(baseName))
                //{
                //    return new ValidationResult(false, "Название таблицы в базе не должно быть пустым");
                //}
                //if (String.IsNullOrEmpty(systemName))
                //{
                //    return new ValidationResult(false, "Название таблицы в системе не должно быть пустым");
                //}
                //if (tableM.IsLayer && geomType == AbsM.EGeomType.None)
                //{
                //    return new ValidationResult(false, "Слой карты должен иметь тип геометрии");
                //}
            }

            return ValidationResult.ValidResult; 
        }
    }
}
