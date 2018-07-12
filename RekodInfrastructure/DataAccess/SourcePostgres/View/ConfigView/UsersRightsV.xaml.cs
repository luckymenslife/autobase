using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Rekod.DataAccess.SourcePostgres.Model;
using Rekod.DataAccess.SourcePostgres.ViewModel;
using System.ComponentModel;

namespace Rekod.DataAccess.SourcePostgres.View.ConfigView
{
    /// <summary>
    /// Interaction logic for UsersRightsV.xaml
    /// </summary>
    public partial class UsersRightsV : UserControl
    {
        public UsersRightsV()
        {
            InitializeComponent();
        }
    }
}