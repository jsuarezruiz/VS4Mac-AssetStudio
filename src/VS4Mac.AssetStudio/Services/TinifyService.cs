using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using VS4Mac.AssetStudio.Exceptions;
using VS4Mac.AssetStudio.Models;

namespace VS4Mac.AssetStudio.Services
{
    public class TinifyService : IDisposable
    {
        private const string ApiEndpoint = "https://api.tinify.com/shrink";

        internal static HttpClient _httpClient;
        internal static JsonSerializerSettings _jsonSettings;

        public string APIKey { get; private set; }

        /// <summary>
        /// Init the service with the specified apiKey.
        /// </summary>
        /// <param name="apiKey">Your tinypng.com API key, signup here: https://tinypng.com/developers </param>
        public void Init(string apiKey)
        {
            APIKey = apiKey;

            if (string.IsNullOrEmpty(APIKey))
                throw new ArgumentNullException(nameof(APIKey));

            // Create basic auth using api key formatting.
            var auth = $"api:{APIKey}";
            var authByteArray = System.Text.Encoding.ASCII.GetBytes(auth);
            var apiKeyEncoded = Convert.ToBase64String(authByteArray);

            _httpClient = _httpClient ?? new HttpClient();

            // Add auth to the headers.
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("basic", apiKeyEncoded);

            // Json settings  camelCase.
            _jsonSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore
            };

            _jsonSettings.Converters.Add(new StringEnumConverter { CamelCaseText = true });
        }

        /// <summary>
        /// Compress a file on disk.
        /// </summary>
        /// <param name="pathToFile">Path to the file.</param>
        /// <returns>TinyPngApiResult, <see cref="TinyPngApiResult"/></returns>
        public async Task<TinyPngApiResult> CompressAsync(string pathToFile)
        {
            if (string.IsNullOrEmpty(APIKey))
                throw new InvalidOperationException("You MUST call TinyService.Init (ApiKey); prior to using it.");

            if (string.IsNullOrEmpty(pathToFile))
                throw new ArgumentNullException(nameof(pathToFile));

            using (var file = File.OpenRead(pathToFile))
            {
                var response = await _httpClient.PostAsync(ApiEndpoint, new StreamContent(file)).ConfigureAwait(false);

                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();

                    TinyPngApiResult result = JsonConvert.DeserializeObject<TinyPngApiResult>(content);

                    return result;
                }

                // If the status code is not success, get the error information.
                var errorContent = await response.Content.ReadAsStringAsync();
                var errorMsg = JsonConvert.DeserializeObject<TinyPngApiErrorResponse>(errorContent);
                throw new TinyPngApiException(errorMsg.Error, errorMsg.Message);
            }
        }

        /// <summary>
        /// Download a compressed image to the specified path.
        /// </summary>
        /// <param name="url">Url to a compressed file.</param>
        /// <param name="path">Local path where download the file.</param>
        public void DownloadImage(string url, string path)
        {
            var webClient = new WebClient();

            // Download a compressed image.
            webClient.DownloadFile(
                url,
                path);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _httpClient?.Dispose();
                _httpClient = null;
            }
        }
    }
}