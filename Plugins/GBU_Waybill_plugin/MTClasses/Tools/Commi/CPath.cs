using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commi
{
    public abstract class CPath
    {
        #region Поля
        //расстояния между городами
        protected double[,] _distance;
        //индексы городов формируют искомый путь
        protected int[] _path;
        #endregion Поля

        #region Конструкторы
        public CPath(double[,] weightArray)
        {
            _distance = weightArray;
            int pointsCount = weightArray.GetLength(0);
            _path = new int[pointsCount + 1];
            for (int i = 0; i < pointsCount; i++)
            {
                _path[i] = i;
            }
            _path[pointsCount] = 0;
        }  
        #endregion Конструкторы 
        
        #region Свойства
        //расстояния между городами
        public double[,] Distance
        {
            get { return _distance; }
        }
        //индексы городов формируют искомый путь
        public int[] Path
        {
            get { return _path; }
        }
        #endregion Свойства

        #region Методы
        public abstract void FindBestPath();
        //возвращает длину пути
        public double PathLength()
        {
            double pathSum = 0;
            for (int i = 0; i < _path.Length - 1; i++)
            {
                pathSum += _distance[_path[i], _path[i + 1]];
            }
            pathSum += _distance[_path[_path.Length - 1], _path[0]];
            return pathSum;
        }  
        #endregion Методы
    }
}