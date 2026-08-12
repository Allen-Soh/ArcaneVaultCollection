//Name: Allen Soh Jun Hong
//AdminNo: 241309M
//Tut Group: IT2814 07

using System.Text.Json;

namespace ArcaneVaultRP.Services
{
    public interface IRecaptchaService
    {
        Task<bool> VerifyAsync(string? token);
    }

    public class RecaptchaService : IRecaptchaService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public RecaptchaService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<bool> VerifyAsync(string? token)
        {
            if (string.IsNullOrWhiteSpace(token)) return false;

            var secretKey = _configuration["ReCaptcha:SecretKey"];
            var client = _httpClientFactory.CreateClient();

            var response = await client.PostAsync(
                "https://www.google.com/recaptcha/api/siteverify",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["secret"] = secretKey!,
                    ["response"] = token
                }));

            if (!response.IsSuccessStatusCode) return false;

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.TryGetProperty("success", out var success) && success.GetBoolean();
        }
    }
}