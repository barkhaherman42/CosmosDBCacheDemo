using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CosmosDBCacheDemo.Models;
using Microsoft.Extensions.Caching.Distributed;
using System.Text;
using System;

namespace CosmosDBCacheDemo.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private IDistributedCache _cache;

        public HomeController(ILogger<HomeController> logger, IDistributedCache cache)
        {
            _logger = logger;
            _cache = cache; 
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddCache(string CacheKey, string CacheItem)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(CacheItem);
            DistributedCacheEntryOptions options = new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60)
            };
            await _cache.SetAsync(CacheKey, bytes, options);
            ViewData.Add("CacheKey", CacheKey);
            return View();
        }

        public async Task<IActionResult> ShowCache(string CacheKey)
        {
            byte[] bytes = await _cache.GetAsync(CacheKey);
            if(bytes != null)
            {
                string str = Encoding.ASCII.GetString(bytes);
                ViewData.Add("CacheItem", str);
            }
            else
            {
                ViewData.Add("CacheItem", "Expired or missing");
            }
            return View();
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
