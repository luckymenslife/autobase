using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rekod.DataAccess.SourcePostgres.Model.PgFullTextSearch
{
    public class PgSearchObjectM
    {
        #region Поля
        private string _labelText;
        private PgSearchTableM _searchTable;
        private int _idObject;        
        #endregion Поля

        #region Конструкторы
        public PgSearchObjectM(PgSearchTableM searchtable, int idobject, String labeltext)
        {
            _idObject = idobject;
            _searchTable = searchtable; 
            _labelText = labeltext;
        }
        #endregion Конструкторы

        #region Свойства
        public string LabelText
        {
            get { return _labelText; }
        }
        public int IdObject
        {
            get { return _idObject; }
        }
        public PgSearchTableM SearchTable
        {
            get { return _searchTable; }
        }
        #endregion Свойства
    }
}