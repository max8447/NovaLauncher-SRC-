using Microsoft.Win32.TaskScheduler;
using Newtonsoft.Json;
using NovaLauncher.Models;
using NovaLauncher.Views.Controls.Settings;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Task = System.Threading.Tasks.Task;

namespace NovaLauncher.Views.Pages.ContentPages
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : Page
    {
        public static SettingsPage settingsPage;
        public static List<string> settingIds = new List<string>();
        public SettingsPage()
        {
            InitializeComponent();

            while (true)
            {
                if (StackSettings != null)
                {
                    settingsPage = this;
                    System.Threading.Tasks.Task.Run(() => LoadSettings());
                    break;
                }
            }
        }
        public static bool Loaded = false;
        public async void LoadSettings()
        {
            LoadUserSettingsAsync();
            Loaded = true;
        }

        public async void LoadUserSettingsAsync()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                StackSettings.Children.Clear();
                List<Control> settings = new List<Control>();
                settings.Add(new EditOnReleaseOption());
                settings.Add(new InstantResetOption());
                settings.Add(new MemoryLeakFixOption());
                settings.Add(new SettingsOptionSBD());

                foreach (Control control in settings)
                {
                    StackSettings.Children.Add(control);
                }
            });
        }

        private void Logoutbtn_Click(object sender, RoutedEventArgs e)
        {
            LogUserOut(Application.Current.Windows[0]);
        }

        public static void LogUserOut(System.Windows.Window windows)
        {
            string? appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string? NovaPath = System.IO.Path.Combine(appData, "Nova", "User", "save.json");

            if (System.IO.File.Exists(NovaPath))
                System.IO.File.Delete(NovaPath);

            foreach (System.Windows.Window window in Application.Current.Windows)
            {
                window.Close();
            }

            CloseFortniteProcesses();
        }

#pragma warning disable VSTHRD100 // Avoid async void methods
        public static async void CloseFortniteProcesses()
#pragma warning restore VSTHRD100 // Avoid async void methods
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

            string? appPath = Process.GetCurrentProcess().MainModule.FileName;
            Process.Start(appPath);
            Environment.Exit(0);
        }
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetModuleFileName(IntPtr handle, StringBuilder buffer, int length);
        private void UninstallLauncher(object sender, RoutedEventArgs e)
        {
            string? localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            if (System.IO.File.Exists(System.IO.Path.Combine(localAppDataPath, "Nova", "NovaUpdater.exe")))
            {

                Process[] processes = Process.GetProcesses();

                foreach (Process process in processes)
                {
                    try
                    {
                        StringBuilder buffer = new StringBuilder(1024);
                        int length = GetModuleFileName(process.Handle, buffer, buffer.Capacity);
                        string? filePath = buffer.ToString(0, length);

                        FileInfo fileInfo = new FileInfo(filePath);

                        if (fileInfo.DirectoryName.Equals(Directory.GetCurrentDirectory()))
                        {
                            process.Kill();
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }

                foreach (Process proc in Process.GetProcessesByName("NovaUpdater.exe"))
                    proc.Kill();
            }

            if (!Directory.Exists(System.IO.Path.Combine(localAppDataPath, "Nova")))
            {
                Directory.CreateDirectory(System.IO.Path.Combine(localAppDataPath, "Nova"));
            }

            try
            {
                using (WebClient client = new WebClient())
                {

                    client.DownloadFile(LauncherData.LauncherDataInfo["Installer"].ToString(), System.IO.Path.Combine(localAppDataPath, "Nova", "NovaUpdater.exe"));
                }

                Process process = new Process();
                process.StartInfo.FileName = System.IO.Path.Combine(localAppDataPath, "Nova", "NovaUpdater.exe");
                process.StartInfo.WorkingDirectory = System.IO.Path.Combine(localAppDataPath, "Nova");
                process.StartInfo.Arguments = "-uninstall";
                process.Start();
                Environment.Exit(0);
            }
            catch (WebException ex)
            {
                Console.WriteLine("Error downloading NovaUpdater: " + ex.Message);
            }
        }

        private void Retrybtn_Click(object sender, RoutedEventArgs e)
        {
            LoadSettings();
        }
    }
}
