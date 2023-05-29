using Microsoft.AspNetCore.Mvc;
using HtmlAgilityPack;
using DataParserService.Models;
using DataParserService.Services;
using DataParserService.Constants;
using System.Text.RegularExpressions;

namespace DataParserService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public partial class DataParserController : ControllerBase
    {
        private readonly HttpClient _client;
        private readonly IUserAgentProviderService _browser;
        private readonly IDataScrapperService _scrapper;

        public DataParserController(IProxyProviderService proxyProviderService, IUserAgentProviderService userAgentService, IDataScrapperService dataScrapperService)
        {
            _client = proxyProviderService.CreateClient();
            _browser = userAgentService;
            _scrapper = dataScrapperService;
        }

        // GET api/values/usb
        [HttpGet("{category}")]
        public async Task<ActionResult<IEnumerable<Post>>> GetDataByCategory(string category)
        {
            HashSet<Post> posts = new();
            string categoryFormatted = GetFormattedCategory(category);
            List<string> agents = await _browser.GetUserAgentsAsync(_client);
            string url = $"https://www.olx.ua/d/uk/list/q-{categoryFormatted}/";
            string htmlPage = await _client.GetStringAsync(url);
            HtmlDocument doc = new();
            doc.LoadHtml(htmlPage);

            string itemQuantityElement = doc.DocumentNode.SelectSingleNode(Xpath.QUANTITY).InnerText;
            HtmlNodeCollection paginationList = doc.DocumentNode.SelectNodes(Xpath.PAGINATIONLIST);

            if (int.TryParse(GetItemQuantityString(itemQuantityElement), out int quantityNumber))
            {
                if (quantityNumber != 0)
                {
                    int pages = GetPagesQuantity(paginationList);                   
                    ParseDataPages(posts, categoryFormatted, pages, agents);
                }
            }
            return Ok(posts);
        }

        private static string GetFormattedCategory(string category)
        {
            return CategoryPattern().Replace($"{category}", "").Replace(" ", "-");
        }
        private static string GetItemQuantityString(string itemQuantityElement)
        {
            if (string.IsNullOrEmpty(itemQuantityElement))
            {
                return "0";
            }

            Match match = ItemPattern().Match(itemQuantityElement);
            
            return match.Value;
        }

        private static int GetPagesQuantity(HtmlNodeCollection paginationList)
        {
            return paginationList != null ? Convert.ToInt32(paginationList.Last().InnerText) : 1;
        }

        private void ParseDataPages(HashSet<Post> posts, string categoryFormatted, int pages, List<string> agents)
        {
            Parallel.For(1, pages + 1, (page) =>
            {
                string userAgent = agents[Random.Shared.Next(agents.Count)];
                _scrapper.ParseDataPage(posts, categoryFormatted, page, userAgent);
            });
        }

        [GeneratedRegex("\\d{1,4}")]
        private static partial Regex ItemPattern();
        [GeneratedRegex("[^0-9A-Za-zА-ЯҐЄІЇа-яґєії ,]")]
        private static partial Regex CategoryPattern();
    }
}

