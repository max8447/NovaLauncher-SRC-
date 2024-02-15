using IWshRuntimeLibrary;
using System;
using System.IO;
using System.Net;
using File = System.IO.File;

namespace NovaLauncher.Models.ShortcutManager
{
    public class ShortcutManager
    {
        public static void LoadShortcuts()
        {
            try
            {
                string shortcutPath = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu) + "\\NovaLauncher.lnk";
                if (File.Exists(shortcutPath))
                {
                    SetShortcutIcon(shortcutPath);
                }
                else
                {

                }
            }
            catch (Exception ex)
            {

            }

            try
            {
                string shortcutPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\NovaLauncher.lnk";
                if (File.Exists(shortcutPath))
                {
                    SetShortcutIcon(shortcutPath);
                }
                else
                {

                }
            }
            catch (Exception ex)
            {

            }
        }


        public static void SetShortcutIcon(string shortcutPath)
        {
            WshShellClass shell = new WshShellClass();

            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutPath);

            string iconPath = shortcut.IconLocation;

            if (iconPath.Equals("%SystemRoot%\\System32\\SHELL32.dll,0", StringComparison.OrdinalIgnoreCase) || !iconPath.Contains("Nova_400x400"))
            {
                string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string novaFolderPath = Path.Combine(appDataPath, "Nova");

                if (!Directory.Exists(novaFolderPath))
                {
                    Directory.CreateDirectory(novaFolderPath);
                }

                string downloadedIconPath = Path.Combine(novaFolderPath, "Nova_400x400.ico");

                if (!File.Exists(downloadedIconPath))
                {
                    using (var client = new System.Net.WebClient())
                    {
                        client.DownloadFile("https://projectnova.nyc3.cdn.digitaloceanspaces.com/Icons/Nova_400x400.ico", downloadedIconPath);
                    }
                }

                shortcut.IconLocation = downloadedIconPath;

                shortcut.Save();
            }
        }

        public static string GetShortcutIconPath(string shortcutPath)
        {
            string iconPath = string.Empty;

            WshShellClass shell = new WshShellClass();

            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutPath);

            iconPath = shortcut.IconLocation;

            return iconPath;
        }
        public static void DownloadIcon(string iconUrl, string savePath)
        {
            using (WebClient client = new WebClient())
            {
                client.DownloadFile(iconUrl, savePath);
            }
        }
    }
}
