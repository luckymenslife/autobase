using System;
using System.Windows;
using System.Windows.Input;
using System.ComponentModel;
using System.Collections.Generic;

namespace Rekod.UserSets
{
    /// <summary>
    /// Логика взаимодействия для SaveReportFrm.xaml
    /// </summary>
    public partial class SaveSetFrm : Window
    {
        public SaveSetFrm()
        {
            InitializeComponent();

            btnSave.Click += (o, e) =>
            {
                this.DialogResult = true;
                this.Close();
            };

            // Фокус в поле имени
            FocusManager.SetFocusedElement(this, txtName);
            Keyboard.Focus(txtName);
        }
    }
}
