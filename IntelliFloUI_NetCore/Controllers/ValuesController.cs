using IntelliFloCore.Database;
using IntelliFloCore.Ingestion;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace IntelliFloUI_NetCore.Controllers
{
    [Route("services/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        // GET: api/<ValuesController>
        [HttpGet("setFilter/{id}")]
        public bool SetFilter(int id)
        {
            return IngestFilterContext.Context.SetEnabled(id);
        }

        [HttpPost("cleanUp")]
        public bool Cleanup()
        {
            return LiteDbContext.CleanupLogs();
        }

        [HttpGet("ingestionStatus")]
        public bool IsIngestionRunning()
        {
            return IngestionState.IsActive;
        }

        [HttpGet("currentLogsIngestion")]
        public string currentLogsIngestion()
        {
            var a = LiteDbContext.Database.GetCollection("queuedLogs").Query().Limit(1).FirstOrDefault();
            if (a != null)
                return a["_rawMessage"].AsString;
            return null;
        }
    }
}
