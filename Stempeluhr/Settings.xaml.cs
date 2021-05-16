using System.Configuration;
using System.Windows;

namespace Stempeluhr
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public Settings()
        {
            InitializeComponent();
            Load();
        }

        private void cb_Pausen_Checked(object sender, RoutedEventArgs e)
        {
            //Wenn Pausenzeiten addiert werden sollen, dann im Config File auf 1 setzen
            Configuration _config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            _config.AppSettings.Settings["TypeOfBreak"].Value = "1";
            _config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }

        private void cb_Pausen_Unchecked(object sender, RoutedEventArgs e)
        {
            //Wenn Pausenzeiten NICHT addiert werden sollen, dann im Config File auf 0 (default) setzen
            Configuration _config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            _config.AppSettings.Settings["TypeOfBreak"].Value = "0";
            _config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }

        private void Load()
        {   
            Configuration _config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            if ( _config.AppSettings.Settings["TypeOfBreak"].Value == "1")
            {
                cb_Pausen.IsChecked = true;
            }

            tb_Urlaub.Text = _config.AppSettings.Settings["VacationDays"].Value.ToString();
        }

        private void but_Save_Click(object sender, RoutedEventArgs e)
        {
            Configuration _config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            _config.AppSettings.Settings["VacationDays"].Value = tb_Urlaub.Text;
            _config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
            MessageBox.Show("Urlaubstage wurden gesichert.");
        }
    }
}
