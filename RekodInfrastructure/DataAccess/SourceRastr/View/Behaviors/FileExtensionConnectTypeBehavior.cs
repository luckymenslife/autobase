using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using RasM = Rekod.DataAccess.SourceRastr.Model;

namespace Rekod.DataAccess.SourceRastr.View.Behaviors
{
    public static class FileExtensionConnectTypeBehavior
    {
        private static List<String> _wmsExtensions;
        private static List<String> _gdalExtensions;

        static FileExtensionConnectTypeBehavior()
        {
            _wmsExtensions = new List<string>() 
            {
                ".rwms", ".rtms", ".rtwms"
            };
            _gdalExtensions = new List<string>() 
            {
                ".gxml"
            };
        }

        public static ComboBox GetConnectTypeBox(DependencyObject obj)
        {
            return (ComboBox)obj.GetValue(ConnectTypeBoxProperty);
        }

        public static void SetConnectTypeBox(DependencyObject obj, ComboBox value)
        {
            obj.SetValue(ConnectTypeBoxProperty, value);
        }

        // Using a DependencyProperty as the backing store for ConnectTypeBox.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ConnectTypeBoxProperty =
            DependencyProperty.RegisterAttached(
                    "ConnectTypeBox", 
                    typeof(ComboBox), 
                    typeof(FileExtensionConnectTypeBehavior), 
                    new PropertyMetadata(null, ConnectTypeBox_Changed));

        private static void ConnectTypeBox_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            textBox.TextChanged += textBox_TextChanged;
        }

        private static void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (!String.IsNullOrEmpty(textBox.Text.Trim()))
            {
                ComboBox connectTypeBox = (ComboBox)GetConnectTypeBox(textBox);
                connectTypeBox.SelectedValue = GetConnectType(textBox.Text);
                connectTypeBox.IsEnabled = GetConnectTypeChooseEnabled(textBox.Text);
            }
        }


        public static RasM.EConnectType GetConnectType(String filePath)
        {
            if (String.IsNullOrEmpty(filePath))
            {
                throw new Exception("File path is empty");
            }
            RasM.EConnectType connectType = RasM.EConnectType.Standard;
            String extension = Path.GetExtension(filePath);
            if (_gdalExtensions.Contains(extension.ToLower()))
            {
                connectType = RasM.EConnectType.Gdal;
            }
            return connectType;
        }
        public static Boolean GetConnectTypeChooseEnabled(String filePath)
        {
            Boolean res = false;
            if (String.IsNullOrEmpty(filePath))
            {
                throw new Exception("File extension is empty");
            }
            String extension = Path.GetExtension(filePath);
            if (!_wmsExtensions.Contains(extension.ToLower()) && !_gdalExtensions.Contains(extension.ToLower()))
            {
                res = true;
            }
            return res;
        }
    }
}