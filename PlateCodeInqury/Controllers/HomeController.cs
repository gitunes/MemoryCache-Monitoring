using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using PlateCodeInqury.Constants;
using PlateCodeInqury.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;

namespace PlateCodeInqury.Controllers
{
    public class HomeController : Controller
    {
        private readonly IMemoryCache _memoryCache;

        public HomeController(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Monitoring()
        {
            return View();
        }

        [HttpPost]
        public JsonResult GetPlateValue(PlateRequestModel request)
        {         
            var city = GetValueFromCache(request.PlateCode);
            if (!string.IsNullOrEmpty(city))
            {
                WriteLogToJson(city, request.PlateCode, StringConstants.Cache);
                return Json($"{DateTime.Now.ToString("HH:mm:ss")} : {request.PlateCode} - {city}");
            }

            city = GetDataFromJson(request.PlateCode);
            if (string.IsNullOrEmpty(city))
                return Json(StringConstants.NotFoundCity);

            WriteLogToJson(city, request.PlateCode, StringConstants.Db);
            SetValueToCache(city, request.PlateCode);
            return Json($"{DateTime.Now.ToString("HH:mm:ss")} : {request.PlateCode} - {city}");
        }

        [HttpGet]
        public Dictionary<int, string> GetLogs()
        {
            var items = new Dictionary<int, string>();
            using (StreamReader r = new StreamReader("log.json"))
            {
                string json = r.ReadToEnd();
                items = JsonConvert.DeserializeObject<Dictionary<int, string>>(json);
            }
            return items;
        }

        private string GetValueFromCache(int plateCode)
        {
            var value = string.Empty;
            _memoryCache.TryGetValue(plateCode, out value);
            return value;
        }

        private void SetValueToCache(string city, int plateCode)
        {
            var cacheExpiryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpiration = DateTime.Now.AddHours(1),
                Priority = CacheItemPriority.High
            };
            _memoryCache.Set(plateCode, city, cacheExpiryOptions);
        }

        private string GetDataFromJson(int plateCode)
        {
            var city = string.Empty;
            using (StreamReader r = new StreamReader("cities.json"))
            {
                string json = r.ReadToEnd();
                var items = JsonConvert.DeserializeObject<Dictionary<int, string>>(json);
                items.TryGetValue(plateCode, out city);
            }
            return city;
        }

        private void WriteLogToJson(string city, int plateCode, string dataFrom)
        {
            string ipAddress = string.Empty;
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.ToString().Contains(StringConstants.ComparisonIPAddress))
                {
                    ipAddress = ip.ToString();
                }
            }
            var items = new Dictionary<int, string>();
            using (StreamReader r = new StreamReader("log.json"))
            {
                string logs = r.ReadToEnd();
                items = JsonConvert.DeserializeObject<Dictionary<int, string>>(logs);
                if (items.Any() || items.Count != 0)
                    items.Add(items.Last().Key + 1, $"{DateTime.Now.ToString("HH:mm:ss")} - {ipAddress} | {plateCode} => {city}({dataFrom}) ");
                else
                    items.Add(1, $"{DateTime.Now.ToString("HH:mm:ss")} - {ipAddress} | {plateCode} => {city}({dataFrom})");
            }
            System.IO.File.WriteAllText($"{Directory.GetCurrentDirectory()}/log.json", JsonConvert.SerializeObject(items));
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
