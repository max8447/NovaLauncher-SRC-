using NovaLauncher.Models;
using NovaLauncher.Models.Controls;
using NovaLauncher.Models.GameSaveManager;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace NovaLauncher.Views.Controls
{
    public partial class UninstallBuildPopup : UserControl
    {
        private string Folder;
        private string SeasonName;

        public UninstallBuildPopup(string folder, string Seasonname)
        {
            InitializeComponent();
            this.Folder = folder;
            this.SeasonName = Seasonname;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            DeleteFolderAsync(Folder, this.SeasonName);
        }

        public async Task DeleteFolderAsync(string folderPath, string SeasonName)
        {
            if (Directory.Exists(folderPath))
            {
                try
                {
                    await Task.Run(() => Directory.Delete(folderPath, true));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        LauncherData.MainContentWindow.HostMethod.IsOpen = false;
                        LauncherData.MainContentWindow.HostMethod.DialogContent = null;
                    });

                    return;
                }
            }

            RemoveGameFromLibrary(SeasonName);

            Application.Current.Dispatcher.Invoke(() =>
            {
                LauncherData.MainContentWindow.HostMethod.IsOpen = false;
                LauncherData.MainContentWindow.HostMethod.DialogContent = null;
            });
        }

        public static void RemoveGameFromLibrary(string Name)
        {
            var gameList_ = GameList.LoadFromFile(UIManager.gameListFilePath);

            var gameList = gameList_;

            if (gameList.Games.Find(g => g.Name == Name) != null)
            {
                int index = gameList.Games.FindIndex(g => g.Name == Name);
                if (index != -1)
                {
                    gameList.Games.RemoveAt(index);
                }
            }

            gameList.SaveToFile(UIManager.gameListFilePath, true);
        }
    }
}
