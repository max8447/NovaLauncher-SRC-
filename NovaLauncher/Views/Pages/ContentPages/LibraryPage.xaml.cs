using NovaLauncher.Models.Controls;
using System.Windows.Controls;


namespace NovaLauncher.Views.Pages.ContentPages
{
    /// <summary>
    /// Interaction logic for LibraryPage.xaml
    /// </summary>
    public partial class LibraryPage : Page
    {
        public static bool? Loaded;
        public LibraryPage()
        {
            InitializeComponent();
            LauncherPages.LibraryPage.LibraryWrapPanel = LibraryWrapPanel;
            LauncherPages.LibraryPage.LibraryImage = BorderBackground;
            LauncherPages.LibraryPage.LibraryTitle = VersionText;
            LauncherPages.LibraryPage.LibraryData = LaunchCount;
            LauncherPages.LibraryPage.NoLibrary = LibraryText;

            UIManager.LoadLibraryAsync();
        }
    }
}
