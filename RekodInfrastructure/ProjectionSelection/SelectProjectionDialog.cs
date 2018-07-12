using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Rekod.ProjectionSelection
{
    public static class SelectProjectionDialog
    {
        /// <summary>
        /// Выбор проекции файла
        /// </summary>
        /// <returns>SRID проекции</returns>
        public static int? Select(int? defaultValue = null)
        {
            int? srid = defaultValue;
                // Окно выбора проекции
                ExportProjSelectionV selectionFrm = new ExportProjSelectionV();
                var datacontext = new ExportProjSelectionVM(defaultValue);
                selectionFrm.DataContext = datacontext;
                selectionFrm.btnSelect.Click += (o, e) =>
                    {
                        selectionFrm.DialogResult = true;
                        selectionFrm.Close();
                    };
                if (selectionFrm.ShowDialog() == true)
                {
                    var proj = datacontext.SelectedProjection;
                    if (proj != null)
                        srid = proj.Srid;
                    else
                        return null;
                }
                else
                    return null;
            return srid;
        }
    }
}
