using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NovaLauncher.Models;
using NovaLauncher.Models.API.NovaBackend;
using NovaLauncher.Models.GameSaveManager;
using NovaLauncher.Models.Logger;
using NovaLauncher.Models.NovaMessageBox;
using NovaLauncher.Models.WebSocket;
using NovaLauncher.Views.Controls;
using NovaLauncher.Views.Pages.ContentPages;
using RestSharp;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;

namespace NovaLauncher.SocketManager
{
    public class SocketManager
    {
        public static SignalRClient scoketClient;
        public TextBlock VersionText;
        public JObject LauncherInfo;
        public StackPanel NewsContent_;
        public Border ScrollButtonControl;

        public void Init()
        {
            scoketClient = new SignalRClient();
            scoketClient.OnMessageReceived += HandleMessageReceived;
            string User = string.Empty;
            ConnectToSignalRServer();
        }

        private async void ConnectToSignalRServer()
        {
            string connectionUrl = $"{NovaLauncher.Models.LauncherData.LauncherAPIUrl}:{NovaLauncher.Models.LauncherData.LauncherAPIPort}/launcher?user={HttpUtility.UrlEncode(NovaLauncher.Models.LauncherData.GetGetUserInfo().access_token)}";
            await scoketClient.Connect(connectionUrl);
            await StartHeartbeat();
        }
        public static void Restart()
        {
            try
            {
                string currentProcessPath = Process.GetCurrentProcess().MainModule.FileName;
                Process.Start(currentProcessPath);
                Process.GetCurrentProcess().Kill();
            }
            catch { }
        }
        public static async Task StartHeartbeat()
        {
            while (true)
            {
                var response = await CreateVerifyRequestAsync(LauncherData.GetGetUserInfo().access_token);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    NovaMessageBox novaMessageBox = new NovaMessageBox();
                    RoutedEventHandler RightButtonClick = null;
                    RightButtonClick += (sender, args) =>
                    {
                        Restart();
                    };
                    await novaMessageBox.ShowMessageAsync("Looks like you've been logged out.", "Restart", RightButtonClick, RightButtonClick, "", true, "", false);
                    break;
                }

                await Task.Delay(TimeSpan.FromMinutes(5));
            }
        }

        private static async Task<RestResponse> CreateVerifyRequestAsync(string token)
        {
            var client = new RestClient($"{LauncherData.LauncherAPIUrl}:{LauncherData.LauncherAPIPort}");
            var request = new RestRequest("/account/api/oauth/verify", Method.Get);

            request.Timeout = 10000;
            request.AddHeader("Authorization", $"basic {token}");
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");

            try
            {
                return await client.ExecuteAsync(request);
            }
            catch (Exception ex)
            {
                return new RestResponse { StatusCode = HttpStatusCode.InternalServerError };
            }
        }



