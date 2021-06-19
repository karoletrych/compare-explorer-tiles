using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using System.Collections.Generic;

namespace backend
{
    public class Token
    {

        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
    }

    public static class Auth
    {

        private static readonly HttpClient _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://www.strava.com"),
        };

        const int clientId = 62340;
        private static string appSecret = Environment.GetEnvironmentVariable("AppSecret", EnvironmentVariableTarget.Process);

        [FunctionName("Auth")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string baseUri = Environment.GetEnvironmentVariable("BaseUri", EnvironmentVariableTarget.Process);

            string code = req.GetQueryParameterDictionary()["code"];
            string tokenUrl = "/api/v3/oauth/token?" +
    $"client_id={clientId}&" +
    $"client_secret={appSecret}&" +
    $"code={code}&" +
    "grant_type=authorization_code";

            var response = await _httpClient.PostAsync(tokenUrl, null);
            await response.EnsureSuccessStatusCodeAsync();

            var result = await response.Content.ReadAsAsync<Token>();

            req.HttpContext.Response.Cookies.Append("token", result.AccessToken, new CookieOptions { Secure = true, SameSite = SameSiteMode.None });
            return new RedirectResult(baseUri);
        }

    }
}
