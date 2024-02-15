using NovaLauncher.Models;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Diagnostics;
using MaterialDesignThemes.Wpf;
using NovaLauncher.Views.Popups;

namespace NovaLauncher.Views.Controls
{
    /// <summary>
    /// Interaction logic for StoreItem.xaml
    /// </summary>
    public partial class StoreItem : UserControl
    {
        public NovaStoreItem MyItem { get; set; }
        public StoreItem(NovaStoreItem novaStoreItem)
        {
            InitializeComponent();
            MyItem = novaStoreItem;
            Titletxt.Text = novaStoreItem.Title;
            PaymentCost.Text = $"${novaStoreItem.Cost}";
            SetButtonState(novaStoreItem);
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
                    PurchaseButton.IsEnabled = false;
                    MonthPaymenttxt.Visibility = Visibility.Collapsed;
                    PurchaseButton.Content = "Purchased";
                }
                else
                {
                    PurchaseButton.IsEnabled = true;
                    MonthPaymenttxt.Visibility = Visibility.Visible;
                    PurchaseButton.Content = "Manage";
                }
            }
            else if (novaStoreItem.NovaItemState == 0)
            {
                if (novaStoreItem.IsOneTimePayment)
                {
                    PurchaseButton.IsEnabled = true;
                    MonthPaymenttxt.Visibility = Visibility.Collapsed;
                    PurchaseButton.Content = "Purchase";
                }
                else
                {
                    PurchaseButton.IsEnabled = true;
                    MonthPaymenttxt.Visibility = Visibility.Visible;
                    PurchaseButton.Content = "Purchase";
                }
            }
            else if (novaStoreItem.NovaItemState == 2)
            {
                PurchaseButton.IsEnabled = false;
                MonthPaymenttxt.Visibility = Visibility.Collapsed;
                PurchaseButton.Content = "Coming Soon...";
            }
        }
        private void PurchaseButton_Click(object sender, RoutedEventArgs e)
        {
            if(MyItem.NovaItemState == 0)
            {
                string urlOrFilePath = $"https://discord.com/api/oauth2/authorize?client_id=1058961156657655811&redirect_uri=https%3A%2F%2Fstore.ender0001.gay%2FcreatePaymentIntent%2F{MyItem.Id}%2F&response_type=code&scope=identify";

                Process.Start(new ProcessStartInfo
                {
                    FileName = urlOrFilePath,
                    UseShellExecute = true
                });
            }
            else if(MyItem.NovaItemState == 1)
            {
				string urlOrFilePath = $"https://discord.com/api/oauth2/authorize?client_id=1058961156657655811&redirect_uri=https%3A%2F%2Fstore.ender0001.gay%2FgetBillingInfo&response_type=code&scope=identify";

				Process.Start(new ProcessStartInfo
				{
					FileName = urlOrFilePath,
					UseShellExecute = true
				});
			}
        }

        private void Morebtn_Click(object sender, RoutedEventArgs e)
        {
            LauncherData.MainContentWindow.HostMethod.ShowDialog(new StoreItemInfoPopup(MyItem));
        }
    }

    public class NovaStoreItem
    {
        public string? Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public int NovaItemState { get; set; }
        public string? Cost { get; set; }
        public bool IsOneTimePayment { get; set; }
        public bool bHasServer { get; set; }
        public string? IconUrl { get; set; }
    }

    public class NovaStoreServerItem
    {
        public string? ServerId { get; set; }
        public string? ServerName { get; set; }
        public string? MatchmakingKey { get; set; }
        public string? Region { get; set; }
        public int? ServerState { get; set; }  
    }

    public enum EServerState
    {
        OFFLINE,
        ONLINE,
        RESTARTING
    }
}
