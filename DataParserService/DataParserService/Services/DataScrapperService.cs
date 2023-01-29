using System;
using DataParserService.Constants;
using DataParserService.Models;
using HtmlAgilityPack;

namespace DataParserService.Services
{
    public class DataScrapperService : IDataScrapperService
    {
        private readonly IProxyProviderService _proxy;
        private readonly IConfiguration _config;

        public DataScrapperService(IProxyProviderService proxyProvider, IConfiguration configuration)
        {
            _proxy = proxyProvider;
            _config = configuration;
        }
        public void ParseDataPage(List<Post> posts, string category, int page, string userAgent)
        {
            var firstPage = $"https://www.olx.ua/d/uk/list/q-{category}/";
            var nthPage = $"https://www.olx.ua/d/uk/list/q-{category}/?page={page}";

            var url = page == 1 ? firstPage : nthPage;

            using (HttpClient client = _proxy.CreateClient())
            {
                client.DefaultRequestHeaders.UserAgent.Clear();
                client.DefaultRequestHeaders.UserAgent.TryParseAdd(userAgent);
                try
                {
                    var htmlPage = client.GetStringAsync(url).GetAwaiter().GetResult();
                    if (htmlPage == null) return;

                    var doc = new HtmlDocument();
                    doc.LoadHtml(htmlPage);

                    var htmlNodes = doc.DocumentNode.SelectNodes(Xpath.ITEMLIST);
                    if (htmlNodes != null)
                    {
                        foreach (var node in htmlNodes)
                        {
                            AddPostInfo(posts, node);
                        }
                    }
                    Console.WriteLine($"Proxy: {client.GetStringAsync("http://ipv4.webshare.io/").GetAwaiter().GetResult()};" +
                        $" User-Agent: {client.DefaultRequestHeaders.UserAgent}\n");
                }
                //TODO: Handling exceptions
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private static void AddPostInfo(List<Post> posts, HtmlNode? node)
        {
            var title = node?.SelectSingleNode(Xpath.TITLE)?.InnerText;
            var image = node?.SelectSingleNode(Xpath.IMAGE)?.Attributes["src"]?.Value;
            var status = node?.SelectSingleNode(Xpath.STATUS)?.InnerText;
            var uri = node?.SelectSingleNode(Xpath.URI)?.Attributes["href"]?.Value;
            var placeDate = node?.SelectSingleNode(Xpath.PLACEDATE)?.InnerText;
            posts.Add(new Post
            {
                Title = title,
                Image = image ?? "Image not found",
                Status = status ?? "Status undefined",
                Uri = $"http://www.olx.ua{uri}" ?? "",
                PlaceDate = placeDate
            });
        }
    }
}

