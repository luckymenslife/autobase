using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using RESTLib.Model.REST;
using Interfaces;
using RESTLib.Model.REST.LayerStyle;
using RESTLib.Enums;
using System.Windows;
using System.Windows.Input;
using sscSync.Controller;

namespace sscSync.ViewModel
{
    public class SSCParametersVM : ViewModelBase
    {
        private Group _group = null;
        private List<Group> _groups;
        private LStyle _style;
        private Visibility _findGroup;
        
        public Visibility FindGroup
        {
            get { return _findGroup; }
        }

        public LStyle Style
        {
            get { return _style; }
            set { _style = value; OnPropertyChanged("Style"); }
        }

        public Group Group
        {
            get { return _group; }
            set { _group = value; OnPropertyChanged("Group"); }
        }

        public List<Group> Groups
        {
            get { return _groups; }
        }

        public SSCParametersVM(List<Group> groups, bool groupFound, tablesInfo tableInfo, LStyle lStyle)
            : this(tableInfo, lStyle)
        {
            _findGroup = groupFound ? Visibility.Collapsed : Visibility.Visible;
            _groups = groups;
        }

        public SSCParametersVM(tablesInfo tableInfo, LStyle lStyle)
        {
            _findGroup = Visibility.Collapsed;
            Style = lStyle ?? new LStyle();
            switch (tableInfo.TypeGeom)
            {
                case TypeGeometry.MULTIPOINT:
                    Style.Type = RESTStyles.Point;
                    break;
                case TypeGeometry.MULTIPOLYGON:
                    Style.Type = RESTStyles.Polygon;
                    break;
                case TypeGeometry.MULTILINESTRING:
                    Style.Type = RESTStyles.Line;
                    break;
            }
        }

        ICommand _closeCommand = null;
        public ICommand CloseCommand
        {
            get
            {
                return _closeCommand ?? (_closeCommand = new RelayCommand((o) => { }, (o) =>
                {
                    return (FindGroup == Visibility.Collapsed || Group != null)
                        && Style.IsValid;
                }));
            }
        }
    }
}
