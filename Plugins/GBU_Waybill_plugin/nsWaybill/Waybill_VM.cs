using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Interfaces.UserControls;
using System.Windows.Input;
using System.Collections.ObjectModel;

namespace GBU_Waybill_plugin.nsWaybill
{
    public class Waybill_VM : ViewModelBase, IUserControlMain
    {
        #region Fields
        
        private bool _cancel_open;
        // Для WinForms
        private System.Windows.Forms.UserControl cntrl1;
        private System.Windows.Forms.Integration.ElementHost elementHost1;

        public Waybill_M waybill_rec;
        private int? id_object;
        
        private ObservableCollection<wb_Organisation> _wb_Orgs = new ObservableCollection<wb_Organisation>();
        private wb_Organisation _selected_org;
        private ObservableCollection<wb_Car> _wb_Cars = new ObservableCollection<wb_Car>();
        private wb_Car _selected_car;
        private ObservableCollection<wb_Trailer> _wb_Trailers = new ObservableCollection<wb_Trailer>();
        private wb_Trailer _selected_trailer;
        private ObservableCollection<wb_Driver> _wb_Drivers = new ObservableCollection<wb_Driver>();
        private wb_Driver _selected_driver;
        private ObservableCollection<wb_Repairer> _wb_Repairers = new ObservableCollection<wb_Repairer>();
        private wb_Repairer _selected_repairer;
        private ObservableCollection<wb_Route> _wb_Routes = new ObservableCollection<wb_Route>();
        private wb_Route _selected_route;
        private ObservableCollection<wb_Work_type> _wb_Work_types = new ObservableCollection<wb_Work_type>();
        private wb_Work_type _selected_work_type;
        private ObservableCollection<wb_Cargo_type> _wb_Cargo_types = new ObservableCollection<wb_Cargo_type>();
        private wb_Cargo_type _selected_cargo_type;
        private ObservableCollection<wb_Work_mode> _wb_Work_modes = new ObservableCollection<wb_Work_mode>();
        private wb_Work_mode _selected_work_mode;
        private ObservableCollection<wb_Fuel_mark> _wb_Fuel_marks = new ObservableCollection<wb_Fuel_mark>();
        private wb_Fuel_mark _selected_fuel_mark_one;
        private wb_Fuel_mark _selected_fuel_mark_two;
        private ObservableCollection<wb_Special_note> _wb_Special_notes = new ObservableCollection<wb_Special_note>();
        private wb_Special_note _selected_special_note;
        private ObservableCollection<wb_Road_type> _wb_Road_types = new ObservableCollection<wb_Road_type>();
        private wb_Road_type _selected_road_type;
        private ObservableCollection<wb_Fuel_card> _wb_Fuel_cards = new ObservableCollection<wb_Fuel_card>();
        private wb_Fuel_card _selected_fuel_card_one;
        private wb_Fuel_card _selected_fuel_card_two;

        #endregion //Fields

        #region Конструктор

        public Waybill_VM(int? id_object, int id_table)
        {
            this.id_object = id_object;
            waybill_rec = new Waybill_M(id_object);
            // Для WinForms
            cntrl1 = new System.Windows.Forms.UserControl();
            elementHost1 = new System.Windows.Forms.Integration.ElementHost();
            elementHost1.Dock = System.Windows.Forms.DockStyle.Fill;
            elementHost1.Location = new System.Drawing.Point(0, 0);
            elementHost1.Name = "elementHost1";
            //elementHost1.Child = new Waybill_V() { DataContext = this };            
            cntrl1.Controls.Add(elementHost1);
            cntrl1.Load += cntrl1_Load;
            
        }


        #endregion //Конструктор

        #region IUserControlMain Реализация

        public string Title
        {
            get { return ""; }
        }

        public object ViewModel
        {
            get { return null; }
        }

        public event EventHandler<eventCloseForm> CloseForm;

        public System.Windows.Forms.UserControl GetUserControl()
        {
            return cntrl1;
        }

        public bool CancelOpen
        {
            get { return this._cancel_open; }
            set { this._cancel_open = false; }
        }

        public System.Drawing.Size SizeWindow
        {
            get { return cntrl1.Size; }
        }

        #endregion //IUserControlMain

        #region Commands Definition

        private ICommand _Close;
        public ICommand CloseCommand
        {
            get { return _Close ?? (_Close = new RelayCommand(this.Close)); }
        }
        private void Close(object obj)
        {
            cntrl1.ParentForm.Close();
        }

        private ICommand _Save;
        //public ICommand SaveCommand;
        private ICommand _Print;
        //public ICommand PrintCommand;
        private ICommand _Edit;
        //public ICommand EditCommand;
        private ICommand _OpenTaskForm;
        //public ICommand OpenTaskFormCommand;

        #endregion //Commands Definition

        #region Загрузка формы

