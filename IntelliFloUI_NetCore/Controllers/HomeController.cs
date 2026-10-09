using IntelliFloCore.Ingestion;
using IntelliFloCore_NetCore.Ingestion;
using IntelliFloUI_NetCore.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace IntelliFloUI_NetCore.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            ViewBag.Filters = IngestFilterContext.Context.GetAllFilter();
            ViewBag.Title = "Home Page";
            ViewBag.EnabledFilter = IngestFilterContext.Context.EnabledFilter;
            ViewBag.Size = ((ServerInsights.GetDetails().Size / 1024 / 1024) + " MB");
            ViewBag.FreeSpace = ((ServerInsights.GetDetails().FreeSpace / 1024 / 1024) + " MB");
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

        static void SplitnQueue(string jsonString)
        {
            string pattern = @"(?<=\})(?=\s*\{)|\}$";
            string[] splitObjects = Regex.Split(jsonString, pattern);

            foreach (string obj in splitObjects)
            {
                string trimmedObject = obj.Trim();
                if (!string.IsNullOrEmpty(trimmedObject))
                {
                    LogIngestor logIngestor = new LogIngestor();
                    logIngestor.QueueLog(obj);
                }
            }
        }
    }
}
