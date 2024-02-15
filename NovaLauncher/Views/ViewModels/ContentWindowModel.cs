using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;
using Wpf.Ui.Common;
using Wpf.Ui.Controls.Interfaces;
using Wpf.Ui.Controls;
using Wpf.Ui.Mvvm.Contracts;
using NovaLauncher.Models;

namespace NovaLauncher.Views.ViewModels
{

    public partial class ContentWindowModel : ObservableObject
    {
        private bool _isInitialized = false;

        [ObservableProperty]
        private string _applicationTitle = String.Empty;

        [ObservableProperty]
        public ObservableCollection<INavigationControl> _navigationItems = new();

        [ObservableProperty]
        private ObservableCollection<INavigationControl> _navigationFooter = new();

        [ObservableProperty]
        private ObservableCollection<MenuItem> _trayMenuItems = new();

        public ContentWindowModel(INavigationService navigationService)
        {
            if (!_isInitialized)
                InitializeViewModel();
        }

        private void InitializeViewModel()
        {
            ApplicationTitle = "Nova Launcher";

            NavigationItems = new ObservableCollection<INavigationControl>
            {
                new NavigationItem()
                {
                    Content = "Home",
                    PageTag = "dashboard",
                    Icon = SymbolRegular.Home24,
                    PageType = typeof(Views.Pages.ContentPages.HomePage)
                },
                new NavigationItem()
                {
                    Content = "News",
                    PageTag = "news",
                    Icon = SymbolRegular.News28,
                    PageType = typeof(Views.Pages.ContentPages.NewsPage)
                },
                new NavigationItem()
                {
                    Content = "Library",
                    PageTag = "library",
                    Icon = SymbolRegular.Library24,
                    PageType = typeof(Views.Pages.ContentPages.LibraryPage)
                }
            };

            if(!Global.bNoMCP)
            {
                NavigationItems.Add(new NavigationItem()
                {
                    Content = "Store",
                    PageTag = "store",
                    Icon = SymbolRegular.ShoppingBag20,
                    PageType = typeof(Views.Pages.ContentPages.StorePage)
                });
            }

            NavigationFooter = new ObservableCollection<INavigationControl>
            {

                new NavigationItem()
                {
                    Content = "Downloads",
                    PageTag = "downloads",
                    Icon = SymbolRegular.ArrowDownload20,
                    PageType = typeof(Views.Pages.ContentPages.DownloadPage)
                },
                new NavigationItem()
                {
                    Content = "Settings",
                    PageTag = "settings",
                    Icon = SymbolRegular.Settings24,
                    PageType = typeof(Views.Pages.ContentPages.SettingsPage)
                }
            };

            _isInitialized = true;
        }
    }
}