        void cntrl1_Load(object sender, EventArgs e)
        {
            cntrl1.ParentForm.Text = "Путевой лист";
            PgDatabase.Load_Waybill_Item(ref waybill_rec);
            elementHost1.Child = new Waybill_V() { DataContext = this };
            OnPropertyChanged("waybill_rec");
            Load_collections_without_dependent();
            
        }

        #endregion //Загрузка формы

        #region Загрузка/Обновление справочников
        private void Load_collections_without_dependent()
        {
            _wb_Orgs.Clear();
            _wb_Orgs = PgDatabase.Load_Organisations();
            OnPropertyChanged("Wb_orgs");
            _wb_Fuel_marks.Clear();
            _wb_Fuel_marks = PgDatabase.Load_Fuel_marks();
            OnPropertyChanged("Wb_Fuel_marks");
            _wb_Work_types.Clear();
            _wb_Work_types = PgDatabase.Load_Work_types();
            OnPropertyChanged("Wb_Work_types");
            _wb_Cargo_types.Clear();
            _wb_Cargo_types = PgDatabase.Load_Cargo_types();
            OnPropertyChanged("Wb_Cargo_types");
            _wb_Work_modes.Clear();
            _wb_Work_modes = PgDatabase.Load_Work_modes();
            OnPropertyChanged("Wb_Work_modes");
            _wb_Special_notes.Clear();
            _wb_Special_notes = PgDatabase.Load_Special_notes();
            OnPropertyChanged("Wb_Special_notes");
            _wb_Road_types.Clear();
            _wb_Road_types = PgDatabase.Load_Road_types();
            OnPropertyChanged("Wb_Road_types");
        }

        #endregion //Загрузка/Обновление справочников

        #region Доступ к справочникам/данным

