using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rekod;

namespace axVisUtils.Styles
{
    public class PgCachedStylesManager : Rekod.Classes.Singleton<PgCachedStylesManager>
    {
        private static Dictionary<DictKey, DictValue> _styles;
        private int _maxQueryCount;

        private PgCachedStylesManager()
        {
            _styles = new Dictionary<DictKey, DictValue>();
            _maxQueryCount = 10;
        }

        #region Methods

        public StylesVM GetStyle(tablesInfo table, fieldInfo field)
        {
            if (field.ref_table != null)
            {
                var tableStyle = classesOfMetods.getTableInfo(field.ref_table.Value);
                if (tableStyle != null)
                {
                    var key = new DictKey(
                        tableStyle.idTable, 
                        classesOfMetods.GetIntGeomType(table.GeomType_GC));

                    if (!_styles.ContainsKey(key)
                        || _styles[key].Counter > _maxQueryCount)
                    {
                        StylesVM style = new StylesPgField_VM(table, field);
                        style.GetListStyles();
                        AddStyle(key, style);
                    }

                    return _styles[key].Style;
                }
            }
            else
            {
                StylesVM style = new StylesPgField_VM(table, field);
                style.GetListStyles();
                return style;
            }
            return null;
        }
        public void ReloadStyleTable(int idTableStyle)
        {
            foreach (var item in _styles)
            {
                if (item.Key.IdTable == idTableStyle)
                {
                    var styleField = item.Value.Style as StylesPgField_VM;
                    if (styleField != null)
                    {
                        styleField.GetListStyles();
                    }

                    var styleTable = item.Value.Style as StylesPgTable_VM;
                    if(styleTable!=null)
                    {
                        styleTable.GetListStyles();
                    }
                    //StylesVM style = new StylesPgField_VM(table, field);
                    //style.GetListStyles();
                    //AddStyle(key, style);
                }
            }
        }
        public void RelaodAllStyleTable()
        {
            foreach (var item in _styles)
            {
                var styleField = item.Value.Style as StylesPgField_VM;
                if (styleField != null)
                {
                    styleField.GetListStyles();
                }

                var styleTable = item.Value.Style as StylesPgTable_VM;
                if (styleTable != null)
                {
                    styleTable.GetListStyles();
                }
                //StylesVM style = new StylesPgField_VM(table, field);
                //style.GetListStyles();
                //AddStyle(key, style);
            }
        }
        public StylesVM GetStyle(tablesInfo table)
        {
            string name = table.style_field;

            var field = classesOfMetods.getFieldInfoTable(table.idTable).FirstOrDefault(f => f.nameDB == name);
            if (field.ref_table != null)
            {
                var tableStyle = classesOfMetods.getTableInfo(field.ref_table.Value);
                if (tableStyle != null)
                {
                    var key = new DictKey(
                        tableStyle.idTable,
                        classesOfMetods.GetIntGeomType(table.GeomType_GC));

                    if (!_styles.ContainsKey(key)
                        || _styles[key].Counter > _maxQueryCount)
                    {
                        StylesVM style = new StylesPgTable_VM(table);
                        style.GetListStyles();
                        AddStyle(key, style);
                    }

                    return _styles[key].Style;
                }
            }
            else
            {
                StylesVM style = new StylesPgTable_VM(table);
                style.GetListStyles();
                return style;
            }
            return null;
        }

        private static void AddStyle(DictKey key, StylesVM style)
        {
            if (!_styles.ContainsKey(key))
            {
                _styles.Add(key, new DictValue(style));
            }
            else
            {
                _styles[key] = new DictValue(style);
            }
        }

        #endregion Methods

        class DictKey
        {
            private int _idTable;
            private int _geomType;

            public int IdTable
            { get { return this._idTable; } }

            public DictKey(int id, int geomType)
            {
                this._idTable = id;
                this._geomType = geomType;
            }

            public override bool Equals(object obj)
            {
                return obj != null 
                    && obj is DictKey
                    && (obj as DictKey)._geomType == _geomType
                    && (obj as DictKey)._idTable == _idTable;
            }

            public override int GetHashCode()
            {
                return _idTable ^ _geomType;
            }
        }

        class DictValue
        {
            private StylesVM _style;
            private int _counter;

            public int Counter
            {
                get { return _counter; }
            }

            public StylesVM Style
            {
                get
                {
                    if (_style != null)
                        _counter++;
                    return _style;
                }
                set
                {
                    _style = value;
                    _counter = 0;
                }
            }

            public DictValue(StylesVM style)
            {
                this.Style = style;
            }
        }
    }
}
