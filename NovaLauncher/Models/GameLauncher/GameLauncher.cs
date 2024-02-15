using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NovaLauncher.Models.Controls;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using static NovaLauncher.Models.Global;
using NovaLauncher.Models.GameSaveManager;
using NovaLauncher.Models.API.NovaBackend;
using NovaLauncher.Models.Discord;
using NovaLauncher.Models.Tools;
using System.Net;
using NovaLauncher.Models.Logger;
using System.Security.Cryptography;
using System.Threading;
using Path = System.IO.Path;
using MessageBox = System.Windows.MessageBox;
using Installer;
using System.Security.Cryptography.X509Certificates;
using System.Management;
using RestSharp;
using System.Windows.Input;
using Microsoft.Win32.TaskScheduler;
using System.ServiceProcess;
using Task = System.Threading.Tasks.Task;
using System.Runtime.InteropServices;
using System.Windows.Navigation;
using System.Text.RegularExpressions;

namespace NovaLauncher.Models.GameLauncher
{
    public static class GameLauncher
    {
        private static string? gameListFilePath;
        private static Thread? checkGameTask;

        static GameLauncher()
        {
            gameListFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Nova", "Game", "Installed", "Version.json");
        }

        public static async Task<List<string>> GetSettingsFromApiAsync()
        {
            var client = new RestClient($"{LauncherData.LauncherAPIUrl}:{LauncherData.LauncherAPIPort}");
            var request = new RestRequest($"/api/launcher/{LauncherData.GetGetUserInfo().access_token}/Settings", Method.Get);
            request.AddHeader("Content-Type", "application/json");

            var response = await client.ExecuteAsync(request);

            if (response.IsSuccessful)
            {
                try
                {
                    var settingsList = JsonConvert.DeserializeObject<List<string>>(response.Content);
                    return settingsList;
                }
                catch (Exception ex)
                {
                    Logger.Logger.Log(LogLevel.Error, ex.Message);
                    return new List<string>();
                }
            }
            else
            {
                return new List<string>();
            }
        }
        public static bool ByteArrayContains(byte[] source, params byte[][] patterns)
        {
            foreach (byte[] pattern in patterns)
            {
                for (int i = 0; i <= source.Length - pattern.Length; i++)
                {
                    if (source.Skip(i).Take(pattern.Length).SequenceEqual(pattern))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        public static void AddGame(string name, string path)
        {
            try
            {
                Logger.Logger.Log(LogLevel.Info, "Adding game to the game list...");

                var gameList = GameList.LoadFromFile(gameListFilePath);
                Game game = new Game
                {
                    Name = name,
                    GamePath = path,
                    DatePlayed = DateTime.MinValue
                };

                gameList.AddGame(game);
                gameList.SaveToFile(gameListFilePath);

                Logger.Logger.Log(LogLevel.Info, "Game added successfully.");
            }
            catch (Exception ex)
            {
                Logger.Logger.Log(LogLevel.Error, $"Failed to add game to the game list: {ex}");
                throw;
            }
        }
        public static void RemoveGame(Game game)
        {
            Logger.Logger.Log(LogLevel.Info, "Removing game from the game list...");

            var gameList = GameList.LoadFromFile(gameListFilePath);

            gameList.RemoveGame(game);
            gameList.SaveToFile(gameListFilePath);

            Logger.Logger.Log(LogLevel.Info, "Game removed successfully.");
        }
        public static async Task<bool> AreAnyGamesRunningAsync()
        {
            Logger.Logger.Log(LogLevel.Info, "Checking if any games are running...");

            string processName = "FortniteClient-Win64-Shipping";
            Process[] processes = Process.GetProcesses();
            foreach (Process process in processes)
            {
                try
                {
                    if (process.ProcessName.Equals(processName, StringComparison.OrdinalIgnoreCase))
                    {
                        Logger.Logger.Log(LogLevel.Info, "Games are running.");
                        return true;
                    }
                }
                catch
                {
                    continue;
                }
            }

            Logger.Logger.Log(LogLevel.Info, "No games are running.");
            return false;
        }
        public static async Task<ACSeasonData> DownloadContent()
        {
            Logger.Logger.Log(LogLevel.Info, "Downloading content...");

            var ACContent = await LauncherAPI.GetLauncherInfoAsyncACAsync();
            if (ACContent == null)
            {
                await Task.Delay(1000);
                MessageBox.Show("Failed to connect to services");
            }

            var downloadedContent = await Task.Run(async () =>
            {
                string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string novaPath = Path.Combine(localAppData, "nova");

                string console = System.IO.Path.Combine(novaPath, "console.dll");

                try
                {
                    if (File.Exists(console))
                    {
                        if (CalculateFileHash(console) == ACContent.fileHash)
                        {

                        }
                        else
                        {
                            File.Delete(console);
                            await DownloadFileAsync(ACContent.url, console);
                        }
                    }
                    else
                    {
                        await DownloadFileAsync(ACContent.url, console);
                    }

                    return ACContent;
                }
                catch (Exception ex)
                {
                    Logger.Logger.Log(LogLevel.Error, $"Failed to delete console.dll: {ex}");
                }

                return null;
            });

            Logger.Logger.Log(LogLevel.Info, "Content download completed.");

            return downloadedContent;
        }
        public static string CalculateFileHash(string filePath)
        {
            using (var sha256 = SHA256.Create())
            {
                using (var fileStream = File.OpenRead(filePath))
                {
                    byte[] hashBytes = sha256.ComputeHash(fileStream);
                    return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                }
            }
        }
        public static async Task DownloadFileAsync(string url, string path)
        {
            try
            {
                using (var client = new WebClient())
                {
                    await client.DownloadFileTaskAsync(url, path);
                    Logger.Logger.Log(LogLevel.Info, "File downloaded successfully.");
                }
            }
            catch (Exception ex)
            {
                Logger.Logger.Log(LogLevel.Error, ex.Message);
            }
        }
        public static bool VerifyChunkHash(string filePath, string expectedHash)
        {
            using (var stream = File.OpenRead(filePath))
            {
                using (var sha1 = SHA1.Create())
                {
                    byte[] hashBytes = sha1.ComputeHash(stream);
                    string actualHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

                    return actualHash == expectedHash;
                }
            }
        }
        public static bool AreGamesRunning()
        {
            var gameList = GameList.LoadFromFile(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Nova", "Game", "Installed", "Version.json"));
            if (gameList == null)
                return false;

            foreach (var game in gameList.Games)
            {
                try
                {
                    Process proc = Process.GetProcessById(game.GameID);
                    if (proc.ProcessName.Contains("Fortnite"))
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Logger.Log(LogLevel.Error, ex.Message);
                    continue;
                }
            }

            return false;

        }

        public static void KillNovaGames()
        {
            Logger.Logger.Log(LogLevel.Info, "Killing all running games...");

            var gameList = GameList.LoadFromFile(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Nova", "Game", "Installed", "Version.json"));
            if (gameList == null)
                return;

            foreach (var game in gameList.Games)
            {
                try
                {
                    Process proc = Process.GetProcessById(game.GameID);
                    if (proc.ProcessName.Contains("Fortnite"))
                    {
                        proc.Kill();
                    }
                }
                catch (Exception ex)
                {
                    Logger.Logger.Log(LogLevel.Info, $"Error when killing processes: {ex.Message}.");
                    continue;
                }
            }

            Logger.Logger.Log(LogLevel.Info, "Finished killing processes.");
        }

        public static bool bLaunchInProgress = false;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD101:Avoid unsupported async delegates", Justification = "<Pending>")]
        public static async Task LaunchGameAsync(Game game, TextBlock statusText)
        {
            Logger.Logger.Log(LogLevel.Info, "Starting LaunchGameAsync");

            var ui = SynchronizationContext.Current;
            Logger.Logger.Log(LogLevel.Info, "Synchronization context obtained");

            if (bLaunchInProgress)
            {
                Logger.Logger.Log(LogLevel.Error, "You are trying to launch another version while a separate game is already launching");
            }

            bLaunchInProgress = true;
            Logger.Logger.Log(LogLevel.Info, "Launch in progress");

            var GameRunning = AreGamesRunning();
            if (GameRunning)
            {
                Logger.Logger.Log(LogLevel.Info, "Another build is already running");

                NovaLauncher.Models.NovaMessageBox.NovaMessageBox messageBox = new NovaLauncher.Models.NovaMessageBox.NovaMessageBox();
                RoutedEventHandler RightButtonClick = null;
                RightButtonClick += (sender, args) =>
                {
                    bLaunchInProgress = false;
                    LauncherData.MainContentWindow.WPFUIHost.Hide();
                };

                RoutedEventHandler ButtonLeftClick = null;
                ButtonLeftClick += (sender, args) =>
                {
                    KillNovaGames();
                    bLaunchInProgress = false;
                    LauncherData.MainContentWindow.WPFUIHost.Hide();
                };

                await messageBox.ShowMessageAsync("You can only have one instance of Fortnite running at once, you can \"Exit\" to close the currently running game.",
                "Exit", RightButtonClick, ButtonLeftClick
                );
            }

            var APIVersion = LauncherData.GetLauncherVersion();

            if (string.IsNullOrEmpty(APIVersion))
            {
                bLaunchInProgress = false;
                Logger.Logger.Log(LogLevel.Info, "Invalid launcher version");
            }

            Logger.Logger.Log(LogLevel.Info, $"API version: {APIVersion}");

            if (bLaunchInProgress == false)
                return;

            if (APIVersion != Global.GetCurrentLauncherVersion())
            {
                bLaunchInProgress = false;
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
                        CloseFortniteProcesses();
                        LauncherData.MainContentWindow.WPFUIHost.Hide();
                    };

                    messageBox.ShowMessageAsync("It appears that the launcher is currently running on an outdated version and requires an update to launch your game.", "Update", RightButtonClick, ButtonLeftClick);
                });

                Logger.Logger.Log(LogLevel.Info, "Launcher version outdated");
                return;
            }

            if (bLaunchInProgress == false)
                return;

            Logger.Logger.Log(LogLevel.Info, "Launcher version up-to-date");

            Logger.Logger.Log(LogLevel.Info, $"Launching game: {game.Name}");
            GameOption.SetButtonState(statusText, 3);

            var Build = await LauncherAPI.BuildVerifyEndpointAsync(game.Name);
            Logger.Logger.Log(LogLevel.Info, "Build verified");

            if(!Global.bSkipACD)
            {
                var P = await PresidioManager.VerifyAsync();
                if (!P.Item1)
                {
                    bLaunchInProgress = false;
                    NovaLauncher.Models.NovaMessageBox.NovaMessageBox messageBox = new NovaLauncher.Models.NovaMessageBox.NovaMessageBox();
                    RoutedEventHandler RightButtonClick = null;
                    RightButtonClick += (sender, args) =>
                    {
                        LauncherData.MainContentWindow.WPFUIHost.Hide();
                    };

                    RoutedEventHandler ButtonLeftClick = null;
                    ButtonLeftClick += (sender, args) =>
                    {
                        LauncherData.MainContentWindow.WPFUIHost.Hide();
                    };
                    await messageBox.ShowMessageAsync(P.Item2,
                    "Ok", RightButtonClick, ButtonLeftClick);
                }
            }

            if (bLaunchInProgress == false)
            {
                GameOption.SetButtonState(statusText, 0);
                return;
            }

            Logger.Logger.Log(LogLevel.Info, "Anticheat verified");

            if (!bSkipVerify)
                await VerifyGameFiles(Build, statusText, game, ui);

            Logger.Logger.Log(LogLevel.Info, "Game files verified");

            if (bLaunchInProgress == false)
            {
                GameOption.SetButtonState(statusText, 0);
                return;
            }

            var gameList = GameList.LoadFromFile(gameListFilePath);

            try
            {
                await FullGameVerify(Build, statusText, game, ui);
                Logger.Logger.Log(LogLevel.Info, "Full game verify completed");

                LauncherData.MainContentWindow.HidePopup();

                if (!bLaunchInProgress)
                {
                    GameOption.SetButtonState(statusText, 0);
                    return;
                }

                GameOption.SetButtonState(statusText, 5);
                await Task.Delay(500);

                var TokenAPIGet = await LauncherAPI.GetETokenAsync(LauncherData.GetGetUserInfo().access_token);
                bool bFailedToLogin = false;

                if (TokenAPIGet.Success == false)
                {
                    LauncherData.MainContentWindow.HidePopup();

                    NovaLauncher.Models.NovaMessageBox.NovaMessageBox messageBox = new NovaLauncher.Models.NovaMessageBox.NovaMessageBox();
                    RoutedEventHandler ButtonLeftClick = null;
                    ButtonLeftClick += (sender, args) =>
                    {
                        bFailedToLogin = true;
                        LauncherData.MainContentWindow.WPFUIHost.Hide();
                    };
                    RoutedEventHandler RightButtonClick = null;
                    RightButtonClick += (sender, args) =>
                    {
                        bFailedToLogin = true;
                        LauncherData.MainContentWindow.WPFUIHost.Hide();
                    };
                    await messageBox.ShowMessageAsync(TokenAPIGet.Error, "OK", RightButtonClick, ButtonLeftClick);
                }

                if (bFailedToLogin)
                {
                    bLaunchInProgress = false;
                    GameOption.SetButtonState(statusText, 0);
                    return;
                }

                List<string> settings = await GetSettingsFromApiAsync();

                string Args = $" -epicapp=Fortnite  -epicenv=Prod -epicportal -AUTH_TYPE=exchangecode -AUTH_LOGIN=none -AUTH_PASSWORD={TokenAPIGet.Token} -epiclocale=en-us -p={TokenAPIGet.p} -fltoken=7a848a93a74ba68876c36C1c -fromfl=none -noeac -nobe -skippatchcheck -cl={NumberToOBV(Process.GetCurrentProcess().Id)}";

                if (IsEOR())
                    Args += " -eor";

                if (IsInstantReset())
                    Args += " -instantreset";
                
                if (InjectMEMFix())
                    Args += " -memfix";
                
                if (SBD(game))
                    Args += " -sprintbydefault";

                //var Agent = await PresidioManager.StartAgent();
                //if (!Agent.Item1)
                //{
                //    NovaLauncher.Models.NovaMessageBox.NovaMessageBox messageBox = new NovaLauncher.Models.NovaMessageBox.NovaMessageBox();
                //    RoutedEventHandler RightButtonClick = null;
                //    RightButtonClick += (sender, args) =>
                //    {
                //        LauncherData.MainContentWindow.WPFUIHost.Hide();
                //    };

                //    RoutedEventHandler ButtonLeftClick = null;
                //    ButtonLeftClick += (sender, args) =>
                //    {
                //        LauncherData.MainContentWindow.WPFUIHost.Hide();
                //    };
                //    await messageBox.ShowMessageAsync("Presidio could not load. If you encounter this error again, please reach out to our support team on our Discord server.",
                //    "Ok", RightButtonClick, ButtonLeftClick);
                //    bLaunchInProgress = false;
                //    GameOption.SetButtonState(statusText, 0);
                //    return;
                //}

                var process = await LaunchShipping(game, Args, new Process());

                //await WaitForMemorySizeAsync(process);

                //if (process == null || Agent.Item2.HasExited == true)
                //{
                //    try
                //    {
                //        process.Kill();
                //    }
                //    catch { }

                //    try
                //    {
                //        Agent.Item2.Kill();
                //    }
                //    catch { }

                //    NovaLauncher.Models.NovaMessageBox.NovaMessageBox messageBox = new NovaLauncher.Models.NovaMessageBox.NovaMessageBox();
                //    RoutedEventHandler RightButtonClick = null;
                //    RightButtonClick += (sender, args) =>
                //    {
                //        LauncherData.MainContentWindow.WPFUIHost.Hide();
                //    };

                //    RoutedEventHandler ButtonLeftClick = null;
                //    ButtonLeftClick += (sender, args) =>
                //    {
                //        LauncherData.MainContentWindow.WPFUIHost.Hide();
                //    };
                //    await messageBox.ShowMessageAsync("Looks like the game failed to start. if you encounter this error again, please reach out to our support team on our Discord server.",
                //    "Ok", RightButtonClick, ButtonLeftClick);
                //    bLaunchInProgress = false;
                //    GameOption.SetButtonState(statusText, 0);
                //    return;
                //}

                process.Exited += (sender, args) =>
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        OnGameExited((Process)sender, statusText);
                    });
                };

                process.EnableRaisingEvents = true;
                bLaunchInProgress = false;
                LauncherData.MainContentWindow.HidePopup();

                var existingGame = gameList.Games.FirstOrDefault(g => g.Name == game.Name);
                if (existingGame != null)
                {
                    existingGame.GamePath = game.GamePath;
                    existingGame.DatePlayed = DateTime.Now;
                    existingGame.LaunchCount++;
                    existingGame.GameID = process.Id;
                }
                else
                {
                    gameList.AddGame(game);
                }

                gameList.SaveToFile(gameListFilePath, true);

                GameOption.SetButtonState(statusText, 2);

                await WaitForGameToStartAsync(process, game, new Process());

                if (process.HasExited)
                {
                    GameOption.SetButtonState(statusText, 0);
                    game.GameID = 0;
                    gameList.SaveToFile(gameListFilePath);
                    return;
                }

                GameOption.SetButtonState(statusText, 1);

                Logger.Logger.Log(LogLevel.Info, "Game launched successfully.");
            }
            catch (Exception ex)
            {
                bLaunchInProgress = false;
                Logger.Logger.Log(LogLevel.Error, $"Failed to launch game: {ex}");
                MessageBox.Show(ex.ToString());
            }

