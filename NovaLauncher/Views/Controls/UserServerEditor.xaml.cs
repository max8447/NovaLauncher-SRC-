using NovaLauncher.Views.Pages.ContentPages;
using System.Windows.Controls;
using static NovaLauncher.SocketManager.UserServerManagerSocket;

namespace NovaLauncher.Views.Controls
{
    /// <summary>
    /// Interaction logic for UserServerEditor.xaml
    /// </summary>
    public partial class UserServerEditor : UserControl
    {
		public string ServerID { get; set; }
		public ServerPage serverPage { get; set; }
		public UserServerEditor(ServerItemUpdateClass serverItemUpdate, ServerPage page)
        {
            InitializeComponent();
			ServerID = serverItemUpdate.server_id;
			serverPage = page;

			SetServerName(serverItemUpdate.server_title);
			SetServerPlaylist(serverItemUpdate.server_playlist);
			SetServerMaxPlayers(serverItemUpdate.server_max_players);
			SetServerMatchmakingKey(serverItemUpdate.custom_matchmaking_key);
			SetServerRegion(serverItemUpdate.server_region);
            SelectAutoStartGame(serverItemUpdate.auto_start_game);

        }

		public void SetServerName(string Title)
		{
			DetailsServerNameTxt.Text = Title;
		}

		public void SetServerPlaylist(string Playlist)
		{
			SelectPlaylist(Playlist);
		}

		public void SetServerRegion(string Region)
		{
			SelectRegion(Region);
		}

        public void SelectAutoStartGame(string itemContent)
        {
            foreach (ComboBoxItem comboBoxItem in AutoStartGameBox.Items)
            {
                if (comboBoxItem.Content.ToString() == itemContent)
                {
                    comboBoxItem.IsSelected = true;
                    break;
                }
            }
        }

        private void SelectPlaylist(string itemContent)
		{
			foreach (ComboBoxItem comboBoxItem in DetailsPlaylistTxt.Items)
			{
				if (comboBoxItem.Content.ToString() == itemContent)
				{
					comboBoxItem.IsSelected = true;
					break;
				}
			}
		}

		private void SelectRegion(string itemContent)
		{
			foreach (ComboBoxItem comboBoxItem in DetailsRegionTxt.Items)
			{
				if (comboBoxItem.Content.ToString() == itemContent)
				{
					comboBoxItem.IsSelected = true;
					break;
				}
			}
		}

		public void SetServerMaxPlayers(string Players)
		{
			if(int.TryParse(Players, out int Value))
				DetailsMaxPlayersTxt.Value = Value;
		}

		public void SetServerMatchmakingKey(string Key)
		{
			DetailsMatchmakingKeytxt.Text = Key;
		}
	}
}
