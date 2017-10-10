using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Vostok.Contrails.Client;
using Vostok.Tracing;

namespace Vostok.Contrails.Api.Controllers
{
    public class TracesByIdResponce
    {
        public Guid TraceId { get; set; }
        public IEnumerable<Span> Spans { get; set; }
    }

    public class ApiController : Controller
    {
        private readonly IContrailsClient contrailsClient;

        public ApiController(IContrailsClient contrailsClient)
        {
            this.contrailsClient = contrailsClient;
        }

        [HttpGet]
        [Route("tracesById")]
        public async Task<TracesByIdResponce> TracesById(Guid id, DateTimeOffset? fromTime, Guid? fromSpan, int limit = 1000)
        {
            if (id == Guid.Empty)
                return new TracesByIdResponce {TraceId = id, Spans = new Span[] {}};
            var spans = await contrailsClient.GetTracesById(id, limit, fromTime != null && fromSpan != null ? new Tuple<DateTimeOffset, Guid>(fromTime.Value, fromSpan.Value) : null);
            return new TracesByIdResponce { TraceId = id, Spans = spans };
        }
    }
}
