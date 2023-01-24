using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using HtmlAgilityPack;
using System.Text;
using DataParserService.Models;

namespace DataParserService.Controllers
{
    [Route("api/[controller]")]
    public class DataParserController : Controller
    {
        // GET api/values/usb
        [HttpGet("{category}")]
        public ActionResult<IEnumerable<Post>> Get(string category)
        {
            var url = $"https://www.olx.ua/d/uk/list/q-{category}/";
            var web = new HtmlWeb
            {
                OverrideEncoding = Encoding.UTF8
            };
            var doc = web.Load(url);
            //check 1 page;
            var pages = Int32.Parse(doc.DocumentNode.SelectNodes("//li[@data-testid='pagination-list-item']").Last().InnerText);


            List<Post> posts = new();
            for (int i = 1; i <= pages; i++)
            {
                url = i == 1 ? $"https://www.olx.ua/d/uk/list/q-{category}/"
                    : $"https://www.olx.ua/d/uk/list/q-{category}/?page={i}";
                doc = web.Load(url);
                var htmlNodes = doc.DocumentNode.SelectNodes("//div[@class='css-1sw7q4x']");
                foreach (var node in htmlNodes)
                {
                    var title = node.SelectSingleNode(".//div[@class='css-u2ayx9']//h6")?.InnerText;
                    var image = node.SelectSingleNode(".//div[@class='css-gl6djm']/img")?.Attributes["src"]?.Value;
                    var status = node.SelectSingleNode(".//div[@class='css-1h0qipy']")?.InnerText;
                    var uri = node.SelectSingleNode(".//a[@class='css-rc5s2u']")?.Attributes["href"]?.Value;
                    var placeDate = node.SelectSingleNode(".//div[@class='css-odp1qd']/p")?.InnerText;
                    if (uri != null)
                    {
                        posts.Add(new Post
                        {
                            Title = title,
                            Image = image ?? "Image not found",
                            Status = status ?? "Status undefined",
                            Uri = $"http://www.olx.ua{uri}",
                            PlaceDate = placeDate
                        });
                    }
                }
            }
            return Ok(posts);
        }
    }
}

