using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rekod.Controllers;
using AbsM=Rekod.DataAccess.AbstractSource.Model;

namespace Rekod.DataAccess.SourceCosmetic.Model
{
    public class CosmeticFieldM : ViewModelBase, AbsM.IFieldM
    {
        #region Поля
        private int _id;
        private int _idTable;
        private CosmeticTableBaseM _table;
        private AbsM.EFieldType _type;
        private string _name;
        private string _text;
        private bool _isReadOnly = false;
        #endregion // Поля

        private static int cosmeticFieldId = 1;

        #region Конструкторы
        public CosmeticFieldM(AbsM.ITableBaseM table, 
            string name, string text, AbsM.EFieldType type, bool isReadOnly = false)
        {
            var cosmTable = table as CosmeticTableBaseM;
            if (cosmTable == null)
                throw new ArgumentNullException("Нет ссылки на таблицу");
            _id = CosmeticFieldM.cosmeticFieldId++;
            _table = cosmTable;
            _idTable = _table.Id;
            _name = name;
            _text = text;
            _type = type;
            _isReadOnly = isReadOnly;
        }
        #endregion // Конструкторы

        #region Свойства
        /// <summary>
        /// Идентификатор таблицы к которой относится атрибут
        /// </summary>
        public AbsM.ITableBaseM Table
        {
            get { return _table; }
        }
        /// <summary>
        /// ID таблицы к которой относится атрибут
        /// </summary>
        public int IdTable
        {
            get { return _idTable; }
        }
        /// <summary>
        /// Тип атрибута
        /// </summary>
        public AbsM.EFieldType Type
        {
            get { return _type; }
            set { OnPropertyChanged(ref _type, value, () => this.Type); }
        }
        /// <summary>
        /// Наименование атрибута
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { OnPropertyChanged(ref _name, value, () => this.Name); }
        }
        /// <summary>
        /// Наименование атрибута, отображаемое в интерфейсе пользователя
        /// </summary>
        public string Text
        {
            get { return _text; }
            set { OnPropertyChanged(ref _text, value, () => this.Text); }
        }
        /// <summary>
        /// Атрибут только для чтения
        /// </summary>
        public bool IsReadOnly
        {
            get { return _isReadOnly; }
            set { OnPropertyChanged(ref _isReadOnly, value, () => this.IsReadOnly); }
        }
        
        public int Id
        {
            get { return _id; }
        }

        object AbsM.IFieldM.IdTable
        {
            get { return Id; }
        }
        #endregion // Свойства        
    }
}
