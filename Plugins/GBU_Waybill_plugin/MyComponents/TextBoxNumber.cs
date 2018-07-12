using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GBU_Waybill_plugin.MyComponents
{
    /// <summary>
    /// Переопределенный компонент TextBox для ввода только числовых значений
    /// </summary>
    public class TextBoxNumber : TextBox
    {
        public TextBoxNumber()
        {
            this.KeyPress += TextBoxNumber_KeyPress;
            this.TextChanged += TextBoxNumber_TextChanged;
        }
        bool have_separator = false;

        /// <summary>
        /// После любого изменения проверяем на то, что у нас в тексте только цифры
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void TextBoxNumber_TextChanged(object sender, EventArgs e)
        {
            string new_value = this.Text;
            string allow_value = "";

            if (new_value == null || new_value.Length <= 0) return;

            foreach (char symbol in new_value.ToCharArray())
            {
                if (char.IsDigit(symbol) || System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator.Equals(symbol.ToString()) || System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NegativeSign.Equals(symbol.ToString()))
                {
                    if (System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator.Equals(symbol.ToString()) && !have_separator)
                    {
                        have_separator = true;
                        allow_value += symbol;
                    }
                    else
                    {
                        allow_value += symbol;
                    }
                }
            }

            this.Text = allow_value;
        }

        /// <summary>
        /// Если набираем с клавиатуры, то сразу проверяем на ввод цифр
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void TextBoxNumber_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!e.KeyChar.Equals(System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator.ToCharArray()[0]) && !e.KeyChar.Equals(System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NegativeSign.ToCharArray()[0]) && !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar))
            {
                e.Handled = true;
            }



        }


    }
}
