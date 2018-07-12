using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Data;

namespace Rekod.ImportExport
{
    /// <summary>
    /// Interaction logic for ExportQuestion.xaml
    /// </summary>
    public partial class ExportQuestion : Window
    {
        private int answer;
        /// <summary>
        /// 1 - не выгружать (значение по умолчанию), 0 - выгрузить без геометрии
        /// </summary>
        public int Answer { get { return answer; } }
        public ExportQuestion(DataTable datas)
        {
            InitializeComponent();
            answer = 1;
            errGrid.ItemsSource = datas.DefaultView;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            answer = 1;
            Close();
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            answer = 0;
            Close();
        }
    }
}
