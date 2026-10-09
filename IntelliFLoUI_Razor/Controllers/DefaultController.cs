using Microsoft.AspNetCore.Mvc;

namespace IntelliFlo.Controllers
{
    public class DefaultController : ControllerBase
    {
        [AcceptVerbs("GET", "POST", "PUT", "DELETE", "OPTIONS", "PATCH", "HEAD")]
        public IActionResult HandleAll()
        {
            //File.AppendAllText("E:\\Logs\\B.txt", HttpContext.Request. Request.RequestUri.ToString());
            return Ok("Request handled by DefaultController.HandleAll");
        }
    }
}