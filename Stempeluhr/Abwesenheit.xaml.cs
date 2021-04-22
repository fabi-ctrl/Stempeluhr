using System;
using System.Collections.Generic;
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
using SQLite;

namespace Stempeluhr
{
    /// <summary>
    /// Interaction logic for Fehltage.xaml
    /// </summary>
    public partial class Abwesenheit : Window
    {
        List<string> typen = new List<string> { "Urlaub", "Gleittag", "Krankheit" };
        List<Fehltage> fehltage = new List<Fehltage>();
        string strDateVon, strDateBis, strtype;


        public Abwesenheit()
        {
            InitializeComponent();

            cb_Type.ItemsSource = typen;

            LoadToDataGrid();
        }

        private void dg_Fehltage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            but_EditRow.IsEnabled = true;
            but_DeleteRow.IsEnabled = true;

            DataGrid gd = (DataGrid)sender;
            DataRowView row_selected = gd.SelectedItem as DataRowView;
            if (row_selected != null)
            {
                //tbd
            }
        }

        private void dg_Fehltage_LostFocus(object sender, RoutedEventArgs e)
        {
            but_EditRow.IsEnabled = false;
            but_DeleteRow.IsEnabled = false;
        }

        private void but_Save_Click(object sender, RoutedEventArgs e)
        {
            if (dp_Bis.SelectedDate == null)
            {
                if (dp_Von.SelectedDate != null)
                {
                    strDateVon = dp_Von.SelectedDate.Value.Date.ToString("yyyy-MM-dd");
                    strDateBis = strDateVon;
                }

            }
            else if (dp_Von.SelectedDate == null)
            {
                strDateBis = dp_Bis.SelectedDate.Value.Date.ToString("yyyy-MM-dd");
                strDateVon = strDateBis;
            }

            if (dp_Von.SelectedDate != null & dp_Bis.SelectedDate != null)
            {
                strDateVon = dp_Von.SelectedDate.Value.Date.ToString("yyyy-MM-dd");
                strDateBis = dp_Bis.SelectedDate.Value.Date.ToString("yyyy-MM-dd");
            }

            if (cb_Type != null)
            {
                strtype = cb_Type.SelectedItem.ToString();
            }
            else
            {
                MessageBox.Show("Bitte eine Art des Fehltags auswählen", "Art nicht gewählt", MessageBoxButton.OK);
            }

            using (SQLiteConnection conn = new SQLiteConnection(App.databasePath))
            {
                conn.CreateTable<Fehltage>();

                Fehltage fehltage = new Fehltage()
                {
                    Von = strDateVon,
                    Bis = strDateBis,
                    Type = strtype,
                };

                conn.Insert(fehltage);
            }
            
            LoadToDataGrid();
            MessageBox.Show(strtype + " eingetragen", "Fehltage eingetragen", MessageBoxButton.OK);
            
        }

        void LoadToDataGrid()
        {  
            string sqliteQuery = "select * from Fehltage";
            using (SQLiteConnection conn = new SQLiteConnection(App.databasePath))
            {
                
                conn.CreateTable<Fehltage>();
                fehltage = conn.Query<Fehltage>(sqliteQuery, "?").ToList();

                if (fehltage != null)
                {
                    dg_Fehltage.ItemsSource = fehltage;
                }
            }
        }
    }
}
