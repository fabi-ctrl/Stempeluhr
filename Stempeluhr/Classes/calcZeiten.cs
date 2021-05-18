using SQLite;
using System;
using System.Configuration;
using System.Globalization;
using System.Windows;

namespace Stempeluhr
{
    class calcZeiten
    {
        public static void Calculate(DateTime Datum, string Kommen, string Gehen, string PauseStart, string PauseEnde, double PauseDiff)
        {   
            string query, today;
            double pauseSOLL, saldo, ZeitSOLL, tmpSaldo, Pause;
            CultureInfo cultureInfo;
            DateTime kommen, pauseStart, pauseEnde, gehen;
            bool PauseTimeSpan = false;
            
            bool update = false, insert = false;
            
            string tag = Datum.Date.ToString("dddd");
            string datum = Datum.Date.ToString("yyyy-MM-dd");
            
            today = DateTime.Now.ToString("dddd");
            cultureInfo = new CultureInfo("de-DE");
            kommen = new DateTime();
            pauseStart = new DateTime();
            pauseEnde = new DateTime();
            gehen = new DateTime();
            

            if (PauseStart != "" && PauseEnde != "")
            {
                pauseStart = DateTime.Parse(PauseStart, cultureInfo);
                pauseEnde = DateTime.Parse(PauseEnde, cultureInfo);
                PauseTimeSpan = true;
            }
            else
            {
                Configuration _config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                if (_config.AppSettings.Settings["TypeOfBreak"].Value == "1")
                {
                    using (SQLiteConnection conn = new SQLiteConnection(App.databasePath))
                    {
                        query = "SELECT ID FROM Pausen WHERE Datum = '" + datum + "' ORDER BY ID ASC LIMIT 1";
                        int id = conn.FindWithQuery<Pausen>(query, "?").ID;
                        query = "SELECT ID FROM Pausen WHERE Datum = '" + datum + "' ORDER BY ID DESC LIMIT 1";
                        int id_max = conn.FindWithQuery<Pausen>(query, "?").ID;

                        while (id <= id_max)
                        {
                            query = "SELECT Pausendauer FROM Pausen WHERE ID = '" + id + "'";
                            PauseDiff += conn.FindWithQuery<Pausen>(query, "?").Pausendauer;
                            id++;
                        }
                    }
                }
            }

            kommen = DateTime.Parse(Kommen, cultureInfo);
            gehen = DateTime.Parse(Gehen, cultureInfo);

            double kommenHours = gehen.Hour - kommen.Hour;
            double kommenMinutes = gehen.Minute - kommen.Minute;

            using (SQLiteConnection conn = new SQLiteConnection(App.databasePath))
            {
                conn.CreateTable<Arbeitstage>();
                //prüfen ob der heutige Tag ein Arbeitstag ist
                query = "SELECT Checked FROM Arbeitstage WHERE Arbeitstag = '" + tag + "'";
                if (conn.FindWithQuery<Arbeitstage>(query, "?")?.Checked == "x")
                {
                    //wenn JA, dann die Max Pause aus DB lesen
                    query = "SELECT * FROM Arbeitstage WHERE Arbeitstag = '" + tag + "'";
                    pauseSOLL = conn.FindWithQuery<Arbeitstage>(query, "?").Pause;
                    ZeitSOLL = conn.FindWithQuery<Arbeitstage>(query, "?").Stunden;
                }
                else //wenn NEIN, dann ZeitSOLL und PauseSOLL auf Null setzen
                {
                    pauseSOLL = 0;
                    ZeitSOLL = 0;
                }
            }

            //beim Aufruf aus MaindWindow Gehen, immer false
            if (PauseTimeSpan)
            {
                double pauseHours = pauseEnde.Hour - pauseStart.Hour;
                double pauseMinutes = pauseEnde.Minute - pauseStart.Minute;
                Pause = (pauseHours * 60 + pauseMinutes) / 60;

                if (Pause < pauseSOLL)
                {
                    Pause = pauseSOLL;
                }
            }
            else
            {
                if (PauseDiff != 0)
                {
                    if (PauseDiff < pauseSOLL)
                    {
                        Pause = pauseSOLL;
                    }
                    else
                    {
                        Pause = PauseDiff;
                    }
                }
                else
                {
                    Pause = pauseSOLL;
                }
            }

            double bewZeit = (kommenHours * 60 + kommenMinutes) / 60;

            bewZeit -= Pause;

            //PauseDiff = String.Format("{0:0.00}", Pause);

            saldo = bewZeit - ZeitSOLL;

            

            using (SQLiteConnection conn = new SQLiteConnection(App.databasePath))
            {
                conn.CreateTables<Zeiten, Saldo, Arbeitstage>();
                query = "Select * From Zeiten where Datum = '" + datum + "'";

                if (conn.FindWithQuery<Zeiten>(query, "?")?.Datum != null)
                {
                    update = true;
                }
                else
                {
                    insert = true;
                }

                Zeiten zeiten = new Zeiten()
                {
                    Datum = datum,
                    Kommen = Kommen,
                    PauseStart = PauseStart,
                    PauseEnde = PauseEnde,
                    Gehen = Gehen,
                    ZeitSOLL = ZeitSOLL,
                    BewZeit = Math.Round(bewZeit, 2),
                    DiffPause = Pause,
                    Saldo = Math.Round(saldo, 2),
                };

                if (conn.FindWithQuery<Saldo>("SELECT saldo FROM Saldo ORDER BY ID DESC LIMIT 1", "?")?.saldo != null)
                {
                    tmpSaldo = conn.FindWithQuery<Saldo>("SELECT saldo FROM Saldo ORDER BY ID DESC LIMIT 1", "?").saldo;
                }
                else
                {
                    tmpSaldo = 0;
                }

                //Tagessaldo zu Gesamtsaldo addieren und in DB sichern
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
                }
                else if (update == false && insert == true)
                {
                    conn.Insert(zeiten);
                }
            }
        }            
    }
}
