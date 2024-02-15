using MaterialDesignThemes.Wpf;
using Microsoft.Extensions.Hosting;
using NovaLauncher.Models;
using NovaLauncher.Models.AccountUtils;
using NovaLauncher.Models.AccountUtils.Classes;
using NovaLauncher.Models.GameLauncher;
using NovaLauncher.Models.Logger;
using NovaLauncher.Services;
using NovaLauncher.Views.Pages.ContentPages;
using NovaLauncher.Views.Popups;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using Wpf.Ui.Common;

namespace NovaLauncher.Views.Pages
{
    /// <summary>
    /// Interaction logic for UserLoginPage.xaml
    /// </summary>
    public partial class UserLoginPage : Page
    {
        public static bool bSaveUser = false;

        public UserLoginPage()
        {
            InitializeComponent();
        }
        private void SlideOutAnimation(object sender, RoutedEventArgs e)
        {
            PlayAnimationAsync();
        }

        private async Task PlayAnimationAsync()
        {
            await Task.Run(() => Thread.Sleep(300));
            Storyboard animation = (Storyboard)Resources["SlideOutAnimation"];
            animation.Begin();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var uiMessageBox = new Wpf.Ui.Controls.MessageBox();
            uiMessageBox.Title = "Nova Security Warning";
            uiMessageBox.MicaEnabled = true;
            uiMessageBox.Content = new TextBlock
            {
                Text =
                    "By choosing to 'Remember my login', your account information will be stored on this device. Please be aware that if any unauthorized access occurs, it is the user's responsibility to ensure the security of the device.",
                TextWrapping = TextWrapping.WrapWithOverflow
            };
            uiMessageBox.ButtonRightClick += (_, _) => WarningCanceled(uiMessageBox);
            uiMessageBox.ButtonLeftName = "Accept";
            uiMessageBox.ButtonLeftClick += (_, _) => WarningAccepted(uiMessageBox);

            uiMessageBox.Show();
        }

        private void WarningAccepted(Wpf.Ui.Controls.MessageBox box)
        {
            bSaveUser = true;
            box.Close();
        }

        private void WarningCanceled(Wpf.Ui.Controls.MessageBox box)
        {
            bSaveUser = false;
            rememberMe.IsChecked = false;
            box.Close();
        }

        private void RequestTryLogin(object sender, RoutedEventArgs e)
        {
            LoginAsync();
        }

        private async Task LoginAsync()
        {
            Logger.Log(LogLevel.Info, "Performing login...");

            LoginCard.Visibility = Visibility.Collapsed;
            var LoginLoadPopupControl = new LoginLoadPopup();
            var result = DialogHost.Show(LoginLoadPopupControl);

            var Result = await NovaLauncher.Models.AccountUtils.AccountUtils.LoginAsync(emailBox.Text, txtPassword.Password);


            if (Result.success)
            {
                Logger.Log(LogLevel.Info, "Login successful.");
                LoginCard.IsHitTestVisible = false;
                AccountUtils.MyLauncherAccount = Result.account;
                LauncherData.UserLoggedIn = new User()
                {
                    Email = emailBox.Text,
                    Password = txtPassword.Password
                };

                await Task.Delay(200);

                SettingsPage.settingIds = await GameLauncher.GetSettingsFromApiAsync();

                DialogHost.Close(null);
                await Task.Delay(200);

                if (Result.account != null)
                {
                    if (!Result.account.accepted_tos)
                    {
                        TOSPage TOSPAGE = new TOSPage();
#pragma warning disable VSTHRD101 // Avoid unsupported async delegates
                        TOSPAGE.acceptButton.Click += async (sender, args) =>
                        {
                            await AccountUtils.AcceptTOSAsync(Result.account.access_token);
                            DialogHost.Close(null);
                        };
#pragma warning restore VSTHRD101 // Avoid unsupported async delegates

                        await DialogHost.Show(TOSPAGE);
                    }
                }

                if (bSaveUser) { AccountUtils.SaveNewUser(emailBox.Text, txtPassword.Password); }
                await Task.Delay(500);

                Logger.Log(LogLevel.Info, "Starting host service and closing main window...");
                Application.Current.Dispatcher.Invoke(() =>
                {
                    HostService._host.StartAsync();
                    LauncherData.MainWindowView.Close();
                });
            }
            else
            {
                DialogHost.Close(null);
                await Task.Delay(200);

                Logger.Log(LogLevel.Error, "Login failed.");

                LoginCard.Visibility = Visibility.Visible;
                snackBar.Timeout = 5000;
                snackBar.Show("Access Denied", Result.error_message, SymbolRegular.LockClosed24, ControlAppearance.Danger);
            }
        }

        private void ProcessUser(object sender, RoutedEventArgs e)
        {
            LoadExistingUserAsync();
        }

        private async Task LoadExistingUserAsync()
        {
            Logger.Log(LogLevel.Info, "Loading existing user...");

            var UserSettings = await AccountUtils.LoadUserSettingsAsync();

            if (UserSettings != null)
            {
                Logger.Log(LogLevel.Info, "Existing user settings found.");

                LoginCard.Visibility = Visibility.Collapsed;
                var LoginLoadPopupControl = new LoginLoadPopup();
                var result = DialogHost.Show(LoginLoadPopupControl);

                var Result = await NovaLauncher.Models.AccountUtils.AccountUtils.LoginAsync(UserSettings.Email, UserSettings.Password);


                if (Result.success)
                {
                    Logger.Log(LogLevel.Info, "Login successful.");
                    LoginCard.IsHitTestVisible = false;
                    AccountUtils.MyLauncherAccount = Result.account;

                    DialogHost.Close(null);
                    await Task.Delay(200);

                    bool bMadeTOSRequest = false;

                    if (Result.account != null && !Result.account.accepted_tos)
                    {
                        TOSPage TOSPAGE = new TOSPage();
#pragma warning disable VSTHRD101 // Avoid unsupported async delegates
                        TOSPAGE.acceptButton.Click += async (sender, args) =>
                        {
                            if (!bMadeTOSRequest)
                            {
                                bMadeTOSRequest = true;
                                await AccountUtils.AcceptTOSAsync(Result.account.access_token);
                                DialogHost.Close(null);
                            }
                        };
#pragma warning restore VSTHRD101 // Avoid unsupported async delegates

                        await DialogHost.Show(TOSPAGE);
                    }

                    LauncherData.UserLoggedIn = new User()
                    {
                        Email = UserSettings.Email,
                        Password = UserSettings.Password
                    };

                    Logger.Log(LogLevel.Info, "Starting host service and closing main window...");
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        HostService._host.Start();
                        LauncherData.MainWindowView.Close();
                    });
                }
                else
                {
                    DialogHost.Close(null);
                    await Task.Delay(200);

                    Logger.Log(LogLevel.Info, "Login failed.");

                    string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    string NovaPath = Path.Combine(appData, "Nova", "User");
                    string savePath = Path.Combine(NovaPath, "save.json");

                    File.Delete(savePath);

                    LoginCard.Visibility = Visibility.Visible;
                    snackBar.Timeout = 5000;
                    snackBar.Show("Access Denied", Result.error_message, SymbolRegular.LockClosed24, ControlAppearance.Danger);
                }

                LauncherData.UserLoggedIn = new User()
                {
                    Email = UserSettings.Email,
                    Password = UserSettings.Password,
                };
            }
        }

    }
}
