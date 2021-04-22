using SQLite;

namespace Stempeluhr
{
    public class Zeiten
    { 
            [PrimaryKey, NotNull, Unique]
            public string Datum { get; set; }
            public string Kommen { get; set; }
            public string PauseStart { get; set; }
            public string PauseEnde { get; set; }
            public string Gehen { get; set; }
            public double ZeitSOLL { get; set; }
            public double DiffPause { get; set; }
            public double BewZeit { get; set; }
            public double Saldo { get; set; }
    }
}
