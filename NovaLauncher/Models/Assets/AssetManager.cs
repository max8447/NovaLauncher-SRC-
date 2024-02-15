using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows;
using NovaLauncher.Models.Logger;
using System.Drawing;
using System.Windows.Interop;
using System.Windows.Media;

namespace NovaLauncher.Models.Assets
{
    public class AssetManager
    {
        public static async Task DownloadImagesAsync(JObject jsonObj, TextBlock progressTextBlock)
        {
            Logger.Logger.Log(LogLevel.Info, "Downloading images...");

            Application.Current.Dispatcher.Invoke(() =>
            {
                LauncherData.AssetData = jsonObj;
            });

            JArray todArray = (JArray)jsonObj["TOD"];
            JArray seasonImagesArray = (JArray)jsonObj["SeasonImages"];
            JArray extraArray = (JArray)jsonObj["Extra"];
            JArray allImagesArray = new JArray();
            allImagesArray.Merge(todArray);
            allImagesArray.Merge(seasonImagesArray);
            allImagesArray.Merge(extraArray);

            if (allImagesArray != null)
            {
                double percentPerFile = 100.0 / allImagesArray.Count;
                double currentDownload = 0.0;
                string destFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Nova", "Assets");
                Directory.CreateDirectory(destFolder);

                long totalSize = 0;
                long downloadedSize = 0;
                foreach (JObject imageObj in allImagesArray)
                {
                    string filename = (string)imageObj["filename"];
                    string url = (string)imageObj["url"];
                    long size = (long)imageObj["size"];

                    string destPath = Path.Combine(destFolder, filename);
                    if (!File.Exists(destPath) || new FileInfo(destPath).Length != size)
                    {
                        try
                        {
                            using (var client = new WebClient())
                            {
                                client.DownloadDataCompleted += (sender, e) =>
                                {
                                    if (e.Error != null)
                                    {
                                        Logger.Logger.Log(LogLevel.Error, $"Error downloading {filename}: {e.Error.Message}");
                                        progressTextBlock.Dispatcher.Invoke(() =>
                                            progressTextBlock.Text = $"Error downloading {filename}: {e.Error.Message}");
                                    }
                                    else
                                    {
                                        currentDownload += percentPerFile;
                                        progressTextBlock.Dispatcher.Invoke(() =>
                                            progressTextBlock.Text = $"Downloading Content {(int)Math.Floor(currentDownload)}%");
                                        File.WriteAllBytes(destPath, e.Result);
                                        totalSize += size;
                                    }
                                };

                                Logger.Logger.Log(LogLevel.Info, $"Downloading image: {filename}");
                                await client.DownloadDataTaskAsync(new Uri(url));
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Logger.Log(LogLevel.Error, $"Error downloading {filename}: {ex.Message}");
                            progressTextBlock.Dispatcher.Invoke(() =>
                                progressTextBlock.Text = $"Error downloading {filename}: {ex.Message}");
                        }
                    }
                    else
                    {
                        totalSize += size;
                        currentDownload += percentPerFile;
                        progressTextBlock.Dispatcher.Invoke(() =>
                            progressTextBlock.Text = $"Downloading Content {(int)Math.Floor(currentDownload)}%");
                    }
                }

                if (totalSize > 0)
                {
                    DriveInfo driveInfo = new DriveInfo(Path.GetPathRoot(destFolder));
                    long freeSpace = driveInfo.AvailableFreeSpace;
                    if (freeSpace < totalSize)
                    {
                        Logger.Logger.Log(LogLevel.Error, "Error: Not enough disk space to download assets");
                        progressTextBlock.Dispatcher.Invoke(() =>
                            progressTextBlock.Text = "Error: Not enough disk space to download assets");
                        return;
                    }
                }

                foreach (string file in Directory.GetFiles(destFolder))
                {
                    string filename = Path.GetFileName(file);
                    if (allImagesArray.FirstOrDefault(obj => (string)obj["filename"] == filename) == null)
                    {
                        try
                        {
                            Logger.Logger.Log(LogLevel.Info, $"Deleting file: {filename}");
                            File.Delete(file);
                        }
                        catch (Exception ex)
                        {
                            Logger.Logger.Log(LogLevel.Error, $"Error deleting {filename}: {ex.Message}");
                            progressTextBlock.Dispatcher.Invoke(() =>
                                progressTextBlock.Text = $"Error deleting {filename}: {ex.Message}");
                        }
                    }
                }
            }
        }

        public static ImageSource GetImageAsTime(string Time)
        {
            Time = Time.ToUpper();
            Bitmap image = null;
            switch (Time)
            {
                case "12.00AM":
                    image = Properties.Resources._12_00AM;
                    break;
                case "01.00AM":
                    image = Properties.Resources._01_00AM;
                    break;
                case "02.00AM":
                    image = Properties.Resources._02_00AM;
                    break;
                case "03.00AM":
                    image = Properties.Resources._03_00AM;
                    break;
                case "04.00AM":
                    image = Properties.Resources._04_00AM;
                    break;
                case "05.00AM":
                    image = Properties.Resources._05_00AM;
                    break;
                case "06.00AM":
                    image = Properties.Resources._06_00AM;
                    break;
                case "07.00AM":
                    image = Properties.Resources._07_00AM;
                    break;
                case "08.00AM":
                    image = Properties.Resources._08_00AM;
                    break;
                case "09.00AM":
                    image = Properties.Resources._09_00AM;
                    break;
                case "10.00AM":
                    image = Properties.Resources._10_00AM;
                    break;
                case "11.00AM":
                    image = Properties.Resources._11_00AM;
                    break;
                case "12.00PM":
                    image = Properties.Resources._12_00PM;
                    break;
                case "01.00PM":
                    image = Properties.Resources._01_00PM;
                    break;
                case "02.00PM":
                    image = Properties.Resources._02_00PM;
                    break;
                case "03.00PM":
                    image = Properties.Resources._03_00PM;
                    break;
                case "04.00PM":
                    image = Properties.Resources._04_00PM;
                    break;
                case "05.00PM":
                    image = Properties.Resources._05_00PM;
                    break;
                case "06.00PM":
                    image = Properties.Resources._06_00PM;
                    break;
                case "07.00PM":
                    image = Properties.Resources._07_00PM;
                    break;
                case "08.00PM":
                    image = Properties.Resources._08_00PM;
                    break;
                case "09.00PM":
                    image = Properties.Resources._09_00PM;
                    break;
                case "10.00PM":
                    image = Properties.Resources._10_00PM;
                    break;
                case "11.00PM":
                    image = Properties.Resources._11_00PM;
                    break;
                default:
                    image = Properties.Resources._05_00PM;
                    break;
            }

            Bitmap bitmap = image;

            ImageSource imageSource;
            imageSource = Imaging.CreateBitmapSourceFromHBitmap(
                bitmap.GetHbitmap(),
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions()
            );


            return imageSource;
        }

        public static ImageSource GetSeasonImage(int Season)
        {
            Bitmap image = null;
            switch (Season)
            {
                case 0:
                    image = Properties.Resources.season_0;
                    break;
                case 1:
                    image = Properties.Resources.season_1;
                    break;
                case 2:
                    image = Properties.Resources.season_2;
                    break;
                case 3:
                    image = Properties.Resources.season_3;
                    break;
                case 4:
                    image = Properties.Resources.season_4;
                    break;
                case 5:
                    image = Properties.Resources.season_5;
                    break;
                case 6:
                    image = Properties.Resources.season_6;
                    break;
                case 7:
                    image = Properties.Resources.season_7;
                    break;
                case 8:
                    image = Properties.Resources.season_8;
                    break;
                case 9:
                    image = Properties.Resources.season_9;
                    break;
                case 10:
                    image = Properties.Resources.season_10;
                    break;
                case 11:
                    image = Properties.Resources.season_11;
                    break;
                case 12:
                    image = Properties.Resources.season_12;
                    break;
                case 13:
                    image = Properties.Resources.season_13;
                    break;
                case 14:
                    image = Properties.Resources.season_14;
                    break;
                default:
                    image = Properties.Resources.season_10;
                    break;
            }

            Bitmap bitmap = image;

            ImageSource imageSource;
            imageSource = Imaging.CreateBitmapSourceFromHBitmap(
                bitmap.GetHbitmap(),
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions()
            );


            return imageSource;
        }

    }
}
