using Newtonsoft.Json;
using NovaLauncher.Models;
using NovaLauncher.Models.Logger;
using NovaLauncher.Models.Tools;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class PresidioManager
{
    public static async Task<Tuple<bool, string>> VerifyAsync()
    {
        string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string presidioFolder = Path.Combine(localAppData, "Nova", "Presidio");

        var presidio = await FetchPresidioAsync();

        if (presidio == null || presidio.PresidioFiles == null) 
        {
            return Tuple.Create(false, "Failed to validate Presidio. if this becomes a repeditive error, please contact the Nova support team. (Invalid)");
        }

        if (!Directory.Exists(presidioFolder))
            Directory.CreateDirectory(presidioFolder);

        foreach (var file in presidio.PresidioFiles)
        {
            if (!CompareHash(file, Path.Combine(presidioFolder, file.FileName)))
            {
                var InstallOut =  await InstallAsync(file, Path.Combine(presidioFolder, file.FileName));

                if (!InstallOut.Item1)
                    return InstallOut;
            }
        }

        return Tuple.Create(true, "");
    }
    private static async Task<bool> IsPresidioLocked(string filePath, int timeout)
    {
        var stopWatch = Stopwatch.StartNew();
        FileStream stream = null;

        while (stopWatch.ElapsedMilliseconds < timeout)
        {
            if (!File.Exists(filePath))
                return false;


            try
            {
                FileInfo file = new FileInfo(filePath);
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                Thread.Sleep(500);
                continue;
            }
            finally
            {
                stream?.Close();
            }

            return false;
        }

        if (!KillProcessAsAdmin("PresidioAgent.exe"))
        {
            return true;
        }

        await Task.Delay(600);
        try
        {
            FileInfo file = new FileInfo(filePath);
            stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        }
        catch (IOException)
        {
            return true;
        }
        finally
        {
            stream?.Close();
        }

        return false;
    }
    public static async Task<Tuple<bool, string>> InstallAsync(PresidioFile presidioFile, string filePath)
    {
        try
        {
            if (presidioFile == null)
            {
                Logger.Log(LogLevel.Error, "Failed to fetch presidio: presidioFile is null");
                return Tuple.Create(false, "Failed to fetch presidio");
            }

            Logger.Log(LogLevel.Debug, "Downloading Presidio file from URL: " + presidioFile.Url);

            if (await IsPresidioLocked(filePath, 30000))
            {
                Logger.Log(LogLevel.Error, "The file is in use: " + filePath);
                return Tuple.Create(false, "Looks like we are having trouble updating our presidio anticheat.");
            }

            using (WebClient webClient = new WebClient())
            {
                await webClient.DownloadFileTaskAsync(presidioFile.Url, filePath);
            }

            Logger.Log(LogLevel.Debug, "Download completed. File saved to: " + filePath);

            if (!CompareHash(presidioFile, filePath))
            {
                Logger.Log(LogLevel.Error, "Failed to validate Presidio. If this becomes a repetitive error, please contact the Nova support team.");
                return Tuple.Create(false, "Failed to validate Presidio. If this becomes a repetitive error, please contact the Nova support team.");
            }

            Logger.Log(LogLevel.Debug, "Presidio validation succeeded. SHA256 hash: " + ComputeSHA256Hash(File.ReadAllBytes(filePath)));

            return Tuple.Create(true, "");
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, "An error occurred: " + ex.Message);
            return Tuple.Create(false, "An error occurred while installing Presidio: " + ex.Message);
        }
    }
    public static bool KillProcessAsAdmin(string processName)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Verb = "runas",
            Arguments = $"/C taskkill /f /im {processName}",
            WindowStyle = ProcessWindowStyle.Hidden,
            UseShellExecute = true,
            CreateNoWindow = true
        };

        try
        {
            var process = new Process { StartInfo = psi };
            process.Start();
            return true;
        }
        catch
        {
            return false; 
        }
    }
    public static async Task<Presidio> FetchPresidioAsync()
    {
        var client = new RestClient($"{LauncherData.LauncherAPIUrl}:{LauncherData.LauncherAPIPort}");
        var request = new RestRequest("/api/launcher/presidio", Method.Get);

        var response = await client.ExecuteAsync(request);

        if (response.StatusCode == System.Net.HttpStatusCode.OK)
        {
            var jsonContent = response.Content;
            return JsonConvert.DeserializeObject<Presidio>(jsonContent);
        }
        else
        {
            return null;
        }
    }
    public static string ComputeSHA256Hash(byte[] inputBytes)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hashBytes = sha256.ComputeHash(inputBytes);

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                builder.Append(hashBytes[i].ToString("x2"));
            }

            return builder.ToString();
        }
    }
    public static bool CompareHash(PresidioFile presidioFile, string filePath)
    {
        if(!File.Exists(filePath))
            return false;

        string CurrentHash = string.Empty;

        try
        {
            CurrentHash = ComputeSHA256Hash(File.ReadAllBytes(filePath));
            Logger.Log(LogLevel.Debug, CurrentHash);

        }
        catch (Exception ex) 
        {
        
        }

        if(CurrentHash == presidioFile.FileHash)
            return true;

        return false;
    }
    public static async Task<(bool, Process)> StartAgent()
    {
        string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string presidioFolder = Path.Combine(localAppData, "Nova", "Presidio");

        string exePath = Path.Combine(presidioFolder, "PresidioAgent.exe");

        try
        {

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = exePath,
                UseShellExecute = true,
                CreateNoWindow = true,
                Verb = "runas"
            };
            Process childProcess;

            try
            {
                childProcess = Process.Start(startInfo);
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                return (false, null);
            }

            await Task.Delay(500);
            if (childProcess.HasExited)
            {
                return (false, null);
            }

            await Task.Delay(500);

            return (true, childProcess);
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, ex.Message);
            return (false, null);
        }
    }
}

public class Presidio
{
    public List<PresidioFile> PresidioFiles { get; set; }
}

public class PresidioFile
{
    public string FileName { get; set; }
    public string FileHash { get; set; }
    public string Url { get; set; }
}
