using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Commi
{
    public class CPathSimAnneal : CPath
    {
        #region Конструкторы
        public CPathSimAnneal(double[,] weightArray): base(weightArray)
        {

        } 
        #endregion Конструкторы

        #region Методы
        //метод, реулизующий алгоритм поиска оптимального пути
        public override void FindBestPath()
        {
            Random random = new Random();
            for (int fails = 0, F = _path.Length * _path.Length; fails < F; )
            {
                //выбираем два случайных города
                //первый и последний индексы не трогаем
                int p1 = 0, p2 = 0;
                while (p1 == p2)
                {
                    p1 = random.Next(1, _path.Length - 1);
                    p2 = random.Next(1, _path.Length - 1);
                }
                //проверка расстояний
                double sum1 = _distance[_path[p1 - 1], _path[p1]] + _distance[_path[p1], _path[p1 + 1]] +
                              _distance[_path[p2 - 1], _path[p2]] + _distance[_path[p2], _path[p2 + 1]];
                double sum2 = _distance[_path[p1 - 1], _path[p2]] + _distance[_path[p2], _path[p1 + 1]] +
                              _distance[_path[p2 - 1], _path[p1]] + _distance[_path[p1], _path[p2 + 1]];
                if (sum2 < sum1)
                {
                    int temp = _path[p1];
                    _path[p1] = _path[p2];
                    _path[p2] = temp;
                }
                else
                {
                    fails++;
                }
            }
        }
        #endregion Методы
    }
}