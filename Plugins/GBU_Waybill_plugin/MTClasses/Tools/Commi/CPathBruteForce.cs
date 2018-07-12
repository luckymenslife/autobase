using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commi
{
    public class CPathBruteForce : CPath
    {
        #region Конструкторы
        public CPathBruteForce(double[,] weightArray): base(weightArray)
        {
            
        } 
        #endregion Конструкторы

        #region Методы
        private List<int> _idealPath = null;
        private double _idealPathWeight = Double.MaxValue;
        //метод, реулизующий алгоритм поиска оптимального пути
        public override void FindBestPath()
        {
            int pointsCount = _distance.GetLength(0);
            List<int> firstPath = new List<int>(); 
            for(int i = 1; i<pointsCount; i++)
            {
                firstPath.Add(i);
            }
            GetNextTestPath(new List<int>() { 0 }, firstPath);            
            for (int i = 0; i < pointsCount; i++)
            {
                _path[i] = _idealPath[i];
            }
            _path[pointsCount] = _path[0];
        }

        private void GetNextTestPath(List<int> leftPart, List<int> rightPart)
        {
            int pointsCount = _distance.GetLength(0);
            if (leftPart.Count == pointsCount)
            {
                double testPathWeight = GetTestPathLength(leftPart);
                if (testPathWeight < _idealPathWeight)
                {
                    _idealPathWeight = testPathWeight;
                    _idealPath = leftPart;
                }
                String route = "";
                for (int i = 0; i < leftPart.Count; i++)
                {
                    route += (leftPart[i] + " ");
                }
            }
            else
            {
                int rightPartLength = rightPart.Count;
                for (int i = 0; i < rightPartLength; i++)
                {
                    List<int> leftPartNew = new List<int>(leftPart);
                    List<int> rightPartNew = new List<int>(rightPart);
                    leftPartNew.Add(rightPart[i]);
                    rightPartNew.RemoveAt(i);
                    GetNextTestPath(leftPartNew, rightPartNew);
                }
            }
        }

        private double GetTestPathLength(List<int> testPath)
        {
            double pathWeight = 0;
            for (int i = 0; i < testPath.Count - 1; i++)
            {
                pathWeight += _distance[testPath[i], testPath[i + 1]];
            }
            pathWeight += _distance[testPath[testPath.Count - 1], testPath[0]];
            return pathWeight;
        }
        #endregion Методы
    }
}