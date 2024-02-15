using Newtonsoft.Json.Linq;
using NovaLauncher.Models.AccountUtils.Classes;
using NovaLauncher.Models.API.NovaBackend.Classes;
using NovaLauncher.Views.Controls;
using NovaLauncher.Views.Pages.ContentPages;
using NovaLauncher.Views.Windows;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using Wpf.Ui.Controls;

namespace NovaLauncher.Models
{
    public class Global
    {
        public static string LauncherVersion = "1.7.1";
        public static bool bNoMCP = false;
        public static bool bSkipUpdate = true;
        public static bool bSkipVerify = false;
        public static bool bSkipACD = false;
        public static bool bSkipAC = false;
        public static bool bDevMode = false;

        public static string GetCurrentLauncherVersion()
        {
            return LauncherVersion;
        }

    }
    public static class LauncherData
    {
        public static MainWindow MainWindowView { get; set; }
        public static ContentWindow MainContentWindow { get; set; }
        public static string LauncherAPIUrl
        {
            get
            {
                return "https://launcher.novafn.dev";
            }
            set
            {

            }
        }


        public static string LauncherAPIPort = "80";
        public static JObject LauncherDataInfo { get; set; }
        public static JObject AssetData { get; set; }
        public static User UserLoggedIn { get; set; }
        public static StackPanel DownloadOptionsStack { get; set; }
        public static DownloadPage DownloadPage { get; set; }
        public static string GetLauncherVersion()
        {
            if (LauncherDataInfo != null && LauncherDataInfo.ContainsKey("LauncherVersion"))
                return LauncherDataInfo["LauncherVersion"].ToString();
            else
                return string.Empty;
        }


        public static Account GetGetUserInfo()
        {
            if (Global.bNoMCP)
            {
                if (AccountUtils.AccountUtils.MyLauncherAccount == null)
                {
                    Account response = new Account();
                    response.account_id = "999";
                    response.access_token = "";
                    AccountUtils.AccountUtils.MyLauncherAccount = response;
                }
                
            }

            return AccountUtils.AccountUtils.MyLauncherAccount;
        }
    }
}
