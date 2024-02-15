using NovaLauncher.Models;
using NovaLauncher.Views.Controls;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace NovaLauncher.Views.Popups
{
    /// <summary>
    /// Interaction logic for StoreItemInfoPopup.xaml
    /// </summary>
    public partial class StoreItemInfoPopup : UserControl
    {
        public StoreItemInfoPopup(NovaStoreItem item)
        {
            InitializeComponent();
            Titletxt.Text = item.Title;
            Biotxt.Text = item.Description;
            PaymentCost.Text = $"${item.Cost}";
            SetButtonState(item);
        }

        public void SetButtonState(NovaStoreItem novaStoreItem)
        {
            try
            {
                BitmapImage bitmap = new BitmapImage(new Uri(novaStoreItem.IconUrl));
                OptionIcon.ImageSource = bitmap;
            }
            catch { OptionIcon.Opacity = 0; }

            if (novaStoreItem.NovaItemState == 1)
            {
                if (novaStoreItem.IsOneTimePayment)
                {
                    MonthPaymenttxt.Visibility = Visibility.Collapsed;
                }
                else
                {
                    MonthPaymenttxt.Visibility = Visibility.Visible;
                }
            }
            else if (novaStoreItem.NovaItemState == 0)
            {
                if (novaStoreItem.IsOneTimePayment)
                {
                    MonthPaymenttxt.Visibility = Visibility.Collapsed;
                }
                else
                {
                    MonthPaymenttxt.Visibility = Visibility.Visible;
                }
            }
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
