using SQLite;

namespace Stempeluhr
{
    public class Arbeitstage
    {
        [PrimaryKey, NotNull, Unique]
        public string Arbeitstag { get; set; }
        public string Checked { get; set; }
        public double Stunden { get; set; }
        public double Pause { get; set; }
    }
}
