using NovaLauncher.Models;
using NovaLauncher.Models.API.NovaBackend;
using NovaLauncher.Models.GameSaveManager;
using NovaLauncher.Views.Controls;
using NovaLauncher.Views.Pages.ContentPages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Wpf.Ui.Common;
using Wpf.Ui.Controls;
using Wpf.Ui.Controls.Interfaces;
using Wpf.Ui.Mvvm.Contracts;
using MessageBox = System.Windows.MessageBox;

namespace NovaLauncher.Views.Windows
{
    /// <summary>
    /// Interaction logic for ContentWindow.xaml
    /// </summary>
    public partial class ContentWindow : INavigationWindow
    {
        public LaunchProgressPopup PopupPopup { get; set; }
        public NovaLauncher.SocketManager.SocketManager MainSocketManagerManager { get; set; }
        private DispatcherTimer reloadTimer;
        public static NavigationItem MyServers;
        public ViewModels.ContentWindowModel ViewModel
        {
            get;
        }

        public ContentWindow(Views.ViewModels.ContentWindowModel viewModel, IPageService pageService, INavigationService navigationService)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
            LauncherData.MainContentWindow = this;
            SetPageService(pageService);
            navigationService.SetNavigationControl(RootNavigation);
            //InitializeReloadTimer();
        }

        protected override async void OnClosed(EventArgs e)
        {
            if (NovaLauncher.SocketManager.SocketManager.scoketClient != null)
                await NovaLauncher.SocketManager.SocketManager.scoketClient.Disconnect();
            base.OnClosed(e);
        }

        private async void MainWindow_ClosingAsync(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (AreGamesRunning())
            {
                MessageBoxResult result = MessageBox.Show("If you close the launcher, any currently launched games will be closed as well.", "Exit", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.No)
                {
                    e.Cancel = true;
                }
                else
                {

                }
            }
        }
        private bool AreGamesRunning()
        {
            var gameList = GameList.LoadFromFile(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Nova", "Game", "Installed", "Version.json"));
            if (gameList == null)
                return false;

            foreach (var game in gameList.Games)
            {
                try
                {
                    Process proc = Process.GetProcessById(game.GameID);
                    if (proc.ProcessName.Contains("Fortnite"))
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    continue;
                }
            }

            return false;

        }

        public LaunchProgressPopup ShowPopup(string Text, bool ProgressBarLoop, int ProgressValue, Wpf.Ui.Controls.SymbolIcon symbolIcon)
        {
            PopupFrame.Visibility = Visibility.Visible;

            LaunchProgressPopup progressControl = null;
            Dispatcher.Invoke(() =>
            {
                PopupFrame.Content = null;

                LaunchProgressPopup progressControl = new LaunchProgressPopup();
                PopupPopup = progressControl;
                progressControl.PopupText.Text = Text;
                progressControl.ProgressBar.IsIndeterminate = ProgressBarLoop;
                if (!ProgressBarLoop)
                    progressControl.ProgressBar.Value = ProgressValue;
                PopupFrame.Content = progressControl;
            });
            return progressControl;

        }

        public void HidePopup()
        {
            Dispatcher.Invoke(() =>
            {
                PopupFrame.Content = null;
                PopupFrame.Visibility = Visibility.Collapsed;
            });
        }

        public TextBlock GetProgressTextBlock()
        {
            Dispatcher.Invoke(() =>
            {
                if (PopupPopup == null)
                    return null;

                return PopupPopup.PopupText;
            });

            return PopupPopup.PopupText;

        }

        public void UpdatePopup(string Text, bool ProgressBarLoop, int ProgressValue)
        {
            Dispatcher.Invoke(() =>
            {
                if (PopupPopup == null)
                    return;

                PopupPopup.PopupText.Text = Text;
                if (PopupPopup.ProgressBar.IsIndeterminate != ProgressBarLoop)
                    PopupPopup.ProgressBar.IsIndeterminate = ProgressBarLoop;
                PopupPopup.ProgressBar.Value = ProgressValue;
            });
        }

        public Frame GetFrame()
            => RootFrame;

        public INavigation GetNavigation()
            => RootNavigation;

        public bool Navigate(Type pageType)
            => RootNavigation.Navigate(pageType);

        public void SetPageService(IPageService pageService)
            => RootNavigation.PageService = pageService;

        public void ShowWindow()
            => Show();

        public void CloseWindow()
            => Close();

        private void WPFUIHost_Loaded(object sender, RoutedEventArgs e)
        {

        }


        private void InitializeReloadTimer()
        {
            MyServers = new NavigationItem()
            {
                Content = "My Servers",
                PageTag = "myServers",
                Icon = SymbolRegular.ServerMultiple20,
                PageType = typeof(Views.Pages.ContentPages.ServerPage)
            };

            if (StorePage.LoadingProgressRing_ != null)
                StorePage.LoadingProgressRing_.Visibility = Visibility.Visible;

            if (StorePage.WrapPanel_ != null)
                StorePage.WrapPanel_.Children.Clear();

            LoadItemsAsync(null, true);
        }
        public static bool LoadedServerList = false;
        public async Task LoadItemsAsync(TextBlock ComingSoontxt, bool skip = false)
        {
            List<Controls.NovaStoreItem>? Items = new List<Controls.NovaStoreItem>();
            if (skip)
                Items = LauncherData.GetGetUserInfo().store;
            else
                Items = await LauncherAPI.GetStoreInfo();

            if (StorePage.WrapPanel_ != null)
                StorePage.WrapPanel_.Children.Clear();

            if (ComingSoontxt != null)
            {
                if (Items.Count == 0)
                    ComingSoontxt.Visibility = Visibility.Visible;
                else
                    ComingSoontxt.Visibility = Visibility.Collapsed;
            }

            if (Items != null)
            {
                if (StorePage.LoadingProgressRing_ != null)
                    StorePage.LoadingProgressRing_.Visibility = Visibility.Collapsed;

                if (StorePage.LoadingProgressRing_ != null)
                    StorePage.WrapPanel_.Visibility = Visibility.Visible;

                foreach (var item in Items)
                {
                    var newItem = new StoreItem(item);

                    if (StorePage.WrapPanel_ != null)
                        StorePage.WrapPanel_.Children.Add(newItem);
                }
            }


            if (Items != null)
            {
                bool bHasServer = false;
                foreach (var item in Items)
                {
                    if (item.bHasServer && item.NovaItemState == 1)
                    {
                        bHasServer = true;
                    }
                }

                if (bHasServer)
                {
                    if (!LauncherData.MainContentWindow.ViewModel.NavigationItems.Contains(MyServers))
                        LauncherData.MainContentWindow.ViewModel.NavigationItems.Add(MyServers);
                }
                else
                {
                    if (LauncherData.MainContentWindow.ViewModel.NavigationItems.Contains(MyServers))
                        LauncherData.MainContentWindow.ViewModel.NavigationItems.Remove(MyServers);
                }
            }
        }
    }
}