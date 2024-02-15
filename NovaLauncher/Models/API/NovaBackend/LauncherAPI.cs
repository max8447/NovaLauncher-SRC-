using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NovaLauncher.Models.Logger;
using NovaLauncher.Views.Controls;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows.Controls;
using static NovaLauncher.Models.AccountUtils.AccountUtils;

namespace NovaLauncher.Models.API.NovaBackend
{
    public class Asset
    {
        public string? FilePath { get; set; }
        public long Size { get; set; }
    }

    public class ExtraAssets
    {
        public string? FilePath { get; set; }
        public string? Url { get; set; }
        public long Size { get; set; }
    }

    public class Build
    {
        public string? BuildVersionString { get; set; }
        public string? DownloadURL { get; set; }
        public List<Asset>? Assets { get; set; }
        public List<Asset>? ExcludedAssets { get; set; }
        public List<ExtraAssets>? ExtraAssets { get; set; }
    }

    public class RootObject
    {
        public List<Build>? Builds { get; set; }
    }

    public class ResultObject
    {
        public bool? Success { get; set; }
        public Build? Build { get; set; }
        public string? ErrorMessage { get; set; }
    }

	public class UpdateResultObject
	{
		public bool? Success { get; set; }
		public string? ErrorMessage { get; set; }
	}
    public class LauncherAPI
    {
        public static async Task<ResultObject> BuildVerifyEndpointAsync(string buildString)
        {
            if (buildString.Contains("Season"))
                buildString = buildString.Replace("Season", "Build");

            var client = new RestClient($"{LauncherData.LauncherAPIUrl}:{LauncherData.LauncherAPIPort}");
            var request = new RestRequest("/api/build-info/verify", Method.Post);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("X-Builds-Verify", buildString);
            request.AddHeader("X-User-Access", LauncherData.GetGetUserInfo().access_token);
            request.AddParameter("application/json", buildString, ParameterType.RequestBody);

            var response = await client.ExecuteAsync(request);
            if (response.IsSuccessful)
            {
                return JsonConvert.DeserializeObject<ResultObject>(response.Content);
            }
            else
            {
                return new ResultObject()
                {
                    Success = false,
                    Build = null,
                    ErrorMessage = "Failed to contact launcher services, please try again later."
                };
            }
        }
        public class ApiResponse
        {
            public bool bSuccess { get; set; }
            public string ErrorMessage { get; set; }
        }
        public static async Task<UpdateResultObject> UpdateServerAsync(string json)
        {
            var client = new RestClient($"{LauncherData.LauncherAPIUrl}:{LauncherData.LauncherAPIPort}");
            var request = new RestRequest("/api/launcher/user-update-server", Method.Post);
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("application/json", json, ParameterType.RequestBody);

            var response = await client.ExecuteAsync(request);

            if (response.IsSuccessful)
            {
                return new UpdateResultObject()
                {
                    Success = true,
                    ErrorMessage = null
                };
            }
            else
            {
                var errorResponse = JsonConvert.DeserializeObject<ApiResponse>(response.Content);
                return new UpdateResultObject()
                {
                    Success = false,
                    ErrorMessage = errorResponse?.ErrorMessage
                };
            }
        }


