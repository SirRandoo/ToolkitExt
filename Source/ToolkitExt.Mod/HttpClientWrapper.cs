using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;
using Verse;

namespace ToolkitNxt.Mod
{
    [StaticConstructorOnStartup]
    internal class HttpClientWrapper
    {
        static RestClient _client;


        static HttpClientWrapper()
        {
            _client = new RestClient("https://tkx-toolkit.jumpingcrab.com/");
        }

        static internal async Task Post(string endpoint, string bearerToken = null, string json = null)
        {
            RestRequest request = new RestRequest(endpoint, Method.POST);

            if (bearerToken != null)
            {
                request.AddHeader("Authorization", "Bearer " + ToolkitExtSettings.token);
            }

            if (json != null)
            {
                request.AddHeader("Content-Type", "application/json");
                request.AddJsonBody(json);
            }


            IRestResponse<AuthResponse> response = await _client.ExecuteAsync<AuthResponse>(request);
            AuthenticationController.PusherAuthTokenRecieved(response.Data);
        }
    }

    public class AuthResponse
    {
        [JsonProperty("auth")]
        public string Auth { get; set; }
    }
}
