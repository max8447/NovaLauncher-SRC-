using Newtonsoft.Json;
using NovaLauncher.Models.AccountUtils.Classes;
using NovaLauncher.Models.API.NovaBackend.Classes;
using NovaLauncher.Models.Logger;
using NovaLauncher.Views.Pages;
using RestSharp;
using System;
using System.IO;
using System.Management;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace NovaLauncher.Models.AccountUtils
{
    public class AccountUtils
    {
        public static Account? MyLauncherAccount;
        
        private static RestRequest CreateLoginRequest(string email, string password)
        {
            var request = new RestRequest("/account/api/oauth/login", Method.Post);
            request.Timeout = -1;
            request.AddParameter("username", email);
            request.AddParameter("password", password);
            request.AddParameter("metadata", Encode(GetHWID()));
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            return request;
        }

        public class UserLoginResponse
        {
            public Account? account { get; set; }
            public bool success { get; set; }
            public string? error_message { get; set; }
        }

        public class ErrorMessage
        {
            public string errorCode { get; set; }
            public string errorMessage { get; set; }
            public int numericErrorCode { get; set; }
            public string error_description { get; set; }
            public string error { get; set; }
        }
        static string Encode(int[] HWID)
        {
            string res = string.Empty;
            foreach (var part in HWID)
                res += part.ToString().Length.ToString("00");

            foreach (var part in HWID)
                res += part.ToString();

            return Reverse(res);
        }
        public static string Reverse(string str)
        {
            char[] stringArray = str.ToCharArray();
            Array.Reverse(stringArray);
            return new string(stringArray);
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern uint GetVolumeInformation(
             string lpRootPathName,
             IntPtr lpVolumeNameBuffer,
             uint nVolumeNameSize,
             out uint lpVolumeSerialNumber,
             IntPtr lpMaximumComponentLength,
             IntPtr lpFileSystemFlags,
             IntPtr lpFileSystemNameBuffer,
             uint nFileSystemNameSize
         );

        public static int GetHash(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = sha256.ComputeHash(inputBytes);

                int hashValue = BitConverter.ToInt32(hashBytes, 0);
                return hashValue;
            }
        }

        static int[] GetHWID()
        {
            int[] parts = new int[4];
            parts[0] = GetHash("00000000");
            parts[1] = GetHash("00000000");
            parts[2] = GetHash("00000000");
            parts[3] = GetHash(string.Concat(parts[0].ToString(), parts[1].ToString(), parts[2].ToString()));
            return parts;
        }
        public static async Task<UserLoginResponse> LoginAsync(string email, string password)
        {
            Logger.Logger.Log(LogLevel.Info, "Performing login request...");
            var client = new RestClient($"{LauncherData.LauncherAPIUrl}:{LauncherData.LauncherAPIPort}");
            var request = CreateLoginRequest(email, password);
            request.Timeout = -1;
            if (Global.bDevMode)
            {
                request.AddHeader("X-Development-Servers", "true");
            }

            Logger.Logger.Log(LogLevel.Info, "Sending login request...");

            try
            {
                var response = await client.ExecuteAsync(request);

                if (!response.IsSuccessful)
                {
                    var failedresponse = JsonConvert.DeserializeObject<ErrorMessage>(response.Content);
                    if (failedresponse != null)
                    {
                        Logger.Logger.Log(LogLevel.Info, "Login request successful.");
                        return new UserLoginResponse
                        {
                            success = false,
                            error_message = failedresponse.errorMessage,
                            account = null
                        };
                    }
                    Logger.Logger.Log(LogLevel.Error, $"Login request failed: {response.ErrorMessage}");
                    throw new Exception($"Request failed: {response.ErrorMessage}");
                }

                var responseJson = response.Content;

                if (!string.IsNullOrEmpty(responseJson))
                {
                    var loginResponse = JsonConvert.DeserializeObject<Account>(responseJson);
                    if (loginResponse != null)
                    {
                        Logger.Logger.Log(LogLevel.Info, "Login request successful.");
                        return new UserLoginResponse
                        {
                            success = true,
                            account = loginResponse
                        };
                    }
                }

                return new UserLoginResponse
                {
                    success = false,
                    error_message = "Failed to parse response from the server.",
                    account = null
                };
            }
            catch (Exception ex)
            {
                Logger.Logger.Log(LogLevel.Error, $"Failed to connect to the endpoint: {ex.Message}");
                return new UserLoginResponse
                {
                    success = false,
                    error_message = "Failed to contact launcher services.",
                    account = null
                };
            }
        }

        private static RestRequest CreateTOSRequest(string password)
        {
            var request = new RestRequest("/account/api/tos/accept", Method.Post);
            request.AddHeader("Authorization", $"bearer {password}");
            request.Timeout = 20000;
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            return request;
        }
        public static async Task AcceptTOSAsync(string authToken)
        {
            try
            {
                var client = new RestClient($"{LauncherData.LauncherAPIUrl}:{LauncherData.LauncherAPIPort}");

                var request = CreateTOSRequest(authToken);

                var response = await client.ExecuteAsync(request);
            }
            catch (Exception ex)
            {
                Logger.Logger.Log(LogLevel.Error, ex.Message);
            }
        }


        public static async Task<Models.AccountUtils.Classes.User?> LoadUserSettingsAsync()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string NovaPath = Path.Combine(appData, "Nova", "User");
            string savePath = Path.Combine(NovaPath, "save.json");

            if (!File.Exists(savePath))
            {
                return null;
            }

            string fileContent = string.Empty;
            try
            {
                using (StreamReader reader = new StreamReader(savePath))
                {
                    fileContent = await reader.ReadToEndAsync();
                }
            }
            catch (Exception)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(fileContent))
            {
                return null;
            }

            try
            {
                var user = JsonConvert.DeserializeObject<Models.AccountUtils.Classes.User>(fileContent);
                return user;
            }
            catch (JsonException)
            {
                return null;
            }
        }


        public static string ES(string plainText)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Convert.FromBase64String("5cHo71aZgr0keV/os6w1fYh8tsehFfj6XIGAe8t7K24=");
                aesAlg.IV = new byte[16];

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
                        {
                            streamWriter.Write(plainText);
                        }
                        byte[] encryptedBytes = memoryStream.ToArray();
                        return Convert.ToBase64String(encryptedBytes);
                    }
                }
            }
        }
        public static async Task<bool> IsUserLoggedInAsync()
        {
            try
            {
                var userSettings = await LoadUserSettingsAsync();

                if (userSettings == null)
                    return false;

                return !string.IsNullOrEmpty(userSettings.Email) && !string.IsNullOrEmpty(userSettings.Password);
            }
            catch (Exception)
            {
                return false;
            }
        }


        public static void SaveNewUser(string Email, string Password)
        {
            User newUser = new User()
            {
                Email = Email,
                Password = Password
            };

            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string NovaPath = Path.Combine(appData, "Nova", "User");

            if (!Directory.Exists(appData + "\\Nova"))
                Directory.CreateDirectory(appData + "\\Nova");

            if (!Directory.Exists(NovaPath))
                Directory.CreateDirectory(NovaPath);

            string savePath = Path.Combine(NovaPath, "save.json");

            if (System.IO.File.Exists(savePath))
            {
                System.IO.File.Delete(savePath);
            }

            if (!System.IO.File.Exists(savePath))
            {
                System.IO.File.Create(savePath).Dispose();
            }

            System.IO.File.WriteAllText(savePath, JsonConvert.SerializeObject(newUser));
        }
    }
}
