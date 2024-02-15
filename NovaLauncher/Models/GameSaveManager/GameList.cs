using Newtonsoft.Json;
using NovaLauncher.Models.Controls;
using NovaLauncher.Models.Logger;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace NovaLauncher.Models.GameSaveManager
{
    public class GameList
    {
        public List<Game> Games { get; set; }

        public GameList()
        {
            Games = new List<Game>();
        }


        public static GameList LoadFromFile(string filePath)
        {
            var gameList = new GameList();
            gameList.Games = new List<Game>();

            try
            {
                if (!File.Exists(filePath))
                {
                    Console.WriteLine("File does not exist. Creating a new game list.");

                    string json1 = JsonConvert.SerializeObject(gameList);
                    File.WriteAllText(filePath, json1);
                    return gameList;
                }

                string fileContents = File.ReadAllText(filePath);
                fileContents = fileContents.Replace("\0", "");

                if (string.IsNullOrWhiteSpace(fileContents))
                {
                    Console.WriteLine("File is empty or contains only whitespace. Creating a new game list.");

                    string jsonNew = JsonConvert.SerializeObject(gameList);
                    File.WriteAllText(filePath, jsonNew);
                    return gameList;
                }

                return JsonConvert.DeserializeObject<GameList>(fileContents);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while loading or deserializing the game list: " + ex.Message);

                try
                {
                    string jsonNew = JsonConvert.SerializeObject(gameList);
                    File.WriteAllText(filePath, jsonNew);
                }
                catch (Exception)
                {
                    Console.WriteLine("An error occurred while saving the game list: " + ex.Message);
                }

                return gameList;
            }
        }



        public void SaveToFile(string filePath, bool bRefresh = false)
        {
            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(filePath, json);

            if (bRefresh)
            {
                UIManager.LoadLibraryAsync();
                UIManager.LoadHomeLibraryAsync();
            }
        }

        public void AddGame(Game game)
        {
            Games.Add(game);
            Logger.Logger.Log(LogLevel.Info, $"Added game: {game.Name}");
        }


        public void RemoveGame(Game game)
        {
            if (Games.Remove(game))
            {
                Logger.Logger.Log(LogLevel.Info, $"Removed game '{game.Name}' from game list");
            }
            else
            {
                Logger.Logger.Log(LogLevel.Warning, $"Game '{game.Name}' not found in game list");
            }
        }


        public bool IsGameRunning(Game game)
        {
            foreach (var process in Process.GetProcesses())
            {
                try
                {
                    string processPath = process.MainModule.FileName;

                    if (processPath.Contains(game.GamePath) && !process.HasExited)
                    {
                        Logger.Logger.Log(LogLevel.Info, $"Game {game.Name} is running.");
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Logger.Log(LogLevel.Error, $"Error checking if game {game.Name} is running.", ex);
                }
            }

            Logger.Logger.Log(LogLevel.Info, $"Game {game.Name} is not running.");
            return false;
        }
    }
}
