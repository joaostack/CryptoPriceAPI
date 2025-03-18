using System.Net;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace API1.Controllers
{
    [ApiController]
    [Route("api")]
    public class CryptoController : ControllerBase
    {
        [HttpGet("coins/{page}")]
        public IActionResult GetAll(string page)
        {
            var coins = GetAllCoins(page);
            var obj = new
            {
                Page = page,
                Coins = coins,
            };

            return Ok(obj);
        }

        [HttpGet("coin/{name}")]
        public IActionResult GetCoinAll(string name)
        {
            var coin = GetCoin(name);
            return Ok(coin);
        }

        // WebScraping starts here...
        public static List<dynamic> GetAllCoins(string page)
        {
            HtmlWeb web = new HtmlWeb();
            HtmlDocument htmlDocument = web.Load($"https://coinmarketcap.com/?page={page}");

            var coinNameList = new List<string>();
            var coinPriceList = new List<string>();

            var coins = htmlDocument.DocumentNode.SelectNodes("//tr//p[contains(@class, 'coin-item-name')]");
            var prices = htmlDocument.DocumentNode.SelectNodes("//table//tr//td[4]//span");
            var singlePriceLink = htmlDocument.DocumentNode.SelectNodes("//td//div[contains(@class, 'sc-4c05d6ef-0 bLqliP')]//a");

            if (coins != null && prices != null)
            {
                for (int i = 0; i < coins.Count; i++)
                {
                    var coin = coins[i];
                    var price = prices[i];

                    var coinSinglePriceLink = $"https://coinmarketcap.com{singlePriceLink[i].GetAttributeValue("href", string.Empty)}";

                    coinNameList.Add(coin.InnerText.Trim());
                    coinPriceList.Add(price.InnerText.Trim());
                }

                var combinedList = coinNameList.Zip(coinPriceList, (name, price) => new
                {
                    Coin = name,
                    Price = price
                }).ToList();

                return combinedList.Cast<dynamic>().ToList();
            }
            else
            {
                return new List<object>() { new { Message = "Error on scraping, no data found!" } };
            }
        }

        public static object GetCoin(string name)
        {
            if (name != null)
            {
                name = name.Trim().ToLower();

                HtmlWeb web = new HtmlWeb();
                HtmlDocument htmlDocument = web.Load($"https://coinmarketcap.com/currencies/{name}");
                var price = htmlDocument.DocumentNode.SelectSingleNode("//div[@class='section-coin-overview']//span");
                if (price != null)
                {
                    return new { Name = name, Price = price.InnerText.Trim() };
                }
                else
                {
                    return "Price not found!";
                }
            }
            else
            {
                return "Coin not found!";
            }
        }
    }
}
