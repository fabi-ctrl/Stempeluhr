using SQLite;
using System;
using System.Windows;

namespace Stempeluhr
{
    /// <summary>
    /// Interaction logic for EditSaldo.xaml
    /// </summary>
    public partial class EditSaldo : Window
    {

        public string query;
        public EditSaldo()
        {
            InitializeComponent();

            LoadSaldo();

        }

        public void LoadSaldo()
        {
            double aktSaldo;
            

            using (SQLiteConnection connection = new SQLiteConnection(App.databasePath))
            {
                connection.CreateTable<Saldo>();

                query = "SELECT saldo FROM Saldo ORDER BY ID DESC LIMIT 1";
                
                if (connection.FindWithQuery<Saldo>(query, "?")?.saldo != null)
                {
                    aktSaldo = connection.FindWithQuery<Saldo>(query, "?").saldo;
                }
                else
                {
                    aktSaldo = 0;
                }
            }

            tb_aktSaldo.Text = String.Format("{0:0.00}", aktSaldo);

        }

        private void but_Save_Click(object sender, RoutedEventArgs e)
        {
            double uptSaldo;

            uptSaldo = Convert.ToDouble(tb_uptSaldo.Text);

            using (SQLiteConnection connection = new SQLiteConnection(App.databasePath))
            {
                connection.CreateTable<Saldo>();
                Saldo c_saldo = new Saldo
                {
                    TimeStmp = DateTime.Now.ToString(),
                    saldo = uptSaldo,
                };
                connection.Insert(c_saldo);
            }

            MessageBox.Show("Saldo gesichert!");

        }
    }
}
