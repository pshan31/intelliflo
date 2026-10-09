using IntelliFloCore.Ingestion;
using IntelliFloCore_NetCore.Ingestion;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IntelliFLoUI_Razor.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            ViewData["Filters"] = IngestFilterContext.Context.GetAllFilter();
            ViewData["Title"] = "Home Page";
            ViewData["EnabledFilter"] = IngestFilterContext.Context.EnabledFilter;
            ViewData["Size"] = ((ServerInsights.GetDetails().Size / 1024 / 1024) + " MB");
            ViewData["FreeSpace"] = ((ServerInsights.GetDetails().FreeSpace / 1024 / 1024) + " MB");
        }
    }
}
