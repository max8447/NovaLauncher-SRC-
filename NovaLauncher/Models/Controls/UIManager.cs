using NovaLauncher.Models.GameSaveManager;
using NovaLauncher.Models.Logger;
using NovaLauncher.Views.Controls;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NovaLauncher.Models.Controls
{
    public static class LauncherPages
    {
        public struct HomePage
        {
            public static WrapPanel RecentlyPlayed { get; set; }

        }
        public struct LibraryPage
        {
            public static TextBlock LibraryTitle;
            public static TextBlock LibraryData;
            public static TextBlock NoLibrary;
            public static ImageBrush LibraryImage;
            public static WrapPanel LibraryWrapPanel;

        }
    }
    public class UIManager
    {
        public static string gameListFilePath;

        static UIManager()
        {
            gameListFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Nova", "Game", "Installed", "Version.json");
        }

        public static async Task LoadLibraryAsync()
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                if (LauncherPages.LibraryPage.NoLibrary == null)
                    return;

                LauncherPages.LibraryPage.LibraryWrapPanel.Children.Clear();

                var gameList = GameList.LoadFromFile(gameListFilePath)?.Games;
                if (gameList == null || gameList.Count == 0)
                {
                    if (LauncherPages.LibraryPage.NoLibrary != null)
                        LauncherPages.LibraryPage.NoLibrary.Visibility = Visibility.Visible;

                    return;
                }

                if (LauncherPages.LibraryPage.NoLibrary != null)
                    LauncherPages.LibraryPage.NoLibrary.Visibility = Visibility.Hidden;

                NewBuild addlibraryButton = new NewBuild();
                LauncherPages.LibraryPage.LibraryWrapPanel.Children.Add(addlibraryButton);

                Separator separator = new Separator()
                {
                    Width = 0,
                    BorderThickness = new Thickness(0)
                };
                LauncherPages.LibraryPage.LibraryWrapPanel.Children.Add(separator);

                foreach (Game Version in gameList)
                {
                    LibraryOption libraryButton = new LibraryOption(Version, 0, null, true, LauncherPages.LibraryPage.LibraryImage);
                    LauncherPages.LibraryPage.LibraryWrapPanel.Children.Add(libraryButton);

                    Separator separator_ = new Separator()
                    {
                        Width = 0,
                        BorderThickness = new Thickness(0)
                    };
                    LauncherPages.LibraryPage.LibraryWrapPanel.Children.Add(separator_);
                }
            });
        }

        public static async Task LoadHomeLibraryAsync()
        {
            Logger.Logger.Log(LogLevel.Info, "Loading home library...");

            try
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    LauncherPages.HomePage.RecentlyPlayed.Children.Clear();
                });

                Logger.Logger.Log(LogLevel.Info, "Loading games from file...");
                var sortedGames = await Task.Run(() => GameList.LoadFromFile(gameListFilePath)?.Games?.OrderByDescending(g => g.DatePlayed)?.ToList());
                var fiveMostRecentlyLaunched = sortedGames?.Take(5);

                if (sortedGames == null || sortedGames.Count == 0)
                {
                    Logger.Logger.Log(LogLevel.Info, "No games found in the game list.");
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        NewBuild addlibraryButton = new NewBuild();
                        LauncherPages.HomePage.RecentlyPlayed.Children.Add(addlibraryButton);
                    });
                }

                if (fiveMostRecentlyLaunched != null)
                {
                    Logger.Logger.Log(LogLevel.Info, "Adding recently played games to home library...");
                    foreach (Game game_ in fiveMostRecentlyLaunched)
                    {
                        await Application.Current.Dispatcher.InvokeAsync(() =>
                        {
                            LibraryOption libraryButton = new LibraryOption(game_, 0, null);
                            LauncherPages.HomePage.RecentlyPlayed.Children.Add(libraryButton);
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Logger.Log(LogLevel.Error, $"An error occurred while loading home library: {ex.Message}");
            }
        }
    }
}
