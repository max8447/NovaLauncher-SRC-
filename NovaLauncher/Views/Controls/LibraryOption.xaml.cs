using NovaLauncher.Models;
using NovaLauncher.Models.Controls;
using NovaLauncher.Models.GameLauncher;
using NovaLauncher.Models.GameSaveManager;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using MessageBox = System.Windows.MessageBox;
using Path = System.IO.Path;
using MaterialDesignThemes.Wpf;
using NovaLauncher.Models.API.NovaBackend;
using System.Threading;

namespace NovaLauncher.Views.Controls
{
    /// <summary>
    /// Interaction logic for LibraryOption.xaml
    /// </summary>
    public partial class LibraryOption : System.Windows.Controls.UserControl
    {
        internal GameOption.ButtonData ButtonData;

        public LibraryOption(Game Version, int ButtonState, Frame frame, bool IsLibraryButton = false, ImageBrush LibraryImage = null)
        {
            InitializeComponent();
            ButtonData = GameOption.LoadButton(Version, SeasonBrush, seasonName, chapterText, statusText, IsLibraryButton, frame, LibraryImage, ButtonState, null);
        }

        public static bool Loading;
        public static ImageSource ImageLoad;
        private void MouseHover(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (ButtonData.LibraryButton)
            {
                ImageLoad = SeasonBrush.ImageSource;
                if (!Loading)
                {
                    Loading = true;

                    if (ButtonData.LibraryImage == null)
                        return;

                    DoubleAnimation Hide = new DoubleAnimation
                    {
                        From = 1,
                        To = 0.3,
                        Duration = TimeSpan.FromSeconds(0.1)
                    };
                    ButtonData.LibraryImage.BeginAnimation(ImageBrush.OpacityProperty, Hide);
                    LauncherPages.LibraryPage.LibraryTitle.BeginAnimation(ImageBrush.OpacityProperty, Hide);
                    LauncherPages.LibraryPage.LibraryData.BeginAnimation(ImageBrush.OpacityProperty, Hide);

                    string gameName = ButtonData.Game.Name;
                    int launchCount = 0;
                    foreach (Game game in GameList.LoadFromFile(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Nova", "Game", "Installed", "Version.json")).Games)
                    {
                        if (game.Name == gameName)
                        {
                            launchCount = game.LaunchCount;
                            break;
                        }
                    }
                    ImageSource imageSource = SeasonBrush.ImageSource;
                    string libraryDataText;
                    if (launchCount == 0)
                    {
                        libraryDataText = $"Has not been played yet";
                    }
                    else if (launchCount == 1)
                    {
                        libraryDataText = $"Has been launched {launchCount} time.";
                    }
                    else
                    {
                        libraryDataText = $"Has been launched {launchCount} times.";
                    }
                    LauncherPages.LibraryPage.LibraryTitle.Text = gameName;
                    LauncherPages.LibraryPage.LibraryData.Text = libraryDataText;
                    ButtonData.LibraryImage.ImageSource = imageSource;

                    DoubleAnimation Show = new DoubleAnimation
                    {
                        From = 0.3,
                        To = 1,
                        Duration = TimeSpan.FromSeconds(0.1)
                    };
                    ButtonData.LibraryImage.BeginAnimation(ImageBrush.OpacityProperty, Show);
                    LauncherPages.LibraryPage.LibraryTitle.BeginAnimation(ImageBrush.OpacityProperty, Show);
                    LauncherPages.LibraryPage.LibraryData.BeginAnimation(ImageBrush.OpacityProperty, Show);

                    Loading = false;
                }
            }
        }
        public LibraryOption()
        {
            InitializeComponent();
        }

        private void ControlLoaded(object sender, RoutedEventArgs e)
        {

        }


        private void OpenInstallPath(object sender, RoutedEventArgs e)
        {
            string path = ButtonData.Game.GamePath;
            Process.Start("explorer.exe", path);
        }
        private void LaunchGame(object sender, RoutedEventArgs e)
        {
            LaunchGameAsync();
        }
        private async Task LaunchGameAsync()
        {
            if (!Global.bSkipUpdate)
            {
                var LauncherVersion = LauncherData.GetLauncherVersion();

                if (string.IsNullOrEmpty(LauncherVersion))
                {
                    return;
                }

                if (LauncherVersion != Global.GetCurrentLauncherVersion())
                {
                    NovaLauncher.Models.NovaMessageBox.NovaMessageBox messageBox = new NovaLauncher.Models.NovaMessageBox.NovaMessageBox();

                    RoutedEventHandler RightButtonClick = null;
                    RightButtonClick += (sender, args) =>
                    {
                        LauncherData.MainContentWindow.WPFUIHost.Hide();
                    };
                    RoutedEventHandler ButtonLeftClick = null;
                    ButtonLeftClick += (sender, args) =>
                    {
                        CloseFortniteProcesses();
                        LauncherData.MainContentWindow.WPFUIHost.Hide();
                    };
                    await messageBox.ShowMessageAsync("It appears that the launcher is currently running on an outdated version and requires an update to launch your game.", "Update", RightButtonClick, ButtonLeftClick); 
                    return;
                }
            }
            await GameLauncher.LaunchGameAsync(ButtonData.Game, statusText);
        }

        public static async void CloseFortniteProcesses()
        {
            string[] processNames = { "FortniteClient-Win64-Shipping", "FortniteClient-Win64-Shipping_BE", "FortniteLauncher" };

            foreach (string processName in processNames)
            {
                Process[] processes = Process.GetProcessesByName(processName);
                foreach (Process process in processes)
                {
                    try
                    {
                        await Task.Run(() =>
                        {
                            process.CloseMainWindow();
                            if (!process.WaitForExit(5000))
                            {
                                process.Kill();
                            }
                        });
                    }
                    catch
                    {

                    }
                }
            }

            string[] Launchargs = Environment.GetCommandLineArgs();
            string[] arguments = Launchargs.Skip(1).ToArray();

            if (arguments == null || arguments.Length > 0)
            {
                LauncherData.MainContentWindow.WPFUIHost.Hide();
            }

            string RestartArgs = string.Join(" ", arguments) + " -update";

            string appPath = Process.GetCurrentProcess().MainModule.FileName;
            Process.Start(appPath, RestartArgs);
            try
            {
                var Account = LauncherData.GetGetUserInfo();
                if (Account != null)
                    LauncherAPI.LogoutAsync(Account.access_token);
            }
            catch { }

            Environment.Exit(0);
        }
        private async void RemoveFromLibrary(object sender, RoutedEventArgs e)
        {
            UIManager.gameListFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Nova", "Game", "Installed", "Version.json");

            if (statusText.Text.Equals("Launch") || statusText.Text.Equals("Build not found"))
            {
                var gameList = GameList.LoadFromFile(UIManager.gameListFilePath);

                if (gameList.Games.Find(g => g.Name == ButtonData.Game.Name) != null)
                {
                    int index = gameList.Games.FindIndex(g => g.Name == ButtonData.Game.Name);
                    if (index != -1)
                    {
                        gameList.Games.RemoveAt(index);
                    }
                }

                await Task.Run(() => gameList.SaveToFile(UIManager.gameListFilePath, true));
                if (LauncherData.DownloadPage != null)
                {
                    LauncherData.DownloadPage.RequestPageUpdate(ButtonData.Game, true);
                }
            }
            else
            {
                NovaLauncher.Models.NovaMessageBox.NovaMessageBox messageBox = new NovaLauncher.Models.NovaMessageBox.NovaMessageBox();

                RoutedEventHandler RightButtonClick = null;
                RightButtonClick += (sender, args) =>
                {
                    LauncherData.MainContentWindow.WPFUIHost.Hide();
                };

                RoutedEventHandler ButtonLeftClick = null;
                ButtonLeftClick += async (sender, args) =>
                {
                    var gameList = GameList.LoadFromFile(UIManager.gameListFilePath);

                    if (gameList.Games.Find(g => g.Name == ButtonData.Game.Name) != null)
                    {
                        int index = gameList.Games.FindIndex(g => g.Name == ButtonData.Game.Name);
                        if (index != -1)
                        {
                            var Game = Process.GetProcessById(gameList.Games[index].GameID);
                            Game.Exited -= (sender, args) =>
                            {
                                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                                {
                                    GameLauncher.OnGameExited((Process)sender, statusText);
                                });
                            };

                            Game.Kill();
                            gameList.Games.RemoveAt(index);
                        }
                    }

                    await Task.Run(() => gameList.SaveToFile(UIManager.gameListFilePath, true));

                    LauncherData.MainContentWindow.WPFUIHost.Hide();
                };

                await messageBox.ShowMessageAsync(
                                    "This version cannot be removed while it is in use, would you like to close the game and then remove from library?",
                                    "Remove", RightButtonClick, ButtonLeftClick); return;
            }
        }


        private void UninstallGame(object sender, RoutedEventArgs e)
        {
            if (statusText.Text != "Launch")
            {
                MessageBox.Show("You cannot uninstall a build thats in use.");
                return;
            }

            string message = @"Are you sure you want to uninstall the selected build?";

            var Result = MessageBox.Show(message, "Nova Launcher", MessageBoxButton.YesNo);

            if (Result == MessageBoxResult.Yes)
                UninstallAsync();


            return;
        }

        private async Task UninstallAsync()
        {
            UninstallBuildPopup NewBuildAdding = new UninstallBuildPopup(ButtonData.Game.GamePath, ButtonData.Game.Name);
            var resault = await LauncherData.MainContentWindow.HostMethod.ShowDialog(NewBuildAdding);
        }

        private void ForceExitGame(object sender, RoutedEventArgs e)
        {
            var gameList = GameList.LoadFromFile(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Nova", "Game", "Installed", "Version.json"));
            var existingGame = gameList.Games.FirstOrDefault(g => g.Name == ButtonData.Game.Name);

            try
            {
                if (existingGame == null)
                {
                    gameList.SaveToFile(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Nova", "Game", "Installed", "Version.json"), true);
                    return;
                }

                var process = Process.GetProcessById(existingGame.GameID);
                process.Kill();
                GameOption.SetButtonState(statusText, 0);
            }
            catch
            {
                MessageBox.Show("Failed to exit game!");
            }
        }

        private void StatusLoaded(object sender, RoutedEventArgs e)
        {

        }

        private void StatusTextChanged(object sender, EventArgs e)
        {
            if (statusText.Text.Equals("Running"))
            {
                CloseGameItem.Visibility = Visibility.Visible;
            }
            else
            {
                CloseGameItem.Visibility = Visibility.Collapsed;
            }

            if (statusText.Text.Equals("Launch"))
            {
                //VerifyGameItem.Visibility = Visibility.Visible;
            }
            else
            {
                //VerifyGameItem.Visibility = Visibility.Collapsed;
            }
        }

        private async void VerifyGameItem_Click(object sender, RoutedEventArgs e)
        {
            var Build = await LauncherAPI.BuildVerifyEndpointAsync(ButtonData.Game.Name);

            var ui = SynchronizationContext.Current;
            GameOption.SetButtonState(statusText, 6);

            await GameLauncher.ForceFullGameVerify(Build, statusText, ButtonData.Game, ui);
            GameOption.SetButtonState(statusText, 0);
        }
    }
}
