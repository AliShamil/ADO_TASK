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
        SqlConnection? connection = null;
        DataTable? categories = null;

        public string? ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        private int categoryId { get; set; }

        public AddProductView(SqlConnection? sqlConnection, DataTable? categories)
        {
            InitializeComponent();
            DataContext = this;
            connection = sqlConnection;
            this.categories = categories;
            categoryId = -1;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CBoxCategories.DataContext = categories;
            CBoxCategories.DisplayMemberPath = categories?.Columns["Name"]?.ColumnName;
        }

        private void CBoxCategories_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CBoxCategories.SelectedItem is DataRowView rowView)
            {
                var row = rowView.Row;
                categoryId = Convert.ToInt32(row["Id"]);
            }
        }


        private void ButtonCancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder builder = new StringBuilder();

            if (string.IsNullOrWhiteSpace(ProductName))
                builder.Append($"{nameof(ProductName)} Cannot Be Empty\n");

            if (Price <= 0)
                builder.Append($"{nameof(Price)} Cannot be below or equal to 0\n");

            if (Quantity< 0)
                builder.Append($"{nameof(Quantity)} Cannot be below 0\n");

            if (categoryId==-1)
                builder.Append($"{nameof(categoryId)} Must be choosen\n");

            if (builder.Length>0)
            {
                MessageBox.Show(builder.ToString());
                return;
            }

            AddProduct();

            DialogResult= true;
        }


        private void AddProduct()
        {
                var tran = connection?.BeginTransaction();
            try
            {
                connection?.Open();

                var command = connection?.CreateCommand();

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
                connection?.Close();
            }
        }
    }
}
