using System;
using System.IO;

namespace NovaLauncher.Models.Logger
{
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error,
        Fatal
    }

    public static class Logger
    {
        private static string logFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Nova", "Logs", "NovaLauncher.log");
        private static void CreateLogDirectory()
        {
            string logDirectory = Path.GetDirectoryName(logFilePath);
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }
        }

        private static void DeleteExistingLogFile()
        {
            if (File.Exists(logFilePath))
            {
                try
                {
                    File.Delete(logFilePath);
                }
                catch (Exception)
                {

                }
            }
        }


        public static void Log(LogLevel level, string message, Exception ex = null, bool bdelete = false)
        {
            CreateLogDirectory();

            if (bdelete == true)
                DeleteExistingLogFile();

            try
            {
                using (StreamWriter streamWriter = File.AppendText(logFilePath))
                {
                    streamWriter.WriteLine($"{DateTime.Now} [{level}] {message}");

                    if (ex != null)
                    {
                        streamWriter.WriteLine($"{DateTime.Now} [{level}] {ex.Message}");
                        streamWriter.WriteLine(ex.StackTrace);
                    }
                }
            }
            catch (Exception)
            {

            }
        }
    }
}
