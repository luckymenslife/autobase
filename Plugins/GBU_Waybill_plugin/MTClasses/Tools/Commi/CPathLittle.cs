using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commi
{
    public class Gij
    {
        private int _i;

        public int I
        {
            get { return _i; }
            set { _i = value; }
        }

        private int _j;

        public int J
        {
            get { return _j; }
            set { _j = value; }
        }

        private double _value;

        public double Value
        {
            get { return _value; }
            set { _value = value; }
        }
        
        public override string ToString()
        {
            return String.Format("i:{0}; j:{1}; val:{2}", _i + 1, _j + 1, _value);
        }
    }

    public class CPathLittle: CPath
    {
        #region Поля
        private double _lowerBound = Double.MaxValue;
        #endregion Поля

        #region Конструкторы
        public CPathLittle(double[,] weightArray):base(weightArray)
        {
            int pointsCount = weightArray.GetLength(0);
            for (int i = 0; i < pointsCount; i++)
            {
                _distance[i, i] = Double.MaxValue;
            }
        } 
        #endregion Конструкторы

        #region Методы
        //метод, реулизующий алгоритм поиска оптимального пути
        public override void FindBestPath()
        {
            double[,] copyDistance = CopyArray(_distance);
            Iteration(copyDistance, new List<Gij>(), 0);
        }

        private double[,] CopyArray(double[,] arr)
        {
            int pointsCount = arr.GetLength(0);
            double[,] copyArray = new double[pointsCount, pointsCount];
            for(int i = 0; i<pointsCount; i++)
            {
                for(int j = 0; j<pointsCount;  j++)
                {
                    copyArray[i, j] = arr[i, j];
                }
            }
            return copyArray;
        }

        private void Iteration(double[,] iterArray, List<Gij> iterRoute, double lowerBound)
        {
            int[] rowsExclude = (from Gij gij in iterRoute select gij.I).ToArray();
            int[] colsExclude = (from Gij gij in iterRoute select gij.J).ToArray();
            int pointsCount = iterArray.GetLength(0);

            int? colWithoutEternity = null;
            int? rowWithoutEternity = null;

            #region Without Eternity
            for (int i = 0; i < pointsCount; i++)
            {
                if (rowsExclude.Contains(i))
                {
                    continue;
                }
                bool hasEternity = false;
                for (int j = 0; j < pointsCount; j++)
                {
                    if (colsExclude.Contains(j))
                    {
                        continue;
                    }
                    if (iterArray[i, j] == double.MaxValue)
                    {
                        hasEternity = true;
                    }
                }
                if(!hasEternity)
                {
                    rowWithoutEternity = i;
                    break;
                }
            }
            for (int j = 0; j < pointsCount; j++)
            {
                if (colsExclude.Contains(j))
                {
                    continue;
                }
                bool hasEternity = false;
                for (int i = 0; i < pointsCount; i++)
                {
                    if (rowsExclude.Contains(i))
                    {
                        continue;
                    }
                    if (iterArray[i, j] == double.MaxValue)
                    {
                        hasEternity = true;
                    }
                }
                if (!hasEternity)
                {
                    colWithoutEternity = j;
                    break;
                }
            }
            if (rowWithoutEternity != null && colWithoutEternity != null)
            {
                iterArray[rowWithoutEternity.Value, colWithoutEternity.Value] = Double.MaxValue;
            }
            #endregion Without Eternity

            #region Remove min from rows and cols
            //В каждой строке матрицы стоимости найдем минимальный элемент и вычтем его из всех элементов строки
            for (int i = 0; i < pointsCount; i++)
            {
                if (rowsExclude.Contains(i))
                {
                    continue;
                }
                double minElement = iterArray[i, 0];
                for (int j = 0; j < pointsCount; j++)
                {
                    if (colsExclude.Contains(j))
                    {
                        continue;
                    }
                    if (iterArray[i, j] < minElement)
                    {
                        minElement = iterArray[i, j];
                    }
                }
                lowerBound += minElement;
                for (int j = 0; j < pointsCount; j++)
                {
                    if (colsExclude.Contains(j))
                    {
                        continue;
                    }
                    iterArray[i, j] = iterArray[i, j] - minElement;
                }
            }

            //Сделаем это и для столбцов, не содержащих нуля.
            for (int j = 0; j < pointsCount; j++)
            {
                if (colsExclude.Contains(j))
                {
                    continue;
                }
                double minElement = iterArray[0, j];
                for (int i = 0; i < pointsCount; i++)
                {
                    if (rowsExclude.Contains(i))
                    {
                        continue;
                    }
                    if (iterArray[i, j] < minElement)
                    {
                        minElement = iterArray[i, j];
                    }
                }
                lowerBound += minElement;
                for (int i = 0; i < pointsCount; i++)
                {
                    if (rowsExclude.Contains(i))
                    {
                        continue;
                    }
                    iterArray[i, j] = iterArray[i, j] - minElement;
                }
            }
            #endregion Remove min from rows and cols

            #region Завершение работы алгоритма
            if(pointsCount - iterRoute.Count == 2)
            {
                if (lowerBound < _lowerBound)
                {
                    Print(iterArray, iterRoute);
                    for (int i = 0; i < pointsCount; i++)
                    {
                        if (rowsExclude.Contains(i))
                        {
                            continue;
                        }
                        for (int j = 0; j < pointsCount; j++)
                        {
                            if (colsExclude.Contains(j))
                            {
                                continue;
                            }
                            if (iterArray[i, j] == 0)
                            {
                                iterRoute.Add(new Gij() { I = i, J = j });
                            }
                        }
                    }

                    if (pointsCount == iterRoute.Count)
                    {
                        for (int i = 1; i < pointsCount; i++)
                        {
                            Gij gij = iterRoute[i - 1];
                            int nextPoint = gij.J;
                            for (int j = i; j < pointsCount; j++)
                            {
                                if (iterRoute[j].I == nextPoint)
                                {
                                    if (i != j)
                                    {
                                        Gij moveItem = iterRoute[j];
                                        iterRoute.RemoveAt(j);
                                        iterRoute.Insert(i, moveItem);
                                    }
                                    break;
                                }
                            }
                        }
                        for (int i = 0; i < iterRoute.Count; i++)
                        {
                            _path[i] = iterRoute[i].I;
                        }
                        _path[iterRoute.Count] = _path[0];
                        _lowerBound = lowerBound;
                    }
                }
                return;
            }
            #endregion Завершение работы алгоритма

            //Для каждого нулевого элемента 
            List<Gij> gijList = new List<Gij>();
            for (int i = 0; i < pointsCount; i++)
            {
                if (rowsExclude.Contains(i))
                {
                    continue;
                }
                for (int j = 0; j < pointsCount; j++)
                {
                    if (colsExclude.Contains(j))
                    {
                        continue;
                    }
                    //Рассчитаем значение Гij, равное сумме наименьшего элемента i строки (исключая элемент Сij=0) и наименьшего элемента j столбца.
                    if (iterArray[i, j] == 0)
                    {
                        Double minIRow = Double.MaxValue;
                        Double minJCol = Double.MaxValue;
                        for (int jj = 0; jj < pointsCount; jj++)
                        {
                            if (colsExclude.Contains(jj))
                            {
                                continue;
                            }
                            if (jj != j)
                            {
                                if (iterArray[i, jj] < minIRow)
                                {
                                    minIRow = iterArray[i, jj];
                                }
                            }
                        }
                        for (int ii = 0; ii < pointsCount; ii++)
                        {
                            if (rowsExclude.Contains(ii))
                            {
                                continue;
                            }
                            if (ii != i)
                            {
                                if (iterArray[ii, j] < minJCol)
                                {
                                    minJCol = iterArray[ii, j];
                                }
                            }
                        }
                        Gij gij = new Gij() { I = i, J = j, Value = (minIRow + minJCol) };
                        gijList.Add(gij);
                    }
                }
            }

            double maxGijVal = gijList[0].Value;
            foreach(Gij gij in gijList)
            {
                if(gij.Value > maxGijVal)
                {
                    maxGijVal = gij.Value;
                }
            }
            var maxGijList = from Gij gij in gijList where gij.Value == maxGijVal select gij;
            foreach (Gij gij in maxGijList)
            {
                List<Gij> nextIterGijList = new List<Gij>(iterRoute);
                nextIterGijList.Add(gij);
                double[,] copyArray = CopyArray(iterArray);
                Iteration(copyArray, nextIterGijList, lowerBound);
            }
        }

        private void Print(double[,] arr, List<Gij> iterRoute)
        {
            int pointsCount = arr.GetLength(0);
            int[] rowsExclude = (from Gij gij in iterRoute select gij.I).ToArray();
            int[] colsExclude = (from Gij gij in iterRoute select gij.J).ToArray();

            String pt = "";
            for(int i = 0; i<pointsCount; i++)
            {
                if(rowsExclude.Contains(i))
                {
                    continue;
                }
                for(int j = 0; j<pointsCount; j++)
                {
                    if(colsExclude.Contains(j))
                    {
                        continue;
                    }
                    double printVal = arr[i, j];
                    if (printVal == Double.MaxValue)
                    {
                        pt += "N\t";
                    }
                    else
                    {
                        pt += arr[i, j] + "\t";
                    }
                }
                pt += "\n";
            }
            Debug.Print("");
            Debug.Print(pt);
        }
        #endregion Методы
    }
}