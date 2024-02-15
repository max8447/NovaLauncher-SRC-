using NovaLauncher.Models;
using NovaLauncher.Models.API.NovaBackend;
using NovaLauncher.Models.GameLauncher;
using NovaLauncher.Views.Pages;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using Wpf.Ui.Controls;
using static NovaLauncher.Models.Logger.Logger;

namespace NovaLauncher.Views.Windows
{
    public partial class MainWindow : UiWindow
    {
        [DllImport("user32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        private const int SW_RESTORE = 9;

        public MainWindow()
        {
            InitializeComponent();
        }

        public Frame GetFrame()
        {
            return FrameView;
        }

        private void Init(object sender, EventArgs e)
        {
            Log(NovaLauncher.Models.Logger.LogLevel.Info, "NovaLauncher Started...", null, true);

            //if (IsProcessAlreadyRunning())
            //{
            //    Environment.Exit(0);
            //}
            //else
            //{
                LauncherData.MainWindowView = this;
                GetFrame().Navigate(new InitializationPage());
                Application.Current.Exit += OnApplicationExit;
            //}
        }
        private void OnApplicationExit(object sender, ExitEventArgs e)
        {
            try
            {
                var Account = LauncherData.GetGetUserInfo();
                if (Account != null)
                    LauncherAPI.LogoutAsync(Account.access_token);
            }
            catch { }

            try
            {
                GameLauncher.CloseFortniteProcesses();
            }
            catch { }
        }

        static bool IsProcessAlreadyRunning()
        {
            Process currentProcess = Process.GetCurrentProcess();
            string currentProcessName = currentProcess.ProcessName;

            Process[] processes = Process.GetProcessesByName(currentProcessName);

            if (processes.Length > 1)
            {
                IntPtr hWnd = processes[0].MainWindowHandle;
                ShowWindowAsync(hWnd, SW_RESTORE);
                SetForegroundWindow(hWnd);
                return true;
            }

            return false;
        }
    }
}
