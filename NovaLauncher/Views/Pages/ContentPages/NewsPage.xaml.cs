using NovaLauncher.Models;
using NovaLauncher.Models.ClockManager;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Page = System.Windows.Controls.Page;

namespace NovaLauncher.Views.Pages.ContentPages
{
    public partial class NewsPage : Page
    {
        public static NovaLauncher.SocketManager.SocketManager MainNewsManager;
        public static StackPanel NewsContent_;

        public NewsPage()
        {
            InitializeComponent();
            NewsContent_ = NewsContent;
            LoginToSocketAsync();
        }
        private void LoadNews(object sender, RoutedEventArgs e)
        {
            FortniteClock.LoadTODVersion(fortniteCount, NovaTODimage);
        }

        private async Task LoginToSocketAsync()
        {
            MainNewsManager = new NovaLauncher.SocketManager.SocketManager();
            MainNewsManager.NewsContent_ = NewsContent;
            MainNewsManager.VersionText = Versiontxt;
            MainNewsManager.ScrollButtonControl = ScrollButtonControl;
            MainNewsManager.Init();
            NovaLauncher.SocketManager.SocketManager.LoadContent(Versiontxt, LauncherData.LauncherDataInfo, NewsContent_, ScrollButtonControl);
        }
    }
}