            Logger.Logger.Log(LogLevel.Info, "LaunchGameAsync completed");
        }

        public static async Task WaitForMemorySizeAsync(Process process)
        {
            if (process == null)
            {
                throw new ArgumentNullException(nameof(process));
            }

            try
            {
                string filePath = process.MainModule.FileName;
                long fileSize = new FileInfo(filePath).Length;

                while (true)
                {
                    process.Refresh();

                    if (process.HasExited)
                    {
                        break;
                    }

                    if (process.PrivateMemorySize64 >= fileSize)
                    {
                        break;
                    }

                    await Task.Delay(1000);
                }
            }
            catch (Exception ex)
            {

            }
        }

        public static async Task<Process> LaunchShipping(Game game, string args, Process Agentprocess)
        {
            Process process = new Process();
            process.StartInfo.FileName = $@"{game.GamePath}\FortniteGame\Binaries\Win64\FortniteClient-Win64-Shipping.exe";
            process.StartInfo.Arguments = args;

            process.Start();
            return process;
        }

        public static void StartTaskSchedulerService()
        {
            string serviceName = "Schedule";

            ServiceController sc = new ServiceController(serviceName);

            try
            {
                if (sc.Status != ServiceControllerStatus.Running)
                {
                    sc.Start();
                    sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(30));
                }
            }
            catch (Exception ex)
            {
                Logger.Logger.Log(LogLevel.Error, ex.Message);
            }
        }

        public static void MakeServiceKnown()
        {
            ServiceController schedulerService = new ServiceController("Schedule");

            if (schedulerService.Status != ServiceControllerStatus.Running)
            {
                try
                {
                    schedulerService.Start();
                    schedulerService.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(30));
                }
                catch (Exception ex)
                {
                    Logger.Logger.Log(LogLevel.Error, ex.Message);
                }
            }
        }

        public static async Task<Process?> LaunchGame(string GamePath, string WorkingDirectory, string Args)
        {
            MakeServiceKnown();

            string taskName = $"NovaLauncher";
            using (TaskService ts = new TaskService())
            {
                TaskDefinition td;

                var existingTask = ts.GetTask(taskName);
                MakeServiceKnown();

                if (existingTask == null)
                {
                    td = ts.NewTask();
                    td.RegistrationInfo.Description = "This task helps with the launching process of your game.";
                    td.Actions.Add(new ExecAction(GamePath, Args, WorkingDirectory));
                    td.Principal.RunLevel = TaskRunLevel.LUA;
                    td.Settings.AllowHardTerminate = true;
                    ts.RootFolder.RegisterTaskDefinition(taskName, td);
                    MakeServiceKnown();

                    ts.GetTask(taskName).Run();
                    return await GetProcessFromTaskAsync(existingTask, GamePath);
                }
                else
                {
                    if (existingTask.State == TaskState.Running)
                        existingTask.Stop();

                    await Task.Delay(500);
                    td = existingTask.Definition;

                    td.Actions.Clear();
                    td.Principal.RunLevel = TaskRunLevel.LUA;
                    td.Settings.AllowHardTerminate = true;
                    td.Actions.Add(new ExecAction(GamePath, Args, WorkingDirectory));
                    MakeServiceKnown();

                    ts.RootFolder.RegisterTaskDefinition(taskName, td);

                    MakeServiceKnown();
                    var task = existingTask.Run();
                    return await GetProcessFromTaskAsync(existingTask, GamePath);
                }
            }
        }

        private static async Task WaitForTaskToStart(Microsoft.Win32.TaskScheduler.Task task)
        {
            while (task.State != TaskState.Running)
            {
                await Task.Delay(100);
            }
        }


        public static async Task<Process> GetProcessFromTaskAsync(Microsoft.Win32.TaskScheduler.Task task, string GamePath)
        {
            int SecondsWaited = 0;
            while (task.State == TaskState.Running || task.State == TaskState.Queued)
            {
                if (SecondsWaited == 20000)
                    break;

                await Task.Delay(1000);
                SecondsWaited += 1000;
                var Processes = Process.GetProcessesByName("FortniteClient-Win64-Shipping");

                if (Processes != null)
                {
                    foreach (var Process in Processes)
                    {
                        if (Process.MainModule.FileName == GamePath)
                        {
                            return Process;
                        }
                    }
                }
            }

            return null;
        }


        public static async System.Threading.Tasks.Task FullGameVerify(ResultObject Build, TextBlock statusText, Game game, SynchronizationContext ui)
        {
            if (!bSkipVerify)
            {
                if (!(bool)Build.Success)
                {
                    LauncherData.MainContentWindow.HidePopup();

                    NovaLauncher.Models.NovaMessageBox.NovaMessageBox messageBox = new NovaLauncher.Models.NovaMessageBox.NovaMessageBox();
                    RoutedEventHandler ButtonLeftClick = null;
                    ButtonLeftClick += (sender, args) =>
                    {
                        LauncherData.MainContentWindow.WPFUIHost.Hide();
                    };
                    RoutedEventHandler RightButtonClick = null;
                    RightButtonClick += (sender, args) =>
                    {
                        bLaunchInProgress = false;
                        LauncherData.MainContentWindow.WPFUIHost.Hide();
                    };

                    if (Build.ErrorMessage.Contains("does not currently support"))
                    {
                        await messageBox.ShowMessageAsync(Build.ErrorMessage, "Launch Anyways", RightButtonClick, ButtonLeftClick);
                    }
                    else
                    {
                        bLaunchInProgress = false;
                        await messageBox.ShowMessageAsync(Build.ErrorMessage, "OK", RightButtonClick, ButtonLeftClick);
                    }
                }
                else
                {
                    bool bNeedsVerification = false;
                    if (Build.Build.Assets != null)
                    {
                        string[] files = Directory.GetFiles(game.GamePath, "*.*", SearchOption.AllDirectories);

                        await Task.Run(() =>
                        {
                            foreach (string file in files)
                            {
                                var FilePathWOBasePath = file.Replace(game.GamePath + "\\", "");
                                bool isAsset = Build.Build.Assets.Any(asset => asset.FilePath == FilePathWOBasePath);
                                bool isExtraAsset = false;

                                if (Build.Build.ExtraAssets != null)
                                {
                                    isExtraAsset = Build.Build.ExtraAssets.Any(asset => asset.FilePath == FilePathWOBasePath);
                                }

                                if (!isAsset && !isExtraAsset)
                                {
                                    try
                                    {
                                        File.Delete(file);
                                    }
                                    catch(Exception ex) 
                                    {
                                        Logger.Logger.Log(LogLevel.Error, ex.Message);
                                    }
                                }
                                else
                                {
                                    bool isExcludedAsset = false;

                                    if (Build.Build.ExcludedAssets != null)
                                    {
                                        isExcludedAsset = Build.Build.ExcludedAssets.Any(asset => asset.FilePath == FilePathWOBasePath);
                                    }

                                    if (!isExcludedAsset && !isExtraAsset)
                                    {
                                        var foundAsset = Build.Build.Assets.First(asset => asset.FilePath == FilePathWOBasePath);
                                        FileInfo fileInfo = new FileInfo(file);
                                        long fileSizeInBytes = fileInfo.Length;

                                        if (foundAsset.Size != fileSizeInBytes)
                                        {
                                            Logger.Logger.Log(LogLevel.Error, $"File is corrupted: {FilePathWOBasePath}");
                                            try
                                            {
                                                File.Delete(file);
                                            }
                                            catch (Exception ex)
                                            {
                                                Logger.Logger.Log(LogLevel.Error, ex.Message);
                                            }
                                            bNeedsVerification = true;
                                        }
                                    }
                                }
                            }
                        });

                        await Task.Run(() =>
                        {
                            foreach (var asset in Build.Build.Assets)
                            {
                                string assetFilePath = Path.Combine(game.GamePath, asset.FilePath);

                                if (!File.Exists(assetFilePath))
                                {
                                    Logger.Logger.Log(LogLevel.Error, $"File does not exist: {asset.FilePath}");
                                    bNeedsVerification = true;
                                }
                            }
                        });
                    }

                    try
                    {
                        if (Build.Build.ExtraAssets != null)
                        {
                            foreach (var asset in Build.Build.ExtraAssets)
                            {
                                string filePath = Path.Combine(game.GamePath, asset.FilePath);
                                if (File.Exists(filePath))
                                {
                                    FileInfo fileInfo = new FileInfo(filePath);
                                    long fileSizeInBytes = fileInfo.Length;

                                    if (asset.Size != fileSizeInBytes)
                                    {
                                        try
                                        {
                                            File.Delete(filePath);
                                        }
                                        catch (Exception ex)
                                        {
                                            Logger.Logger.Log(LogLevel.Error, ex.Message);
                                        }
                                        await Task.Delay(500);
                                        await DownloadFileAsync(asset.Url, filePath);
                                    }
                                }
                                else
                                {
                                    await DownloadFileAsync(asset.Url, filePath);
                                }
                            }
                        }
                        else
                        {
                            Logger.Logger.Log(LogLevel.Debug, "No Extra Content");
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Logger.Log(LogLevel.Error, ex.Message);
                    }

                    if (bNeedsVerification)
                    {
                        LauncherData.MainContentWindow.ShowPopup($"Verifying Build 0%", true, 0, null);

                        GameOption.SetButtonState(statusText, 6);

                        if (!string.IsNullOrEmpty(Build.Build.DownloadURL))
                        {
                            Logger.Logger.Log(LogLevel.Error, $"Fetching Manifest from: {Build.Build.DownloadURL}");

                            TextBlock TextBlock = new TextBlock();
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                if (LauncherData.MainContentWindow.PopupPopup.PopupText != null)
                                    TextBlock = LauncherData.MainContentWindow.PopupPopup.PopupText;
                            });

                            await Task.Run(async () =>
                            {
                                var httpClient = new WebClient();
                                ui.Post(_ => TextBlock.Text = "Downloading Manifest...", null);

                                var manifest = JsonConvert.DeserializeObject<Main.ManifestFile>(httpClient.DownloadString(Build.Build.DownloadURL));
                                await Main.Download(manifest, game.GamePath, TextBlock, ui, true);
                            });
                        }
                    }
                }
            }
        }
        public static async Task ForceFullGameVerify(ResultObject Build, TextBlock statusText, Game game, SynchronizationContext ui)
        {
            if (!(bool)Build.Success)
            {
                LauncherData.MainContentWindow.HidePopup();

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

                if (Build.ErrorMessage.Contains("does not currently support"))
                {
                    await messageBox.ShowMessageAsync(Build.ErrorMessage, "Ok", RightButtonClick, ButtonLeftClick);
                }
                else
                {
                    await messageBox.ShowMessageAsync(Build.ErrorMessage, "OK", RightButtonClick, ButtonLeftClick);
                }
            }
            else
            {
                bool bNeedsVerification = false;
                if (Build.Build.Assets != null)
                {
                    string[] files = Directory.GetFiles(game.GamePath, "*.*", SearchOption.AllDirectories);

                    await Task.Run(() =>
                    {
                        foreach (string file in files)
                        {
                            var FilePathWOBasePath = file.Replace(game.GamePath + "\\", "");
                            bool isAsset = Build.Build.Assets.Any(asset => asset.FilePath == FilePathWOBasePath);
                            bool isExtraAsset = false;

                            if (Build.Build.ExtraAssets != null)
                            {
                                isExtraAsset = Build.Build.ExtraAssets.Any(asset => asset.FilePath == FilePathWOBasePath);
                            }

                            if (!isAsset && !isExtraAsset)
                            {
                                try
                                {
                                    File.Delete(file);
                                }
                                catch (Exception ex)
                                {
                                    Logger.Logger.Log(LogLevel.Error, ex.Message);
                                }
                            }
                            else
                            {
                                bool isExcludedAsset = false;

                                if (Build.Build.ExcludedAssets != null)
                                {
                                    isExcludedAsset = Build.Build.ExcludedAssets.Any(asset => asset.FilePath == FilePathWOBasePath);
                                }

                                if (!isExcludedAsset && !isExtraAsset)
                                {
                                    var foundAsset = Build.Build.Assets.First(asset => asset.FilePath == FilePathWOBasePath);
                                    FileInfo fileInfo = new FileInfo(file);
                                    long fileSizeInBytes = fileInfo.Length;

                                    if (foundAsset.Size != fileSizeInBytes)
                                    {
                                        Logger.Logger.Log(LogLevel.Error, $"File is corrupted: {FilePathWOBasePath}");
                                        try
                                        {
                                            File.Delete(file);
                                        }
                                        catch (Exception ex) { Logger.Logger.Log(LogLevel.Error, ex.Message); }
                                        bNeedsVerification = true;
                                    }
                                }
                            }
                        }
                    });

                    await Task.Run(() =>
                    {
                        foreach (var asset in Build.Build.Assets)
                        {
                            string assetFilePath = Path.Combine(game.GamePath, asset.FilePath);

                            if (!File.Exists(assetFilePath))
                            {
                                Logger.Logger.Log(LogLevel.Error, $"File does not exist: {asset.FilePath}");
                                bNeedsVerification = true;
                            }
                        }
                    });
                }

                try
                {
                    if (Build.Build.ExtraAssets != null)
                    {
                        foreach (var asset in Build.Build.ExtraAssets)
                        {
                            string filePath = Path.Combine(game.GamePath, asset.FilePath);
                            if (File.Exists(filePath))
                            {
                                FileInfo fileInfo = new FileInfo(filePath);
                                long fileSizeInBytes = fileInfo.Length;

                                if (asset.Size != fileSizeInBytes)
                                {
                                    try
                                    {
                                        File.Delete(filePath);
                                    }
                                    catch (Exception ex) { Logger.Logger.Log(LogLevel.Error, ex.Message); }
                                    await Task.Delay(500);
                                    await DownloadFileAsync(asset.Url, filePath);
                                }
                            }
                            else
                            {
                                await DownloadFileAsync(asset.Url, filePath);
                            }
                        }
                    }
                    else
                    {
                        Logger.Logger.Log(LogLevel.Debug, "No Extra Content");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Logger.Log(LogLevel.Error, ex.Message);
                }

                if (bNeedsVerification)
                {
                    LauncherData.MainContentWindow.ShowPopup($"Verifying Build 0%", true, 0, null);

                    GameOption.SetButtonState(statusText, 6);

                    if (!string.IsNullOrEmpty(Build.Build.DownloadURL))
                    {
                        Logger.Logger.Log(LogLevel.Error, $"Fetching Manifest from: {Build.Build.DownloadURL}");

                        TextBlock TextBlock = new TextBlock();
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            if (LauncherData.MainContentWindow.PopupPopup.PopupText != null)
                                TextBlock = LauncherData.MainContentWindow.PopupPopup.PopupText;
                        });

                        await Task.Run(async () =>
                        {
                            var httpClient = new WebClient();
                            ui.Post(_ => TextBlock.Text = "Downloading Manifest...", null);

                            var manifest = JsonConvert.DeserializeObject<Main.ManifestFile>(httpClient.DownloadString(Build.Build.DownloadURL));
                            await Main.Download(manifest, game.GamePath, TextBlock, ui, true);
                        });
                    }
                }
            }
        }


        public static async Task VerifyGameFiles(ResultObject? Build, TextBlock statusText, Game game, SynchronizationContext ui)
        {
            if (!File.Exists($@"{game.GamePath}\FortniteGame\Binaries\Win64\FortniteClient-Win64-Shipping.exe"))
            {
                if ((bool)Build.Success)
                {
                    bool bNeedsVerification = false;
                    if (Build.Build.Assets != null)
                    {
                        string[] files = Directory.GetFiles(game.GamePath, "*.*", SearchOption.AllDirectories);

                        await Task.Run(() =>
                        {
                            foreach (string file in files)
                            {
                                var FilePathWOBasePath = file.Replace(game.GamePath + "\\", "");
                                bool isAsset = Build.Build.Assets.Any(asset => asset.FilePath == FilePathWOBasePath);
                                bool isExtraAsset = false;

                                if (Build.Build.ExtraAssets != null)
                                {
                                    isExtraAsset = Build.Build.ExtraAssets.Any(asset => asset.FilePath == FilePathWOBasePath);
                                }

                                if (!isAsset && !isExtraAsset)
                                {
                                    try
                                    {
                                        File.Delete(file);
                                    }
                                    catch (Exception ex) { Logger.Logger.Log(LogLevel.Error, ex.Message); }
                                }
                                else
                                {
                                    bool isExcludedAsset = false;

                                    if (Build.Build.ExcludedAssets != null)
                                    {
                                        isExcludedAsset = Build.Build.ExcludedAssets.Any(asset => asset.FilePath == FilePathWOBasePath);
                                    }

                                    if (!isExcludedAsset && !isExtraAsset)
                                    {
                                        var foundAsset = Build.Build.Assets.First(asset => asset.FilePath == FilePathWOBasePath);
                                        FileInfo fileInfo = new FileInfo(file);
                                        long fileSizeInBytes = fileInfo.Length;

                                        if (foundAsset.Size != fileSizeInBytes)
                                        {
                                            Logger.Logger.Log(LogLevel.Error, $"File is corrupted: {FilePathWOBasePath}");
                                            try
                                            {
                                                File.Delete(file);
                                            }   
                                            catch (Exception ex) { Logger.Logger.Log(LogLevel.Error, ex.Message); }
                                            bNeedsVerification = true;
                                        }
                                    }
                                }
                            }
                        });

                        await Task.Run(() =>
                        {
                            foreach (var asset in Build.Build.Assets)
                            {
                                string assetFilePath = Path.Combine(game.GamePath, asset.FilePath);

                                if (!File.Exists(assetFilePath))
                                {
                                    Logger.Logger.Log(LogLevel.Error, $"File does not exist: {asset.FilePath}");
                                    bNeedsVerification = true;
                                }
                            }
                        });
                    }

                    try
                    {
                        if (Build.Build.ExtraAssets != null)
                        {
                            foreach (var asset in Build.Build.ExtraAssets)
                            {
                                string filePath = Path.Combine(game.GamePath, asset.FilePath);
                                if (File.Exists(filePath))
                                {
                                    FileInfo fileInfo = new FileInfo(filePath);
                                    long fileSizeInBytes = fileInfo.Length;

                                    if (asset.Size != fileSizeInBytes)
                                    {
                                        try
                                        {
                                            File.Delete(filePath);
                                        }
                                        catch (Exception ex) { Logger.Logger.Log(LogLevel.Error, ex.Message); }
                                        await Task.Delay(500);
                                        await DownloadFileAsync(asset.Url, filePath);
                                    }
                                }
                                else
                                {
                                    await DownloadFileAsync(asset.Url, filePath);
                                }
                            }
                        }
                        else
                        {
                            Logger.Logger.Log(LogLevel.Debug, "No Extra Content");
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Logger.Log(LogLevel.Error, ex.Message);
                    }

                    if (bNeedsVerification)
                    {
                        LauncherData.MainContentWindow.ShowPopup($"Verifying Build 0%", true, 0, null);

                        GameOption.SetButtonState(statusText, 6);

                        if (!string.IsNullOrEmpty(Build.Build.DownloadURL))
                        {
                            Logger.Logger.Log(LogLevel.Error, $"Fetching Manifest from: {Build.Build.DownloadURL}");

                            TextBlock TextBlock = new TextBlock();
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                if (LauncherData.MainContentWindow.PopupPopup.PopupText != null)
                                    TextBlock = LauncherData.MainContentWindow.PopupPopup.PopupText;
                            });

                            await Task.Run(async () =>
                            {
                                var httpClient = new WebClient();
                                var manifest = JsonConvert.DeserializeObject<Main.ManifestFile>(httpClient.DownloadString(Build.Build.DownloadURL));
                                await Main.Download(manifest, game.GamePath, TextBlock, ui, true);
                            });
                        }
                    }
                }
                else
                {
                    MessageBox.Show("The build that you are trying to launch is not installed.");
                    GameOption.SetButtonState(statusText, 4);

                    bLaunchInProgress = false;
                    return;
                }
            }
        }
        public static void CloseFortniteProcesses()
        {
            Logger.Logger.Log(LogLevel.Info, "Closing Fortnite processes...");

            string novaFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Nova\";

            foreach (var process in Process.GetProcessesByName("FortniteClient-Win64-Shipping_BE"))
            {
                try
                {
                    if (process.MainModule.FileName.StartsWith(novaFolderPath))
                    {
                        process.Kill();
                    }
                }
                catch (Exception ex)
                {
                    Logger.Logger.Log(LogLevel.Error, $"Failed to kill process: {ex}");
                }
            }

            foreach (var process in Process.GetProcessesByName("FortniteLauncher"))
            {
                try
                {
                    if (process.MainModule.FileName.StartsWith(novaFolderPath))
                    {
                        process.Kill();
                    }
                }
                catch (Exception ex)
                {
                    Logger.Logger.Log(LogLevel.Error, $"Failed to kill process: {ex}");
                }
            }

            Logger.Logger.Log(LogLevel.Info, "Fortnite processes closed.");
        }
        private static Process FindProcessByPath(string filePath)
        {
            Process[] processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(filePath));

            foreach (Process process in processes)
            {
                try
                {
                    string processPath = process.MainModule.FileName;

                    if (string.Equals(processPath, filePath, StringComparison.OrdinalIgnoreCase))
                    {
                        return process;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Logger.Log(LogLevel.Error, ex.Message);
                }
            }

            return null;
        }
        private static void StartAnticheat()
        {
            Logger.Logger.Log(LogLevel.Info, "Starting anticheat process...");

            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string novaFolder = Path.Combine(localAppData, "Nova");

            if (!Directory.Exists(Path.Combine(novaFolder, "dud")))
                Directory.CreateDirectory(Path.Combine(novaFolder, "dud"));

            string bePath = Path.Combine(novaFolder, "dud", "FortniteClient-Win64-Shipping_BE.exe");
            string launcherPath = Path.Combine(novaFolder, "dud", "FortniteLauncher.exe");

            byte[] launcherBytes = Properties.Resources.FortniteLauncher;
            byte[] clientBytes = Properties.Resources.FortniteClient_Win64_Shipping_BE;

            if (!File.Exists(bePath) || new FileInfo(bePath).Length != clientBytes.Length)
            {
                CloseProcessByNameInFolder("FortniteClient-Win64-Shipping_BE", novaFolder);

                File.WriteAllBytes(bePath, clientBytes);
                Logger.Logger.Log(LogLevel.Info, "BE file created or updated.");
            }

            if (!File.Exists(launcherPath) || new FileInfo(launcherPath).Length != launcherBytes.Length)
            {
                CloseProcessByNameInFolder("FortniteLauncher", novaFolder);

                File.WriteAllBytes(launcherPath, launcherBytes);
                Logger.Logger.Log(LogLevel.Info, "Launcher file created or updated.");
            }

            var beProcess = new Process();
            beProcess.StartInfo.FileName = bePath;
            beProcess.StartInfo.CreateNoWindow = true;
            beProcess.Start();

            var launcherProcess = new Process();
            launcherProcess.StartInfo.FileName = launcherPath;
            launcherProcess.StartInfo.CreateNoWindow = true;
            launcherProcess.Start();
        }


        private static void CloseProcessByNameInFolder(string processName, string folderPath)
        {
            Process[] processes = Process.GetProcessesByName(processName);
            foreach (Process process in processes)
            {
                try
                {
                    string processPath = process.MainModule.FileName;
                    if (processPath.StartsWith(folderPath, StringComparison.OrdinalIgnoreCase))
                    {
                        process.CloseMainWindow();
                        process.WaitForExit(1000);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Logger.Log(LogLevel.Error, $"Error while closing {processName}: {ex.Message}");
                }
            }
        }

        private static void ShowMessageBox(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        public static bool ByteArrayContains(byte[] source, byte[] target)
        {
            if (source == null || target == null || source.Length < target.Length)
                return false;

            for (int i = 0; i <= source.Length - target.Length; i++)
            {
                bool found = true;
                for (int j = 0; j < target.Length; j++)
                {
                    if (source[i + j] != target[j])
                    {
                        found = false;
                        break;
                    }
                }
                if (found)
                    return true;
            }
            return false;
        }
        private static string CalculateMD5Hash(string filePath)
        {
            using (var sha256 = SHA256.Create())
            {
                using (var fileStream = File.OpenRead(filePath))
                {
                    byte[] hashBytes = sha256.ComputeHash(fileStream);
                    return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                }
            }
        }
        private static async Task<bool> StartP(Process process, Game game)
        {
            if (process.HasExited)
                return false;

            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string presidioFolder = Path.Combine(localAppData, "Nova", "Presidio");
            
            string dllPath = Path.Combine(presidioFolder, "Presidio.dll");

            try
            {
                if (process.HasExited)
                    return false;

                Injector.InjectAsync(process.Id, dllPath);

                using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(60)))
                {
                    try
                    {
                        await Task.Run(async () =>
                        {
                            while (true)
                            {
                                if (cancellationTokenSource.Token.IsCancellationRequested)
                                    break;

                                Process updatedProcess = Process.GetProcessById(process.Id);
                                foreach (ProcessModule module in updatedProcess.Modules)
                                {
                                    if (module.FileName == dllPath)
                                    {
                                        return;
                                    }
                                }

                                await Task.Delay(500);
                            }
                        }, cancellationTokenSource.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.Logger.Log(LogLevel.Error, ex.Message);
                return false;
            }
        }

        
        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, IntPtr lpString, int nMaxCount);

        [DllImport("user32.dll")]
        static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern bool IsWindowVisible(IntPtr hWnd);

        private static async Task WaitForGameToStartAsync(Process process, Game game, Process Agentprocess)
        {
            //if(Agentprocess.HasExited)
            //{
            //    NovaLauncher.Models.NovaMessageBox.NovaMessageBox messageBox = new NovaLauncher.Models.NovaMessageBox.NovaMessageBox();
            //    RoutedEventHandler RightButtonClick = null;
            //    RightButtonClick += (sender, args) =>
            //    {
            //        LauncherData.MainContentWindow.WPFUIHost.Hide();
            //    };

            //    RoutedEventHandler ButtonLeftClick = null;
            //    ButtonLeftClick += (sender, args) =>
            //    {
            //        LauncherData.MainContentWindow.WPFUIHost.Hide();
            //    };
            //    await messageBox.ShowMessageAsync("Presidio could not load. If you encounter this error again, please reach out to our support team on our Discord server.",
            //    "Ok", RightButtonClick, ButtonLeftClick);
            //}

            GameLauncher.CloseFortniteProcesses();

            try
            {
                DiscordPresence.SetState(DiscordPresence.RichPresenceEnum.LaunchingBuild, game.Name);
                Injector.InjectAsync(process.Id, Injector.GetCumar());
                StartAnticheat();
                //if (!Global.bSkipAC)
                //{
                //    using (var cancellationTokenSource = new CancellationTokenSource())
                //    {
                //        if (!await StartP(process, game))
                //        {
                //            if (process.HasExited)
                //                return;

                //            process.Kill();

                //            NovaLauncher.Models.NovaMessageBox.NovaMessageBox messageBox = new NovaLauncher.Models.NovaMessageBox.NovaMessageBox();
                //            RoutedEventHandler RightButtonClick = null;
                //            RightButtonClick += (sender, args) =>
                //            {
                //                LauncherData.MainContentWindow.WPFUIHost.Hide();
                //            };

                //            RoutedEventHandler ButtonLeftClick = null;
                //            ButtonLeftClick += (sender, args) =>
                //            {
                //                LauncherData.MainContentWindow.WPFUIHost.Hide();
                //            };
                //            await messageBox.ShowMessageAsync("Presidio could not load. If you encounter this error again, please reach out to our support team on our Discord server.",
                //            "Ok", RightButtonClick, ButtonLeftClick);
                //        }
                //    }
                //}
            }
            catch (Exception ex)
            {
                Logger.Logger.Log(LogLevel.Error, $"Failed to wait for the game to start: {ex}");
            }
        }
        private static bool IsEOR()
        {
            Logger.Logger.Log(LogLevel.Info, "Checking EOR status...");

            string settingsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Nova", "Settings", "Settings.json");
            dynamic settings = new JObject();

            if (!File.Exists(settingsFilePath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(settingsFilePath));
                File.WriteAllText(settingsFilePath, settings.ToString());
            }

            try
            {
                settings = JObject.Parse(File.ReadAllText(settingsFilePath));
            }
            catch (JsonReaderException ex)
            {
                Logger.Logger.Log(LogLevel.Error, $"Failed to parse JSON settings: {ex}");
                MessageBox.Show($"Failed to parse JSON settings: {ex.Message}");
            }

            bool bEnableEOR = settings.bEnableEOR ?? false;

            Logger.Logger.Log(LogLevel.Info, $"EOR status: {bEnableEOR}");

            return bEnableEOR;
        }
        private static bool IsInstantReset()
        {
            Logger.Logger.Log(LogLevel.Info, "Checking Instant Reset status...");

            string settingsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Nova", "Settings", "Settings.json");
            dynamic settings = new JObject();

            if (!File.Exists(settingsFilePath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(settingsFilePath));
                File.WriteAllText(settingsFilePath, settings.ToString());
            }

            try
            {
                settings = JObject.Parse(File.ReadAllText(settingsFilePath));
            }
            catch (JsonReaderException ex)
            {
                Logger.Logger.Log(LogLevel.Error, $"Failed to parse JSON settings: {ex}");
                MessageBox.Show($"Failed to parse JSON settings: {ex.Message}");
            }

            bool bEnableInstantReset = settings.bInstantReset ?? false;

            Logger.Logger.Log(LogLevel.Info, $"Instant Reset status: {bEnableInstantReset}");

            return bEnableInstantReset;
        }
        private static bool InjectMEMFix()
        {
            Logger.Logger.Log(LogLevel.Info, "Checking MEM Fix status...");

            string settingsFilePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Nova", "Settings", "Settings.json");

            dynamic settings = new JObject();
            if (File.Exists(settingsFilePath))
            {
                try
                {
                    settings = JObject.Parse(File.ReadAllText(settingsFilePath));
                    if (settings.bEnableMLF == null)
                        settings.bEnableMLF = true;
                }
                catch (JsonReaderException ex)
                {
                    Logger.Logger.Log(LogLevel.Error, $"Failed to parse JSON settings: {ex}");
                    Console.WriteLine($"Failed to parse JSON settings: {ex.Message}");
                }
            }
            else
            {
                settings.bEnableEOR = true;
                settings.bEnableMLF = true;

                Directory.CreateDirectory(Path.GetDirectoryName(settingsFilePath));
                File.WriteAllText(settingsFilePath, settings.ToString());
            }

            bool bEnableMEM = settings.bEnableMLF ?? false;

            Logger.Logger.Log(LogLevel.Info, $"MEM Fix status: {bEnableMEM}");

            return bEnableMEM;
        }

        private static bool SBD(Game game)
        {
            string pattern = @"[1-5]\.";
            if (Regex.IsMatch(game.Name, pattern))
            {
                Logger.Logger.Log(LogLevel.Info, "Checking sbd Fix status...");

                string settingsFilePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Nova", "Settings", "Settings.json");

                dynamic settings = new JObject();
                if (File.Exists(settingsFilePath))
                {
                    try
                    {
                        settings = JObject.Parse(File.ReadAllText(settingsFilePath));
                        if (settings.bSprintByDefault == null)
                            settings.bSprintByDefault = true;
                    }
                    catch (JsonReaderException ex)
                    {
                        Logger.Logger.Log(LogLevel.Error, $"Failed to parse JSON settings: {ex}");
                        Console.WriteLine($"Failed to parse JSON settings: {ex.Message}");
                    }
                }
                else
                {
                    settings.bEnableEOR = true;
                    settings.bEnableMLF = true;
                    settings.bSprintByDefault = true;

                    Directory.CreateDirectory(Path.GetDirectoryName(settingsFilePath));
                    File.WriteAllText(settingsFilePath, settings.ToString());
                }

                bool bEnableMEM = settings.bEnableMLF ?? false;

                Logger.Logger.Log(LogLevel.Info, $"MEM Fix status: {bEnableMEM}");
                return bEnableMEM;
            }
            else
            {
                return false;
            }
        }
        public static void OnGameExited(Process process, TextBlock statusText)
        {
            Logger.Logger.Log(LogLevel.Info, "Game process exited.");

            var gameList = GameList.LoadFromFile(gameListFilePath);

            Game game = gameList.Games.FirstOrDefault(g => g.GameID == process.Id);

            if (game != null)
            {
                game.GameID = 0;
                gameList.SaveToFile(gameListFilePath, true);
            }

            KillFortniteProcessesAsync();
        }

        public static async Task KillFortniteProcessesAsync()
        {
            string localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string novaDudPath = Path.Combine(localAppDataPath, "Nova");

            string[] processesToKill = { "FortniteClient-Win64-Shipping_BE", "FortniteLauncher" };

            foreach (string processName in processesToKill)
            {
                Process[] processes = Process.GetProcessesByName(processName);

                foreach (Process process in processes)
                {
                    try
                    {
                        if (IsProcessInPath(process, novaDudPath))
                        {
                            await KillProcessAsync(process);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Logger.Log(LogLevel.Error, ex.Message);
                    }
                }
            }
        }

        private static bool IsProcessInPath(Process process, string path)
        {
            try
            {
                
                string processPath = process.MainModule.FileName;
                return processPath.StartsWith(path, StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                Logger.Logger.Log(LogLevel.Error, ex.Message);
                return false;
            }
        }

        private static Task KillProcessAsync(Process process)
        {
            return Task.Run(() =>
            {
                try
                {
                    process.Kill();
                }
                catch (Exception ex)
                {
                    Logger.Logger.Log(LogLevel.Error, ex.Message);
                }
            });
        }

        public static BiosInfo GetBiosInformation()
        {
            BiosInfo biosInfo = new BiosInfo();

            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_BIOS");
            foreach (ManagementObject obj in searcher.Get())
            {
                biosInfo.Manufacturer = obj["Manufacturer"].ToString();
                biosInfo.Version = obj["SMBIOSBIOSVersion"].ToString();
            }

            return biosInfo;
        }
        static string GetVolumeId(string driveLetter)
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT VolumeSerialNumber FROM Win32_LogicalDisk WHERE DeviceID = '" + driveLetter + "'");
            ManagementObjectCollection collection = searcher.Get();

            foreach (ManagementObject obj in collection)
            {
                return obj["VolumeSerialNumber"]?.ToString();
            }

            return null;
        }

        static long GetDriveSizeBytes(string driveLetter)
        {
            DriveInfo drive = new DriveInfo(driveLetter);

            if (drive.IsReady)
            {
                return drive.TotalSize;
            }

            return 0;
        }

        static string ComputeSHA256Hash(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = sha256.ComputeHash(inputBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }

        public static int NumberToOBV(int integer)
        {
            int realPart = (integer * 2) + ((5 * 5) / 2) + 9;
            return realPart;
        }
    }
    public class BiosInfo
    {
        public string Manufacturer { get; set; }
        public string Version { get; set; }
    }
}