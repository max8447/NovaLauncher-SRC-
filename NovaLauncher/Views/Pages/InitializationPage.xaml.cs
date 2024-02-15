using NovaLauncher.Models;
using NovaLauncher.Models.API.NovaBackend;
using NovaLauncher.Models.ShortcutManager;
using NovaLauncher.Models.Assets;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Path = System.IO.Path;
using NovaLauncher.Models.Discord;
using NovaLauncher.Services;
using System.Diagnostics;
using System.Runtime.InteropServices;
using NovaLauncher.Models.Logger;
using System.Security.Cryptography;
using System.Security.Principal;
using NovaLauncher.Views.Popups;
using MaterialDesignThemes.Wpf;

namespace NovaLauncher.Views.Pages
{
    /// <summary>
    /// Interaction logic for InitializationPage.xaml
    /// </summary>
    public partial class InitializationPage : Page
    {
        public InitializationPage()
        {
            InitializeComponent();
            DiscordPresence.InitializeRPC();
        }

        private void Setup(object sender, RoutedEventArgs e)
        {
            LoadAsync();
        }
        private void SetStatus(string String)
        {
            statusText.Dispatcher.Invoke(() =>
            {
                statusText.Text = String;
            });
        }
        private async Task LoadAsync()
        {
            //if(!IsRunningAsAdmin())
            //{
            //    LoginCard.Visibility = Visibility.Collapsed;
            //    var ui = new NoAdmin();
            //    await uihost.ShowDialog(ui);
            //    return;
            //}

            Global.bNoMCP = await MCP();
            Global.bSkipVerify = await CheckForVerify();
            Global.bSkipACD = await CheckForbSkipACD();
            Global.bSkipAC = await CheckForbSkipAC();

            ShortcutManager.LoadShortcuts();
            await GatherAPIInfo();

            if (CheckForUpdateOnLaunch())
            {
                await UpdateLauncher();
                return;
            }

            await CheckForUpdates();
            await CheckForDevOnLaunch();
            await CheckAssetContent();
            DiscordPresence.SetState(DiscordPresence.RichPresenceEnum.LoggingIn, "");
            await HandleUserLogin();

        }
        static bool IsRunningAsAdmin()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        private async Task GatherAPIInfo()
        {
            Logger.Log(LogLevel.Info, "Gathering API information...");

            try
            {
                LauncherData.LauncherDataInfo = await LauncherAPI.GetLauncherInfoAsync(999, statusText);

                if (LauncherData.LauncherDataInfo == null)
                {
                    Logger.Log(LogLevel.Warning, "Failed to retrieve API information.");
                    SetStatus("Failed to contact services...");
                    return;
                }

                await Task.Delay(500);

                Logger.Log(LogLevel.Info, "API information gathering completed.");
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "An error occurred while gathering API information.", ex);
            }
        }
        public static bool CheckForUpdateOnLaunch()
        {
            Logger.Log(LogLevel.Info, "Checking for update on launch...");

            string[] args = Environment.GetCommandLineArgs();

            bool bUpdateClient = args.Any(arg => arg.Contains("-update"));

            if (bUpdateClient)
            {
                Logger.Log(LogLevel.Info, "Update required on launch. Client will be updated.");
                return true;
            }

            Logger.Log(LogLevel.Info, "No update required on launch. Client is up-to-date.");
            return false;
        }

        private async Task<bool> CheckForVerify()
        {
            string[] args = Environment.GetCommandLineArgs();

            bool bSkipVerify = false;
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-skipverify")
                {
                    bSkipVerify = true;
                }
            }

            if (bSkipVerify)
            {
                return true;
            }

