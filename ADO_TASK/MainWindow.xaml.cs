using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
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

namespace ADO_TASK
{

    public partial class MainWindow : Window
    {

        SqlConnection? connection = null;
        SqlDataAdapter? adapter = null;
        DataViewManager? dataView = null;
        DataSet? dataSet = null;
        public MainWindow()
        {
            InitializeComponent();
            Configuration();
        }
        private void Configuration()
        {
            var conStr = new ConfigurationBuilder()
                        .AddJsonFile("appsettings.json")
                        .Build()
                        .GetConnectionString("ConStr");

            connection = new SqlConnection(conStr);
            adapter = new SqlDataAdapter("SELECT * FROM Products; SELECT * FROM Categories; SELECT * FROM Ratings", connection);
            dataSet = new DataSet();
            dataView = new DataViewManager(dataSet);

            adapter.TableMappings.Add("Table", "Products");
            adapter.TableMappings.Add("Table1", "Categories");
            adapter.TableMappings.Add("Table2", "Ratings");
        }
        private  void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (connection is not null && dataSet is not null)
            {
                adapter?.Fill(dataSet);
                ProductListView.ItemsSource = dataSet.Tables["Products"]?.AsDataView();

                Categories_Cbox.DataContext = dataSet.Tables["Categories"];
                Categories_Cbox.DisplayMemberPath = dataSet.Tables["Categories"]?.Columns["Name"]?.ColumnName;
            }
        }



        private  void Categories_Cbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (Categories_Cbox.SelectedItem is DataRowView rowView)
            {

                var id = rowView.Row["Id"];

                var table = dataSet?.Tables["Products"];

                if (table != null && dataView != null)
                {
                    var view = dataView.CreateDataView(table);

                    view.RowFilter = $"CategoryId = {id}";

                    ProductListView.ItemsSource = view;
                }

            }
        }

        private void Txt_Search_SelectionChanged(object sender, RoutedEventArgs e)
        {


            if (string.IsNullOrWhiteSpace(Txt_Search.Text))
            {
                ProductListView.ItemsSource = dataSet?.Tables["Products"]?.AsDataView();
                return;
            }

            var view = dataView?.CreateDataView(dataSet?.Tables["Products"]);

            view.RowFilter =  $"Name LIKE '%{Txt_Search.Text}%'";


            ProductListView.ItemsSource = view;
        }


        private void Btn_Remove_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Btn_Edit_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ProductListView_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            
        }

        private void Btn_AddRating_Click(object sender, RoutedEventArgs e)
        {
            if (ProductListView.SelectedItem is not null && connection is not null)
            { 

            AddRatingView addView = new(connection, (int)(ProductListView.SelectedItem as DataRowView)?.Row["Id"]);

            var result = addView.ShowDialog();
            if (result is false)
            {
                dataSet.Clear();
                Window_Loaded(sender,e);
            }
            }
        }
    }

}
