using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Npgsql;
using System.Windows.Forms;
using axVisUtils.Styles;

namespace Rekod
{
    class GeomDumpInfo
    {
        public int idObj;
        public int pathId;
        public Byte[] geom;
    }
    public class itemObj
    {
        //*******************************************************
        // класс для использывания в listbox и comboBox-e
        //*******************************************************
        protected int internalId_o;
        protected string internalName_o;
        protected string internalLayer;

        public int Id_o
        {
            get { return internalId_o; }
            set { internalId_o = value; }
        }

        public string Name_o
        {
            get { return internalName_o; }
            set { internalName_o = value; }
        }

        public string Layer
        {
            get { return internalLayer; }
            set { internalLayer = value; }
        }

        public override string ToString()
        {
            return this.internalName_o;
        }

        // конструктор
        public itemObj(int Id_o, string Name_o, string Layer)
        {
            this.internalId_o = Id_o;
            this.internalName_o = Name_o;
            this.internalLayer = Layer;
        }
        protected itemObj() { }
    }
    public class itemObjOrdered : itemObj
    {
        public int order;
        private itemObjOrdered() { }
        public itemObjOrdered(int Id_o, string Name_o, string Layer, int ord)
        {
            this.internalId_o = Id_o;
            this.internalName_o = Name_o;
            this.internalLayer = Layer;
            this.order = ord;
        }
    }
    // для сортировки list<tablesInfo>
    public class DinoComparer : IComparer<tablesInfo>
    {
        public int Compare(tablesInfo x, tablesInfo y)
        {
            if (x == null)
            {
                if (y == null)
                {
                    // If x is null and y is null, they're
                    // equal. 
                    return 0;
                }
                else
                {
                    // If x is null and y is not null, y
                    // is greater. 
                    return -1;
                }
            }
            else
            {
                // If x is not null...
                //
                if (y == null)
                // ...and y is null, x is greater.
                {
                    return 1;
                }
                else
                {
                    // ...and y is not null, compare the 
                    // lengths of the two strings.
                    //
                    int retval = x.nameMap.CompareTo(y.nameMap);

                    if (retval != 0)
                    {
                        // If the strings are not of equal length,
                        // the longer string is greater.
                        //
                        return retval;
                    }
                    else
                    {
                        // If the strings are of equal length,
                        // sort them with ordinary string comparison.
                        //
                        return x.nameMap.CompareTo(y.nameMap);
                    }
                }
            }
        }
    }
    public struct objStyleRef
    {
        public string Val;
        public string FieldName;
        public int idMap;
    }
    public struct fieldArray
    {
        public int idField;
        public string nameDB;
        public string nameMap;
    }
    public struct tablesArray
    {
        public int idTable;
        public string nameDB;
        public string nameMap;
        public string scema_name;
    }
    public struct filtrTableInfo
    {
        public int idFilter;
        public int idTable;
        public int idField;
        public int idOperator;
    }
    public class SetInfo
    {
        public int idSet;
        public string name;
        public int idUser;
        public bool showSet;
    }
    public class user_right
    {
        public int id_table;
        public int id_ref_table;
        public bool read;
        public bool write;
    }
    public class ref_table_constr
    {
        public int id_table;
        public int id_ref_table;
    }
    public class userInfo
    {
        public int id_user;
        public bool admin;
        public string windowText;
        public string nameUser;
        public string loginUser;
        public int type_user;
    }
    public class tablesInfo : IComparer<tablesInfo>
    {
        public int idTable { get; set; }
        public string nameSheme { get; set; }
        public string nameDB { get; set; }
        public string nameMap { get; set; }
        public string lableFieldName { get; set; }
        public int type { get; set; }
        public bool photo { get; set; }
        //public int typeGeom { get; set; }
        public bool read_only { get; set; }
        public string geomFieldName { get; set; }
        public string pkField { get; set; }
        public bool map_style;
        public int? srid;
        public bool sourceLayer;
        public string imageColumn;
        public string angleColumn;
        public bool useBounds;
        public int minScale;
        public int maxScale;
        public string style_field;
        public string view_name;
        public string sql_view_string;
        public int precision_point;
        public bool hidden;

        public bool label_showlabel;
        public bool label_uselabelstyle;
        public bool label_showframe;
        public uint label_framecolor;
        public bool label_parallel;
        public bool label_overlap;
        public bool label_usebounds;
        public uint label_minscale;
        public uint label_maxscale;
        public int label_offset;
        public bool label_graphicunits;
        public String label_fontname;
        public uint label_fontcolor;
        public int label_fontsize;
        public bool label_fontstrikeout;
        public bool label_fontitalic;
        public bool label_fontunderline;
        public bool label_fontbold;
        public bool isHistory = false;
        public bool graphic_units = false;
        public bool defaultVisible = false;
        public string GeomType_GC;
        public string default_order;

#if DEBUG
        public override string ToString()
        {
            return string.Format("id:{0} Sheme:\"{1}\"  DB:\"{2}\"", idTable, nameSheme, nameDB);
        }
#endif
        public int Compare(tablesInfo x, tablesInfo y)
        {
            if (x.nameMap.CompareTo(y.nameMap) != 0)
            {
                return x.nameMap.CompareTo(y.nameMap);
            }
            else if (x.nameMap.CompareTo(y.nameMap) != 0)
            {
                return x.nameMap.CompareTo(y.nameMap);
            }
            else if (x.nameMap.CompareTo(y.nameMap) != 0)
            {
                return x.nameMap.CompareTo(y.nameMap);
            }
            else
            {
                return 0;
            }
        }

        public int MinObjectSize { get; set; }

        public int? RefTable { get; set; }

        public bool DisplayWhenOpening { get; set; }

        public objStylesM Style { get; set; }

        public StylesVM StyleVM { get; set; }
    }
    public struct fieldInfo
    {
        public int idField;
        public int idTable;
        public string nameDB;
        public string nameMap { get; set; }
        public string nameLable;
        public int type;
        public bool is_reference;
        public bool is_interval;
        public bool is_style;
        public int? ref_table;
        public int? ref_field;
        public int? ref_field_end;
        public int? ref_field_name;
        public bool read_only;
        public bool visible;

        public int Order { get; set; }
        public bool is_not_null;
    }
    public struct tipTable
    {
        public int idTipTable;
        public string nameTip;
        public bool mapLayer;
    }
    public struct tipGeom
    {
        public int idTipGeom;
        public string nameGeom;
        public string nameDb;
    }
    public struct tipData
    {
        public int idTipData;
        public string nameTipData;
        public string nameTipDataDB;
    }
    public struct tipOperator
    {
        public int idTipOperator;
        public string nameTipOperator;
        public string namePered;
        public string namePosle;
    }
    public struct photoInfo
    {
        public int idTable;
        public string nameFieldID;
        public string namePhotoTable;
        public string namePhotoField;
        public string namePhotoFile;
    }
    public struct tableAndGroupInfo
    {
        public int IdTable;
        public int IdGroup;
        public int OrderNum;
        public tableAndGroupInfo(int idTable, int idGroup, int orderNum)
        {
            IdTable = idTable;
            IdGroup = idGroup;
            OrderNum = orderNum;
        }
    }
    public struct groupInfo
    {
        public int id;
        public string name;
        public string descript;
    }

    public struct MenuItem
    {
        public int IdTable;
        public Func<ToolStripMenuItem> ToolStripMenuItem;
    }

    public class FieldKeyValue
    {
        public fieldInfo Key { get; private set; }
        public int Value { get; private set; }

        public FieldKeyValue(fieldInfo key, int value)
        {
            Key = key;
            Value = value;
        }
    }

}
