using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DataParserService.Services
{
    public class UserAgentProviderService : IUserAgentProviderService
    {
        public async Task<List<string>> GetUserAgentsAsync(HttpClient httpClient)
        {
            string response = await httpClient.GetStringAsync("https://www.useragents.me/api");
            Root? root = JsonSerializer.Deserialize<Root>(response);
            if (root != null)
            {
                return root.data.Select(d => d.ua).ToList();
            }
            return new List<string>();
        }

        class UserAgent
        {
            public string ua { get; set; }
            public double pct { get; set; }
        }

        class Root
        {
            public List<UserAgent> data { get; set; } = null!;
        }
    }
}

