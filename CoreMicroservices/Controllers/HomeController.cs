using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CoreMicroservices.Models;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace CoreMicroservices.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IDistributedCache _distributedCache;

        public HomeController(ILogger<HomeController> logger,
            IConfiguration configuration,
            IDistributedCache distributedCache)
        {
            _logger = logger;
            this._configuration = configuration;
            this._distributedCache = distributedCache;
        }

        private static int _TotalCount = 0;

        public IActionResult Index()
        {
            this._logger.LogWarning($"this is HomeController index {this._configuration["port"]}");
            base.ViewBag.BrowserUrl = $@"{base.Request.Scheme}://{base.Request.Host.Host}:{Request.Host.Port}";
            base.ViewBag.InternalUrl = $@"{base.Request.Scheme}://{base.Request.Host.Host}:{this._configuration["port"]}";

            base.ViewBag.TotalCount = _TotalCount++;

            string user = base.HttpContext.Session.GetString("CurrentUser");

            if (string.IsNullOrWhiteSpace(user))
            {
                base.HttpContext.Session.SetString("CurrentUser", $"jacky-{this._configuration["port"]}");
                this._logger.LogWarning($"this is HomeController  {this._configuration["port"]}");
            }
            base.ViewBag.SessionUser = base.HttpContext.Session.GetString("CurrentUser");
            //UserModel userModel = new UserModel();
            //userModel.ID = 1;
            //userModel.Name = "jacky";

            //_distributedCache.Set("Sample", ObjectToByteArray(userModel));
            //var model = ByteArrayToObject<UserModel>(_distributedCache.Get("Sample"));

            //base.ViewBag.SessionUser = model.ID + "-" + model.Name;
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

        private byte[] ObjectToByteArray(object obj)
        {
            if (obj == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        private T ByteArrayToObject<T>(byte[] bytes)
        {
            using (var memoryStream = new MemoryStream())
            {
                var binaryFormatter = new BinaryFormatter();
                memoryStream.Write(bytes, 0, bytes.Length);
                memoryStream.Seek(0, SeekOrigin.Begin);
                var obj = binaryFormatter.Deserialize(memoryStream);
                return (T)obj;
            }
        }
    }
}