        public ObservableCollection<wb_Organisation> Wb_orgs
        {
            get { return _wb_Orgs; }
        }
        public wb_Organisation Selected_Org
        {
            get { return _selected_org; }
            set
            {
                OnPropertyChanged(ref _selected_org, value, () => Selected_Org);
                if (_selected_org != null)
                {
                    _wb_Cars.Clear();
                    _wb_Cars = PgDatabase.Load_Cars(_selected_org.Gid, waybill_rec.Car_id);
                    OnPropertyChanged("Wb_Cars");

                    _wb_Trailers.Clear();
                    _wb_Trailers = PgDatabase.Load_Trailer(_selected_org.Gid, waybill_rec.Trailer_id);
                    OnPropertyChanged("Wb_Trailers");

                    _wb_Drivers.Clear();
                    _wb_Drivers = PgDatabase.Load_Driver(_selected_org.Gid, waybill_rec.Driver_id);
                    OnPropertyChanged("Wb_Drivers");

                    _wb_Repairers.Clear();
                    _wb_Repairers = PgDatabase.Load_Repairer(_selected_org.Gid, waybill_rec.Automaster_id);
                    OnPropertyChanged("Wb_Repairers");

                    _wb_Routes.Clear();
                    _wb_Routes = PgDatabase.Load_Route(_selected_org.Gid);
                    OnPropertyChanged("Wb_Routes");

                    _wb_Fuel_cards.Clear();
                    _wb_Fuel_cards = PgDatabase.Load_Fuel_cards(_selected_org.Gid, waybill_rec.Fuel_card_id, waybill_rec.Fuel_card2_id);
                    OnPropertyChanged("Wb_Fuel_cards");
                }
            }
        }
        public ObservableCollection<wb_Car> Wb_Cars
        {
            get { return _wb_Cars; }
        }
        public wb_Car Selected_Car
        {
            get { return _selected_car; }
            set
            {
                OnPropertyChanged(ref _selected_car, value, () => Selected_Car);
                if (_selected_car != null)
                {
                    // Обновление зависимых стравочников, ну и т.п.
                }
            }
        }
        public ObservableCollection<wb_Fuel_mark> Wb_Fuel_marks
        {
            get { return _wb_Fuel_marks; }
        }
        public wb_Fuel_mark Selected_Fuel_mark_one
        {
            get { return _selected_fuel_mark_one; }
            set
            {
                OnPropertyChanged(ref _selected_fuel_mark_one, value, () => Selected_Fuel_mark_one);
                if (_selected_fuel_mark_one != null)
                {
                    // Обновление зависимых стравочников, ну и т.п.
                }
            }
        }
        public wb_Fuel_mark Selected_Fuel_mark_two
        {
            get { return _selected_fuel_mark_two; }
            set
            {
                OnPropertyChanged(ref _selected_fuel_mark_two, value, () => Selected_Fuel_mark_two);
                if (_selected_fuel_mark_two != null)
                {
                    // Обновление зависимых стравочников, ну и т.п.
                }
            }
        }
        public ObservableCollection<wb_Trailer> Wb_Trailers
        {
            get { return _wb_Trailers; }
        }
        public wb_Trailer Selected_Trailer
        {
            get { return _selected_trailer; }
            set
            {
                OnPropertyChanged(ref _selected_trailer, value, () => Selected_Trailer);
                if (_selected_trailer != null)
                {
                    // Обновление зависимых стравочников, ну и т.п.
                }
            }
        }
        public ObservableCollection<wb_Driver> Wb_Drivers
        {
            get { return _wb_Drivers; }
        }
        public wb_Driver Selected_Driver
        {
            get { return _selected_driver; }
            set
            {
                OnPropertyChanged(ref _selected_driver, value, () => Selected_Driver);
                if (_selected_driver != null)
                {
                    // Обновление зависимых стравочников, ну и т.п.
                }
            }
        }
        public ObservableCollection<wb_Repairer> Wb_Repairers
        {
            get { return _wb_Repairers; }
        }
        public wb_Repairer Selected_Repairer
        {
            get { return _selected_repairer; }
            set
            {
                OnPropertyChanged(ref _selected_repairer, value, () => Selected_Repairer);
                if (_selected_repairer != null)
                {
                    // Обновление зависимых стравочников, ну и т.п.
                }
            }
        }
        public ObservableCollection<wb_Route> Wb_Routes
        {
            get { return _wb_Routes; }
        }
        public wb_Route Selected_Route
        {
            get { return _selected_route; }
            set
            {
                OnPropertyChanged(ref _selected_route, value, () => Selected_Route);
                if (_selected_route != null)
                {
                    // Обновление зависимых стравочников, ну и т.п.
                }
            }
        }
        public ObservableCollection<wb_Work_type> Wb_Work_types
        {
            get { return _wb_Work_types; }
        }
        public wb_Work_type Selected_Work_type
        {
            get { return _selected_work_type; }
            set
            {
                OnPropertyChanged(ref _selected_work_type, value, () => Selected_Work_type);
                if (_selected_work_type != null)
                {
                    // Обновление зависимых стравочников, ну и т.п.
                }
            }
        }
        public ObservableCollection<wb_Cargo_type> Wb_Cargo_types
        {
            get { return _wb_Cargo_types; }
        }
        public wb_Cargo_type Selected_Cargo_type
        {
            get { return _selected_cargo_type; }
            set
            {
                OnPropertyChanged(ref _selected_cargo_type, value, () => Selected_Cargo_type);
                if (_selected_cargo_type != null)
                {
                    // Обновление зависимых стравочников, ну и т.п.
                }
            }
        }
        public ObservableCollection<wb_Work_mode> Wb_Work_modes
        {
            get { return _wb_Work_modes; }
        }
        public wb_Work_mode Selected_Work_mode
        {
            get { return _selected_work_mode; }
            set
            {
                OnPropertyChanged(ref _selected_work_mode, value, () => Selected_Work_mode);
                if (_selected_work_mode != null)
                {
                    // Обновление зависимых стравочников, ну и т.п.
                }
            }
        }
        public ObservableCollection<wb_Special_note> Wb_Special_notes
        {
            get { return _wb_Special_notes; }
        }
        public wb_Special_note Selected_Special_note
        {
            get { return _selected_special_note; }
            set
            {
                OnPropertyChanged(ref _selected_special_note, value, () => Selected_Special_note);
                if (_selected_special_note != null)
                {
                    // Обновление зависимых стравочников, ну и т.п.
                }
            }
        }
        public ObservableCollection<wb_Road_type> Wb_Road_types
        {
            get { return _wb_Road_types; }
        }
        public wb_Road_type Selected_Road_type
        {
            get { return _selected_road_type; }
            set
            {
                OnPropertyChanged(ref _selected_road_type, value, () => Selected_Road_type);
                if (_selected_road_type != null)
                {
                    // Обновление зависимых стравочников, ну и т.п.
                }
            }
        }
        public ObservableCollection<wb_Fuel_card> Wb_Fuel_cards
        {
            get { return _wb_Fuel_cards; }
        }
        public wb_Fuel_card Selected_Fuel_card_one
        {
            get { return _selected_fuel_card_one; }
            set
            {
                OnPropertyChanged(ref _selected_fuel_card_one, value, () => Selected_Fuel_card_one);
                if (_selected_fuel_card_one != null)
                {
                    // Обновление зависимых стравочников, ну и т.п.
                }
            }
        }
        public wb_Fuel_card Selected_Fuel_card_two
        {
            get { return _selected_fuel_card_two; }
            set
            {
                OnPropertyChanged(ref _selected_fuel_card_two, value, () => Selected_Fuel_card_two);
                if (_selected_fuel_card_two != null)
                {
                    // Обновление зависимых стравочников, ну и т.п.
                }
            }
        }


        #endregion //Доступ к справочникам/данным
    }
}
