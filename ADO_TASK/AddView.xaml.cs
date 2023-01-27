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
using System.Windows.Shapes;

namespace ADO_TASK
{

    public partial class AddView : Window
    {
        private DbConnection? _connection;
        private DataTable? _table;
        DataTable CategoriesTable = null;

        public  AddView(DbConnection? connection,DataTable table)
        {
            InitializeComponent();
            _connection = connection;
            _table = table;
            DataContext = this;
            CategoriesTable =new();

            SqlDataReader? reader = null;

            try
            {
                _connection?.Open();

                using SqlCommand command = new SqlCommand("SELECT Id,Name FROM Categories;", (SqlConnection)_connection);
                reader =  command.ExecuteReader();
                int line = 0;

                do
                {
                    while (reader.Read())
                    {
                        if (line == 0)
                        {
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                CategoriesTable.Columns.Add(reader.GetName(i));
                            }
                            line++;
                        }
                        DataRow row = CategoriesTable.NewRow();

                        for (int i = 0; i < reader.FieldCount; i++)
                            row[i] = reader[i];

                        CategoriesTable.Rows.Add(row);
                    }
                } while (reader.NextResult());

                Categories_Cbox.DataContext = CategoriesTable;
                Categories_Cbox.DisplayMemberPath = CategoriesTable.Columns[1].ToString();



            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                _connection?.Close();
                reader?.Close();
            }
        }

        private async void Btn_Accept_Click(object sender, RoutedEventArgs e)
        {
            SqlDataReader? reader = null;

            try
            {
                _connection?.Open();

                using SqlCommand command = new SqlCommand("SELECT Id,Name FROM Categories;", (SqlConnection)_connection);
                reader = await command.ExecuteReaderAsync();
                int line = 0;

                do
                {
                    while (reader.Read())
                    {
                        if (line == 0)
                        {
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                CategoriesTable.Columns.Add(reader.GetName(i));
                            }
                            line++;
                        }
                        DataRow row = CategoriesTable.NewRow();

                        for (int i = 0; i < reader.FieldCount; i++)
                            row[i] = reader[i];

                        CategoriesTable.Rows.Add(row);
                    }
                } while (reader.NextResult());

                Categories_Cbox.DataContext = CategoriesTable;
                Categories_Cbox.DisplayMemberPath = CategoriesTable.Columns[1].ToString();



            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                _connection?.Close();
                reader?.Close();
            }
        }

        private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }


    }
}
