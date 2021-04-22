using SQLite;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Stempeluhr
{
    /// <summary>
    /// Interaction logic for Arbeitszeiten.xaml
    /// </summary>
    public partial class Arbeitszeiten : Window
    {
        bool butSaveClick = false;
        public Arbeitszeiten()
        {
            InitializeComponent();
            butSave.Content = "Bearbeiten";
            ArbeitstageSchutz();
            LoadArbeitstage();
        }

        //Läd alle Objecte von einem StackPanel, genutzt für CheckBoxen und TextBoxen
        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        //Schreiben der Arbeitstage in die ZeitenDB
        private void butSave_Click(object sender, RoutedEventArgs e)
        {
            string tag, query, sqliteQuery;
            double stunden, pause;

            if (butSaveClick)
            {
                NumberFormatInfo nfi = new CultureInfo("en-US", false).NumberFormat;

                stunden = 0;
                pause = 0;

                TextBox tbStunden = new TextBox();
                TextBox tbPause = new TextBox();

                //jede CheckBox durchgehen
                foreach (CheckBox cb in FindVisualChildren<CheckBox>(spTage))
                {
                    //Tag aus CheckBox Content auslesen
                    tag = cb.Content.ToString();

                    //Nur beachten wenn aktuelle CheckBox angehakt ist
                    if (cb.IsChecked == true)
                    {
                        //Auslesen welche Checkbox gerade erreicht ist

                        tbStunden.Name = "tbStunden_" + tag;
                        tbPause.Name = "tbPause_" + tag;

                        //Arbeitsstunden auslesne
                        foreach (TextBox tb in FindVisualChildren<TextBox>(spArbeitsstunden))
                        {
                            if (tb.Name == tbStunden.Name)
                            {
                                stunden = Double.Parse(tb.Text);

                            }
                        }

                        //Länge der Pause auslesen
                        foreach (TextBox tb in FindVisualChildren<TextBox>(spPause))
                        {
                            if (tb.Name == tbPause.Name)
                            {
                                pause = Double.Parse(tb.Text);
                            }
                        }

                        //Daten aus Zeile in DB schreiben
                        using (SQLiteConnection connection = new SQLiteConnection(App.databasePath))
                        {
                            connection.CreateTable<Arbeitstage>();

                            query = "SELECT Checked FROM Arbeitstage WHERE Arbeitstag = '" + tag + "'";

                            //IF Updated den Arbeitstag wenn er bis jetzt noch nicht gesetzt war
                            if (connection.FindWithQuery<Arbeitstage>(query, "?")?.Checked == null)
                            {
                                Arbeitstage arbeitstage = new Arbeitstage()
                                {
                                    Arbeitstag = tag,
                                    Checked = "x",
                                    Stunden = stunden,
                                    Pause = pause,
                                };

                                connection.Insert(arbeitstage);
                                //MessageBox.Show("Daten wurden gesichert!", "Daten gesichert!", MessageBoxButton.OK);
                                //sqliteQuery = @"UPDATE Arbeitstage SET Checked = 'x', Stunden = " + stunden.ToString("N", nfi) + ", Pause = " + pause.ToString("N", nfi) + " WHERE Arbeitstag = '" + tag + "'";
                            }
                            else //ELSE Updated nur die Stunden und die Pause, da der Arbeitstag an sich schon gesetzt war
                            {
                                sqliteQuery = @"UPDATE Arbeitstage SET Stunden = " + stunden.ToString("N", nfi) + ", Pause = " + pause.ToString("N", nfi) + " WHERE Arbeitstag = '" + tag + "'";
                                connection.Query<Arbeitstage>(sqliteQuery, "?");
                                //MessageBox.Show("Daten wurden geupadated!", "Daten geupdated!", MessageBoxButton.OK);
                            }
                        }

                        
                    }
                    else //Wenn CheckBox nicht gechecked ist, wird geprüft ob sie vorher gechecked war, wenn ja werden die Daten zurückgesetzt
                    {
                        using (SQLiteConnection connection = new SQLiteConnection(App.databasePath))
                        {
                            connection.CreateTable<Arbeitstage>();

                            query = "SELECT Checked FROM Arbeitstage WHERE Arbeitstag = '" + tag + "'";

                            if (connection.FindWithQuery<Arbeitstage>(query, "?")?.Checked == "x")
                            {
                                sqliteQuery = @"UPDATE Arbeitstage SET Checked = NULL, Stunden = NULL, Pause = NULL WHERE Arbeitstag = '" + tag + "'";

                                connection.Query<Arbeitstage>(sqliteQuery, "?");
                            }
                        }
                        //MessageBox.Show("Daten wurden für Tag " + tag + "gelöscht.", "Daten geupdated!", MessageBoxButton.OK);
                    }
                }
                ArbeitstageSchutz();
                MessageBox.Show("Daten wurden gesichert!", "Daten gesichert!", MessageBoxButton.OK);
            }
            else
            {
                butSaveClick = true;
                ArbeitstageBearbeiten();
            }
        }

        //Laden der aktuellen Einstellungen in das Arbeitszeiten Window
        public void LoadArbeitstage()
        {
            string tag, query, sqliteQuery;
            double stunden, pause;

            TextBox tb = new TextBox();

            //Prüfen welcher Tag in der Datenbank hinterlegt wurde
            //Dann jeweilige CheckBox anhaken und entsprechende Zeiten auslesen
            foreach (CheckBox cb in FindVisualChildren<CheckBox>(gridArbeistage))
            {
                tag = cb.Content.ToString();
                query = "SELECT Checked FROM Arbeitstage WHERE Arbeitstag = '" + tag + "'";

                using (SQLiteConnection connection = new SQLiteConnection(App.databasePath))
                {
                    connection.CreateTable<Arbeitstage>();

                    if (connection.FindWithQuery<Arbeitstage>(query, "?")?.Checked == "x")
                    {
                        cb.IsChecked = true;

                        tb.Name = "tbStunden_" + tag;

                        //Looped durch die Stunden, bis Stunden die gefüllt werden sollen gefunden
                        foreach(TextBox textbox in FindVisualChildren<TextBox>(spArbeitsstunden))
                        {
                            if (textbox.Name == tb.Name)
                            {
                                query = "SELECT Stunden FROM Arbeitstage WHERE Arbeitstag = '" + tag + "'";
                                textbox.Text = connection.FindWithQuery<Arbeitstage>(query, "?").Stunden.ToString();
                            }
                        }

                        tb.Name = "tbPause_" + tag;

                        //Looped durch die Pause, bis Pause die gefüllt werden sollen gefunden
                        foreach (TextBox textbox in FindVisualChildren<TextBox>(spPause))
                        {
                            if (textbox.Name == tb.Name)
                            {
                                query = "SELECT Pause FROM Arbeitstage WHERE Arbeitstag = '" + tag + "'";
                                textbox.Text = connection.FindWithQuery<Arbeitstage>(query, "?").Pause.ToString();
                            }
                        }
                    }
                }
            }        
        }

        //Form zum Bearbeiten entsperren
        public void ArbeitstageBearbeiten()
        {
            foreach (CheckBox cb in FindVisualChildren<CheckBox>(gridArbeistage))
            {
                cb.IsEnabled = true;
            }
            foreach (TextBox tb in FindVisualChildren<TextBox>(gridArbeistage))
            {
                tb.IsEnabled = true;
            }
            butSave.Content = "Speichern";
        }

        public void ArbeitstageSchutz()
        {
            foreach (CheckBox cb in FindVisualChildren<CheckBox>(gridArbeistage))
            {
                cb.IsEnabled = false;
            }
            foreach (TextBox tb in FindVisualChildren<TextBox>(gridArbeistage))
            {
                tb.IsEnabled = false;
            }
            butSave.Content = "Bearbeiten";
        }
    }
}
