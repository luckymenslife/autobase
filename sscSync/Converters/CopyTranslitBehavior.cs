using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows; 
using System.Windows.Controls;

namespace sscSync.Converters
{
    public class CopyTranslitBehavior: DependencyObject
    {
        #region TranslitTextBox
        private static Dictionary<UIElement, List<UIElement>> spyTextChangeCollection = new Dictionary<UIElement, List<UIElement>>();

        static CopyTranslitBehavior()
        {
            _letterDictionary = InitalizeDictionary();
        }

        public static TextBox GetTranslitTextBox(DependencyObject obj)
        {
            return (TextBox)obj.GetValue(TranslitTextBoxProperty);
        }

        public static void SetTranslitTextBox(DependencyObject obj, TextBox value)
        {
            obj.SetValue(TranslitTextBoxProperty, value);
        }

        // Using a DependencyProperty as the backing store for TranslitTextBox. This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TranslitTextBoxProperty =
            DependencyProperty.RegisterAttached("TranslitTextBox", typeof(TextBox), typeof(CopyTranslitBehavior), new UIPropertyMetadata(null));
        #endregion TranslitTextBox

        #region UseTranslit
        public static bool GetUseTranslit(DependencyObject obj)
        {
            return (bool)obj.GetValue(UseTranslitProperty);
        }

        public static void SetUseTranslit(DependencyObject obj, bool value)
        {
            obj.SetValue(UseTranslitProperty, value);
        }

        // Using a DependencyProperty as the backing store for UseTranslit. This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UseTranslitProperty =
            DependencyProperty.RegisterAttached("UseTranslit", typeof(bool), typeof(CopyTranslitBehavior), new UIPropertyMetadata(false, new PropertyChangedCallback(UseTranslitChanged)));


        private static void UseTranslitChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            TextBox fromTextBox = textBox.GetValue(TranslitTextBoxProperty) as TextBox;
            if (fromTextBox != null)
            {
                if ((bool)e.NewValue)
                {
                    if (!spyTextChangeCollection.ContainsKey(fromTextBox))
                    {
                        spyTextChangeCollection.Add(fromTextBox, new List<UIElement>() { textBox });
                        //fromTextBox.Unloaded += new RoutedEventHandler(fromTextBox_Unloaded);
                    }
                    else if (!spyTextChangeCollection[fromTextBox].Contains(textBox))
                    {
                        spyTextChangeCollection[fromTextBox].Add(textBox);
                    }
                    fromTextBox.TextChanged += new TextChangedEventHandler(fromTextBox_TextChanged);
                }
                else
                {
                    fromTextBox.TextChanged -= new TextChangedEventHandler(fromTextBox_TextChanged);
                }
            }
        }

        static void fromTextBox_Unloaded(object sender, RoutedEventArgs e)
        {
            if (spyTextChangeCollection.ContainsKey(sender as TextBox))
            {
                spyTextChangeCollection.Remove(sender as TextBox);
            }
        }

        static void fromTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox fromTextBox = sender as TextBox;
            if (spyTextChangeCollection.ContainsKey(fromTextBox))
            {
                foreach (UIElement uiElement in spyTextChangeCollection[fromTextBox])
                {
                    TextBox textBox = uiElement as TextBox;
                    textBox.Text = Translit(fromTextBox.Text);
                }
            }
        }

        private static Dictionary<char, string> _letterDictionary;
        private static Dictionary<char, string> InitalizeDictionary()
        {
            Dictionary<char, string> dicc = new Dictionary<char, string>();
            dicc.Add('й', "j");
            dicc.Add('ц', "c");
            dicc.Add('у', "u");
            dicc.Add('к', "k");
            dicc.Add('е', "e");
            dicc.Add('н', "n");
            dicc.Add('г', "g");
            dicc.Add('ш', "sh");
            dicc.Add('щ', "w");
            dicc.Add('з', "z");
            dicc.Add('х', "h");
            dicc.Add('ъ', "");
            dicc.Add('ф', "f");
            dicc.Add('ы', "y");
            dicc.Add('в', "v");
            dicc.Add('а', "a");
            dicc.Add('п', "p");
            dicc.Add('р', "r");
            dicc.Add('о', "o");
            dicc.Add('л', "l");
            dicc.Add('д', "d");
            dicc.Add('ж', "zh");
            dicc.Add('э', "je");
            dicc.Add('я', "ja");
            dicc.Add('ч', "ch");
            dicc.Add('с', "s");
            dicc.Add('м', "m");
            dicc.Add('и', "i");
            dicc.Add('т', "t");
            dicc.Add('ь', "");
            dicc.Add('б', "b");
            dicc.Add('ю', "ju");
            dicc.Add('ё', "jo");
            return dicc;
        }

        public static string Translit(string source)
        {
            source = source.ToLower();
            string ret = "";
            foreach (char c in source)
            {
                if (_letterDictionary.ContainsKey(c))
                    ret += _letterDictionary[c];
                else if (char.IsLetterOrDigit(c))
                    ret += c.ToString();
                else ret += "_";
            }
            return ret;
        }
        #endregion UseTranslit

        #region CheckString
        public static bool GetCheckString(DependencyObject obj)
        {
            return (bool)obj.GetValue(CheckStringProperty);
        }

        public static void SetCheckString(DependencyObject obj, bool value)
        {
            obj.SetValue(CheckStringProperty, value);
        }

        // Using a DependencyProperty as the backing store for CheckString.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CheckStringProperty =
            DependencyProperty.RegisterAttached("CheckString", typeof(bool), typeof(CopyTranslitBehavior), new UIPropertyMetadata(false, new PropertyChangedCallback(CheckStringChanged)));

        private static void CheckStringChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if ((bool)e.NewValue == true)
            {
                textBox.TextChanged += new TextChangedEventHandler(textBox_TextChanged);
            }
            else 
            {
                textBox.TextChanged -= new TextChangedEventHandler(textBox_TextChanged);
            }
        }

        static void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            int start = textBox.SelectionStart;
            int len = textBox.SelectionLength;
     
            String properString = "";
            if (textBox.Text.Length > 0 && char.IsNumber(textBox.Text[0]))
            {
                properString += "_";
            }
            if (textBox.Text.Length > 0)
            {
                foreach (char c in textBox.Text)
                {
                    String translit = Translit(c.ToString().ToLower());
                    start += translit.Length - 1;
                    properString += translit; 
                }
            }    
            if (properString != textBox.Text)
            {
                textBox.Text = properString;
                textBox.SelectionStart = start;
                textBox.SelectionLength = len;
            }
        }
        #endregion CheckString
    }
}