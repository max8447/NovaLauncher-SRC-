using Newtonsoft.Json;
using NovaLauncher.Models.Logger;
using NovaLauncher.Models.WebSocket;
using NovaLauncher.Views.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using MessageBox = System.Windows.MessageBox;

namespace NovaLauncher.SocketManager
{
    public class UserServerManagerSocket
    {
        public static SignalRClient ServerscoketClient;
        private StackPanel ServerItemList { get; set; }
        public void Init(StackPanel ServerItemList_)
        {
            ServerItemList = ServerItemList_;
            ServerscoketClient = new SignalRClient();
            ServerscoketClient.OnMessageReceived += HandleMessageReceived;
            string User = string.Empty;
            ConnectToSignalRServer();
        }

        private async void ConnectToSignalRServer()
        {
            string connectionUrl = $"{NovaLauncher.Models.LauncherData.LauncherAPIUrl}:{NovaLauncher.Models.LauncherData.LauncherAPIPort}/MyServer?user={HttpUtility.UrlEncode(NovaLauncher.Models.LauncherData.GetGetUserInfo().account_id)}&token={HttpUtility.UrlEncode(NovaLauncher.Models.LauncherData.GetGetUserInfo().access_token)}";

            await ServerscoketClient.Connect(connectionUrl);
        }

        private async void HandleMessageReceived(string user, string Incommingmessage)
        {

            try
            {
                var Item = JsonConvert.DeserializeObject<ServerItemUpdateClass>(Incommingmessage);
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    List<ServerItem> serverItems = ServerItemList.Children.OfType<ServerItem>().ToList();
                    foreach (var item in serverItems)
                    {
                        if (item.ServerID == Item.server_id)
                        {

                            if(!string.IsNullOrEmpty(Item.server_title))
                                item.SetServerName(Item.server_title);

							if (!string.IsNullOrEmpty(Item.server_playlist))
								item.SetServerPlaylist(Item.server_playlist);

							if (!string.IsNullOrEmpty(Item.server_max_players))
								item.SetServerMaxPlayers(Item.server_max_players);

							if (!string.IsNullOrEmpty(Item.server_version))
								item.SetServerGameVersion(Item.server_version);

                            if (!string.IsNullOrEmpty(Item.server_region))
                                item.SetServerRegion(Item.server_region);

                            if (!string.IsNullOrEmpty(Item.auto_start_game))
                                item.SetToggleAutoStartGame(Item.auto_start_game);

                            if (!string.IsNullOrEmpty(Item.custom_matchmaking_key))
                                item.SetServerMatchmakingKey(Item.custom_matchmaking_key);
                            
                            if (!string.IsNullOrEmpty(Item.players_alive) && !string.IsNullOrEmpty(Item.server_max_players))
                                item.SetPlayersAlive(Item.players_alive, Item.server_max_players);

							if (!string.IsNullOrEmpty(Item.server_state))
							{
								if (int.TryParse(Item.server_state, out int result))
									item.SetServerStatusStateAsync(result);
							}
						}
                    }
                }));

            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
                Logger.Log(LogLevel.Error, ex.Message);
            }
        }

        public class ServerItemUpdateClass
        {
			public string server_id { get; set; }
			public string server_title { get; set; }
			public string server_playlist { get; set; }
			public string server_max_players { get; set; }
			public string custom_matchmaking_key { get; set; }
			public string server_version { get; set; }
			public string players_alive { get; set; }
            public string server_region { get; set; }
            public string auto_start_game { get; set; }
            public string server_state { get; set; }
        }
    }
}
