using System;
namespace DataParserService.Constants
{
	public static class Xpath
	{
        public const int PAGES = 25;
        public const string ITEMLIST = "//div[@class='css-1sw7q4x']";
        public const string TITLE = ".//div[@class='css-u2ayx9']//h6";
        public const string IMAGE = ".//div[@class='css-gl6djm']//img";
        public const string STATUS = ".//div[@class='css-1h0qipy']";
        public const string URI = ".//a[@class='css-rc5s2u']";
        public const string PRICE = ".//div[@class='css-u2ayx9']//p";
        public const string PLACEDATE = ".//div[@class='css-odp1qd']/p";
    }
}

