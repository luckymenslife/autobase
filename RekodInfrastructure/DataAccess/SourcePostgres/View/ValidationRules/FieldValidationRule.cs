using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Globalization;
using System.Windows.Data;

namespace Rekod.DataAccess.SourcePostgres.View.ValidationRules
{
    public class FieldValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            BindingGroup bindingGroup = (BindingGroup)value;
            if (bindingGroup.Items.Count > 0)
            {
                //object obj = bindingGroup.Items[0];
                //Type t = obj.GetType();
                //ViewModel.FieldViewModel fieldVM = obj as ViewModel.FieldViewModel;

                //ViewModel.TableViewModelBase tableVM = bindingGroup.GetValue(fieldVM, "ReferenceTableId") as ViewModel.TableViewModelBase;
                //ViewModel.FieldViewModel fieldIdVm = bindingGroup.GetValue(fieldVM, "ReferenceFieldId") as ViewModel.FieldViewModel;
                //object fieldEndVm = null; 
                //bindingGroup.TryGetValue(fieldVM, "ReferenceFieldEnd", out fieldEndVm);
            }
            return ValidationResult.ValidResult;
        }
    }
}