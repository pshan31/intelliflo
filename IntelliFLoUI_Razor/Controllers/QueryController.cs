using IntelliFloCore.Querying;
using IntelliFLoUI_Razor.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace IntelliFlo.Controllers
{
    public class QueryController : ControllerBase
    {
        private readonly IMemoryCache _memoryCache;

        public QueryController(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        [HttpPost]
        [Route("services/query/oldprocess")]
        public async Task<QueryMModel> Process([FromBody] QueryRequestModel request)
        {
            Query q = new Query();
            return q.GetLogs(request);
        }

        [HttpPost]
        [Route("services/query/process")]
        public async Task<QueryMModel> Process2([FromBody] QueryRequestModel request)
        {
            _memoryCache.Set(request.guid, "null", DateTime.Now.AddMinutes(3));
            Query q = new Query();
            var data = q.GetFastLogs(request);
            _memoryCache.Set(request.guid, data, DateTime.Now.AddSeconds(9));

            return data;
        }

        [HttpPost]
        [Route("services/query/processWait")]
        public async Task<WaitModel> ProcessWait([FromBody] QueryRequestModel request)
        {
            WaitModel response = new();
            var data = _memoryCache.Get(request.guid);
            if (data != null)
            {
                if (data is string && data.ToString() == "null")
                {
                    response.WaitMore = true;
                }
                else
                {
                    response.Response = (QueryMModel)data;
                    response.Flag = true;
                }
            }
            else
            {
                response.Flag = false;
            }

            return response;
        }
    }
}
