using System;
using System.Collections.Generic;
using System.Text;

namespace Rekod
{
    public class layerRastrM
    {
        #region Поля
        private string name1, path1;
        private bool _isExternal;
        private bool _buildPyramids;
        private int _methodUse;
        private bool _usebounds;
        private int _minscale;
        private int _maxscale;
        #endregion // Поля

        #region Свойства
        public string Name
        {
            get { return name1; }
            set { name1 = value; }
        }
        public string Path
        {
            get { return path1; }
            set { path1 = value; }
        }
        public bool IsExternal
        {
            get { return _isExternal; }
            set { _isExternal = value; }
        }

        public bool BuildPyramids
        {
            get { return _buildPyramids; }
            set { _buildPyramids = value; }
        }
        public int MethodUse
        {
            get { return _methodUse; }
            set { _methodUse = value; }
        }
        public bool Usebounds
        {
            get { return _usebounds; }
            set { _usebounds = value; }
        }
        public int Minscale
        {
            get { return _minscale; }
            set { _minscale = value; }
        }
        public int Maxscale
        {
            get { return _maxscale; }
            set { _maxscale = value; }
        }
        #endregion // Свойства

        #region Конструктор
        public layerRastrM(string name, string path, bool isexternal)
        {
            name1 = name;
            path1 = path;
            _isExternal = isexternal;
        }
        #endregion // Конструктор

        #region Методы
        public override string ToString()
        {
            return name1;
        }
        #endregion // Методы
    }
}
