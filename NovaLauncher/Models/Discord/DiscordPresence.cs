using DiscordRPC;
using NovaLauncher.Models.Logger;
using System;

namespace NovaLauncher.Models.Discord
{
    public class DiscordPresence
    {
        public static void SetState(RichPresenceEnum richPresenceEnum, string season)
        {
            try
            {
                switch (richPresenceEnum)
                {
                    case RichPresenceEnum.CheckingForUpdates:
                        DiscordPresence.presence.State = "Checking for launcher updates...";
                        break;
                    case RichPresenceEnum.LoggingIn:
                        DiscordPresence.presence.State = "Logging in...";
                        break;
                    case RichPresenceEnum.WaitingToLaunch:
                        DiscordPresence.presence.State = "Waiting to launch a build...";
                        break;
                    case RichPresenceEnum.LaunchingBuild:
                        DiscordPresence.presence.State = $"Starting {season}";
                        break;
                    case RichPresenceEnum.Playing:
                        DiscordPresence.presence.State = $"Playing Nova as {LauncherData.GetGetUserInfo().displayName}!";
                        break;
                    case RichPresenceEnum.Custom:
                        DiscordPresence.presence.State = $"{season}";
                        break;
                }

                DiscordPresence.client.SetPresence(DiscordPresence.presence);

                Logger.Logger.Log(LogLevel.Info, $"Rich presence state set to {DiscordPresence.presence.State}");
            }
            catch (Exception ex)
            {
                Logger.Logger.Log(LogLevel.Error, "Failed to set rich presence state", ex);
            }
        }

        public enum RichPresenceEnum
        {
            CheckingForUpdates,
            LoggingIn,
            WaitingToLaunch,
            LaunchingBuild,
            Playing,
            Custom
        }

        public static DiscordRpcClient client;
        private static RichPresence presence;
        public static Timestamps rpctimestamp { get; set; }

        public static void InitializeRPC()
        {
            try
            {
                DiscordPresence.rpctimestamp = Timestamps.Now;
                DiscordPresence.client = new DiscordRpcClient("1029800081009942539");
                DiscordPresence.client.Initialize();
                Button[] buttonArray = new Button[2]
                {
            new Button()
            {
                Label = "Nova Discord",
                Url = "https://discord.com/invite/novafn?lang=en"
            },
            new Button()
            {
                Label = "Nova TikTok",
                Url = "https://www.tiktok.com/@projectnovafn?lang=en"
            }
                };
                RichPresence richPresence = new RichPresence();
                richPresence.State = "Doing launcher stuff idk";
                richPresence.Timestamps = DiscordPresence.rpctimestamp;
                richPresence.Buttons = buttonArray;
                richPresence.Assets = new DiscordRPC.Assets()
                {
                    LargeImageKey = "mainnovaocon",
                    LargeImageText = "Nova",
                    SmallImageKey = "small"
                };
                DiscordPresence.presence = richPresence;

               Logger.Logger.Log(LogLevel.Info, "RPC initialized successfully", null);
            }
            catch (Exception ex)
            {
                Logger.Logger.Log(LogLevel.Error, "Failed to initialize RPC", ex);
            }
        }

    }
}
