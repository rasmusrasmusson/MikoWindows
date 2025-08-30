using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MikoMe.Services
{
    /// <summary>
    /// Azure Translator helper (v3). Reads credentials from environment variables:
    /// AZURE_TRANSLATOR_KEY, AZURE_TRANSLATOR_REGION, (optional) AZURE_TRANSLATOR_ENDPOINT
    /// </summary>
    public static class TranslatorService
    {
        private static readonly string Key = ReadEnv("AZURE_TRANSLATOR_KEY");
        private static readonly string Region = ReadEnv("AZURE_TRANSLATOR_REGION");
        private static readonly string Endpoint =
            Environment.GetEnvironmentVariable("AZURE_TRANSLATOR_ENDPOINT",
                                               EnvironmentVariableTarget.Process)
            ?? "https://api.cognitive.microsofttranslator.com";

        private static string ReadEnv(string name) =>
            // process > user > machine (first non-empty wins)
            Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process)
            ?? Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.User)
            ?? Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Machine)
            ?? string.Empty;

        private static readonly HttpClient _http = CreateHttp();

        private static HttpClient CreateHttp()
        {
            if (string.IsNullOrWhiteSpace(Key) || string.IsNullOrWhiteSpace(Region))
            {
                // Fail early with a helpful message instead of a confusing 401 later.
                throw new InvalidOperationException(
                    "Azure Translator credentials are missing. " +
                    "Set AZURE_TRANSLATOR_KEY and AZURE_TRANSLATOR_REGION " +
                    "as environment variables and restart Visual Studio.");
            }

            var http = new HttpClient();
            http.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", Key);
            http.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Region", Region);
            return http;
        }

        // --- existing API (no changes for the rest of your app) ---

        public static async Task<string> TransliterateToPinyinAsync(string hanzi)
        {
            if (string.IsNullOrWhiteSpace(hanzi)) return string.Empty;
            var route = "/transliterate?api-version=3.0&language=zh-Hans&fromScript=Hans&toScript=Latn";
            var body = new object[] { new { Text = hanzi } };
            using var resp = await _http.PostAsync(Endpoint + route,
                new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json"));
            resp.EnsureSuccessStatusCode();
            using var doc = await JsonDocument.ParseAsync(await resp.Content.ReadAsStreamAsync());
            return doc.RootElement[0].GetProperty("text").GetString() ?? string.Empty;
        }

        public static async Task<string> TranslateToEnglishAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;
            var route = "/translate?api-version=3.0&from=zh-Hans&to=en";
            var body = new object[] { new { Text = text } };
            using var resp = await _http.PostAsync(Endpoint + route,
                new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json"));
            resp.EnsureSuccessStatusCode();
            using var doc = await JsonDocument.ParseAsync(await resp.Content.ReadAsStreamAsync());
            return doc.RootElement[0].GetProperty("translations")[0].GetProperty("text").GetString() ?? string.Empty;
        }

        public static async Task<string> TranslateToChineseAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;
            var route = "/translate?api-version=3.0&from=en&to=zh-Hans";
            var body = new object[] { new { Text = text } };
            using var resp = await _http.PostAsync(Endpoint + route,
                new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json"));
            resp.EnsureSuccessStatusCode();
            using var doc = await JsonDocument.ParseAsync(await resp.Content.ReadAsStreamAsync());
            return doc.RootElement[0].GetProperty("translations")[0].GetProperty("text").GetString() ?? string.Empty;
        }
    }
}
