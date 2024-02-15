using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace NovaLauncher.Models.NovaMessageBox
{
    public class NovaMessageBox
    {
        private RoutedEventHandler? ButtonRightClick;
        private RoutedEventHandler? ButtonLeftClick;

        public async Task ShowMessageAsync(string Text, string LeftButtonText, RoutedEventHandler? ButtonRightClick_, RoutedEventHandler? ButtonLeftClick_, string Title = "", bool bShowLeftButton = true, string RightButtonText = "", bool bShowRightButton = true)
        {
            if (ButtonRightClick != null)
                LauncherData.MainContentWindow.WPFUIHost.ButtonRightClick -= ButtonRightClick;
            if (ButtonLeftClick != null)
                LauncherData.MainContentWindow.WPFUIHost.ButtonLeftClick -= ButtonLeftClick;

            LauncherData.MainContentWindow.WPFUIHost.Title = "";
            LauncherData.MainContentWindow.WPFUIHost.Content = "";

            LauncherData.MainContentWindow.WPFUIHost.Title = Title;
            if (bShowLeftButton)
                LauncherData.MainContentWindow.WPFUIHost.ButtonLeftVisibility = Visibility.Visible;
            else
                LauncherData.MainContentWindow.WPFUIHost.ButtonLeftVisibility = Visibility.Hidden;

            if (bShowRightButton)
                LauncherData.MainContentWindow.WPFUIHost.ButtonRightVisibility = Visibility.Visible;
            else
                LauncherData.MainContentWindow.WPFUIHost.ButtonRightVisibility = Visibility.Hidden;

            LauncherData.MainContentWindow.WPFUIHost.Content = new TextBlock
            {
                Text = Text,
                TextWrapping = TextWrapping.WrapWithOverflow
            };
            ButtonRightClick = ButtonRightClick_;
            ButtonLeftClick = ButtonLeftClick_;
            LauncherData.MainContentWindow.WPFUIHost.ButtonRightClick += ButtonRightClick_;
            LauncherData.MainContentWindow.WPFUIHost.ButtonLeftClick += ButtonLeftClick_;
            LauncherData.MainContentWindow.WPFUIHost.ButtonLeftName = LeftButtonText;
            if (!string.IsNullOrEmpty(RightButtonText))
                LauncherData.MainContentWindow.WPFUIHost.ButtonRightName = RightButtonText;
            else
                LauncherData.MainContentWindow.WPFUIHost.ButtonRightName = "Hide";

            await LauncherData.MainContentWindow.WPFUIHost.ShowAndWaitAsync();
        }
    }
}
