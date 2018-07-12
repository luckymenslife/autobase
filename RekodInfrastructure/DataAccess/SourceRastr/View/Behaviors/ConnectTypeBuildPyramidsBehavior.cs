using Rekod.DataAccess.SourcePostgres.View.ConfigView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using RasM = Rekod.DataAccess.SourceRastr.Model;

namespace Rekod.DataAccess.SourceRastr.View.Behaviors
{
    public static class ConnectTypeBuildPyramidsBehavior
    {
        public static ComboBox GetBuildPyramidsBox(DependencyObject obj)
        {
            return (ComboBox)obj.GetValue(BuildPyramidsBoxProperty);
        }

        public static void SetBuildPyramidsBox(DependencyObject obj, ComboBox value)
        {
            obj.SetValue(BuildPyramidsBoxProperty, value);
        }

        // Using a DependencyProperty as the backing store for BuildPyramidsBox.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BuildPyramidsBoxProperty =
            DependencyProperty.RegisterAttached(
                    "BuildPyramidsBox", 
                    typeof(ComboBox), 
                    typeof(ConnectTypeBuildPyramidsBehavior), 
                    new PropertyMetadata(null, BuildPyramidsBox_Changed));
        
        private static void BuildPyramidsBox_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ComboBox connectTypeBox = sender as ComboBox;
            connectTypeBox.SelectionChanged += ConnectTypeBox_SelectionChanged;
        }

        static void ConnectTypeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox connectTypeBox = sender as ComboBox;
            ComboBox buildPyramidsBox = GetBuildPyramidsBox(connectTypeBox);
            if (e.AddedItems.Count == 1 && (e.AddedItems[0] is EnumWrapper) && (e.AddedItems[0] as EnumWrapper).Value is RasM.EConnectType)
            {
                RasM.EConnectType connectType = (RasM.EConnectType)(e.AddedItems[0] as EnumWrapper).Value;
                switch (connectType)
                {
                    case Rekod.DataAccess.SourceRastr.Model.EConnectType.Standard:
                        {
                            buildPyramidsBox.SelectedItem = Rekod.Properties.Resources.LocNo;
                            buildPyramidsBox.IsEnabled = false;
                            break;
                        }
                    case Rekod.DataAccess.SourceRastr.Model.EConnectType.Gdal:
                        {
                            buildPyramidsBox.IsEnabled = true;
                            //buildPyramidsBox.SelectedItem = Rekod.Properties.Resources.LocYes;                            
                            break;
                        }
                }
            }
        }
    }
}