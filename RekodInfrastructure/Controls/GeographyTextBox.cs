using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Rekod.DataAccess.SourcePostgres.ViewModel.PgAttributes;
using System.Windows;
using System.Text.RegularExpressions;
using System.Globalization;

namespace Rekod.Controls
{
    public class GeographyTextBox: TextBox
    {
        #region Поля
        private String _exValue = "";
        #endregion Поля

        #region Статические члены
        // Using a DependencyProperty as the backing store for CoordsViewType.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CoordsViewTypeProperty =
            DependencyProperty.Register("CoordsViewType", typeof(ECoordsViewType), typeof(GeographyTextBox), new UIPropertyMetadata(ECoordsViewType.Degrees, new PropertyChangedCallback(CoordsViewTypeChanged)));
        private static void CoordsViewTypeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            GeographyTextBox textBox = sender as GeographyTextBox;            
            textBox.Text = textBox.DoubleToFormattedString
                (
                    textBox.Coordinate,
                    textBox.IsWgsProjection,
                    textBox.CoordsViewType
                 );
            textBox.HasError = false;
        }

        // Using a DependencyProperty as the backing store for IsWgsProjection.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsWgsProjectionProperty =
            DependencyProperty.Register("IsWgsProjection", typeof(bool), typeof(GeographyTextBox), new UIPropertyMetadata(false, new PropertyChangedCallback(IsWgsProjectionChanged)));
        private static void IsWgsProjectionChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            GeographyTextBox textBox = sender as GeographyTextBox;
            textBox.Text = textBox.DoubleToFormattedString
                (
                    textBox.Coordinate,
                    textBox.IsWgsProjection,
                    textBox.CoordsViewType
                 );
            textBox.HasError = false;
        }

        // Using a DependencyProperty as the backing store for Coordinate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CoordinateProperty =
            DependencyProperty.Register("Coordinate", typeof(double), typeof(GeographyTextBox), new UIPropertyMetadata(0.0, new PropertyChangedCallback(CoordinateChanged)));
        private static void CoordinateChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            GeographyTextBox textBox = sender as GeographyTextBox;
            textBox.Text = textBox.DoubleToFormattedString
                (
                    Convert.ToDouble(e.NewValue), 
                    textBox.IsWgsProjection, 
                    textBox.CoordsViewType
                 );
            textBox.HasError = false;
        }

        // Using a DependencyProperty as the backing store for HasError.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HasErrorProperty =
            DependencyProperty.Register("HasError", typeof(bool), typeof(GeographyTextBox), new UIPropertyMetadata(false));

        // Using a DependencyProperty as the backing store for HasError.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CoordTypeProperty =
            DependencyProperty.Register("CoordType", typeof(ECoordinateType), typeof(GeographyTextBox), new UIPropertyMetadata(ECoordinateType.Longitude));

        private static CultureInfo _coordsCulture = new CultureInfo("en-US"); 
        #endregion Статические члены

        #region Конструкторы
        public GeographyTextBox()
            : base()
        {
            TextChanged += GeographyTextBox_TextChanged;
            GotFocus += new RoutedEventHandler(GeographyTextBox_GotFocus);
            LostFocus += new RoutedEventHandler(GeographyTextBox_LostFocus);
            PreviewKeyDown += new System.Windows.Input.KeyEventHandler(GeographyTextBox_PreviewKeyDown);
            _exValue = Text;           
        }
        #endregion Конструкторы

        #region Свойства
        public ECoordsViewType CoordsViewType
        {
            get { return (ECoordsViewType)GetValue(CoordsViewTypeProperty); }
            set { SetValue(CoordsViewTypeProperty, value); }
        }
        public bool IsWgsProjection
        {
            get { return (bool)GetValue(IsWgsProjectionProperty); }
            set { SetValue(IsWgsProjectionProperty, value); }
        }
        public double Coordinate
        {
            get { return (double)GetValue(CoordinateProperty); }
            set { SetValue(CoordinateProperty, value); }
        }
        public bool HasError
        {
            get { return (bool)GetValue(HasErrorProperty); }
            set { SetValue(HasErrorProperty, value); }
        }
        public ECoordinateType CoordType
        {
            get { return (ECoordinateType)GetValue(CoordTypeProperty); }
            set 
            {
                SetValue(CoordTypeProperty, value);
            }
        }
        #endregion Свойства

        #region Методы
        public String DoubleToFormattedString(Double coordvalue, bool iswgs, ECoordsViewType viewtype)
        {
            String result = "";
            int sign = Math.Sign(coordvalue);
            coordvalue = Math.Abs(coordvalue); 
            if (!iswgs)
            {
                result = coordvalue.ToString();
            }
            else
            {
                switch (viewtype)
                {
                    case ECoordsViewType.Degrees:
                        {
                            result = String.Format(_coordsCulture, "{0:f12}°", coordvalue);
                            break;
                        }
                    case ECoordsViewType.DegreesMinutes:
                        {
                            int degree = (int)Math.Floor(coordvalue);
                            double minutes = (coordvalue - degree)*60;
                            result = String.Format(_coordsCulture, "{0}° {1:f12}'", degree, minutes);
                            break;
                        }
                    case ECoordsViewType.DegreesMinutesSeconds:
                        {
                            double degree = (int)Math.Truncate(coordvalue);
                            double minutes = (coordvalue - degree) * 60;
                            int minutesint = (int)Math.Floor(minutes);
                            double seconds = (minutes - minutesint) * 60;
                            result = String.Format(_coordsCulture, "{0}° {1}' {2:f12}''", degree, minutesint, seconds);
                            break;
                        }
                }
            }
            if (sign < 0)
            {
                result = "-" + result; 
            }
            return result;
        }
        public Double? FormattedStringToDouble(String formstring, bool iswgs, ECoordsViewType viewtype)
        {
            formstring = formstring.Trim();
            bool makeNegativ = false;
            if (!String.IsNullOrEmpty(formstring))
            {
                makeNegativ = formstring.StartsWith("-");
                if (makeNegativ)
                {
                    formstring = formstring.Substring(1);
                }
            }

            Double? result = null;
            bool parsed = true; 
            if (!iswgs)
            {
                double degrees;
                formstring = Regex.Replace(formstring, "\\D", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
                parsed &= Double.TryParse(formstring, out degrees);
                if (parsed)
                {
                    result = degrees;
                }
            }
            else
            {
                double degrees = 0.0;
                double minutes = 0.0;
                double seconds = 0.0;

                switch (viewtype)
                {
                    case ECoordsViewType.Degrees:
                        {
                            String pattern = @"^(.*)°$";
                            Match match = Regex.Match(formstring, pattern);
                            string coordDegrees = match.Groups[1].Value;
                            coordDegrees = Regex.Replace(coordDegrees, "\\D", ".");
                            parsed &= Double.TryParse(coordDegrees, NumberStyles.Any, _coordsCulture, out degrees);
                            break;
                        }
                    case ECoordsViewType.DegreesMinutes:
                        {
                            String pattern = @"^(.*)° (.*)'$";
                            Match match = Regex.Match(formstring, pattern);
                            string coordDegrees = match.Groups[1].Value;
                            string coordMinutes = match.Groups[2].Value;
                            coordMinutes = Regex.Replace(coordMinutes, "\\D", ".");
                            parsed &= Double.TryParse(coordDegrees, NumberStyles.Any, _coordsCulture, out degrees);
                            parsed &= Double.TryParse(coordMinutes, NumberStyles.Any, _coordsCulture, out minutes);
                            if (parsed)
                            {
                                degrees = Math.Truncate(degrees); 
                            }
                            break;
                        }
                    case ECoordsViewType.DegreesMinutesSeconds:
                        {
                            String pattern = @"^(.*)° (.*)' (.*)''$";
                            Match match = Regex.Match(formstring, pattern);
                            string coordDegrees = match.Groups[1].Value;
                            string coordMinutes = match.Groups[2].Value;
                            string coordSeconds = match.Groups[3].Value;
                            coordSeconds = Regex.Replace(coordSeconds, "\\D", ".");
                            parsed &= Double.TryParse(coordDegrees, NumberStyles.Any, _coordsCulture, out degrees);
                            parsed &= Double.TryParse(coordMinutes, NumberStyles.Any, _coordsCulture, out minutes);
                            parsed &= Double.TryParse(coordSeconds, NumberStyles.Any, _coordsCulture, out seconds);
                            if (parsed)
                            {
                                degrees = Math.Truncate(degrees);
                                minutes = Math.Truncate(minutes);
                            }
                            break;
                        }
                }
                if (parsed)
                {
                    double coordLimit = (CoordType == ECoordinateType.Longitude) ? 180 : 90;
                    if (degrees < -coordLimit)
                    {
                        degrees = -coordLimit;
                    }
                    if (degrees > coordLimit)
                    {
                        degrees = coordLimit;
                    }
                    if (viewtype != ECoordsViewType.Degrees && Math.Abs(degrees) == coordLimit)
                    {
                        minutes = 0;
                        seconds = 0;
                    }
                    else
                    {
                        if (minutes < 0)
                        {
                            minutes = 0;
                        }
                        if (minutes > 60)
                        {
                            minutes = 60;
                        }
                        if (seconds < 0)
                        {
                            seconds = 0;
                        }
                        if (seconds > 60)
                        {
                            seconds = 60;
                        }
                    }
                    int sign = Math.Sign(degrees) == 0 ? 1 : Math.Sign(degrees);
                    result = ((seconds / 60d + minutes) / 60d + Math.Abs(degrees)) * sign;
                }
            }
            if (makeNegativ)
            {
                result = result * (-1);
            }
            return result; 
        }
        public bool TextIsValid(String text)
        {
            bool result = true; 
            if(IsWgsProjection)
            {
                switch (CoordsViewType)
                {
                    case ECoordsViewType.Degrees:
                        {
                            String regExp = @"^-?.*°$";
                            result = Regex.IsMatch(text, regExp);
                            break;
                        }
                    case ECoordsViewType.DegreesMinutes:
                        {
                            String regExp = @"^-?.*° .*'$";
                            result = Regex.IsMatch(text, regExp);
                            break;
                        }
                    case ECoordsViewType.DegreesMinutesSeconds:
                        {
                            String regExp = @"^-?.*° .*' .*''$";
                            result = Regex.IsMatch(text, regExp);
                            break;
                        }
                }
            }
            return result; 
        }
        private void CoerceCoordinate()
        {
            Double? res = FormattedStringToDouble
                            (
                                Text,
                                IsWgsProjection,
                                CoordsViewType
                            );
            if (res != null)
            {
                Coordinate = (Double)res;
                HasError = false;
                Text = DoubleToFormattedString(Coordinate, IsWgsProjection, CoordsViewType);
            }
            else
            {
                HasError = true;
            }
        }
        #endregion Методы 

        #region Обработчики
        void GeographyTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            int selStart = SelectionStart;
            if (TextIsValid(Text))
            {
                _exValue = Text;
            }
            else
            {
                Text = _exValue;
                SelectionStart = selStart; 
            }
        }
        void GeographyTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            CoerceCoordinate();
        }
        void GeographyTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            
        }
        void GeographyTextBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Tab && IsWgsProjection)
            {
                int selStart = SelectionStart;
                int indexOfDeg = Text.IndexOf("°");
                int indexOfMin = Text.IndexOf("'");
                int indexOfSec = Text.IndexOf("''");

                switch (CoordsViewType)
                {
                    case ECoordsViewType.Degrees:
                        {
                            break;
                        }
                    case ECoordsViewType.DegreesMinutes:
                        {
                            if (selStart <= indexOfDeg)
                            {
                                if (indexOfMin > indexOfDeg)
                                {
                                    SelectionStart = indexOfDeg + 2;
                                    SelectionLength = (indexOfMin - indexOfDeg - 2);
                                }
                                e.Handled = true;
                            }
                            break;
                        }
                    case ECoordsViewType.DegreesMinutesSeconds:
                        {
                            if (selStart <= indexOfDeg)
                            {
                                if (indexOfMin > indexOfDeg)
                                {
                                    SelectionStart = indexOfDeg + 2;
                                    SelectionLength = (indexOfMin - indexOfDeg - 2);
                                }
                                e.Handled = true;
                            }
                            else if (selStart > indexOfDeg && selStart <= indexOfMin)
                            {
                                if (indexOfSec > indexOfMin)
                                {
                                    SelectionStart = indexOfMin + 2;
                                    SelectionLength = (indexOfSec - indexOfMin - 2);
                                }
                                e.Handled = true;
                            }
                            break;
                        }
                }
            }
            else
            {
                e.Handled = false; 
            }
        }
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
 	        base.OnPropertyChanged(e);
            if (e.Property == GeographyTextBox.IsWgsProjectionProperty)
            {
                if (IsWgsProjection)
                {
                    CoerceCoordinate();
                }
            }
        }
        #endregion Обработчики
    }
    public enum ECoordinateType
    {
        Longitude, Latitude
    }
}