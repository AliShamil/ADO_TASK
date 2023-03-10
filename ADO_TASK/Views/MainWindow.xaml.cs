using ADO_TASK.Views;
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
        SqlTransaction tran = null;
        SqlConnection? connection = null;
        SqlDataAdapter? adapter = null;
        DataViewManager? dataView = null;
        DataSet? dataSet = null;

        public MainWindow()
        {
            InitializeComponent();
            Configuration();

            #region TableMapping

            adapter?.TableMappings.Add("Table", "Products");
            adapter?.TableMappings.Add("Table1", "Categories");
            adapter?.TableMappings.Add("Table2", "Ratings");

            #endregion
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
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (connection is not null && dataSet is not null)
            {
                adapter?.Fill(dataSet);
                ProductListView.ItemsSource = dataSet.Tables["Products"]?.AsDataView();

                Categories_Cbox.DataContext = dataSet.Tables["Categories"];
                Categories_Cbox.DisplayMemberPath = dataSet.Tables["Categories"]?.Columns["Name"]?.ColumnName;
            }
        }



        private void Categories_Cbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (Categories_Cbox.SelectedItem is DataRowView selectedView)
            {

                var id = selectedView.Row["Id"];

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

            var view = dataView?.CreateDataView(dataSet?.Tables["Products"]!)!;

            view.RowFilter =  $"Name LIKE '%{Txt_Search.Text}%'";


            ProductListView.ItemsSource = view;
        }


        private void Btn_Remove_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                connection?.Open();
                tran = connection?.BeginTransaction();

                var command = connection?.CreateCommand();

                if (command is null)
                    return;


                command.Transaction = tran;

                command.CommandText = "sp_RemoveProduct";
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add("productid", SqlDbType.Int);
                command.Parameters["productid"].Value = (int)(ProductListView.SelectedItem as DataRowView)?.Row["Id"]!;



                command.ExecuteNonQuery();

                tran?.Commit();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                tran?.Rollback();
            }
            finally
            {
                connection?.Close();
            }

            dataSet?.Clear();
            Window_Loaded(sender, e);
        }

        private void Btn_Edit_Click(object sender, RoutedEventArgs e)
        {
            if (connection is not null)
            {
                UpdateProductView updateProductView = new(connection, dataSet?.Tables["Categories"], ProductListView.SelectedItem as DataRowView);
                
                var result = updateProductView.ShowDialog();
                if (result is true)
                {
                    dataSet?.Clear();
                    Window_Loaded(sender, e);
                }
            }
        }

        private void Btn_AddRating_Click(object sender, RoutedEventArgs e)
        {
            if (ProductListView.SelectedItem is not null && connection is not null)
            {
                var selectedItemId = (int)(ProductListView.SelectedItem as DataRowView)?.Row["Id"]!;

                AddRatingView addView = new(connection, selectedItemId);

                var result = addView.ShowDialog();
                if (result is true)
                {
                    dataSet?.Clear();
                    Window_Loaded(sender, e);
                }
            }
        }

        private void Btn_AddProduct_Click(object sender, RoutedEventArgs e)
        {
            if (connection is not null)
            {
                AddProductView addProductView = new(connection, dataSet?.Tables["Categories"]);

                var result = addProductView.ShowDialog();
                if (result is true)
                {
                    dataSet?.Clear();
                    Window_Loaded(sender, e);
                }
            }
        }
    }

}
