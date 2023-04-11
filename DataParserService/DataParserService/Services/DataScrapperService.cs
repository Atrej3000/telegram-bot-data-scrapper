using System;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using DataParserService.Constants;
using DataParserService.Extensions;
using DataParserService.Models;
using HtmlAgilityPack;

namespace DataParserService.Services
{
    public partial class DataScrapperService : IDataScrapperService
    {
        private readonly IProxyProviderService _proxy;

        public DataScrapperService(IProxyProviderService proxyProvider)
        {
            _proxy = proxyProvider;
        }
        public void ParseDataPage(HashSet<Post> posts, string category, int page, string userAgent)
        {
            string url = GetUrlForPage(category, page);

            using HttpClient client = _proxy.CreateClient();
            client.DefaultRequestHeaders.UserAgent.Clear();
            client.DefaultRequestHeaders.UserAgent.TryParseAdd(userAgent);

            try
            {
                string htmlPage = client.GetStringAsync(url).GetAwaiter().GetResult();

                if (string.IsNullOrEmpty(htmlPage)) return;
                
                HtmlDocument doc = new();
                doc.LoadHtml(htmlPage);

                HtmlNodeCollection htmlNodes = doc.DocumentNode.SelectNodes(Xpath.ITEMLIST);

                if (htmlNodes != null)
                {
                    foreach (var node in htmlNodes)
                    {
                        AddPostInfo(posts, node);
                    }
                }

                LogRequestInfo(client);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                Console.WriteLine($"Error: Page not found. Url: {url}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private static string GetUrlForPage(string category, int page)
        {
            const string baseUrl = "https://www.olx.ua/d/uk/list/q-{0}/{1}";
            const string firstPageParams = "?search%5Bphotos%5D=1&search%5Border%5D=created_at:desc";
            const string nthPageParams = "?page={page}&search%5Border%5D=created_at%3Adesc&search%5Bphotos%5D=1";

            string queryParams = page == 1 ? firstPageParams : nthPageParams.Replace("{page}", page.ToString());
            return string.Format(baseUrl, category, queryParams);
        }

        private static void AddPostInfo(HashSet<Post> posts, HtmlNode? node)
        {
            string? title = node?.SelectSingleNode(Xpath.TITLE)?.InnerText;
            string? uri = node?.SelectSingleNode(Xpath.URI)?.Attributes["href"]?.Value;
            string? price = node?.SelectSingleNode(Xpath.PRICE)?.InnerText;
            string? placeDate = node?.SelectSingleNode(Xpath.PLACEDATE)?.InnerText;

            if (!string.IsNullOrEmpty(placeDate))
            {
                Match match = TimePattern().Match(placeDate);
                string time = match.Value;

                if (!string.IsNullOrEmpty(time))
                {
                    DateTime dateTime = GetDateTimeFromAdvert(time);
                    posts.Add(new Post
                    {
                        Title = title.CheckNull().ToUTF8(),
                        Uri = $"http://www.olx.ua{uri}".CheckNull(),
                        Price = price.CheckNull().ToUTF8(),
                        Date = dateTime
                    });
                }
            }
        }

        private static DateTime GetDateTimeFromAdvert(string time)
        {
            int hours = Convert.ToInt32(time[..2]);
            int minutes = Convert.ToInt32(time[^2..]);

            DateTime now = DateTime.Now;

            return new DateTime(now.Year, now.Month, now.Day, hours, minutes, 0);
        }

        private static void LogRequestInfo(HttpClient client)
        {
            Console.WriteLine($"Proxy: {client.GetStringAsync("http://ipv4.webshare.io/").GetAwaiter().GetResult()};" +
                $" User-Agent: {client.DefaultRequestHeaders.UserAgent}\n");
        }

        [GeneratedRegex("\\d{2}\\:\\d{2}")]
        private static partial Regex TimePattern();
    }
}

