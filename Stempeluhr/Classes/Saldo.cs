using SQLite;

namespace Stempeluhr
{
    public class Saldo
    {
        [PrimaryKey, NotNull, Unique, AutoIncrement]

        public int ID { get; set; }
        public string TimeStmp { get; set; }
        public double saldo { get; set; }

    }
}
