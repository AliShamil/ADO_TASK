using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Data;
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
using System.Windows.Shapes;

namespace ADO_TASK
{
    public partial class AddRatingView : Window
    {
        SqlConnection _connection;
        SqlTransaction tran = null;
        private int _productId;
        public AddRatingView(SqlConnection connection, int productId)
        {
            InitializeComponent();
            _productId = productId;
            _connection = connection;
        }



        private void Button_Accept_Click(object sender, RoutedEventArgs e)
        {
            
            try
            {
                _connection?.Open();
                 tran = _connection?.BeginTransaction();

                var command = _connection?.CreateCommand();

                if (command is null)
                    return;


                command.Transaction = tran;

                command.CommandText = "AddRating";
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add("productid", SqlDbType.Int);
                command.Parameters["productid"].Value = _productId;

                command.Parameters.Add("rate", SqlDbType.Float);
                command.Parameters["rate"].Value = BasicRatingBar.Value;

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
            DialogResult = true;
        }

        private void Button_Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;

    }
}
