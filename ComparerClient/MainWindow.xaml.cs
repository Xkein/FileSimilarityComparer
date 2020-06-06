using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ComparerClient
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        CClient comparerClient;
        public MainWindow()
        {
            comparerClient = new CClient();
            DataContext = comparerClient;
            InitializeComponent();
        }


        private void Compare_Click(object sender, RoutedEventArgs e)
        {
            comparerClient.StartCompare();
        }

        private void SelectFolderButton_Click(object sender, RoutedEventArgs e)
        {
            comparerClient.SelectFolder();
        }
    }
}
