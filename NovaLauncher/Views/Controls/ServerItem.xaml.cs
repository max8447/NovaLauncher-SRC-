using NovaLauncher.Views.Pages.ContentPages;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static NovaLauncher.SocketManager.UserServerManagerSocket;

namespace NovaLauncher.Views.Controls
{
    /// <summary>
    /// Interaction logic for ServerItem.xaml
    /// </summary>
    public partial class ServerItem : UserControl
    {
        public string ServerID { get; set; }
		public ServerPage serverPage { get; set; }
        public int ItemState { get; set; }
		public ServerItemUpdateClass UpdateClass { get; set; }
		public ServerItem(ServerItemUpdateClass serverItemUpdate, ServerPage page)
        {
            InitializeComponent();
            ServerID = serverItemUpdate.server_id;
			serverPage = page;
			UpdateClass = serverItemUpdate;

			if (int.TryParse(serverItemUpdate.server_state, out int result))
				SetServerStatusStateAsync(result);

			SetServerName(serverItemUpdate.server_title);
			SetServerPlaylist(serverItemUpdate.server_playlist);
			SetServerMaxPlayers(serverItemUpdate.server_max_players);
			SetServerMatchmakingKey(serverItemUpdate.custom_matchmaking_key);
			SetServerGameVersion(serverItemUpdate.server_version);
			SetPlayersAlive(serverItemUpdate.players_alive, serverItemUpdate.server_max_players);
			SetToggleAutoStartGame(serverItemUpdate.auto_start_game);
        }

		public void SetServerName(string Title)
		{
			Titletxt.Text = Title;
			DetailsServerNameTxt.Text = Title;
			UpdateClass.server_title = Title;
		}

		public void SetServerPlaylist(string Playlist)
		{
			DetailsPlaylistTxt.Text = Playlist;
			GameModeTxt.Text = Playlist;
			UpdateClass.server_playlist = Playlist;
		}

		public void SetServerMaxPlayers(string Players)
		{
			DetailsMaxPlayersTxt.Text = Players;
			UpdateClass.server_max_players = Players;
			SetPlayersAlive(UpdateClass.players_alive, Players);
		}

        public void SetServerRegion(string Region)
        {
            UpdateClass.server_region = Region;
        }

        public void SetToggleAutoStartGame(string on)
        {
			UpdateClass.auto_start_game = on;
        }

        public void SetServerMatchmakingKey(string Key)
		{
			DetailsMatchmakingKeytxt.Text = Key;
			UpdateClass.custom_matchmaking_key = Key;
		}

		public void SetPlayersAlive(string Current, string Max)
		{
			PlayersAlivetxt.Text = $"{Current}/{Max}";
			UpdateClass.players_alive = Current;
		}

		public void SetServerGameVersion(string Version)
		{
			DetailsGameVersionTxt.Text = Version;
			UpdateClass.server_version = Version;
		}
		public async Task SetServerStatusStateAsync(int State)
		{
			ItemState = State;
			UpdateClass.server_state = State.ToString();
			if (State == 0)
			{
				StateChangeButton.IsEnabled = true;
				StateChangeButton.Content = "Start Match";

			}
			else if (State == 1)
			{
				StateChangeButton.IsEnabled = false;
				StateChangeButton.Content = "Starting...";
			}
			else if (State == 2)
			{
				StateChangeButton.IsEnabled = true;

				StateChangeButton.Content = "End Game";
			}
			else if (State == 3)
			{
				StateChangeButton.IsEnabled = false;
				StateChangeButton.Content = "Restarting...";
			}
			else if (State == 4)
			{
				StateChangeButton.IsEnabled = false;
				StateChangeButton.Content = "Offline";
			}
		}

        private void StateChangeButton_Click(object sender, RoutedEventArgs e)
        {
			if (ItemState == 0)
			{

			}
			else if (ItemState == 2)
			{

			}
		}

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
			serverPage.WPFUIHost.Content = new UserServerEditor(UpdateClass, serverPage);
			serverPage.WPFUIHost.Show();
		}
	}
}

