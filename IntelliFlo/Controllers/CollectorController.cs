using System.IO;
using System.Threading.Tasks;
using System.Web.Http;

namespace IntelliFlo.Controllers
{
    public class CollectorController : ApiController
    {
        // POST api/values
        public void Post([FromBody] string value)
        {
            File.AppendAllText("E:\\Logs\\A.txt", "z" + value + "z\n\n\n\n\n");
            //SplitnProcess(value);
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
            string value = await Request.Content.ReadAsStringAsync();
            Post(value);
        }

        [HttpPost]
        [Route("services/collector/event/1.0")]
        public async Task<IHttpActionResult> EventCollector1()
        {
            string value = await Request.Content.ReadAsStringAsync();
            Post(value);
            return Json(new
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
    }

}
