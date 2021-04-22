using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace Stempeluhr
{ 
    class Fehltage
    {
        
        [PrimaryKey, NotNull, Unique, AutoIncrement]

        public int ID { get; set; }

        public string Von { get; set; }

        public string Bis { get; set; }

        public string Type { get; set; }

        public string Done { get; set; }

    }
}
