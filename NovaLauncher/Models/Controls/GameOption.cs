using NovaLauncher.Models.Assets;
using NovaLauncher.Models.GameSaveManager;
using NovaLauncher.Models.Logger;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Media;
using Application = System.Windows.Application;


namespace NovaLauncher.Models.Controls
{
    class GameOption
    {
        public struct ButtonData
        {
            public Game Game;
            public bool DownloadButton;
            public bool LibraryButton;
            public Frame ContentFrame;
            public ImageBrush LibraryImage;
        }

        public enum EButtonState
        {
            Launch,
            Running,
            Download,
            Installing,
            BuildNotFound
        }

        public static void SetButtonState(TextBlock statusText, int State)
        {
            if (statusText == null)
            {
                return;
            }

            var dispatcher = System.Windows.Application.Current.Dispatcher;

            switch (State)
            {
                case 0:
                    dispatcher.Invoke(() => statusText.Text = "Launch");
                    break;
                case 1:
                    dispatcher.Invoke(() => statusText.Text = "Running");
                    break;
                case 2:
                    dispatcher.Invoke(() => statusText.Text = "Launching...");
                    break;
                case 3:
                    dispatcher.Invoke(() => statusText.Text = "Installing Content...");
                    break;
                case 4:
                    dispatcher.Invoke(() => statusText.Text = "Build not found");
                    break;
                case 5:
                    dispatcher.Invoke(() => statusText.Text = "Logging In...");
                    break;
                case 6:
                    dispatcher.Invoke(() => statusText.Text = "Verifying...");
                    break;
                case 7:
                    dispatcher.Invoke(() => statusText.Text = "Preparing...");
                    break;
            }

            Logger.Logger.Log(LogLevel.Info, $"Button state set to: {statusText.Text}");
        }

        public static int FindSeason(Game game)
        {
            Match match = Regex.Match(game.Name, @"\d+");

            if (match.Success)
            {
                int ParsedInt =  Int32.Parse(match.Value);

                return ParsedInt;
            }
            else if (game.Name.Contains("X"))
            {
                return 10;
            }

            return 0;
        }

        public static int FindSeason(string game)
        {
            Match match = Regex.Match(game, @"\d+");


            if (match.Success)
            {
                int ParsedInt = Int32.Parse(match.Value);

                return ParsedInt;
            }
            else if (game.Contains("X"))
            {
                return 10;
            }

            return 0;
        }

        public static int FindSeasonFromName(string name)
        {
            Match match = Regex.Match(name, @"\d+");

            if (match.Success)
            {
                return Int32.Parse(match.Value);
            }
            else if (name.Contains("X"))
            {
                return 10;
            }

            return 0;
        }

        public static void SetChapterTemplate(Game game, TextBlock templateName)
        {
            int currentVersion = FindSeason(game);

            if (currentVersion > 22)
                templateName.Text = $"Unknown Chapter";
            else if (currentVersion > 19)
                templateName.Text = $"Chapter 3";
            else if (currentVersion > 10)
                templateName.Text = $"Chapter 2";
            else
                templateName.Text = $"Chapter 1";

            Logger.Logger.Log(LogLevel.Info, $"Chapter template set: {templateName.Text}");
        }

        public static ButtonData LoadButton(Game game, ImageBrush seasonBrush, TextBlock templateName, TextBlock templateChapter, TextBlock statusText, bool isLibraryButton, Frame contentFrame, ImageBrush imageBrush, int buttonState, EventHandler launchedGameExited)
        {
            ButtonData buttonData = new ButtonData
            {
                ContentFrame = contentFrame,
                LibraryButton = isLibraryButton,
                LibraryImage = imageBrush,
                Game = game
            };

            if (game != null)
            {
                templateName.Text = game.Name;

                try
                {
                    Application.Current.Dispatcher.Invoke(() => {
                        seasonBrush.ImageSource = AssetManager.GetSeasonImage(GameOption.FindSeason(game));
                    });

                    Logger.Logger.Log(LogLevel.Info, $"Loaded season brush for game: {game.Name}");
                }
                catch
                {
                    Application.Current.Dispatcher.Invoke(() => {
                        seasonBrush.ImageSource = AssetManager.GetSeasonImage(999);
                    });

                    Logger.Logger.Log(LogLevel.Error, $"Failed to load season brush for game: {game.Name}");
                }

                SetChapterTemplate(game, templateChapter);

                if (!File.Exists($"{game.GamePath}\\FortniteGame\\Binaries\\Win64\\FortniteClient-Win64-Shipping.exe"))
                {
                    GameOption.SetButtonState(statusText, 4);
                }
                else
                {
                    foreach (var process in Process.GetProcesses())
                    {
                        try
                        {
                            if (process.Id == game.GameID && !process.HasExited && process.ProcessName.Contains("FortniteClient-Win64-Shipping"))
                            {
                                GameOption.SetButtonState(statusText, 1);
                                break;
                            }
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                    }
                }
            }

            Logger.Logger.Log(LogLevel.Info, $"Button loaded for game: {game?.Name}");

            return buttonData;
        }

        public static bool IsGameRunning(int gameID)
        {
            try
            {
                Process gameProcess = Process.GetProcessById(gameID);
                return (gameProcess.ProcessName == "FortniteClient-Win64-Shipping");
            }
            catch (ArgumentException)
            {
                return false;
            }
        }
    }
}
