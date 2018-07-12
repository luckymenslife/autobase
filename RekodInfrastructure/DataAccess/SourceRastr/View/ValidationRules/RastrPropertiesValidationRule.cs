using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Globalization;
using System.Windows.Data;
using RasM = Rekod.DataAccess.SourceRastr.Model;
using AbsM = Rekod.DataAccess.AbstractSource.Model;
using RasVM = Rekod.DataAccess.SourceRastr.ViewModel;
using Rekod.Converters;

namespace Rekod.DataAccess.SourceRastr.View.ValidationRules
{
    class RastrPropertiesValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            BindingGroup bindingGroup = (BindingGroup)value;
            if (bindingGroup.Items.Count > 0)
            {
                RasM.RastTableBaseM rastrLayer = (RasM.RastTableBaseM)(bindingGroup.Items[0]);
                RasVM.RastrDataRepositoryVM rastrRepo = rastrLayer.Source as RasVM.RastrDataRepositoryVM;
                Object filePathObject = null;
                if (bindingGroup.TryGetValue(rastrLayer, "FilePath", out filePathObject))
                {
                    String filePath = Convert.ToString(filePathObject);
                    bool fileExists = false;
                    fileExists = filePath != "" && System.IO.File.Exists(filePath);
                    if (!fileExists)
                    {
                        if (filePath.StartsWith("."))
                        {
                            filePath = System.Windows.Forms.Application.StartupPath + "\\" + filePath.TrimStart(new char[] { '.', '\\' });
                            if (System.IO.File.Exists(filePath))
                            {
                                fileExists = true;
                            }
                        }  
                    }
                    if (!fileExists)
                    {
                        return new ValidationResult(false, "Путь к файлу задан неправильно");
                    }
                    else 
                    {
                        foreach (var iTable in rastrRepo.Tables)
                        {
                            RasM.RastTableBaseM rastrTable = iTable as RasM.RastTableBaseM;
                            if (rastrTable != rastrLayer && rastrTable.FilePath == filePath)
                            {
                                return new ValidationResult(false, 
                                    "В системе уже присутствует растровый слой который указывает на этот файл"); 
                            }
                        }
                    }
                }
                else
                {
                    return new ValidationResult(false, "Не задано свойство \"Путь к файлу\""); 
                }

                String rastrName = (String)bindingGroup.GetValue(rastrLayer, "Text");
                if (String.IsNullOrEmpty(rastrName.Trim()))
                {
                    return new ValidationResult(false, "Не задано название растрового слоя"); 
                }

                bool rastrNameExists = 
                    (from AbsM.TableBaseM rastrTable in rastrRepo.Tables where (rastrTable.Text == rastrName && rastrLayer != rastrTable) select rastrTable).Count() > 0;
                if (rastrNameExists)
                {
                    return new ValidationResult(false, "Слой с таким названием уже существует"); 
                }


                BooleanYesNoConverter boolYesNo = new BooleanYesNoConverter();
                bool useBounds = Convert.ToBoolean(boolYesNo.ConvertBack(bindingGroup.GetValue(rastrLayer, "UseBounds"), null, null, null));

                object minScaleObj = bindingGroup.GetValue(rastrLayer, "MinScale");
                object maxScaleObj = bindingGroup.GetValue(rastrLayer, "MaxScale");

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