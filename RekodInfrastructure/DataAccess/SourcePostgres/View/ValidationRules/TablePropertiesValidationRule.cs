using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Globalization;
using System.Windows.Data;
using PgVM = Rekod.DataAccess.SourcePostgres.ViewModel;
using PgM = Rekod.DataAccess.SourcePostgres.Model;
using AbsM = Rekod.DataAccess.AbstractSource.Model;
using Rekod.DataAccess.SourcePostgres.View.ConfigView; 

namespace Rekod.DataAccess.SourcePostgres.View.ValidationRules
{
    public class TablePropertiesValidationRule: ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            BindingGroup bindingGroup = (BindingGroup)value;
            if (bindingGroup.Items.Count > 0)
            {
                object obj = bindingGroup.Items[0];
                PgM.PgTableBaseM tableM = obj as PgM.PgTableBaseM;

                String schemeName = (String)bindingGroup.GetValue(tableM, "SchemeName");
                String baseName = (String)bindingGroup.GetValue(tableM, "Name");
                String systemName = (String)bindingGroup.GetValue(tableM, "Text");
                AbsM.EGeomType geomType = (AbsM.EGeomType)((EnumWrapper)(bindingGroup.GetValue(tableM, "GeomType"))).Value;

                if (String.IsNullOrEmpty(schemeName))
                {
                    return new ValidationResult(false, "Схема не должна быть пустой"); 
                }
                if (String.IsNullOrEmpty(baseName))
                {
                    return new ValidationResult(false, "Название таблицы в базе не должно быть пустым"); 
                }
                if (String.IsNullOrEmpty(systemName))
                {
                    return new ValidationResult(false, "Название таблицы в системе не должно быть пустым"); 
                }
                if (tableM.IsLayer && geomType == AbsM.EGeomType.None)
                {
                    return new ValidationResult(false, "Слой карты должен иметь тип геометрии"); 
                }
            }

            return ValidationResult.ValidResult; 
        }
    }
}