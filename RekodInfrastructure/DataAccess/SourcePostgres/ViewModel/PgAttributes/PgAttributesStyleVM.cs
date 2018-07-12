using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using AbsVM = Rekod.DataAccess.AbstractSource.ViewModel;
using PgM = Rekod.DataAccess.SourcePostgres.Model;
using PgVM = Rekod.DataAccess.SourcePostgres.ViewModel;
using PgV = Rekod.DataAccess.SourcePostgres.View;
using PgAtVM = Rekod.DataAccess.SourcePostgres.ViewModel.PgAttributes;
using Rekod.Controllers;
using Rekod.DataAccess.SourcePostgres.View.ConfigView;
using System.Windows;
using Rekod.Behaviors;
using System.Windows.Controls;
using Rekod.Services;


namespace Rekod.DataAccess.SourcePostgres.ViewModel.PgAttributes
{
    public class PgAttributesStyleVM: WindowViewModelBase_VM
    {
        #region Поля
        private PgAttributesVM _attributeVM;
        private bool _isDebug;
        private Npgsql.NpgsqlConnectionStringBuilder _connect;
        private bool _isReadOnly;
        private PgM.PgTableBaseM _table;
        private PgM.PgStyleObjectM _style;
        private PgM.PgAttributes.PgAttributeM _pkAttribute;

        private ICommand _editStyleCommand;
        private ICommand _saveStyleCommand;
        #endregion // Поля

        #region Свойства
        /// <summary>
        /// Получает стиль объекта (для справочников и интервалов если значение первичного ключа не равен null)
        /// </summary>
        public PgM.PgStyleObjectM Style
        {
            get { return _style; }
        }
        #endregion // Свойства

        #region Конструктор
        public PgAttributesStyleVM(PgAtVM.PgAttributesVM attributeVM)
        {
            _attributeVM = attributeVM;
            _isDebug = attributeVM.IsDebug;
            _connect = attributeVM.Connect;
            _isReadOnly = attributeVM.IsReadOnly;
            _table = attributeVM.Table;
            _pkAttribute = attributeVM.AttributesListVM.PkAttribute;
            _style = new PgM.PgStyleObjectM(_table, _pkAttribute.Value);
        }
        #endregion // Конструктор

        #region Команды
        #region Command: EditStyleCommand
        /// <summary>
        /// Команда для редактирования стиля
        /// </summary>
        public ICommand EditStyleCommand
        {
            get { return _editStyleCommand ?? (_editStyleCommand = new RelayCommand(this.EditStyle, this.CanEditStyle)); }
        }
        /// <summary>
        /// Открывает окно для редактирования стиля
        /// </summary>
        public void EditStyle(object parameter = null)
        {
            using (SqlWork sqlWork = new SqlWork(_connect))
            {
                sqlWork.sql =
                    String.Format(@"SELECT {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}
                                    FROM sys_scheme.style_object_info
                                    WHERE id_table={12} AND id_obj={13}",
                                "fontname",
                                "fontcolor",
                                "fontframecolor",
                                "fontsize",
                                "brushbgcolor",
                                "brushfgcolor",
                                "brushstyle",
                                "brushhatch",
                                "pencolor",
                                "pentype",
                                "penwidth",
                                "symbol",
                                _table.Id,
                                _pkAttribute.Value);
                sqlWork.ExecuteReader();
                sqlWork.CanRead();

                String fontname = sqlWork.GetString(0);
                Int32 fontcolor = sqlWork.GetInt32(1);
                Int32 fontframecolor = sqlWork.GetInt32(2);
                Int32 fontsize = sqlWork.GetInt32(3);

                Int32 brushbgcolor = sqlWork.GetInt32(4);
                Int32 brushfgcolor = sqlWork.GetInt32(5);
                Int16 brushstyle = (short)sqlWork.GetInt32(6);
                Int16 brushhatch = (short)sqlWork.GetInt32(7);

                Int32 pencolor = sqlWork.GetInt32(8);
                Int32 pentype = sqlWork.GetInt32(9);
                Int32 penwidth = sqlWork.GetInt32(10);
                Int32 symbol = sqlWork.GetInt32(11);

                sqlWork.Close();

                Style.SetFont(fontname, fontcolor, fontframecolor, fontsize);
                Style.SetBrush(brushbgcolor, brushfgcolor, brushhatch, brushstyle);
                Style.SetPen(pencolor, pentype, penwidth);
                Style.SetSymbol(symbol);
            }

            PgV.ObjectStyleEditV styleWindow = new PgV.ObjectStyleEditV();
            styleWindow.DataContext = this;
            styleWindow.Owner = Program.WinMain;
            styleWindow.Show();
        }
        /// <summary>
        /// Можно ли редактриовать стиль
        /// </summary>
        public bool CanEditStyle(object parameter = null)
        {
            return _table.IsMapStyle && _pkAttribute.Value != null;
        }
        #endregion

        #region Command: SaveStyleCommand
        /// <summary>
        /// Команда для сохранения стиля
        /// </summary>
        public ICommand SaveStyleCommand
        {
            get { return _saveStyleCommand ?? (_saveStyleCommand = new RelayCommand(this.SaveStyle, this.CanSaveStyle)); }
        }
        /// <summary>
        /// Метод для сохранения стиля
        /// </summary>
        public void SaveStyle(object parameter = null)
        {
            using (SqlWork sqlWork = new SqlWork(_connect))
            {
                sqlWork.BeginTransaction();
                sqlWork.sql =
                    String.Format(@"SELECT EXISTS (SELECT true FROM sys_scheme.style_object_info WHERE id_table={0} AND id_obj={1})",
                                        _table.Id,
                                        _pkAttribute.Value);
                bool exists = sqlWork.ExecuteScalar<Boolean>();
                if (!exists)
                {
                    sqlWork.sql = String.Format("INSERT INTO sys_scheme.style_object_info (id_table, id_obj) VALUES ({0}, {1})",
                            _table.Id,
                            _pkAttribute.Value);
                    sqlWork.ExecuteNonQuery();
                }

                sqlWork.sql =
                    String.Format(@"UPDATE sys_scheme.style_object_info SET
                                    fontname = '{0}', 
                                    fontcolor = {1}, 
                                    fontframecolor = {2}, 
                                    fontsize = {3}, 
                                    brushbgcolor = {4}, 
                                    brushfgcolor = {5}, 
                                    brushstyle = {6}, 
                                    brushhatch = {7}, 
                                    pencolor = {8}, 
                                    pentype = {9}, 
                                    penwidth = {10}, 
                                    symbol = {11}
                                    WHERE id_table = {12} AND id_obj = {13}",
                                    Style.FontName,
                                    Style.FontColor,
                                    Style.FontFrameColor,
                                    Style.FontSize,
                                    Style.BrushBgColor,
                                    Style.BrushFgColor,
                                    Style.BrushStyle,
                                    Style.BrushHatch,
                                    Style.PenColor,
                                    Style.PenType,
                                    Style.PenWidth,
                                    Style.Symbol,
                                    _table.Id,
                                    _pkAttribute.Value);
                sqlWork.ExecuteNonQuery();
                sqlWork.EndTransaction();
            }
        }
        /// <summary>
        /// Можно ли сохранить стиль
        /// </summary>
        public bool CanSaveStyle(object parameter = null)
        {
            return true;
        }
        #endregion // SaveStyleCommand
        #endregion Команды
    }
}