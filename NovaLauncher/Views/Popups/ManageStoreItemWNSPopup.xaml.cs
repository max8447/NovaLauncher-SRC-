using NovaLauncher.Models;
using System.Windows;
using System.Windows.Controls;

namespace NovaLauncher.Views.Popups
{
    /// <summary>
    /// Interaction logic for ManageStoreItemPopup.xaml
    /// </summary>
    public partial class ManageStoreItemWNSPopup : UserControl
    {
        public ManageStoreItemWNSPopup()
        {
            InitializeComponent();
        }

        private void Closebtn_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                LauncherData.MainContentWindow.HostMethod.IsOpen = false;
                LauncherData.MainContentWindow.HostMethod.DialogContent = null;
            });
        }
    }
}
