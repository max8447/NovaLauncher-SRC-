using System;

namespace NovaLauncher.Models.GameSaveManager
{
    public class Game
    {
        public string Name { get; set; }
        public string GamePath { get; set; }
        public int LaunchCount { get; set; }
        public DateTime DatePlayed { get; set; }
        public int GameID { get; set; }
    }
}
