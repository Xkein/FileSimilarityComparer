using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        private void ColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            var clickedHeader = e.OriginalSource as GridViewColumnHeader;
            if (clickedHeader != null)
            {
                //Get clicked column
                GridViewColumn clickedColumn = clickedHeader.Column;
                if (clickedColumn != null)
                {
                    var binding = clickedColumn.DisplayMemberBinding as Binding;
                    //Get binding property of clicked column
                    string bindingProperty = binding.Path.Path;

                    var lv = sender as ListView;
                    SortDescriptionCollection sdc = lv.Items.SortDescriptions;
                    ListSortDirection sortDirection = ListSortDirection.Ascending;
                    if (sdc.Count > 0)
                    {
                        SortDescription sd = sdc[0];
                        sortDirection = sd.Direction == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
                        sdc.Clear();
                    }
                    sdc.Add(new SortDescription(bindingProperty, sortDirection));
                }
            }
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            comparerClient.StopCompare();
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            comparerClient.ExportResult();
        }
    }
}
