using IntelliFloCore.Ingestion;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;

namespace IntelliFlo.Controllers
{
    public class CollectorController : ControllerBase
    {
        // POST api/values
        public async Task Post([FromBody] string value)
        {
            SplitnQueue(value);
            return;
        }

        [HttpOptions]
        [Route("services/collector/event/1")]
        public string EventCollectorOpt()
        {
            return "";
        }

        [HttpOptions]
        [Route("services/collector/event/1.0")]
        public string EventCollectorOpt1()
        {
            return "";
        }

        [HttpOptions]
        [Route("services/collector/health")]
        public string HealthOpt()
        {
            return "";

        }

        [HttpPost]
        [Route("services/collector/event/1")]
        public async Task EventCollector()
        {
            var bodyStream = new StreamReader(HttpContext.Request.Body);
            var bodyText = await bodyStream.ReadToEndAsync();
            await Post(bodyText);
        }

        [HttpPost]
        [Route("services/collector/event/1.0")]
        public async Task<IActionResult> EventCollector1()
        {

            var bodyStream = new StreamReader(HttpContext.Request.Body);
            var bodyText = await bodyStream.ReadToEndAsync();
            await Post(bodyText);
            return new JsonResult(new
            {
                text = "Success",
                code = 0
            });
        }

        [HttpGet]
        [Route("services/collector/health")]
        public string Health()
        {
            //LogIngestor logIngestor = new LogIngestor();
            //logIngestor.ParseAndIngestLog("{\"event\":\"2024-04-02 09:15:37,959 BLZCMUSTANG MONITOR Method=Net5.Mustang.RemoteConfigManager::Initialize (null) Message=EXTERNALCONFIG completed. STATUS=True ResponseTimeInMS=111 Thread=1 ProcessId=1 ip-10-10-21-222.ec2.internal \",\"time\":\"1712063737.959255\",\"host\":\"ip-10-0-138-45.ec2.internal\",\"source\":\"NV_V3-TransactionStatusUpdate\",\"sourcetype\":\"MustangV3_ECS_DEV\",\"index\":\"riskservice_test\"}");

            return ("{\"text\":\"HEC is healthy\",\"code\":17}");
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
