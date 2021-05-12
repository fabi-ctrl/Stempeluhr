using SQLite;
using System;
using System.Globalization;

namespace Stempeluhr
{
    public static class calcZeiten
    {
        /*
        public static void Calculate(string Kommen, string Gehen, string PauseStart, string PauseEnde, string PauseDiff, bool noPauseTimeSpan)
        {
            string query, today;
            double pauseSOLL, saldo, stunden, ZeitSOLL;
            CultureInfo cultureInfo;
            DateTime kommen, pauseStart, pauseEnde, gehen;
            double Pause;

            today = DateTime.Now.ToString("dddd");
            cultureInfo = new CultureInfo("de-DE");
            kommen = new DateTime();
            pauseStart = new DateTime();
            pauseEnde = new DateTime();
            gehen = new DateTime();
            

            pauseStart = DateTime.Parse(PauseStart, cultureInfo);
            pauseEnde = DateTime.Parse(PauseEnde, cultureInfo);

            double kommenHours = gehen.Hour - kommen.Hour;
            double kommenMinutes = gehen.Minute - kommen.Minute;

            if (noPauseTimeSpan == false)
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
                if (PauseDiff != "")
                {
                    if (Convert.ToDouble(PauseDiff) < pauseSOLL)
                    {
                        Pause = pauseSOLL;
                    }
                    else
                    {
                        Pause = Convert.ToDouble(PauseDiff);
                    }
                }
                else
                {
                    Pause = pauseSOLL;
                }
            }

            double bewZeit = (kommenHours * 60 + kommenMinutes) / 60;

            bewZeit -= Pause;

            tb_BewZeit.Text = String.Format("{0:0.00}", bewZeit);
            PauseDiff = String.Format("{0:0.00}", Pause);

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
        }*/
            
    }

}
