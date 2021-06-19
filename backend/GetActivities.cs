using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Linq;

namespace backend
{
    public class Map
    {

        [JsonProperty("summary_polyline")]
        // [JsonProperty("polyline")]
        public string Polyline { get; set; }
    }

    public class Activity
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("map")]
        public Map Map { get; set; }
    }

    public static class GetActivities
    {
        private static readonly HttpClient _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://www.strava.com"),
        };


        [FunctionName("GetActivities")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string baseUri = Environment.GetEnvironmentVariable("BaseUri", EnvironmentVariableTarget.Process);

            string token = req.Cookies["token"];

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.GetAsync("/api/v3/athlete/activities?per_page=200");
            await response.EnsureSuccessStatusCodeAsync();
            var result = await response.Content.ReadAsAsync<Activity[]>();

            var nonnullActivities = result.Where(x => x.Map.Polyline != null);
            var points = nonnullActivities.Select(a => CoordsDecoder.Decode(a.Map.Polyline));
            var tiles = points.SelectMany(x => x.Select(y => new Tile(y))).Distinct();

            req.HttpContext.Response.Headers.Add("Access-Control-Allow-Credentials", "true");
            req.HttpContext.Response.Headers.Add("Access-Control-Allow-Origin", baseUri);
            return new OkObjectResult(new
            {
                routes = points,
                tiles = tiles
            });
        }
    }
}
