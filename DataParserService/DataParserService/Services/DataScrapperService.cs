using System;
using System.Globalization;
using System.Text.RegularExpressions;
using DataParserService.Constants;
using DataParserService.Models;
using HtmlAgilityPack;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DataParserService.Services
{
    public partial class DataScrapperService : IDataScrapperService
    {
        private readonly IProxyProviderService _proxy;

        public DataScrapperService(IProxyProviderService proxyProvider)
        {
            _proxy = proxyProvider;
        }
        public void ParseDataPage(List<Post> posts, string category, int page, string userAgent)
        {
            var firstPage = $"https://www.olx.ua/d/uk/list/q-{category}/?search%5Bphotos%5D=1&search%5Border%5D=created_at:desc";
            var nthPage = $"https://www.olx.ua/d/uk/list/q-{category}/?page={page}&search%5Border%5D=created_at%3Adesc&search%5Bphotos%5D=1";

            var url = page == 1 ? firstPage : nthPage;

            using HttpClient client = _proxy.CreateClient();
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

        private static void AddPostInfo(List<Post> posts, HtmlNode? node)
        {
            var title = node?.SelectSingleNode(Xpath.TITLE)?.InnerText;
            var image = node?.SelectSingleNode(Xpath.IMAGE)?.Attributes["src"]?.Value;
            var status = node?.SelectSingleNode(Xpath.STATUS)?.InnerText;
            var uri = node?.SelectSingleNode(Xpath.URI)?.Attributes["href"]?.Value;
            var price = node?.SelectSingleNode(Xpath.PRICE)?.InnerText;
            var placeDate = node?.SelectSingleNode(Xpath.PLACEDATE)?.InnerText;
            if (placeDate is not null)
            {
                Match match = TimePattern().Match(placeDate);
                var time = match.Value;
                if (!string.IsNullOrEmpty(time))
                {
                    var now = DateTime.Now;
                    var dateTime = new DateTime(now.Year, now.Month, now.Day, Convert.ToInt32(time[..2]), Convert.ToInt32(time[^2..]), 0);
                    Console.WriteLine(dateTime.ToString());
                    posts.Add(new Post
                    {
                        Title = title,
                        Image = image ?? "Image not found",
                        Status = status ?? "Status undefined",
                        Uri = $"http://www.olx.ua{uri}" ?? "",
                        Price = price,
                        Date = dateTime
                    });
                }
            }
        }

        [GeneratedRegex("\\d{2}\\:\\d{2}")]
        private static partial Regex TimePattern();
    }
}

