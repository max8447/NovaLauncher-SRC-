using Newtonsoft.Json;
using NovaLauncher.Models;
using NovaLauncher.Models.API.NovaBackend;
using NovaLauncher.SocketManager;
using NovaLauncher.Views.Controls;
using System.Windows;
using System.Windows.Controls;
using static NovaLauncher.SocketManager.UserServerManagerSocket;

namespace NovaLauncher.Views.Pages.ContentPages
{
    /// <summary>
    /// Interaction logic for ServerPage.xaml
    /// </summary>
    public partial class ServerPage : Page
    {
        public static UserServerManagerSocket userServerManagerSocket {  get; set; }
        public ServerPage()
        {
            InitializeComponent();
            LoadingProgressRing.Visibility = Visibility.Collapsed;
            userServerManagerSocket = new UserServerManagerSocket();
            userServerManagerSocket.Init(ControlStackPanel);

			ControlStackPanel.Children.Add(new ServerItem(new ServerItemUpdateClass 
            {
				server_id = "9999",
				server_title = "Ahava's Server",
				server_playlist = "Solos",
				server_max_players = "100",
				custom_matchmaking_key = "Ender Is Gay",
				server_version = "7.40",
				players_alive = "100",
				server_region = "NA East",
				server_state = "0",
				auto_start_game = "On"
			}, this));


			ControlStackPanel.Children.Add(new ServerItem(new ServerItemUpdateClass
			{
				server_id = "99",
				server_title = "Ahava's Server (1)",
				server_playlist = "One Shot",
				server_max_players = "100",
				custom_matchmaking_key = "Ender soooo Gay",
				server_version = "7.40",
				players_alive = "100",
				server_region = "NA West",
				server_state = "0",
                auto_start_game = "Off"
            }, this));
		}

        private void WPFUIHost_ButtonRightClick(object sender, RoutedEventArgs e)
        {
			WPFUIHost.Content = null;
			WPFUIHost.Hide();
		}


		private void WPFUIHost_ButtonLeftClick(object sender, RoutedEventArgs e)
        {
			UpdateInfo();
		}

		private async void UpdateInfo()
		{
			var Editor = (UserServerEditor)WPFUIHost.Content;
			if (Editor != null)
			{
				var Server = new UserUpdateServer
				{
					account_id = LauncherData.GetGetUserInfo().account_id,
					server_id = Editor.ServerID,
					server_playlist = Editor.DetailsPlaylistTxt.Text,
					server_max_players = Editor.DetailsMaxPlayersTxt.Value.ToString(),
					custom_matchmaking_key = Editor.DetailsMatchmakingKeytxt.Text,
					server_region = Editor.DetailsRegionTxt.Text,
					access_token = LauncherData.GetGetUserInfo().access_token,
					auto_start_game = Editor.AutoStartGameBox.Text
                };

				WPFUIHost.Hide();
				ProcessingRequestHost.Show();
				var UpdatedInfo = JsonConvert.SerializeObject(Server);
				var Resault = await LauncherAPI.UpdateServerAsync(UpdatedInfo);

				ProcessingRequestHost.Hide();

				if (Resault.Success == false) 
				{
					MessageBox.Show(Resault.ErrorMessage);
					WPFUIHost.Show();
				}
			}
		}
	}

	public class UserUpdateServer
	{
		public string account_id { get; set; }
		public string access_token { get; set; }
		public string server_id { get; set; }
		public string server_playlist { get; set; }
		public string server_max_players { get; set; }
		public string custom_matchmaking_key { get; set; }
		public string server_region { get; set; }
        public string auto_start_game { get; set; }
    }
}
