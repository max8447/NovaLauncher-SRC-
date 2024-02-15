using System.Windows;
using System.Windows.Controls;

namespace NovaLauncher.Views.Popups
{
    /// <summary>
    /// Interaction logic for LoginLoadPopup.xaml
    /// </summary>
    public partial class LoginLoadPopup : UserControl
    {
        public LoginLoadPopup()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

        }

        public void SetText(string text)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (text.Length > 16)
                {
                    SeasonName.Text = text.Substring(0, 16) + "...";
                }
                else
                {
                    SeasonName.Text = text;
                }
            });
        }
    }
}
