using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SEDetector.Models;
using SEDetector.Services;

namespace SEDetector.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DetectorService _detectorService;

        public HomeController(ILogger<HomeController> logger, DetectorService detectorService)
        {
            _logger = logger;
            _detectorService = detectorService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new DashboardViewModel { Request = new AnalysisRequest() });
        }

        [HttpPost]
        public async Task<IActionResult> Index(DashboardViewModel model)
        {
            if (model.Request == null) return View(model);

            if (model.Request.AnalysisType == "Text" && !string.IsNullOrEmpty(model.Request.InputText))
            {
                model.Result = _detectorService.AnalyzeText(model.Request.InputText);
            }
            else if (model.Request.AnalysisType == "URL" && !string.IsNullOrEmpty(model.Request.InputUrl))
            {
                model.Result = _detectorService.AnalyzeUrl(model.Request.InputUrl);
            }
            else if (model.Request.AnalysisType == "Screenshot" && model.Request.Screenshot != null)
            {
                model.Result = await _detectorService.AnalyzeImageAsync(model.Request.Screenshot);
            }

            return View(model);
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