using NovaLauncher.Models;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using MaterialDesignThemes.Wpf;
using System.Windows.Forms;

namespace NovaLauncher.Views.Controls
{
    public partial class NewBuild : System.Windows.Controls.UserControl
    {
        public NewBuild()
        {
            InitializeComponent();
        }

        private void SendButtonRequest(object sender, RoutedEventArgs e)
        {
            var folderPath = SelectFolder();

            if (string.IsNullOrEmpty(folderPath))
            {
                return;
            }

            var exePath = System.IO.Path.Combine(folderPath, "FortniteGame", "Binaries", "Win64", "FortniteClient-Win64-Shipping.exe");

            if (!File.Exists(exePath))
            {
                System.Windows.MessageBox.Show("Build not found");
                return;
            }

            NewBuildAsync(folderPath);
        }

        private async Task NewBuildAsync(string folder)
        {
            NewBuildAddingPopup NewBuildAdding = new NewBuildAddingPopup(folder);
            var resault = await LauncherData.MainContentWindow.HostMethod.ShowDialog(NewBuildAdding);
        }

        public string SelectFolder()
        {
            using (var folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "Select a folder to open";
                folderDialog.ShowNewFolderButton = false;

                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    return folderDialog.SelectedPath;
                }
            }

            return null;
        }
    }
}
