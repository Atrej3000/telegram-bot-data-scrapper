using Microsoft.AspNetCore.Mvc;
using HtmlAgilityPack;
using DataParserService.Models;
using DataParserService.Services;
using DataParserService.Constants;

namespace DataParserService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DataParserController : ControllerBase
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
            var posts = new List<Post>();
            var categoryFormatted = category.Replace(" ", "-");
            var agents = await _browser.GetUserAgents(_client);

            Parallel.For(1, Xpath.PAGES + 1, page =>
            {
                var userAgent = agents[Random.Shared.Next(agents.Count)];
                _scrapper.ParseDataPage(posts, categoryFormatted, page, userAgent);
            });

            return Ok(posts);
        }
    }
}

