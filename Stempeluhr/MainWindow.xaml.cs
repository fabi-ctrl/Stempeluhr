using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Diagnostics;
using System.Timers;
using System.Configuration;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Squirrel;

namespace Stempeluhr
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        List<Zeiten> zeiten;
        string query, strDateVon, strDateBis, _filename, DEBUG;
        static string today = DateTime.Now.ToString("yyyy-MM-dd");
        static string tag = DateTime.Now.ToString("dddd");
        double saldo;
        private Stopwatch stopwatch;
        private Timer timer;
        const string startTimeDisplay = "00:00:00";
        UpdateManager manager;

        public MainWindow()
        {
            try
            {
                DataContext = this;
                InitializeComponent();
                
                Loaded += MainWindow_Loaded;

                tbTimer.Text = startTimeDisplay;

                stopwatch = new Stopwatch();
                timer = new Timer(interval: 1000);

                timer.Elapsed += OnTimerElapse;

                butPauseStart.IsEnabled = false;
                butPauseEnde.IsEnabled = false;
                butGehen.IsEnabled = false;

                zeiten = new List<Zeiten>();

                tbHeute.Text = DateTime.Now.ToString("D");

                LoadConfig();

                if (tbFileName.Text == "")
                {
                    tbFileName.Text = "Dein DB File: " + System.IO.Path.GetFileName(ConfigurationManager.AppSettings.Get("DBPath"));
                }

                ReadDatabase();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private string _saldo;
        public string Saldo
        {
            get { return _saldo; }
            set
            {
                if (_saldo != value)
                {
                    _saldo = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                manager = await UpdateManager.GitHubUpdateManager(@"https://github.com/fabi-ctrl/Stempeluhr");

                this.Title = "Stempeluhr - " + DateTime.Now.ToString("D") + " (v" + manager.CurrentlyInstalledVersion().ToString() + " - BETA)";
            }
            catch(Exception ex)
            {
                MessageBox.Show("Ups... etwas ist schief gelaufen. " + ex.Message.ToString());
                this.Title = "Stempeluhr - " + DateTime.Now.ToString("D");
            }
        }

        private void LoadConfig()
        {
            try
            {
                if (ConfigurationManager.AppSettings.Get("DBPath") == "0")
                {
                    LoadDB loadDB = new LoadDB();
                    loadDB.ShowDialog();
                }
                else
                {
                    App.databasePath = ConfigurationManager.AppSettings.Get("DBPath");
                }

                ReturnSaldo();

            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), "Ups... etwas ist schief gelaufen", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ReturnSaldo()
        {
            using (SQLiteConnection conn = new SQLiteConnection(App.databasePath))
            {
                saldo = conn.FindWithQuery<Saldo>("SELECT saldo FROM Saldo ORDER BY ID DESC LIMIT 1", "?").saldo;
                Saldo = String.Format("{0:0.00}", saldo);
            }
        }

        
        private async void OnTimerElapse(object sender, ElapsedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() => tbTimer.Text = stopwatch.Elapsed.ToString(format:@"hh\:mm\:ss"));
        }

        /// All Buttons
#region
        /// <buttons>
        /// All buttons
        /// </buttons>
        private void butKommen_Click(object sender, RoutedEventArgs e)
        {
            //Kann vllt noch vereinfacht werden durch Prozeduren
            //initial alles unten muss nur ausfeührt werden, wenn das erstmal auf Kommen geklickt wird
            
            string Jetzt = DateTime.Now.ToString("HH:mm"), Gehen;
            double ZeitSOLL, PauseSOLL;

            if (stopwatch.IsRunning)
            {
                stopwatch.Stop();
                timer.Stop();
                stopwatch.Reset();
                tbTimer.Text = startTimeDisplay;
            }

            stopwatch.Start();
            timer.Start();

            this.tbKommen.Text = Jetzt;

            //SOLL Stunden aus Arbeitstage Tabelle lesen
            using (SQLiteConnection connection = new SQLiteConnection(App.databasePath))
            {
                connection.CreateTable<Arbeitstage>();

                query = "SELECT Arbeitstag FROM Arbeitstage WHERE Arbeitstag = '" + tag + "'";
                if (connection.FindWithQuery<Arbeitstage>(query, "?")?.Arbeitstag != null)
                {
                    query = "SELECT Stunden FROM Arbeitstage WHERE Arbeitstag = '" + tag + "'";
                    ZeitSOLL = connection.FindWithQuery<Arbeitstage>(query, "?").Stunden;
                    query = "SELECT Pause FROM Arbeitstage WHERE Arbeitstag = '" + tag + "'";
                    PauseSOLL = connection.FindWithQuery<Arbeitstage>(query, "?").Pause;
                }
                else
                {
                    ZeitSOLL = 0;
                    PauseSOLL = 0;
                }
            }

            //Uhrzeit für Mindesarbeitszeit anzeigen
            ZeitSOLL += PauseSOLL;
            DateTime HoursGehen = DateTime.Now.AddHours(ZeitSOLL);
            if (ZeitSOLL > 0)
            { 
                Gehen = HoursGehen.ToString("HH:mm");
            }
            else
            {
                Gehen = "Keine Stunden gefordert";
            }
            this.tbGehenPlan.Text = Gehen;

            Zeiten zeiten = new Zeiten()
            {
                Datum = today,
                Kommen = Jetzt,
                ZeitSOLL = ZeitSOLL,
            };
            
            using (SQLiteConnection connection = new SQLiteConnection(App.databasePath))
            {
                query = "select Datum from Zeiten where Datum = '" + zeiten.Datum + "'";
                
                connection.CreateTable<Zeiten>();
                    
                if (connection.FindWithQuery<Zeiten>(query, "?")?.Datum != null)
                {
                    UpdateTable("Kommen");
                }
                else
                {
                    connection.Insert(zeiten);
                }
            }
            
            butPauseStart.IsEnabled = true;
            butGehen.IsEnabled = true;

            ReadDatabase();
        }

        private void butZeitenAnzeigen_Click(object sender, RoutedEventArgs e)
        {
            query = "select * from Zeiten where strftime('%Y-%m-%d', Datum) BETWEEN ";

            if (dpVon.SelectedDate == null & dpBis.SelectedDate == null)
            {
                ReadDatabase();
                MessageBox.Show("DB gelesen.");
                //return;
            }
            else
            {
                if (dpBis.SelectedDate == null)
                {
                    if (dpVon.SelectedDate != null)
                    {
                        strDateVon = dpVon.SelectedDate.Value.Date.ToString("yyyy-MM-dd");
                        strDateBis = strDateVon;
                    }

                }
                else if (dpVon.SelectedDate == null)
                {
                    strDateBis = dpBis.SelectedDate.Value.Date.ToString("yyyy-MM-dd");
                    strDateVon = strDateBis;
                }

                if (dpVon.SelectedDate != null & dpBis.SelectedDate != null)
                {
                    strDateVon = dpVon.SelectedDate.Value.Date.ToString("yyyy-MM-dd");
                    strDateBis = dpBis.SelectedDate.Value.Date.ToString("yyyy-MM-dd");
                }

                query = query + "'" + strDateVon + "' AND '" + strDateBis + "' ORDER BY Datum DESC";
                LoadToDataGrid(query);
            } 
        }

        private void butPauseStart_Click(object sender, RoutedEventArgs e)
        {
            UpdateTable("PauseStart");

            butKommen.IsEnabled = false;
            butPauseEnde.IsEnabled = true;   
        }

        private void butPauseEnde_Click(object sender, RoutedEventArgs e)
        {
            UpdateTable("PauseEnde");
            butPauseEnde.IsEnabled = false;
        }

        private void butGehen_Click(object sender, RoutedEventArgs e)
        {
            stopwatch.Stop();
            timer.Stop();
            stopwatch.Reset();

            UpdateTable("Gehen");
            ZeitBerechnen();

            //tbTimer.Text = startTimeDisplay;

            butPauseStart.IsEnabled = false;
            butPauseEnde.IsEnabled = false;
            butKommen.IsEnabled = true;
        }
        
        private void butArbeitstage_Click(object sender, RoutedEventArgs e)
        {
            Arbeitszeiten winArbeitszeiten = new Arbeitszeiten();
            winArbeitszeiten.ShowDialog();
        }

        private void but_Saldo_Click(object sender, RoutedEventArgs e)
        {
            EditSaldo winEditSaldo = new EditSaldo();
            winEditSaldo.ShowDialog();
            ReturnSaldo();
        }

        private void but_ZeitenUpt_Click(object sender, RoutedEventArgs e)
        {
            ZeitenUpt winZeitenUpt = new ZeitenUpt();
            winZeitenUpt.ShowDialog();
            ReturnSaldo();
            ReadDatabase();
        }

        private void butZeitenReset_Click(object sender, RoutedEventArgs e)
        {
            dpVon.SelectedDate = null;
            dpBis.SelectedDate = null;

            ReadDatabase();
        }

#endregion


        /// All Methods
#region
        /// <methods>
        /// All additional methods
        /// </methods>
        void UpdateTable(string sqliteColumn)
        {
            string now = DateTime.Now.ToString("HH:mm");
            string sqliteQuery = "UPDATE Zeiten SET " + sqliteColumn + " = '" + now + "' WHERE Datum = '" + today + "'";

            using (SQLiteConnection connection = new SQLiteConnection(App.databasePath))
            {
                query = "SELECT Datum FROM Zeiten WHERE Datum = '" + today + "'";

                connection.CreateTable<Zeiten>();

                if (connection.FindWithQuery<Zeiten>(query, "?")?.Datum != null)
                {

                    connection.Query<Zeiten>(sqliteQuery, "?");
                }

            }

            ReadDatabase();

        }

        void ReadDatabase()
        {
            LoadToDataGrid("select * from Zeiten ORDER BY Datum DESC");
        }

        void LoadToDataGrid(string sqliteQuery)
        {
            using (SQLiteConnection connection = new SQLiteConnection(App.databasePath))
            {

                connection.CreateTable<Zeiten>();
                zeiten = connection.Query<Zeiten>(sqliteQuery, "?").ToList();

                if (zeiten != null)
                {
                    dgZeiten.ItemsSource = zeiten;
                }
            }

        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void m_Updates_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var updateInfo = await manager.CheckForUpdate();
                
                if (updateInfo.ReleasesToApply.Count > 0)
                {
                    await manager.UpdateApp();

                    MessageBox.Show("Updated succesfuly!");
                }
                else
                {
                    MessageBox.Show("Du hast bereits die aktuelle Version installiert.");
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Ups... es ist ein Fehler aufgetreten. " + ex.Message.ToString());
            }
        }

        private async void m_Info_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBox.Show("Aktuelle Version: " + manager.CurrentlyInstalledVersion().ToString());
            }
            catch(Exception ex)
            {
                MessageBox.Show("Ups... es ist ein Fehler aufgetreten. " + ex.Message.ToString());
            }
            
        }

        private void tbFileName_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            string path = System.IO.Path.GetDirectoryName(App.databasePath);
            Process.Start("explorer", path);
        }

        private void m_LoadDB_Click(object sender, RoutedEventArgs e)
        {
            LoadDB loadDB = new LoadDB();
            loadDB.ShowDialog();
            tbFileName.Text = "Dein DB File: " + System.IO.Path.GetFileName(ConfigurationManager.AppSettings.Get("DBPath"));
        }

        private void m_Fehltage_Click(object sender, RoutedEventArgs e)
        {
            Abwesenheit winFehltage = new Abwesenheit();
            winFehltage.ShowDialog();
        }


        private void ZeitBerechnen()
        {
            //try
            //{
                //string today = DateTime.Now.ToString("yyyy-MM-dd");
                string Kommen, Gehen, PauseStart, PauseEnde;
                DateTime Datum = DateTime.Now;
                double bewzeit;

                using (SQLiteConnection conn = new SQLiteConnection(App.databasePath))
                {
                    conn.CreateTable<Zeiten>();
                    query = "SELECT * FROM Zeiten WHERE Datum = '" + today + "'";
                    Kommen = conn.FindWithQuery<Zeiten>(query, "?").Kommen;
                    Gehen = conn.FindWithQuery<Zeiten>(query, "?").Gehen;

                    if (conn.FindWithQuery<Zeiten>(query, "?")?.PauseStart == null && conn.FindWithQuery<Zeiten>(query, "?")?.PauseEnde == null)
                    {
                        PauseStart = "";
                        PauseEnde = "";
                    }
                    else
                    {
                        PauseStart = conn.FindWithQuery<Zeiten>(query, "?")?.PauseStart;
                        PauseEnde = conn.FindWithQuery<Zeiten>(query, "?")?.PauseEnde;
                    }
                }

                Stempeluhr.calcZeiten.Calculate(Datum, Kommen, Gehen, PauseStart, PauseEnde, "", false);

                ReadDatabase();

                using (SQLiteConnection conn = new SQLiteConnection(App.databasePath))
                {
                    conn.CreateTables<Zeiten, Saldo>();
                    query = "SELECT * FROM Zeiten WHERE Datum = '" + today + "'";
                    bewzeit = conn.FindWithQuery<Zeiten>(query, "?").BewZeit;
                    saldo = conn.FindWithQuery<Zeiten>(query, "?").Saldo;
                    saldo = conn.FindWithQuery<Saldo>("SELECT saldo FROM Saldo ORDER BY ID DESC LIMIT 1", "?").saldo;
                }

                tbTimer.Text = "Bew. Zeit: " + String.Format("{0:0.00}", bewzeit) + " | Saldo heute: '" + String.Format("{0:0.00}", saldo) + "'";
                Saldo = String.Format("{0:0.00}", saldo);
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show("Ups... etwas ist schief gelaufen: '" + ex.Message.ToString() + "'", "Fehler beim Berechnen");
            //}
        }

        /*private void ZeitBerechnen()
        {
            double DiffPause, maxPause, tmpZeit, bewzeit, tmpSaldo;
            string _DiffPause, _tmpZeit;
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(App.databasePath))
                {
                    connection.CreateTable<Zeiten>();
                    connection.CreateTable<Arbeitstage>();

                    //prüfen ob der heutige Tag ein Arbeitstag ist
                    query = "SELECT Checked FROM Arbeitstage WHERE Arbeitstag = '" + tag + "'";
                    if (connection.FindWithQuery<Arbeitstage>(query, "?")?.Checked == "x")
                    {
                        //wenn JA, dann die Max Pause aus DB lesen
                        query = "SELECT Pause FROM Arbeitstage WHERE Arbeitstag = '" + tag + "'";
                        maxPause = connection.FindWithQuery<Arbeitstage>(query, "?").Pause;
                    }
                    else //wenn NEIN, dann Stunden und Max Pause auf Null setzen
                    {
                        maxPause = 0;
                    }

                    //Berechnung der Pausen-Dauer
                    query = "UPDATE Zeiten SET DiffPause = round(((strftime('%s', PauseEnde) - strftime('%s', PauseStart))) / 3600.0, 2) WHERE Datum = '" + today + "'";
                    connection.Query<Zeiten>(query, "?");
                    query = "SELECT DiffPause FROM Zeiten where Datum = '" + today + "'";
                    DiffPause = connection.FindWithQuery<Zeiten>(query, "?").DiffPause;

                    //Prüfen ob die max. Pausen-Dauer für diesen Tag überschritten wurde
                    if (DiffPause > maxPause)
                    {
                        //wenn JA, dann wird die tatsächliche Pausen-Dauer von der Gesamtzeit abgezogen
                        _DiffPause = String.Format("{0:0.00}", DiffPause);
                        query = "UPDATE Zeiten SET BewZeit = round(((strftime('%s', Gehen) - strftime('%s', Kommen))) / 3600.0, 2) - " + _DiffPause + " WHERE Datum = '" + today + "'";
                    }
                    else
                    {
                        //wenn Nein, dann wird die "MUSS"-Pause von der Gesamtzeit abgezogen
                        //Dazu wird temprör die BewZeit in der DB zwischen Gehen und Kommen berechnet
                        query = "UPDATE Zeiten SET BewZeit = round(((strftime('%s', Gehen) - strftime('%s', Kommen))) / 3600.0, 2) WHERE Datum = '" + today + "'";
                        connection.Query<Zeiten>(query, "?");
                        query = "SELECT BewZeit FROM Zeiten WHERE Datum = '" + today + "'";
                        tmpZeit = connection.FindWithQuery<Zeiten>(query, "?").BewZeit;

                        //Von der gestempleten Zeit wird die "MUSS"-Pause abgezogen...
                        tmpZeit = tmpZeit - maxPause;
                        _tmpZeit = String.Format("{0:0.00}", tmpZeit);
                        //... und endgültig in die DB als bewertete Zeit geschrieben
                        query = "UPDATE Zeiten SET BewZeit = round(((strftime('%s', Gehen) - strftime('%s', Kommen))) / 3600.0, 2) - " + _tmpZeit + " WHERE Datum = '" + today + "'";
                    }

                    //Die Updates werden auf die DB geschrieben
                    connection.Query<Zeiten>(query, "?");

                    //Saldo berechnen
                    query = "UPDATE Zeiten SET Saldo = BewZeit - ZeitSOLL WHERE Datum = '" + today + "'";
                    connection.Query<Zeiten>(query, "?");

                    //Saldo addieren
                    query = "SELECT Saldo FROM Zeiten WHERE Datum = '" + today + "'";
                    saldo = connection.FindWithQuery<Zeiten>(query, "?").Saldo;

                    connection.CreateTable<Saldo>();

                    if (connection.FindWithQuery<Saldo>("SELECT saldo FROM Saldo ORDER BY ID DESC LIMIT 1", "?")?.saldo != null)
                    {
                        tmpSaldo = connection.FindWithQuery<Saldo>("SELECT saldo FROM Saldo ORDER BY ID DESC LIMIT 1", "?").saldo;
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

                    connection.Insert(c_saldo);

                    ReadDatabase();

                    //Die bewertete Zeit wird ausgegeben
                    query = "SELECT BewZeit FROM Zeiten WHERE Datum = '" + today + "'";
                    bewzeit = connection.FindWithQuery<Zeiten>(query, "?").BewZeit;
                    query = "SELECT Saldo from Zeiten WHERE Datum = '" + today + "'";
                    saldo = connection.FindWithQuery<Zeiten>(query, "?").Saldo;
                    tbTimer.Text = "Bew. Zeit: " + String.Format("{0:0.00}", bewzeit) + " | Saldo heute: '" + String.Format("{0:0.00}", saldo) + "'";
                    saldo = connection.FindWithQuery<Saldo>("SELECT saldo FROM Saldo ORDER BY ID DESC LIMIT 1", "?").saldo;
                    Saldo = String.Format("{0:0.00}", saldo);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Ups... es ist ein Fehler aufgetreten. " + ex.Message.ToString());
            }
        }*/

#endregion
    }
}
