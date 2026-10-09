using IntelliFloCore.Querying;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IntelliFlo.Controllers
{
    public class QueryController : ControllerBase
    {

        [HttpPost]
        [Route("services/query/process")]
        public async Task<QueryMModel> Process([FromBody]QueryRequestModel request)
        {
            Query q = new Query();
            return q.GetLogs(request);
        }
    }
}
