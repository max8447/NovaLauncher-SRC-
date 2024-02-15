using NovaLauncher.Models;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using MaterialDesignThemes.Wpf;
using Path = System.IO.Path;
using MessageBox = System.Windows.MessageBox;
using NovaLauncher.Models.Controls;
using System.Linq;
using System.Net;
using NovaLauncher.Models.GameSaveManager;
using NovaLauncher.Models.Assets;
using Newtonsoft.Json;
using Installer;

namespace NovaLauncher.Views.Controls
{
    public partial class InstallNewBuild : System.Windows.Controls.UserControl
    {
        public bool bDownloading;
        public string Version;
        private string CL;
        private string VSize;
        private string Date;
        private string url;

        public InstallNewBuild(string version, string cL, string date, string size, string url = "")
        {
            InitializeComponent();
            Version = version;
            CL = cL;
            Date = $"Released: {date}";
            VSize = size;
            LoadContent();
            this.url = url;
        }

        public void SetButtonState(int state)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                switch (state)
                {
                    case 0:
                        DownloadButtonText.Text = "Download";
                        Downloadbtn.IsEnabled = true;
                        ButtonCard.IsEnabled = true;
                        break;
                    case 1:
                        DownloadButtonText.Text = "Installing...";
                        Downloadbtn.IsEnabled = false;
                        ButtonCard.IsEnabled = false;
                        break;
                    case 2:
                        DownloadButtonText.Text = "Installed";
                        Downloadbtn.IsEnabled = false;
                        ButtonCard.IsEnabled = false;
                        break;
                    case 3:
                        DownloadButtonText.Text = "Verify";
                        Downloadbtn.IsEnabled = true;
                        ButtonCard.IsEnabled = true;
                        break;
                }
            });
        }


        public void LoadContent(object? sender = null, RoutedEventArgs? e = null)
        {
            SeasonName.Text = $"Build {Version}";
            RDate.Text = Date;
            CLtxt.Text = CL;
            Sizetxt.Text = VSize;

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                BackImage.ImageSource = AssetManager.GetSeasonImage(GameOption.FindSeason(Version));
            });


            var Games = GameList.LoadFromFile(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Nova", "Game", "Installed", "Version.json"));
            if (Games == null)
                MessageBox.Show("");

            foreach (Game version in Games.Games)
            {
                if (version.Name.Contains(Version))
                {
                    var exePath = System.IO.Path.Combine(version.GamePath, "FortniteGame", "Binaries", "Win64", "FortniteClient-Win64-Shipping.exe");

                    if (!File.Exists(exePath))
                    {
                        SetButtonState(0);
                    }
                    else
                    {
                        SetButtonState(2);
                    }

                    return;
                }
            }

            if (bDownloading)
            {
                SetButtonState(1);
            }
            else
            {
                SetButtonState(0);
            }
        }

        private async Task NewBuildAsync(string folder)
        {
            NewBuildAddingPopup NewBuildAdding = new NewBuildAddingPopup(folder);
            var resault = await LauncherData.MainContentWindow.HostMethod.ShowDialog(NewBuildAdding);
        }

        public static bool HasEnoughSpace(string folderPath, long requiredSizeInBytes)
        {
            try
            {
                DriveInfo driveInfo = new DriveInfo(Path.GetPathRoot(folderPath));

                if (driveInfo.AvailableFreeSpace >= requiredSizeInBytes)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error while checking available space: " + ex.Message);
            }

            return false;
        }

        
        public void Install()
        {
            if (bDownloading)
            {
                MessageBox.Show("You can only download one build at once");
                return;
            }

            var path = SelectFolder();

            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            if (!Directory.Exists(Path.Combine(path, CL)))
                Directory.CreateDirectory(Path.Combine(path, CL));

            string[] parts = VSize.Split(' ');
            float sizeValue = float.Parse(parts[0]);
            long sizeInBytes = (long)(sizeValue * Math.Pow(1024, 3));


            if (!HasEnoughSpace(path, sizeInBytes))
            {
                MessageBox.Show("You do not have enough space to download this build.");
                return;
            }
            SetButtonState(1);
            bDownloading = true;
            path = Path.Combine(Path.Combine(path, CL));

            var ui = SynchronizationContext.Current;
            Task.Run(async () =>
            {
                if (url.Contains(".zip"))
                {
                    //var installer = new Main();
                    //await installer.DownloadAndExtractZip(url, Version, path, Sizetxt, ui);
                    //BuildFinished(FindFortniteDirectory(path));

                }
                else
                {
                    ui.Post(_ => Sizetxt.Text = "Downloading Manifest...", null);

                    var httpClient = new WebClient();
                    var manifest = JsonConvert.DeserializeObject<Main.ManifestFile>(httpClient.DownloadString(url));

                    await Main.Download(manifest, path, Sizetxt, ui);
                    BuildFinished(path);
                }
            });
        }

        public static string FindFortniteDirectory(string parentDirectory)
        {

            string[] subDirectories = Directory.GetDirectories(parentDirectory);

            foreach (string directory in subDirectories)
            {
                try
                {
                    if (Directory.GetDirectories(Path.GetDirectoryName(directory)).Any(subdir => Path.GetFileName(subdir) == "FortniteGame"))
                    {
                        return Path.GetDirectoryName(directory);
                    }

                    string subDirectory = FindFortniteDirectory(directory);

                    if (subDirectory != null)
                    {
                        return subDirectory;
                    }
                }
                catch (UnauthorizedAccessException)
                {

                }
            }

            return null;
        }

        private void BuildFinished(string path)
        {
            if (System.Windows.Application.Current != null)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    if (Sizetxt != null)
                    {
                        SetButtonState(2);
                        bDownloading = false;

                        var exePath = System.IO.Path.Combine(path, "FortniteGame", "Binaries", "Win64", "FortniteClient-Win64-Shipping.exe");

                        if (!File.Exists(exePath))
                        {
                            System.Windows.MessageBox.Show("Build not found");
                            return;
                        }

                        NewBuildAsync(System.IO.Path.Combine(path));

                        if (LauncherData.MainContentWindow == null)
                            MessageBox.Show("null");

                        LauncherData.MainContentWindow.Navigate(typeof(Views.Pages.ContentPages.LibraryPage));
                    }
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

            return null;
        }

        private void CardAction_Click(object sender, RoutedEventArgs e)
        {
            Install();
        }
    }
}