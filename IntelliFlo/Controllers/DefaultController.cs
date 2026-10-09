using Newtonsoft.Json.Linq;
using System.IO;
using System.Web.Http;

namespace IntelliFlo.Controllers
{
    public class DefaultController : ApiController
    {
        [AcceptVerbs("GET", "POST", "PUT", "DELETE", "OPTIONS", "PATCH", "HEAD")]
        public IHttpActionResult HandleAll()
        {
            File.AppendAllText("E:\\Logs\\B.txt", Request.RequestUri.ToString());
            return Ok("Request handled by DefaultController.HandleAll");
        }
    }
}