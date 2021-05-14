using System;
using SQLite;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Globalization;
using Stempeluhr;

namespace Stempeluhr
{
    /// <summary>
    /// Interaction logic for ZeitenUpt.xaml
    /// </summary>
    public partial class ZeitenUpt : Window
    {
        private string query, today;
        private double pause, saldo, stunden;
        CultureInfo cultureInfo;
        DateTime kommen, pauseStart, pauseEnde, gehen;

       

        public ZeitenUpt()
        {
            today = DateTime.Now.ToString("dddd");
            cultureInfo = new CultureInfo("de-DE");
            kommen = new DateTime();
            pauseStart = new DateTime();
            pauseEnde = new DateTime();
            gehen = new DateTime();

            InitializeComponent();
            dp_Datum.SelectedDate = DateTime.Now;
            LoadTimes();
        }

        private void butSave_Click(object sender, RoutedEventArgs e)
        {
            bool update = false, insert = false;
            string datum = dp_Datum.SelectedDate.Value.Date.ToString("yyyy-MM-dd");
            double tmpSaldo;

            using (SQLiteConnection conn = new SQLiteConnection(App.databasePath))
            {
                conn.CreateTables<Zeiten, Saldo>();
                query = "Select * From Zeiten where Datum = '" + datum + "'";

                if(conn.FindWithQuery<Zeiten>(query, "?")?.Datum != null)
                {
                    MessageBoxResult result = MessageBox.Show(
                        "Es besteht bereits ein Eintrag für dieses Datum. Zeit überschreiben?", "Eintrag bereits vorhanden!", MessageBoxButton.YesNoCancel
                        );

                    if (result == MessageBoxResult.Yes)
                    {
                        update = true;
                    }
                    else
                    {   
                        return;
                    }
                }
                else
                {
                    insert = true;
                }

                Zeiten zeiten = new Zeiten()
                {
                    Datum = datum,
                    Kommen = tb_Kommen.Text,
                    PauseStart = tb_PauseStart.Text,
                    PauseEnde = tb_PauseEnde.Text,
                    Gehen = tb_Gehen.Text,
                    ZeitSOLL = Convert.ToDouble(tb_ZeitSOLL.Text),
                    BewZeit = Convert.ToDouble(tb_BewZeit.Text),
                    DiffPause = Convert.ToDouble(tb_PauseDiff.Text),
                    Saldo = Convert.ToDouble(tb_Saldo.Text),
                };


                if (conn.FindWithQuery<Saldo>("SELECT saldo FROM Saldo ORDER BY ID DESC LIMIT 1", "?")?.saldo != null)
                {
                    tmpSaldo = conn.FindWithQuery<Saldo>("SELECT saldo FROM Saldo ORDER BY ID DESC LIMIT 1", "?").saldo;
                }
                else
                {
                    tmpSaldo = 0;
                }

                Saldo c_saldo = new Saldo
                {
                    TimeStmp = DateTime.Now.ToString(),
                    saldo = tmpSaldo + saldo,
                };

                conn.Insert(c_saldo);

                //DB updaten oder neuen Zeiteintrag einfügen
                if (update == true && insert == false)
                {
                    conn.Update(zeiten);

                    //query = "UPDATE Zeiten SET Kommen = '" + tb_Kommen.Text + "' Where Datum = '" + datum + "'";
                }
                else if (update == false && insert == true)
                {
                    conn.Insert(zeiten);
                }
            }

            MessageBox.Show("Daten wurden gesichert!", "Daten gesichert!", MessageBoxButton.OK);
            LoadTimes();
            ResetValues();
        }

        private void dp_Datum_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadTimes();
        }

