using SQLite;

namespace Stempeluhr
{
    class Pausen
    {
        [PrimaryKey, NotNull, Unique, AutoIncrement]
        public int ID { get; set; }
        public string Datum { get; set; }
        public string Start_Time { get; set; }
        public string Ende_Time { get; set; }
        public string Pausendauer { get; set; }
    }
}
