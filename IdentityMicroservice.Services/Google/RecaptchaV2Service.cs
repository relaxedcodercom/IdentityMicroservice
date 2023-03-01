using IdentityMicroservice.Services.Contracts.Google;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Net;

namespace IdentityMicroservice.Services.Google
{
    public class RecaptchaV2Service : IRecaptchaV2Service
    {
        private readonly IConfiguration configuration;
        private readonly IHttpClientFactory httpClientFactory;

        public RecaptchaV2Service(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            this.configuration = configuration;
            this.httpClientFactory = httpClientFactory;
        }

        public async Task<bool> ValidateReCaptchaResponse(string response)
        {
            var httpRequestMessage = new HttpRequestMessage(
            HttpMethod.Get,
            string.Format(configuration.GetSection("Authentication:ReCaptcha:v2:Url").Get<string>(),
                configuration.GetSection("Authentication:ReCaptcha:v2:PrivateKey").Get<string>(), response));

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

                        var data = JsonConvert.DeserializeObject<RecaptchaResponse>(jsonResponse);

                        return data.Success;
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

    public class RecaptchaResponse
    {
        public bool Success { get; set; }
    }
}
