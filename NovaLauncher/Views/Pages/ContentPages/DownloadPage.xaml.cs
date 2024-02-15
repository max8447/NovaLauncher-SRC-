using NovaLauncher.Models;
using NovaLauncher.Models.API.NovaBackend.Classes;
using NovaLauncher.Models.GameSaveManager;
using NovaLauncher.Models.NovaMessageBox;
using NovaLauncher.Views.Controls;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace NovaLauncher.Views.Pages.ContentPages
{
    /// <summary>
    /// Interaction logic for DownloadPage.xaml
    /// </summary>
    public partial class DownloadPage : Page
    {
        public static List<InstallNewBuild> installNewBuildControls = new List<InstallNewBuild>();
        public static bool bInit;
        public DownloadPage()
        {

            InitializeComponent();
            if (!bInit)
            {
                LauncherData.DownloadOptionsStack = DownloadOptionsStack;
                LauncherData.DownloadPage = this;
                LoadPage(LauncherData.GetGetUserInfo().builds);
                bInit = true;
            }
        }


        public bool IsVersionInDownloads(List<BuildInfo> Builds, Game game)
        {
            foreach (BuildInfo build in Builds)
                if (game.Name.Contains(build.build_version))
                    return false;

            return true;
        }

        public async Task LoadPage(List<BuildInfo> Builds)
        {
            if (Global.bNoMCP)
                LibraryText.Text = "NOT AVALIABLE IN OFFLINE MODE";

            if (LauncherData.DownloadOptionsStack == null)
                return;

            await Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    LauncherData.DownloadOptionsStack.Children.Clear();
                    foreach (BuildInfo build in Builds)
                    {
                        InstallNewBuild buildcontrol = new InstallNewBuild(build.build_version, build.build_cl, build.date, build.size, build.url);
                        installNewBuildControls.Add(buildcontrol);
                        DownloadOptionsStack.Children.Add(buildcontrol);
                    }

                    if (LauncherData.DownloadOptionsStack.Children.Count <= 0)
                    {
                        LibraryText.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        LibraryText.Visibility = Visibility.Hidden;
                    }
                });
            });

            return;
        }
        public bool RequestPageUpdate(Game game, bool bReinitControl)
        {
            if (bReinitControl)
            {
                foreach (BuildInfo build in LauncherData.GetGetUserInfo().builds)
                {
                    if (game.Name.Contains(build.build_version))
                    {
                        foreach (var download in installNewBuildControls)
                        {
                            if (download.Version.Contains(game.Name))
                            {
                                download.LoadContent();
                            }
                        }
                    }
                }
                return true;
            }


            if (LauncherData.MainContentWindow.HostMethod.IsOpen)
                LauncherData.MainContentWindow.HostMethod.IsOpen = false;

            bool Update = true;
            foreach (BuildInfo build in LauncherData.GetGetUserInfo().builds)
            {
                if (game.Name.Contains(build.build_version))
                {

                    foreach (var download in installNewBuildControls)
                    {
                        if (download.bDownloading == true && download.Version.Contains(game.Name))
                        {
                            Update = false;
                            NovaMessageBox message = new NovaMessageBox();
                            RoutedEventHandler RightButtonClick = null;
                            RightButtonClick += (sender, args) =>
                            {
                                LauncherData.MainContentWindow.WPFUIHost.Hide();
                            };
                            RoutedEventHandler ButtonLeftClick = null;
                            ButtonLeftClick += (sender, args) =>
                            {
                                LauncherData.MainContentWindow.Navigate(typeof(Views.Pages.ContentPages.DownloadPage));
                                LauncherData.MainContentWindow.WPFUIHost.Hide();
                            };
                            message.ShowMessageAsync($"The game you're trying to add to your library is currently being downloaded. If you wish to cancel the download and try again, please visit the download page.", "Go to downloads", RightButtonClick, ButtonLeftClick).ConfigureAwait(true).GetAwaiter();

                        }
                    }
                }
            }
            return Update;
        }
    }
}
