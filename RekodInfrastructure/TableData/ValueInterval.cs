using System;
using System.Collections.Generic;
using System.Text;

namespace axVisUtils.TableData
{
    public class ValueInterval
    {
        private double _begin;
        private double _end;
        private bool nothingBegin;
        private bool nothingEnd;

        public ValueInterval(double? begin, double? end)
        {
            if (begin == null)
                nothingBegin = true;
            else
                _begin = begin.Value;

            if (end == null)
                nothingEnd = true;
            else
                _end = end.Value;
        }
        public double Start
        {
            get { return _begin; }
            set { _begin = value; }
        }

        public double End
        {
            get { return _end; }
            set { _end = value; }
        }

        public override string ToString()
        {
            return _begin.ToString() + " - " + _end.ToString();
        }

        public bool isInclude(double val)
        {
            bool rez = false;
            if (nothingBegin == true && val <= _end)
            {
                rez = true;
            }
            else if (nothingEnd == true && val >= _begin)
            {
                rez = true;
            }
            else if (val >= _begin && val < _end)
            {
                rez = true;
            }
            else if (val > _begin && val <= _end)
            {
                rez = true;
            }
            return rez;
        }
    }
}