            await Task.Delay(500);
            return false;
        }
        private async Task<bool> MCP()
        {
            string[] args = Environment.GetCommandLineArgs();

            bool bSkipVerify = false;
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-nomcp")
                {
                    bSkipVerify = true;
                }
            }

            if (bSkipVerify)
            {
                return true;
            }

            await Task.Delay(500);
            return false;
        }
        private async Task<bool> CheckForbSkipACD()
        {
            string[] args = Environment.GetCommandLineArgs();

            bool bSkipVerify = false;
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-skipacd")
                {
                    bSkipVerify = true;
                }
            }

            if (bSkipVerify)
            {
                return true;
            }

            await Task.Delay(500);
            return false;
        }

        private async Task<bool> CheckForbSkipAC()
        {
            string[] args = Environment.GetCommandLineArgs();

            bool bSkipVerify = false;
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-skipac")
                {
                    bSkipVerify = true;
                }
            }

            if (bSkipVerify)
            {
                return true;
            }

            await Task.Delay(500);
            return false;
        }
        private async Task<bool> CheckForbSkipProxy()
        {
            string[] args = Environment.GetCommandLineArgs();

            bool bSkipVerify = false;
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-skipproxy")
                {
                    bSkipVerify = true;
                }
            }

            if (bSkipVerify)
            {
                return true;
            }

            await Task.Delay(500);
            return false;
        }
        private async Task CheckForDevOnLaunch()
        {

            string[] args = Environment.GetCommandLineArgs();

            bool bUseDev = false;
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Contains("-dev"))
                {
                    bUseDev = true;
                }
            }

            await Task.Delay(500);
            Global.bDevMode = bUseDev;
        }
        private async Task<bool> CheckForUpdates()
        {
            Logger.Log(LogLevel.Info, "Checking for updates...");
            SetStatus("Checking for updates...");

            string[] args = Environment.GetCommandLineArgs();

            bool bSkipUpdate = false;
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Contains("skippatchcheck"))
                {
                    Logger.Log(LogLevel.Debug, "Skip update flag found in command-line arguments.");
                    bSkipUpdate = true;
                    Global.bSkipUpdate = true;
                }
            }

            if (bSkipUpdate)
            {
                Logger.Log(LogLevel.Info, "Skipping update check.");
                return true;
            }

            var LauncherVersion = LauncherData.GetLauncherVersion();

            if (string.IsNullOrEmpty(LauncherVersion))
            {
                Logger.Log(LogLevel.Error, "Failed to check for updates.");
                SetStatus("Failed to check for updates...");
                await Task.Delay(600);
                return true;
            }

            if (LauncherVersion != Global.GetCurrentLauncherVersion())
            {
                SetStatus("Update available...");
                await Task.Delay(600);
                SetStatus("Restarting Launcher...");
                await CloseFortniteProcesses(true, true);

                try
                {
                    var Account = LauncherData.GetGetUserInfo();
                    if (Account != null)
                        LauncherAPI.LogoutAsync(Account.access_token);
                }
                catch { }

                await Task.Delay(600);
                Environment.Exit(0);
            }
            Logger.Log(LogLevel.Info, "No updates available.");
            return false;
        }
        public async Task CloseFortniteProcesses(bool shouldClose, bool Restart)
        {
            try
            {
                if (shouldClose)
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
                }

                if(Restart)
                {
                    string[] launchArgs = Environment.GetCommandLineArgs();
                    string[] arguments = launchArgs.Skip(1).ToArray();

                    if (arguments != null && arguments.Length > 0)
                    {
                        if (LauncherData.MainContentWindow != null)
                        {
                            LauncherData.MainContentWindow.WPFUIHost?.Hide();
                        }
                    }

                    string restartArgs = string.Join(" ", arguments) + " -update";

                    string appPath = Process.GetCurrentProcess().MainModule.FileName;
                    Process.Start(appPath, restartArgs);

                    try
                    {
                        var Account = LauncherData.GetGetUserInfo();
                        if (Account != null)
                            await LauncherAPI.LogoutAsync(Account.access_token);
                    }
                    catch
                    {

                    }
                }
            }
            catch (Exception ex)
            { 
            }
        }

        private async Task CheckAssetContent()
        {
            Logger.Log(LogLevel.Info, "Loading Content Assets...");
            SetStatus("Loading Content Assets...");

            if(!Global.bNoMCP)
            {
                var Json = await LauncherAPI.GetAssetsAsObjectAsync();

                if (Json == null)
                {
                    Logger.Log(LogLevel.Error, "Failed to load Assets.");
                    SetStatus("Failed to load Assets...");
                    return;
                }

                Logger.Log(LogLevel.Info, "Downloading images...");
                await AssetManager.DownloadImagesAsync(Json, statusText);
            }
        }

        private async Task HandleUserLogin()
        {
            Logger.Log(LogLevel.Info, "Handling user login...");

            string[] args = Environment.GetCommandLineArgs();

            bool bSkipLogin = false;
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Contains("-nomcp"))
                {
                    Logger.Log(LogLevel.Debug, "Skip login flag found in command-line arguments.");
                    bSkipLogin = true;
                }
            }

            if (bSkipLogin)
            {
                Logger.Log(LogLevel.Info, "Skipping user login.");
                Global.bNoMCP = true;
                await Application.Current.Dispatcher.Invoke(async () =>
                {
                    await HostService._host.StartAsync();
                    LauncherData.MainWindowView.Close();
                });
                return;
            }

            try
            {
                Logger.Log(LogLevel.Info, "Navigating to UserLoginPage.");
                UserLoginPage loginPage = new UserLoginPage();
                LauncherData.MainWindowView.GetFrame().Navigate(loginPage);
                return;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "An error occurred while handling user login: " + ex.Message);
                UserLoginPage loginPage = new UserLoginPage();
                LauncherData.MainWindowView.GetFrame().Navigate(loginPage);
                return;
            }
        }
        private async Task UpdateLauncher()
        {
            try
            {
                Logger.Log(LogLevel.Info, "Preparing update...");
                SetStatus("Preparing update...");
                await Task.Delay(1000);

                await CloseFortniteProcesses(true, false);
                string localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string novaUpdaterPath = Path.Combine(localAppDataPath, "Nova", "NovaUpdater.exe");

                KillProcessesByNameAndPath("NovaUpdater.exe", Directory.GetCurrentDirectory());

                if (!Directory.Exists(Path.GetDirectoryName(novaUpdaterPath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(novaUpdaterPath));
                }

                const int maxRetries = 3;
                int retryCount = 0;
                bool downloadSuccessful = false;

                while (retryCount < maxRetries && !downloadSuccessful)
                {
                    try
                    {
                        using (WebClient client = new WebClient())
                        {
                            Logger.Log(LogLevel.Info, $"Downloading NovaUpdater - Attempt {retryCount + 1}...");
                            await client.DownloadFileTaskAsync(LauncherData.LauncherDataInfo["Installer"].ToString(), novaUpdaterPath);
                            Logger.Log(LogLevel.Info, "File downloaded");
                            downloadSuccessful = true;
                        }
                    }
                    catch (WebException ex)
                    {
                        Logger.Log(LogLevel.Warning, $"Download attempt {retryCount + 1} failed: {ex.Message}");
                        retryCount++;
                        await Task.Delay(TimeSpan.FromSeconds(5));
                    }
                }

                if (downloadSuccessful)
                {
                    Logger.Log(LogLevel.Info, "Launching NovaUpdater for update...");
                    Process process = new Process();
                    process.StartInfo.FileName = novaUpdaterPath;
                    process.StartInfo.WorkingDirectory = Path.GetDirectoryName(novaUpdaterPath);
                    process.StartInfo.Arguments = "-update";
                    process.Start();

                    try
                    {
                        var Account = LauncherData.GetGetUserInfo();
                        if (Account != null)
                            await LauncherAPI.LogoutAsync(Account.access_token);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Warning, $"Error during logout: {ex.Message}");
                    }

                    Environment.Exit(0);
                }
                else
                {
                    Logger.Log(LogLevel.Error, "Failed to download NovaUpdater after multiple attempts.");
                    MessageBox.Show("Failed to download NovaUpdater after multiple attempts.");
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "An error occurred while updating the launcher: " + ex.Message);
            }
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetModuleFileName(IntPtr handle, StringBuilder buffer, int length);
        private void KillProcessesByNameAndPath(string processName, string processPath)
        {
            Process[] processes = Process.GetProcessesByName(processName);
            foreach (Process process in processes)
            {
                try
                {
                    StringBuilder buffer = new StringBuilder(1024);
                    int length = GetModuleFileName(process.Handle, buffer, buffer.Capacity);
                    string filePath = buffer.ToString(0, length);

                    FileInfo fileInfo = new FileInfo(filePath);
                    if (fileInfo.DirectoryName.Equals(processPath))
                    {
                        process.Kill();
                    }
                }
                catch
                {
                    continue;
                }
            }
        }
    }
}
