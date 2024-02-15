using MaterialDesignThemes.Wpf;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace NovaLauncher.Views.Popups
{
    /// <summary>
    /// Interaction logic for SeasonTrailer.xaml
    /// </summary>
    public partial class SeasonTrailer : UserControl
    {
        public SeasonTrailer()
        {
            InitializeComponent();

        }

        private void skipbtn_Click(object sender, RoutedEventArgs e)
        {
            DialogHost.Close(null);
        }

        private void myMediaElement_Loaded(object sender, RoutedEventArgs e)
        {
            string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string assetsFolder = Path.Combine(appDataFolder, "Nova", "Assets");
            string videoPath = Path.Combine(assetsFolder, "Season7.mp4");
            myMediaElement.Source = new Uri(videoPath);
        }
    }
}
