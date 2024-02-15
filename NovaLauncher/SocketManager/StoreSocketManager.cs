using NovaLauncher.Models;
using NovaLauncher.Models.WebSocket;
using System.Web;
using System.Windows;
using System.Windows.Controls;

namespace NovaLauncher.SocketManager
{
    public class StoreSocketManager
    {
        public static SignalRClient StoreScoketClient;
        private WrapPanel StoreItemList { get; set; }
        private TextBlock ComingSoontxt { get; set; }
        public void Init(WrapPanel ServerItemList_, TextBlock ComingSoontxt)
        {
            StoreItemList = ServerItemList_;
            StoreScoketClient = new SignalRClient();
            StoreScoketClient.OnMessageReceived += HandleMessageReceived;
            this.ComingSoontxt = ComingSoontxt;
            ConnectToSignalRServer();
        }

        private async void ConnectToSignalRServer()
        {
            string connectionUrl = $"{NovaLauncher.Models.LauncherData.LauncherAPIUrl}:{NovaLauncher.Models.LauncherData.LauncherAPIPort}/Store?user={HttpUtility.UrlEncode(NovaLauncher.Models.LauncherData.GetGetUserInfo().account_id)}&token={HttpUtility.UrlEncode(NovaLauncher.Models.LauncherData.GetGetUserInfo().access_token)}";
            await StoreScoketClient.Connect(connectionUrl);
        }

        private async void HandleMessageReceived(string user, string Incommingmessage)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                LauncherData.MainContentWindow.LoadItemsAsync(ComingSoontxt);
            });
        }
    }
}
