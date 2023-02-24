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
            _browser.GetUserAgents(_client).GetAwaiter().GetResult();
        }

        // GET api/values/usb
        [HttpGet("{category}")]
        public async Task<ActionResult<IEnumerable<Post>>> GetDataByCategory(string category)
        {
            var posts = new HashSet<Post>();
            var categoryFormatted = CategoryPattern().Replace($"{category}", "").Replace(" ", "-");
            var agents = await _browser.GetUserAgents(_client);

            var url = $"https://www.olx.ua/d/uk/list/q-{categoryFormatted}/";
            var htmlPage = _client.GetStringAsync(url).GetAwaiter().GetResult();
            var doc = new HtmlDocument();
            doc.LoadHtml(htmlPage);

            var itemQuantityElement = doc.DocumentNode.SelectSingleNode(Xpath.QUANTITY).InnerText;
            var paginationList = doc.DocumentNode.SelectNodes(Xpath.PAGINATIONLIST);

            if (itemQuantityElement is not null)
            {
                Match match = ItemPattern().Match(itemQuantityElement);
                var quantityString = match.Value;
                if (!string.IsNullOrEmpty(quantityString))
                {
                    int quantityNumber = Convert.ToInt32(quantityString);
                    if (quantityNumber != 0)
                    {
                        if (paginationList is not null)
                        {
                            var pages = Convert.ToInt32(paginationList.Last().InnerText);
                            Parallel.For(1, pages + 1, page =>
                            {
                                var userAgent = agents[Random.Shared.Next(agents.Count)];
                                _scrapper.ParseDataPage(posts, categoryFormatted, page, userAgent);
                            });
                            return Ok(posts);
                        }
                        else
                        {
                            var userAgent = agents[Random.Shared.Next(agents.Count)];
                            _scrapper.ParseDataPage(posts, categoryFormatted, 1, userAgent);
                            return Ok(posts);
                        }
                    }
                    return Ok(posts);
                }
                return Ok(posts);
            }
            return Ok(posts);
        }
        [GeneratedRegex("\\d{1,4}")]
        private static partial Regex ItemPattern();
        [GeneratedRegex("[^0-9A-Za-z ,]")]
        private static partial Regex CategoryPattern();
    }
}

