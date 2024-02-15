using NovaLauncher.Models;
using NovaLauncher.SocketManager;
using System.Windows;
using System.Windows.Controls;
using Wpf.Ui.Controls;

namespace NovaLauncher.Views.Pages.ContentPages
{
    /// <summary>
    /// Interaction logic for StorePage.xaml
    /// </summary>
    public partial class StorePage : Page
    {
        public static ProgressRing LoadingProgressRing_ {  get; private set; }
        public static StoreSocketManager storeSocketManager { get; private set; }
        public static WrapPanel WrapPanel_ { get; private set; }
        public static bool Loaded = false;
        public StorePage()
        {
            InitializeComponent();
        }

        private void StoreItemList_Loaded(object sender, RoutedEventArgs e)
        {
            LoadingProgressRing_ = LoadingProgressRing;
            WrapPanel_ = StoreItemList;

            if (!Loaded)
            {
                LoadingProgressRing.Visibility = Visibility.Collapsed;
                StoreItemList.Visibility = Visibility.Visible;
                LauncherData.MainContentWindow.LoadItemsAsync(ComingSoontxt, true);
                Loaded = true;
            }

            if (storeSocketManager == null)
            {
                storeSocketManager = new StoreSocketManager();
                storeSocketManager.Init(WrapPanel_, ComingSoontxt);
            }
        }
    }
}