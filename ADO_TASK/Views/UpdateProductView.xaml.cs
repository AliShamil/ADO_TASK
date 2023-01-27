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
using System.Diagnostics;

namespace ADO_TASK.Views
{

    public partial class UpdateProductView : Window
    {
        SqlConnection? connection = null;
        DataTable? categories = null;

        public string? ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        private int categoryId { get; set; }
        private int productId { get; set; }

        public UpdateProductView(SqlConnection? sqlConnection, DataTable? categories, DataRowView? SelectedItem)
        {
            InitializeComponent();
            DataContext = this;
            this.connection = connection;
            this.categories = categories;
            ProductName = SelectedItem.Row["Name"].ToString();
            Quantity = Convert.ToInt32(SelectedItem.Row["Quantity"].ToString());
            Price =  Convert.ToDecimal(SelectedItem.Row["Price"].ToString());
            this.categoryId = Convert.ToInt32(SelectedItem.Row["CategoryID"].ToString());
            this.productId = Convert.ToInt32(SelectedItem.Row["Id"].ToString());
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CBoxCategories.DataContext = categories;
            CBoxCategories.DisplayMemberPath = categories?.Columns["Name"]?.ColumnName;

            CBoxCategories.SelectedIndex = categoryId - 1;
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

        private void ButtonUpdate_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder builder = new StringBuilder();

            if (string.IsNullOrWhiteSpace(ProductName))
                builder.Append($"{nameof(ProductName)} Cannot Be Empty\n");

            if (Price <= 0)
                builder.Append($"{nameof(Price)} Cannot be below or equal to 0\n");

            if (Quantity < 0)
                builder.Append($"{nameof(Quantity)} Cannot be below 0\n");

            if (categoryId == -1)
                builder.Append($"{nameof(categoryId)} Must be choosen\n");

            if (builder.Length > 0)
            {
                MessageBox.Show(builder.ToString());
                return;
            }

            UpdateProduct();

            DialogResult = false;
        }

        private void UpdateProduct()
        {
            try
            {
                connection?.Open();

                var command = connection?.CreateCommand();

                if (command is null)
                    return;

                var tran = connection?.BeginTransaction();

                command.Transaction = tran;

                command.CommandText = "sp_UpdateProduct";
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add("productId", SqlDbType.Int);
                command.Parameters["productId"].Value = productId;

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
            }
            finally
            {
                connection?.Close();
            }
        }
    }
}