        public static async Task<JObject?> GetLauncherInfoAsync(int maxRetries, TextBlock errorTextBlock)
        {
            Logger.Logger.Log(LogLevel.Info, "Contacting Launcher Services...");

            errorTextBlock.Dispatcher.Invoke(() =>
            {
                errorTextBlock.Text = "Contacting Launcher Services...";
            });

            if(Global.bNoMCP)
            {
                string Json = $@"{{""LauncherVersion"": ""{Global.LauncherVersion}"",""Launcher"": """",""Installer"": """",""Assets"": """",""News"": {{""NewsId"": ""00000000"",""LauncherUpdates"": [{{""title"": ""Offline"",""description"": ""You are currently on offline mode, some features may be limited"",""url"": """"}}],""ServerUpdates"": []}}}}";
                return JObject.Parse(Json);
            }

            var client = new RestClient($"{LauncherData.LauncherAPIUrl}:{LauncherData.LauncherAPIPort}");
            var request = new RestRequest("/api/launcher-info", Method.Get);
            request.Timeout = 15000;

            int retryCount = 0;

            while (retryCount < maxRetries)
            {
                var response = await client.ExecuteAsync(request);
                int statusCode = Convert.ToInt32(response.StatusCode);
                if (statusCode == 200)
                {
                    Logger.Logger.Log(LogLevel.Info, "Successfully retrieved Launcher info.");
                    var content = response.Content;
                    if (content != null)
                        return JObject.Parse(content);
                }


                Logger.Logger.Log(LogLevel.Warning, $"Failed to retrieve Launcher info. Retrying... (Retry Count: {retryCount + 1})");
                retryCount++;

                for (int i = 10; i > 0; i--)
                {
                    errorTextBlock.Dispatcher.Invoke(() =>
                    {
                        errorTextBlock.Text = $"Connection failed. Retrying in {i} seconds...";
                    });

                    await Task.Delay(1000);
                }
            }

            Logger.Logger.Log(LogLevel.Error, "Failed to connect to the server.");
            errorTextBlock.Dispatcher.Invoke(() =>
            {
                errorTextBlock.Text = "Failed to connect to the server.";
            });

            return null;
        }

        public static async Task<List<NovaStoreItem>?> GetStoreInfo()
        {
            try
            {
                var client = new RestClient($"{LauncherData.LauncherAPIUrl}:{LauncherData.LauncherAPIPort}");
                var accessToken = LauncherData.GetGetUserInfo().access_token;

                var request = new RestRequest($"/api/{accessToken}/launcher-store", Method.Get);
                request.Timeout = 15000;

                var response = await client.ExecuteAsync(request);

                if (response.IsSuccessful)
                {
                    var jsonResponse = response.Content;
                    var storeItems = JsonConvert.DeserializeObject<List<NovaStoreItem>>(jsonResponse);

                    return storeItems;
                }
                else
                {

                }
            }
            catch (Exception ex)
            {

            }

            return null;
        }


        public static string ES(string plainText)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Convert.FromBase64String("noIH8xPpFe/J/oecmyF++QNGpZFPmEAyX/TkRkRhhWY=");
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

        public static async Task<JObject?> GetAssetsAsObjectAsync()
        {
            Logger.Logger.Log(LogLevel.Info, "Fetching assets from Launcher API...");

            var client = new RestClient($"{LauncherData.LauncherAPIUrl}:{LauncherData.LauncherAPIPort}");
            var request = new RestRequest("/api/launcher-assets", Method.Post);
            var response = await client.ExecuteAsync(request);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                Logger.Logger.Log(LogLevel.Error, "Failed to fetch assets from Launcher API. Response status code: " + response.StatusCode);
                return null;
            }

            string json = response.Content;

            if (string.IsNullOrEmpty(json))
            {
                Logger.Logger.Log(LogLevel.Error, "Empty or null asset data received from Launcher API.");
                return null;
            }

            Logger.Logger.Log(LogLevel.Info, "Successfully fetched assets from Launcher API.");
            JObject jsonData = JObject.Parse(json);
            return jsonData;
        }


        public static async Task<ACSeasonData?> GetLauncherInfoAsyncACAsync()
        {
            try
            {
                var client = new RestClient($"{LauncherData.LauncherAPIUrl}:{LauncherData.LauncherAPIPort}");
                var request = new RestRequest("/api/launcher-info/game/ac", Method.Get);
                request.AddHeader("Content-Type", "application/json");

                var response = await client.ExecuteAsync(request);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    if (response.Content != null)
                    {
                        string content = response.Content;
                        ACSeasonData? seasonData = JsonConvert.DeserializeObject<ACSeasonData>(content);
                        return seasonData;
                    }

                    return null;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
                return null;
            }
        }

