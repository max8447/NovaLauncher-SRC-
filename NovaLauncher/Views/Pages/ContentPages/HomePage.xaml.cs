using NovaLauncher.Models;
using NovaLauncher.Models.ClockManager;
using NovaLauncher.Models.Controls;
using NovaLauncher.Models.Discord;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace NovaLauncher.Views.Pages.ContentPages
{
    public partial class HomePage : Page
    {
        public static bool LoadedHomePage = false;
        public HomePage()
        {
            InitializeComponent();
            DiscordPresence.SetState(DiscordPresence.RichPresenceEnum.WaitingToLaunch, "");
            LauncherPages.HomePage.RecentlyPlayed = recentlyPlayed;
        }
        private void OpenLibrary(object sender, RoutedEventArgs e)
        {
            LauncherData.MainContentWindow.Navigate(typeof(Views.Pages.ContentPages.LibraryPage));
        }

        private void OpenDownloadPage(object sender, RoutedEventArgs e)
        {
            LauncherData.MainContentWindow.Navigate(typeof(Views.Pages.ContentPages.DownloadPage));
        }

        private void OpenNewsPage(object sender, RoutedEventArgs e)
        {
            LauncherData.MainContentWindow.Navigate(typeof(Views.Pages.ContentPages.NewsPage));
        }

        private async Task LoadContentAsync()
        {

            if (LoadedHomePage == true)
                return;

            LoadedHomePage = true;

            while (true)
            {
                if (fortniteCount != null && NovaTODimage != null)
                {
                    try
                    {
                        FortniteClock.LoadClock(fortniteCount, NovaTODimage);
                    }
                    catch (Exception ex)
                    {
  
                    }

                    try
                    {
                        await UIManager.LoadHomeLibraryAsync();
                    }
                    catch (Exception ex)
                    {

                    }

                    break;
                }
            }
        }


        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void imageBorder_Loaded(object sender, RoutedEventArgs e)
        {
            LoadContentAsync();
        }
    }
}
