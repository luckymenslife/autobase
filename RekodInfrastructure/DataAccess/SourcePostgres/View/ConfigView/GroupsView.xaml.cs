﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.ComponentModel;
using PgM = Rekod.DataAccess.SourcePostgres.Model;
using PgVM = Rekod.DataAccess.SourcePostgres.ViewModel;
using AbsM = Rekod.DataAccess.AbstractSource.Model;

namespace Rekod.DataAccess.SourcePostgres.View.ConfigView
{
    /// <summary>
    /// Interaction logic for GroupsView.xaml
    /// </summary>
    public partial class GroupsView : UserControl
    {
        public GroupsView()
        {
            InitializeComponent();
        }
    }
}