using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;

namespace NovaLauncher.Views.Popups
{
    /// <summary>
    /// Interaction logic for TOSPage.xaml
    /// </summary>
    public partial class TOSPage : UserControl
    {
        public TOSPage()
        {
            InitializeComponent();
        }
        public void SetTextFromURL()
        {
            try
            {
                WebClient client = new WebClient();
                string url = "https://projectnova.b-cdn.net/tos.txt";
                string text = client.DownloadString(url);

                TOSTextBox.Text = text;
                TOSTextBox.TextWrapping = TextWrapping.Wrap;
            }
            catch (Exception ex)
            {

            }
        }

        private void SetTextFromURL(object sender, RoutedEventArgs e)
        {
            SetTextFromURL();
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var scrollViewer = (ScrollViewer)sender;
            if (scrollViewer.VerticalOffset == scrollViewer.ScrollableHeight)
            {
                acceptButton.Visibility = Visibility.Visible;
            }
            else
            {
                acceptButton.Visibility = Visibility.Collapsed;
            }
        }

    }
}
