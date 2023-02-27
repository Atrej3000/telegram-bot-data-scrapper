using Newtonsoft.Json.Linq;

namespace bot.SyncDataServices.Http
{
	public class HttpNgrokDataClient : INgrokDataClient
	{
		private readonly HttpClient _httpClient;
		private readonly IConfiguration _configuration;
		public HttpNgrokDataClient(HttpClient httpClient, IConfiguration configuration)
		{
			_httpClient = httpClient;
			_configuration = configuration;
		}
		public async Task<string> GetNgrokUrl()
		{
			HttpResponseMessage response = await _httpClient.GetAsync($"{_configuration["ngrok"]}");

			// Parse the JSON response to extract the public URL of the first tunnel
			JObject jsonResponse = JObject.Parse(await response.Content.ReadAsStringAsync());
			string ngrokUrl = jsonResponse["tunnels"][0]["public_url"].ToString();
			return ngrokUrl;
		}
	}
}
