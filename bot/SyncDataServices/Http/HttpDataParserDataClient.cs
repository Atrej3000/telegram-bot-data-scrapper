using Newtonsoft.Json;
using NuGet.Protocol;
using System.Text;

namespace bot.SyncDataServices.Http
{
	public class HttpDataParserDataClient : IDataParserDataClient
	{
		private readonly HttpClient _httpClient;
		private readonly IConfiguration _configuration;
		public HttpDataParserDataClient(HttpClient httpClient, IConfiguration configuration)
		{
			_httpClient = httpClient;
			_configuration = configuration;
		}
		public async Task<string> ParseData(string query)
		{
			var response = await _httpClient.GetStringAsync($"{_configuration["DataParserService"]}" + query);

			return response;
		}
	}
}
