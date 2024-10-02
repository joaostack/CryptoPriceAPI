using HtmlAgilityPack;
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

        // WebScraping starts here...
        public static string[] GetAllCoins(string page)
        {
            HtmlWeb web = new HtmlWeb();
            HtmlDocument htmlDocument = web.Load($"https://coinmarketcap.com/?page={page}");

            var results = new List<string>();
            var coins = htmlDocument.DocumentNode.SelectNodes("//tr//p[contains(@class, 'coin-item-name')]");
            var prices = htmlDocument.DocumentNode.SelectNodes("//td//div[contains(@class, 'sc-b3fc6b7-0 dzgUIj')]//span");
            var singlePriceLink = htmlDocument.DocumentNode.SelectNodes("//td//div[contains(@class, 'sc-4c05d6ef-0 bLqliP')]//a");

            if (coins != null)
            {
                for (int i = 0; i < coins.Count; i++)
                {
                    var coin = coins[i];
                    var price = prices[i];
                    var coinSinglePriceLink = $"https://coinmarketcap.com{singlePriceLink[i].GetAttributeValue("href", string.Empty)}";

                    results.Add($"{coin.InnerText.Trim()} : Price {price.InnerText.Trim()} : {coinSinglePriceLink}");
                }

                return results.ToArray();
            }
            else
            {
                return new string[] { "No results Found!" };
            }
        }
    }
}
