using System;
namespace DataParserService.Constants
{
	public static class Xpath
	{
        public const string QUANTITY = "//div[@data-testid='total-count']";
        public const string ITEMLIST = "//div[@class='css-1sw7q4x']";
        public const string TITLE = ".//div[@class='css-u2ayx9']//h6";
        public const string URI = ".//a[@class='css-rc5s2u']";
        public const string PRICE = ".//div[@class='css-u2ayx9']//p";
        public const string PLACEDATE = ".//div[@class='css-odp1qd']/p";
        public const string PAGINATIONLIST = "//li[@data-testid='pagination-list-item']";
    }
}

