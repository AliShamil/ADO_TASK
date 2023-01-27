using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
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
using System.Windows.Shapes;

namespace ADO_TASK.Views
{

    public partial class AddProductView : Window
    {
        SqlConnection? _connection = null;
        DataTable? _categories = null;
        SqlTransaction tran = null;
        public string? ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        private int categoryId { get; set; }

        public AddProductView(SqlConnection? connection, DataTable? categories)
        {
            InitializeComponent();
            DataContext = this;
            categoryId = -1;
            _connection = connection;
            _categories = categories;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CBoxCategories.DataContext = _categories;
            CBoxCategories.DisplayMemberPath = _categories?.Columns["Name"]?.ColumnName;
        }

        private void Categories_Cbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CBoxCategories.SelectedItem is DataRowView rowView)
            {
                categoryId = Convert.ToInt32(rowView.Row["Id"]);
            }
        }


        private void Btn_Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;

        private void Btn_Add_Click(object sender, RoutedEventArgs e)
        {
            if(Validation() is false)
                return;
           
            AddProduct();

            DialogResult= true;
        }

        private bool Validation()
        {
            StringBuilder builder = new();

            if (string.IsNullOrWhiteSpace(ProductName))
                builder.Append($"{nameof(ProductName)} Can't be empty or null!\n");

            if (Price <= 0)
                builder.Append($"{nameof(Price)} Can't be less or equal to zero!\n");

            if (categoryId==-1)
                builder.Append($"{nameof(categoryId)} Can't be empty!\n");

            if (Quantity< 0)
                builder.Append($"{nameof(Quantity)} Can't be less than zero!\n");

            if (builder.Length>0)
            {
                MessageBox.Show(builder.ToString());
                return false;
            }
            return true;
        }

        private void AddProduct()
        {
            try
            {
                _connection?.Open();
                tran = _connection?.BeginTransaction();

                var command = _connection?.CreateCommand();

                if (command is null)
                    return;


                command.Transaction = tran;

                command.CommandText = "sp_AddProduct";
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add("name", SqlDbType.NVarChar);
                command.Parameters["name"].Value = ProductName;

                command.Parameters.Add("categoryId", SqlDbType.Int);
                command.Parameters["categoryId"].Value = categoryId;

                command.Parameters.Add("price", SqlDbType.Money);
                command.Parameters["price"].Value = Price;

                command.Parameters.Add("quantity", SqlDbType.SmallInt);
                command.Parameters["quantity"].Value = Quantity;


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
                _connection?.Close();
            }
        }
    }
}