        public class S10Client
        {
            public string? url { get; set; }
            public string? fileHash { get; set; }
        }

        public static async Task<S10Client?> GetS10ClientAsync()
        {
            try
            {
                var client = new RestClient($"{LauncherData.LauncherAPIUrl}:{LauncherData.LauncherAPIPort}");
                var request = new RestRequest("/api/launcher-info/game/10_40/client", Method.Get);
                request.AddHeader("Content-Type", "application/json");

                var response = await client.ExecuteAsync(request);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    if (response.Content != null)
                    {
                        S10Client? seasonData = JsonConvert.DeserializeObject<S10Client>(response.Content);
                        return seasonData;
                    }
                    return null;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
                return null;
            }
        }

        private static RestRequest CreateTokenRequest(string bearerToken)
        {
            var request = new RestRequest("/account/api/oauth/exchange", Method.Post);
            request.AddHeader("Authorization", $"bearer {bearerToken}");
            request.Timeout = 20000;
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            return request;
        }

        public class LoginTokenResponse
        {
            public string Token { get; set; }
            public string p { get; set; }
            public bool Success { get; set; }
            public string Error { get; set; }
        }
        public class TokenResponse
        {
            public string? Token { get; set; }
            public string? p { get; set; }
        }
        public static async Task<LoginTokenResponse> GetETokenAsync(string password)
        {
            try
            {
                RestClient client = new RestClient($"{LauncherData.LauncherAPIUrl}:{LauncherData.LauncherAPIPort}");
                RestRequest request = CreateTokenRequest(password);
                request.AddHeader("Content-Type", "application/json");

                if (Global.bDevMode)
                {
                    request.AddHeader("X-Development-Servers", "true");
                }

                Logger.Logger.Log(LogLevel.Info, "Sending login request...");
                var response = await client.ExecuteAsync(request);
                if(response.IsSuccessStatusCode)
                {
                    try
                    {
                        return new LoginTokenResponse
                        {
                            Success = true,
                            Token = JsonConvert.DeserializeObject<TokenResponse>(response.Content).Token,
                            p = JsonConvert.DeserializeObject<TokenResponse>(response.Content).p
                        };
                    }
                    catch (Exception ex) 
                    {
                        return new LoginTokenResponse
                        {
                            Success = false,
                            Error = "Failed to connect to services."
                        };
                    }
                }
                else
                {
                    try
                    {
                        var failedresponse = JsonConvert.DeserializeObject<ErrorMessage>(response.Content);
                        if (failedresponse != null)
                        {
                            return new LoginTokenResponse
                            {
                                Success = false,
                                Error = failedresponse.errorMessage
                            };
                        }
                    }
                    catch
                    {
                    
                    
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return new LoginTokenResponse
            {
                Success = false,
                Error = "Failed to connect to services."
            };
        }

        public static async Task LogoutAsync(string password)
        {
            try
            {
                RestClient client = new RestClient($"{LauncherData.LauncherAPIUrl}:{LauncherData.LauncherAPIPort}");
                RestRequest request = new RestRequest("/launcher/user/logout", Method.Post);
                request.AddHeader("Content-Type", "application/json");

                if (Global.bDevMode)
                {
                    request.AddHeader("X-Development-Servers", "true");
                }

                var requestBody = new { password = password };
                var requestBodyJson = JsonConvert.SerializeObject(requestBody);
                request.AddParameter("application/json", requestBodyJson, ParameterType.RequestBody);
                Logger.Logger.Log(LogLevel.Info, "Sending logout request...");
                await client.ExecuteAsync(request);
            }
            catch
            {

            }
        }


    }
    public class ACSeasonData
    {
        public string? seasonId { get; set; }
        public string? url { get; set; }
        public string? fileHash { get; set; }
    }
}
