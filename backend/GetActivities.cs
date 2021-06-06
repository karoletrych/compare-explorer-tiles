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
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace backend
{
   

    public static class HttpResponseMessageExtensions
    {
        public static async Task EnsureSuccessStatusCodeAsync(this HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            var content = await response.Content.ReadAsStringAsync();

            if (response.Content != null)
                response.Content.Dispose();

            throw new SimpleHttpResponseException(response.StatusCode, content);
        }
    }

    public class SimpleHttpResponseException : Exception
    {
        public HttpStatusCode StatusCode { get; private set; }

        public SimpleHttpResponseException(HttpStatusCode statusCode, string content) : base(content)
        {
            StatusCode = statusCode;
        }
    }

    public class Point
    {
        public double lat { get; set; }
        public double lng { get; set; }
    }

    public class Tile
    {

        public int X { get; set; }
        public int Y { get; set; }
        static readonly double DIV = Math.Pow(2, 14);

        double Deg2Rad(double t)
        {
            return t * (Math.PI / 180);
        }

        public override bool Equals(object obj)
        {
            return obj is Tile tile &&
                   X == tile.X &&
                   Y == tile.Y;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

        public Tile(Point point)
        {
            X = (int)(((point.lng + 180) / 360) * DIV);
            Y = (int)(((1 - Math.Log(Math.Tan(Deg2Rad(point.lat)) + 1 / Math.Cos(Deg2Rad(point.lat))) / Math.PI) / 2) * DIV);
        }
    }

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

        // [JsonProperty("map")]
        // public Map Map { get; set; }
    }


    public static class GetActivities
    {
        private static readonly HttpClient _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://www.strava.com"),
        };

        public static IEnumerable<Point> Decode(string polylineString)
        {
            if (string.IsNullOrEmpty(polylineString))
                throw new ArgumentNullException(nameof(polylineString));

            var polylineChars = polylineString.ToCharArray();
            var index = 0;

            var currentLat = 0;
            var currentLng = 0;

            while (index < polylineChars.Length)
            {
                // Next lat
                var sum = 0;
                var shifter = 0;
                int nextFiveBits;
                do
                {
                    nextFiveBits = polylineChars[index++] - 63;
                    sum |= (nextFiveBits & 31) << shifter;
                    shifter += 5;
                } while (nextFiveBits >= 32 && index < polylineChars.Length);

                if (index >= polylineChars.Length)
                    break;

                currentLat += (sum & 1) == 1 ? ~(sum >> 1) : (sum >> 1);

                // Next lng
                sum = 0;
                shifter = 0;
                do
                {
                    nextFiveBits = polylineChars[index++] - 63;
                    sum |= (nextFiveBits & 31) << shifter;
                    shifter += 5;
                } while (nextFiveBits >= 32 && index < polylineChars.Length);

                if (index >= polylineChars.Length && nextFiveBits >= 32)
                    break;

                currentLng += (sum & 1) == 1 ? ~(sum >> 1) : (sum >> 1);

                yield return new Point
                {
                    lat = Convert.ToDouble(currentLat) / 1E5,
                    lng = Convert.ToDouble(currentLng) / 1E5
                };
            }
        }

        [FunctionName("GetActivities")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string token = req.Cookies["token"];

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.GetAsync("/api/v3/athlete/activities?per_page=200");
            await response.EnsureSuccessStatusCodeAsync();
            var result = await response.Content.ReadAsAsync<Activity[]>();

            // var points = await Task.WhenAll(result.Select(async x =>
            // {
            //     var response2 = await _httpClient.GetAsync($"/api/v3/activities/{x.Id}");
            //     await response2.EnsureSuccessStatusCodeAsync();

            //     var result2 = await response2.Content.ReadAsAsync<Activity>();

            //     if (string.IsNullOrEmpty(result2?.Map?.Polyline))
            //     {
            //         return null;
            //     }
            //     return Decode(result2.Map.Polyline);
            // }));

            // var nonnullpoints = points.Where(x => x != null);
            // var tiles = nonnullpoints.SelectMany(x => x.Select(y => new Tile(y))).Distinct();

            var nonnullActivities = result.Where(x => x.Map.Polyline != null);
            var points = nonnullActivities.Select(a => Decode(a.Map.Polyline));
            var tiles = points.SelectMany(x => x.Select(y => new Tile(y))).Distinct();

            req.HttpContext.Response.Headers.Add("Access-Control-Allow-Credentials", "true");
            return new OkObjectResult(new
            {
                routes = points,
                tiles = tiles

                // points = result,
                // Tiles = tiles
            });
        }
    }
}
