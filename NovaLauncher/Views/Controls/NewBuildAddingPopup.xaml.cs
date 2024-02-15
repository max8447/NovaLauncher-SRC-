using NovaLauncher.Models.Controls;
using NovaLauncher.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;
using Clipboard = System.Windows.Clipboard;
using Application = System.Windows.Application;
using UserControl = System.Windows.Controls.UserControl;
using NovaLauncher.Models.GameSaveManager;
using NovaLauncher.Models.Logger;

namespace NovaLauncher.Views.Controls
{
    /// <summary>
    /// Interaction logic for NewBuildAddingPopup.xaml
    /// </summary>
    public partial class NewBuildAddingPopup : UserControl
    {
        private string Folder;
        public NewBuildAddingPopup(string folder)
        {
            InitializeComponent();
            this.Folder = folder;
        }


        private async Task NewBuildAsync()
        {
            try
            {
                Logger.Log(LogLevel.Info, "Starting NewBuildAsync");
                    
                await Task.Delay(500);
                Logger.Log(LogLevel.Info, "Delay executed");

                var exePath = System.IO.Path.Combine(this.Folder, "FortniteGame", "Binaries", "Win64", "FortniteClient-Win64-Shipping.exe");
                Logger.Log(LogLevel.Info, "Exe path: " + exePath);

                string? fileName = exePath;
                Logger.Log(LogLevel.Info, "File name: " + fileName);

                Random random = new Random();
                Logger.Log(LogLevel.Info, "Random initialized");

                string? path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Nova", "Game", "Installed");
                Logger.Log(LogLevel.Info, "Path: " + path);

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                    Logger.Log(LogLevel.Info, "Created directory");
                }

                string? filePath = System.IO.Path.Combine(path, "Version.json");
                Logger.Log(LogLevel.Info, "File path: " + filePath);

                if (!File.Exists(filePath))
                {
                    using (StreamWriter writer = File.CreateText(filePath))
                    {
                        await writer.WriteAsync("{\"Games\": []}");
                    }
                    Logger.Log(LogLevel.Info, "Created new file");
                }

                File.SetAttributes(filePath, File.GetAttributes(filePath) & ~FileAttributes.ReadOnly);
                Logger.Log(LogLevel.Info, "Modified file attributes");

                var games = GameList.LoadFromFile(filePath);
                Logger.Log(LogLevel.Info, "Games loaded from file");

                string Version = string.Empty;
                await Task.Run(() =>
                {
                    NewVersion newBuild = new NewVersion(fileName);
                    Version = newBuild._version;
                });
                Logger.Log(LogLevel.Info, "Version: " + Version);

                if (Version == "Failed finding a valid version" || string.IsNullOrEmpty(Version))
                {
                    Logger.Log(LogLevel.Warning, "Unsupported version detected.");
                    MessageBox.Show("Unsupported version");
                    await Task.Delay(1000);

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        LauncherData.MainContentWindow.HostMethod.IsOpen = false;
                        LauncherData.MainContentWindow.HostMethod.DialogContent = null;
                    });

                    return;
                }

                bool CancelAdd = true;
                if (LauncherData.DownloadPage != null)
                {
                    CancelAdd = LauncherData.DownloadPage.RequestPageUpdate(new Game() { Name = Version }, false);

                    if (!CancelAdd)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            LauncherData.MainContentWindow.HostMethod.DialogContent = null;
                        });
                    }
                }
                Logger.Log(LogLevel.Info, "CancelAdd: " + CancelAdd);

                if (!CancelAdd)
                    return;

                if (!LauncherData.MainContentWindow.HostMethod.IsOpen)
                    LauncherData.MainContentWindow.HostMethod.IsOpen = true;

                Logger.Log(LogLevel.Info, "Opened ");

                var existingGame = games.Games.Find(g => g.Name.Split(" ").Last() == Version);
                Logger.Log(LogLevel.Info, "Existing game: " + (existingGame != null ? existingGame.Name : "null"));

                if (existingGame != null)
                {
                    existingGame.GamePath = this.Folder;
                    existingGame.DatePlayed = DateTime.Now;
                    existingGame.LaunchCount++;
                    existingGame.GameID = random.Next(0, 999999);
                    games.SaveToFile(System.IO.Path.Combine(path, "Version.json"), true);
                }
                else
                {
                    Game newGame = new Game
                    {
                        Name = $"Build {Version}",
                        GamePath = this.Folder,
                        LaunchCount = 0,
                        DatePlayed = DateTime.MinValue,
                        GameID = random.Next(0, 999999)
                    };
                    games.AddGame(newGame);
                    games.SaveToFile(System.IO.Path.Combine(path, "Version.json"), true);
                }

                await Task.Delay(1000);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    LauncherData.MainContentWindow.HostMethod.IsOpen = false;
                    LauncherData.MainContentWindow.HostMethod.DialogContent = null;
                });

                Logger.Log(LogLevel.Info, "New build successfully processed.");
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, $"An error occurred: {ex}");
                MessageBox.Show(ex.ToString());
                Clipboard.SetText(ex.ToString());
                Application.Current.Dispatcher.Invoke(() =>
                {
                    LauncherData.MainContentWindow.HostMethod.IsOpen = false;
                    LauncherData.MainContentWindow.HostMethod.DialogContent = null;
                });
            }
        }


        public string SelectFolder()
        {
            using (var folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "Select a folder to open";
                folderDialog.ShowNewFolderButton = false;

                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    if (folderDialog.SelectedPath.Contains("OneDrive"))
                    {
                        MessageBox.Show("Please select a path outside \"OneDrive\". Example: C:\\, D:\\");
                        return SelectFolder();
                    }

                    return folderDialog.SelectedPath;
                }
            }

            return String.Empty;
        }

        private void Start(object sender, RoutedEventArgs e)
        {
            StartProcessAsync();
        }

        private async Task StartProcessAsync()
        {
            await NewBuildAsync();
        }
    }
}
