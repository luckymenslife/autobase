using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Rekod.Controllers;
using Rekod.Services;

namespace Rekod.ProjectionSelection
{
    public class ExportProjSelectionVM : ViewModelBase
    {
        private Projection _selectedProjection;
        private ObservableCollection<Projection> _projList;

        public Projection SelectedProjection
        {
            get { return _selectedProjection; }
            set { OnPropertyChanged(ref _selectedProjection, value, () => this.SelectedProjection); }
        }

        public ObservableCollection<Projection> ProjList
        {
            get { return _projList; }
        }

        public ExportProjSelectionVM(int? defaultValue)
        {
            _projList = LoadProjections();
            if (defaultValue != null)
            {
                SelectedProjection = ProjList.FirstOrDefault(w => w.Srid == (int)defaultValue);
            }
        }

        private ObservableCollection<Projection> LoadProjections()
        {
            ObservableCollection<Projection> projs = new ObservableCollection<Projection>();
            using (var sqlCmd = new SqlWork())
            {
                sqlCmd.sql = "SELECT srid, auth_name, auth_srid, srtext, proj4text FROM spatial_ref_sys;";
                sqlCmd.ExecuteReader();
                while (sqlCmd.CanRead())
                {
                    projs.Add(new Projection(
                        sqlCmd.GetInt32("srid"),
                        sqlCmd.GetString("auth_name"),
                        sqlCmd.GetInt32("auth_srid"),
                        sqlCmd.GetString("srtext"),
                        sqlCmd.GetString("proj4text"),
                        true));
                }
            }
            return projs;
        }
    }
}