        private async void HandleMessageReceived(string user, string Incommingmessage)
        {
            var NotiConvert = GetNotification(Incommingmessage);
            if (Incommingmessage.Contains("LauncherVersion") && Incommingmessage.Contains("NewsId"))
            {
                HandleLauncherUpdate(Incommingmessage);
            }
            else if (NotiConvert != null)
            {
                await ShowMessageAsync(NotiConvert.Title, NotiConvert.Content);
            }
            else if (BanCheck(Incommingmessage))
            {
                var gameList = GameList.LoadFromFile(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Nova", "Game", "Installed", "Version.json"));
                foreach (Game game in gameList.Games)
                {
                    bool isBuild740 = game.Name.Equals("Build 7.40", StringComparison.OrdinalIgnoreCase) || game.Name.Equals("Season 7.40", StringComparison.OrdinalIgnoreCase);

                    if (isBuild740)
                    {
                        if (Directory.Exists(game.GamePath))
                        {
                            try
                            {
                                var Process = GetRunningProcess($@"{game.GamePath}\FortniteGame\Binaries\Win64\FortniteClient-Win64-Shipping.exe");
                                if (Process != null)
                                {
                                    Process.Kill();
                                    Process.WaitForExit();

                                }
                                await Task.Delay(300);

                                await Task.Run(() =>
                                {
                                    Directory.Delete(game.GamePath, true);
                                });
                            }
                            catch (Exception ex) { continue; }
                        }
                    }
                }
            }
            else if(Incommingmessage.Contains("USER UPDATE SETTINGS {SETTINGS}"))
            {
                if(SettingsPage.settingsPage != null)
                {
                    SettingsPage.settingsPage.LoadUserSettingsAsync();
                }
            }
                
        }
        static Process GetRunningProcess(string exePath)
        {
            Process[] processes = Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(exePath));

            foreach (Process process in processes)
            {
                if (process.MainModule.FileName.Equals(exePath, StringComparison.OrdinalIgnoreCase))
                {
                    return process;
                }
            }

            return null;
        }
        private async Task ShowMessageAsync(string title, string content)
        {
            content = Environment.NewLine + content;
            await System.Windows.Application.Current.Dispatcher.InvokeAsync(async () =>
            {
                NovaLauncher.Models.NovaMessageBox.NovaMessageBox messageBox = new NovaLauncher.Models.NovaMessageBox.NovaMessageBox();
                RoutedEventHandler ButtonLeftClick = null;
                ButtonLeftClick += (sender, args) =>
                {
                    LauncherData.MainContentWindow.WPFUIHost.Hide();
                };
                RoutedEventHandler RightButtonClick = null;
                RightButtonClick += (sender, args) =>
                {
                    LauncherData.MainContentWindow.WPFUIHost.Hide();
                };
                await messageBox.ShowMessageAsync(content, "", RightButtonClick, ButtonLeftClick, title, false);
            });
        }
        private Message GetNotification(string message)
        {
            try
            {
                if(message.Contains("Title") && message.Contains("Content"))
                {
                    Message receivedMessage = JsonConvert.DeserializeObject<Message>(message);
                    return receivedMessage;
                }

                return null;
            }
            catch (Exception ex) { return null; }
        }
        private bool BanCheck(string message)
        {
            try
            {
                BanNoti receivedMessage = JsonConvert.DeserializeObject<BanNoti>(message);

                if(receivedMessage == null)
                    return false;

                string CurrentUser = LauncherData.GetGetUserInfo().displayName.ToLower();
                if (CurrentUser == receivedMessage.Username.ToLower() && receivedMessage.bBanUser)
                    return true;

                return false;
            }
            catch (Exception ex) 
            { 
                return false;
            }
        }
        public class Message
        {
            public string Title { get; set; }
            public string Content { get; set; }
        }
        private void HandleLauncherUpdate(string message)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    LauncherData.MainContentWindow.Navigate(typeof(Views.Pages.ContentPages.NewsPage));
                });

                JObject LauncherInfo = JObject.Parse(message);
                LoadContent(VersionText, LauncherInfo, NewsContent_, ScrollButtonControl);
                if ((string)LauncherInfo["LauncherVersion"] != Global.GetCurrentLauncherVersion())
                {
                    var launcherVersion = LauncherData.GetLauncherVersion();

                    if (string.IsNullOrEmpty(launcherVersion))
                    {
                        return;
                    }

                    if (launcherVersion != Global.GetCurrentLauncherVersion())
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            NovaLauncher.Models.NovaMessageBox.NovaMessageBox messageBox = new NovaLauncher.Models.NovaMessageBox.NovaMessageBox();

                            RoutedEventHandler RightButtonClick = null;
                            RightButtonClick += (sender, args) =>
                            {
                                LauncherData.MainContentWindow.WPFUIHost.Hide();
                            };

                            RoutedEventHandler ButtonLeftClick = null;
                            ButtonLeftClick += (sender, args) =>
                            {
                                CloseFortniteProcessesAsync();
                                LauncherData.MainContentWindow.WPFUIHost.Hide();
                            };

                            messageBox.ShowMessageAsync("It appears that the launcher is currently running on an outdated version and requires an update to launch your game.", "Update", RightButtonClick, ButtonLeftClick);
                        });

                        return;
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public async Task CloseFortniteProcessesAsync()
        {
            string[] processNames = { "FortniteClient-Win64-Shipping", "FortniteClient-Win64-Shipping_BE", "FortniteLauncher" };

            foreach (string processName in processNames)
            {
                Process[] processes = Process.GetProcessesByName(processName);
                foreach (Process process in processes)
                {
                    try
                    {
                        await Task.Run(() =>
                        {
                            process.CloseMainWindow();
                            if (!process.WaitForExit(5000))
                            {
                                process.Kill();
                            }
                        });
                    }
                    catch
                    {


                    }
                }
            }

            string[] Launchargs = Environment.GetCommandLineArgs();
            string[] arguments = Launchargs.Skip(1).ToArray();

            if (arguments == null || arguments.Length > 0)
            {
                LauncherData.MainContentWindow.WPFUIHost.Hide();
            }

            string RestartArgs = string.Join(" ", arguments) + " -update";

            string appPath = Process.GetCurrentProcess().MainModule.FileName;
            try
            {
                var Account = LauncherData.GetGetUserInfo();
                if (Account != null)
                    LauncherAPI.LogoutAsync(Account.access_token);
            }
            catch { }
            Process.Start(appPath, RestartArgs);
            Environment.Exit(0);
        }
        public static void LoadContent(TextBlock VersionText, JObject LauncherInfo, StackPanel NewsContent_, Border ScrollButtonControl)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                LauncherData.LauncherDataInfo = LauncherInfo;
                VersionText.Text = $"V{Global.GetCurrentLauncherVersion()}";
                JArray launcherUpdates = (JArray)LauncherInfo["News"]["LauncherUpdates"];
                if (NewsContent_ != null)
                {
                    for (int i = NewsContent_.Children.Count - 1; i >= 0; i--)
                    {
                        if (i != 0)
                        {
                            UIElement child = NewsContent_.Children[i];
                            NewsContent_.Children.Remove(child);
                        }
                    }
                }

                foreach (JObject update in launcherUpdates)
                {
                    string title = (string)update["title"];
                    string description = (string)update["description"];
                    string url = (string)update["url"];
                    NewsOption newsOption = new NewsOption(title, description, url);
                    NewsContent_.Children.Add(newsOption);
                }

                ScrollButtonControl.Visibility = Visibility.Hidden;

                Logger.Log(LogLevel.Info, "Loaded news content.");
            });
        }
    }
    public class BanNoti
    {
        public string Username { get; set; }
        public bool bBanUser { get; set; }

        public BanNoti(string username, bool banUser)
        {
            Username = username;
            bBanUser = banUser;
        }
    }
}
