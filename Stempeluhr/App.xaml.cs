using System.Windows;

namespace Stempeluhr
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /*
        static string databaseFolder = "./";
        static string databaseName = "ZeitenDB.db";
        public static string databasePath = System.IO.Path.Combine(databaseFolder, databaseName);
        */

        public static string databasePath;

        static string ConfigDBFolder = "./";
        static string ConfigDBName = "ConfigDB.db";
        public static string configDBPath = System.IO.Path.Combine(ConfigDBFolder, ConfigDBName);

    }
}