        public void LoadTimes ()
        {
            today = dp_Datum.SelectedDate.Value.Date.ToString("dddd");
            using (SQLiteConnection conn = new SQLiteConnection(App.databasePath))
            {
                conn.CreateTables<Zeiten, Arbeitstage, Saldo>();

                query = "Select * from Arbeitstage where Arbeitstag = '" + today + "'";

                if (conn.FindWithQuery<Arbeitstage>(query, "?")?.Checked != null)
                {
                    query = "Select Pause from Arbeitstage where Arbeitstag = '" + today + "'";
                    pause = conn.FindWithQuery<Arbeitstage>(query, "?").Pause;
                    query = "Select Stunden from Arbeitstage where Arbeitstag = '" + today + "'";
                    stunden = conn.FindWithQuery<Arbeitstage>(query, "?").Stunden;
                }
                else
                {
                    pause = 0;
                    stunden = 0;
                }

                tb_PauseSOLL.Text = String.Format("{0:0.00}", pause);
                tb_ZeitSOLL.Text = String.Format("{0:0.00}", stunden);

                tb_GesamtSaldo.Text = String.Format("{0:0.00}", conn.FindWithQuery<Saldo>("SELECT saldo FROM Saldo ORDER BY ID DESC LIMIT 1", "?")?.saldo);

            }

        }

        private void butReset_Click(object sender, RoutedEventArgs e)
        {
            ResetValues();
        }

        private void ResetValues()
        {
            tb_Kommen.Text = "";
            tb_PauseStart.Text = "";
            tb_PauseEnde.Text = "";
            tb_Gehen.Text = "";
            tb_PauseDiff.Text = "";
            tb_BewZeit.Text = "";
            tb_Saldo.Text = "";
        }
        private void tb_Gehen_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                Calculate();
            }
            butSave.IsEnabled = true;
        }

        public void Calculate()
        {
            double ZeitSOLL, Pause;
            bool noPauseTimeSpan = false;

            tb_BewZeit.Text = "";
            tb_Saldo.Text = "";

            if (tb_PauseStart.Text == "" || tb_PauseEnde.Text == "")
            {
                tb_PauseStart.Text = "00:00";
                tb_PauseEnde.Text = "00:00";
                noPauseTimeSpan = true;
            }

            today = dp_Datum.SelectedDate.Value.Date.ToString("dddd");

            kommen = DateTime.Parse(tb_Kommen.Text, cultureInfo);
            gehen = DateTime.Parse(tb_Gehen.Text, cultureInfo);
            pauseStart = DateTime.Parse(tb_PauseStart.Text, cultureInfo);
            pauseEnde = DateTime.Parse(tb_PauseEnde.Text, cultureInfo);
            
            double kommenHours = gehen.Hour - kommen.Hour;
            double kommenMinutes = gehen.Minute - kommen.Minute;
            
            if (noPauseTimeSpan == false)
            {
                double pauseHours = pauseEnde.Hour - pauseStart.Hour;
                double pauseMinutes = pauseEnde.Minute - pauseStart.Minute;
                Pause = (pauseHours * 60 + pauseMinutes) / 60;

                if (Pause < pause)
                {
                    Pause = pause;
                }
            }
            else
            {
                if (tb_PauseDiff.Text != "")
                {
                    if (Convert.ToDouble(tb_PauseDiff.Text) < pause)
                    {
                        Pause = pause;
                    }
                    else
                    {
                        Pause = Convert.ToDouble(tb_PauseDiff.Text);
                    }
                }
                else
                {
                    Pause = pause;
                }
            }

            double bewZeit = (kommenHours * 60 + kommenMinutes) / 60;

            bewZeit -= Pause;
            
            tb_BewZeit.Text = String.Format("{0:0.00}", bewZeit);
            tb_PauseDiff.Text = String.Format("{0:0.00}", Pause);

            today = dp_Datum.SelectedDate.Value.Date.ToString("dddd");

            using (SQLiteConnection conn = new SQLiteConnection(App.databasePath))
            {
                conn.CreateTable<Arbeitstage>();

                query = "Select * from Arbeitstage where Arbeitstag = '" + today + "'";

                if (conn.FindWithQuery<Arbeitstage>(query, "?")?.Checked != null)
                {
                    query = "Select Stunden From Arbeitstage Where Arbeitstag = '" + today + "'";
                    ZeitSOLL = conn.FindWithQuery<Arbeitstage>(query, "?").Stunden;
                }
                else
                {
                    ZeitSOLL = 0;
                }
            }


            saldo = bewZeit - ZeitSOLL;

            tb_Saldo.Text = String.Format("{0:0.00}", saldo);
        }

    }
}
