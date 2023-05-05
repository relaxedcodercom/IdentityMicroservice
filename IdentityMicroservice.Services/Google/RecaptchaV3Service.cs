using IdentityMicroservice.Services.Contracts.Google;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Net;

namespace IdentityMicroservice.Services.Google
{
    public class RecaptchaV3Service : IRecaptchaV3Service
    {
        private readonly IConfiguration configuration;
        private readonly IHttpClientFactory httpClientFactory;

        public RecaptchaV3Service(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            this.configuration = configuration;
            this.httpClientFactory = httpClientFactory;
        }

        public async Task<bool> ValidateReCaptchaResponse(string response)
        {
            var httpRequestMessage = new HttpRequestMessage(
            HttpMethod.Get,
            string.Format(configuration.GetSection("Authentication:ReCaptcha:v3:Url").Get<string>(),
                configuration.GetSection("Authentication:ReCaptcha:v3:PrivateKey").Get<string>(), response));

            var httpClient = httpClientFactory.CreateClient();

            try
            {
                var httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    using var contentStream = await httpResponseMessage.Content.ReadAsStreamAsync();

                    using (var readStream = new StreamReader(contentStream))
                    {
                        var jsonResponse = readStream.ReadToEnd();

                        var data = JsonConvert.DeserializeObject<RecaptchaV3Response>(jsonResponse);

                        return data.Success && data.Score >= 0.5;
                    }
                }

                return false;
            }
            catch (WebException)
            {
                return false;
            }
        }
    }

    public class RecaptchaV3Response
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("score")]
        public float Score { get; set; }

        [JsonProperty("action")]
        public string Action { get; set; }

        [JsonProperty("challenge_ts")]
        public DateTime ChallengeTs { get; set; }

        [JsonProperty("hostname")]
        public string HostName { get; set; }

        [JsonProperty("error-codes")]
        public string[] ErrorCodes { get; set; }
    }
}
