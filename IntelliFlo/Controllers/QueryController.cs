using IntelliFloCore.Querying;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;

namespace IntelliFlo.Controllers
{
    public class QueryController : ApiController
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
