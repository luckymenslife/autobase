using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Globalization;
using System.Windows.Data;
using AbsM = Rekod.DataAccess.AbstractSource.Model;

namespace Rekod.DataAccess.SourcePostgres.View.ValidationRules
{
    public class GroupPropertiesValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            BindingGroup bindingGroup = (BindingGroup)value;
            if (bindingGroup.Items.Count > 0)
            {
                AbsM.GroupM group = bindingGroup.Items[0] as AbsM.GroupM;
                if (group != null)
                {
                    object groupText = null;
                    bindingGroup.TryGetValue(group, "Text", out groupText);
                    if(!String.IsNullOrEmpty((String)(groupText)))
                    {
                        return ValidationResult.ValidResult; 
                    }
                    else
                    {
                        return new ValidationResult(false, "Название группы в пользовательском интерфейсе не должно быть пустым"); 
                    }
                }
            }
            return ValidationResult.ValidResult;
        }
    }
}