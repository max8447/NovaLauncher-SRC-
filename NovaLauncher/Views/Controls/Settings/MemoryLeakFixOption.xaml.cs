using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Path = System.IO.Path;

namespace NovaLauncher.Views.Controls.Settings
{
    /// <summary>
    /// Interaction logic for MemoryLeakFixOption.xaml
    /// </summary>
    public partial class MemoryLeakFixOption : UserControl
    {
        public MemoryLeakFixOption()
        {
            InitializeComponent();
            string settingsFilePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Nova", "Settings", "Settings.json");

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
                settings.bEnableEOR = true;
                settings.bEnableMLF = true;

                Directory.CreateDirectory(Path.GetDirectoryName(settingsFilePath));
                File.WriteAllText(settingsFilePath, settings.ToString());
            }


            bool? bEnableEOR = (bool?)settings.bEnableMLF;
            ToggleMLFix(bEnableEOR ?? false);
        }

        private void ToggleMLFix(bool enable, bool saveChanges = false)
        {
            MLFSwitch.IsChecked = enable;
            MLFText.Text = enable ? "On" : "Off";

            string settingsFilePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Nova", "Settings", "Settings.json");
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
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(settingsFilePath));
                File.WriteAllText(settingsFilePath, settings.ToString());
            }

            settings.bEnableMLF = enable;

            if (saveChanges)
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


        private void EnableMLF(object sender, RoutedEventArgs e)
        {
            ToggleMLFix(true, true);
        }

        private void MLFSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            ToggleMLFix(false, true);
        }

        private void SettingCard_Click(object sender, RoutedEventArgs e)
        {
            if (MLFSwitch.IsChecked == true)
                ToggleMLFix(false, true);

            else
                ToggleMLFix(true, true);
        }

        private void LoadContent(object sender, RoutedEventArgs e)
        {

        }
    }
}
