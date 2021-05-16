using Microsoft.Win32;
using System.Configuration;
using System.IO;
using System.Windows;

namespace Stempeluhr
{
    /// <summary>
    /// Interaction logic for LoadDB.xaml
    /// </summary>
    public partial class LoadDB : Window
    {
        public LoadDB()
        {
            InitializeComponent();
        }

        private void but_createDB_Click(object sender, RoutedEventArgs e)
        {
            
            SaveFileDialog sfd = new SaveFileDialog()
            {
                Title = "Erstelle deine Zeiten Datenbank",
                Filter = "SQLite Database (.db)|*.db",
            };

            if (sfd.ShowDialog() == true)
            {
                StreamWriter sw = new StreamWriter(File.Create(sfd.FileName));
                sw.Dispose();
            }

            //tbFileName.Text = "Dein DB File: " + System.IO.Path.GetFileName(sfd.FileName);

            App.databasePath = sfd.FileName;

            Configuration _config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            _config.AppSettings.Settings["DBPath"].Value = sfd.FileName;
            _config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");

            MessageBox.Show("Deine Zeiten DB liegt hier: " + ConfigurationManager.AppSettings.Get("DBPath"));
            this.Close();
                
        }

        private void but_openDB_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog()
            {
                Title = "Öffne deine Zeiten Datenbank",
                Filter = "SQLite Database (.db)|*.db",
            };

            if(ofd.ShowDialog() == true)
            {
                App.databasePath = ofd.FileName;
            }

            Configuration _config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            _config.AppSettings.Settings["DBPath"].Value = ofd.FileName;
            _config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");

            MessageBox.Show("Deine Zeiten DB wurde geladen");
            this.Close();
        }
    }
}
