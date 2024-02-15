using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Path = System.IO.Path;

namespace NovaLauncher.Views.Controls.Settings
{
    /// <summary>
    /// Interaction logic for EditOnReleaseOption.xaml
    /// </summary>
    public partial class EditOnReleaseOption : UserControl
    {
        public EditOnReleaseOption()
        {
            InitializeComponent();
            string settingsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Nova", "Settings", "Settings.json");
            dynamic settings = new JObject();

            if (File.Exists(settingsFilePath))
            {
                try
                {
                    settings = JObject.Parse(File.ReadAllText(settingsFilePath));
                }
                catch (JsonReaderException ex)
                {
                    Console.WriteLine($"Failed to parse JSON settings: {ex.Message}");
                }
            }
            else
            {
                Directory.CreateDirectory(Path.GetDirectoryName(settingsFilePath));
                File.WriteAllText(settingsFilePath, settings.ToString());
            }

            bool? bEnableEOR = (bool?)settings.bEnableEOR;
            ToggleEditOnRelease(bEnableEOR ?? false);
        }


        private void ToggleEditOnRelease(bool bTrue, bool bSave = false)
        {
            EORSwitch.IsChecked = bTrue;

            if (bTrue)
                EORText.Text = "On";
            else
                EORText.Text = "Off";

            // Load settings from the custom config file
            string settingsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Nova", "Settings", "Settings.json");
            dynamic settings = new JObject();

            if (File.Exists(settingsFilePath))
            {
                try
                {
                    settings = JObject.Parse(File.ReadAllText(settingsFilePath));
                }
                catch (JsonReaderException ex)
                {
                    Console.WriteLine($"Failed to parse JSON settings: {ex.Message}");
                }
            }
            else
            {
                Directory.CreateDirectory(Path.GetDirectoryName(settingsFilePath));
                File.WriteAllText(settingsFilePath, settings.ToString());
            }

            settings.bEnableEOR = bTrue;

            if (bSave)
            {
                try
                {
                    File.WriteAllText(settingsFilePath, settings.ToString());
                }
                catch (IOException ex)
                {
                    Console.WriteLine($"Failed to save JSON settings: {ex.Message}");
                }
            }
        }



        private void EnableEOR(object sender, RoutedEventArgs e)
        {
            ToggleEditOnRelease(true, true);
        }

        private void EORSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            ToggleEditOnRelease(false, true);
        }

        private void LoadContent(object sender, RoutedEventArgs e)
        {

        }

        private void SettingCard_Click(object sender, RoutedEventArgs e)
        {
            if (EORSwitch.IsChecked == true)
                ToggleEditOnRelease(false, true);

            else
                ToggleEditOnRelease(true, true);

        }
    }
}
