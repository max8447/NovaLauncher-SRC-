using NovaLauncher.Models.Assets;
using NovaLauncher.Models.Logger;
using System;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NovaLauncher.Models.ClockManager
{
    public class TodAsset
    {
        public string filename { get; set; }
        public string url { get; set; }
        public int size { get; set; }
    }

    public class LauncherDataAsset
    {
        public string version { get; set; }
        public TodAsset[] TOD { get; set; }
    }

    public class FortniteClock
    {
        public static ImageBrush NovaTod;
        public static TextBlock _fortniteCounter;
        public static TextBlock Versiontxt;


        public static void LoadClock(TextBlock fortniteCounter, ImageBrush NovaTODimage, bool bLoadVersion = false)
        {
            try
            {
                DateTime currentTime = DateTime.Now;

                fortniteCounter.Dispatcher.Invoke(() => {
                    fortniteCounter.Text = currentTime.ToString("h:mm:ss tt");
                });

                NovaTod = NovaTODimage;
                if (LauncherData.AssetData != null)
                {
                    Application.Current.Dispatcher.Invoke(() => {
                        var imageObject = GetImageObject();
                        if (imageObject != null)
                        {
                            if (imageObject != null)
                            {
                                NovaTod.ImageSource = imageObject;
                            }
                        }
                        else
                        {

                        }
                    });
                }

                _fortniteCounter = fortniteCounter;
                Timer timer = new Timer();
                timer.Interval = 1000;
                timer.Elapsed += Timer_Elapsed;
                timer.Start();
            }
            catch (Exception ex)
            {
                string errorMessage = "An error occurred: " + ex.Message + "\n\nStackTrace: " + ex.StackTrace;
                Application.Current.Dispatcher.Invoke(() => { MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error); });
            }
        }

        public static ImageSource GetImageObject()
        {
            DateTime currentTime = DateTime.Now;

            string timeString = currentTime.ToString("hh.00tt");
            return AssetManager.GetImageAsTime(timeString);
        }


        public static void LoadTODVersion(TextBlock fortniteCounter, ImageBrush NovaTODimage)
        {
            string Version = Global.GetCurrentLauncherVersion();

            fortniteCounter.Dispatcher.Invoke(() => {
                fortniteCounter.Text = $"Nova News";
            });

            Versiontxt = fortniteCounter;
            Timer timer = new Timer();
            timer = new Timer();
            timer.Interval = 1000;
            timer.Elapsed += LoadVersion;
            timer.Start();
        }

        private static int refreshTime = 999;
        public static void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            DateTime currentTime = DateTime.Now;
            int hour = currentTime.Hour;
            int minute = currentTime.Minute;
            int imageIndex = hour % 3;

            if (imageIndex != refreshTime)
            {
                try
                {
                    Application.Current.Dispatcher.Invoke(() => {
                        var imageSource = GetImageObject();

                        if (imageSource != null)
                        {
                            NovaTod.ImageSource = imageSource;
                        }
                    });

                    refreshTime = imageIndex;
                }
                catch (Exception ex)
                {
                    Logger.Logger.Log(LogLevel.Error, ex.Message);
                }
            }

            try
            {
                _fortniteCounter.Dispatcher.Invoke(() => {
                    _fortniteCounter.Text = currentTime.ToString("h:mm:ss tt");
                });
            }
            catch (Exception ex) { }
        }

        public static void LoadVersion(object sender, ElapsedEventArgs e)
        {
            DateTime currentTime = DateTime.Now;
            int hour = currentTime.Hour;
            int minute = currentTime.Minute;
            int imageIndex = hour % 3;

            if (imageIndex != refreshTime)
            {
                try
                {
                    Application.Current.Dispatcher.Invoke(() => {
                        var imageSource = GetImageObject();

                        if (imageSource != null)
                        {
                            NovaTod.ImageSource = imageSource;
                        }
                        else
                        {

                        }
                    });

                    refreshTime = imageIndex;
                }
                catch { }
            }

            string Version = Global.GetCurrentLauncherVersion();

            try
            {
                Versiontxt.Dispatcher.Invoke(() => {
                    Versiontxt.Text = $"Nova News";
                });
            }
            catch (Exception ex) { }
        }

    }
}
