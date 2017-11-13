using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Cassandra;
using Cassandra.Data.Linq;
using Newtonsoft.Json;
using Vostok.Logging;
using Vostok.RetriableCall;
using Vostok.Tracing;

namespace Vostok.Contrails.Client
{
    public class ContrailsClientSettings
    {
        public CassandraRetryExecutionStrategySettings CassandraRetryExecutionStrategySettings { get; set; }
        public IEnumerable<string> CassandraNodes { get; set; }
        public string Keyspace { get; set; }
    }

    public interface IContrailsClient
    {
        Task AddSpan(Span span);
        Task<IEnumerable<Span>> GetTracesById(Guid traceId, DateTimeOffset? fromTimestamp, Guid? fromSpan, DateTimeOffset? toTimestamp, Guid? toSpan, bool ascending, int limit = 1000);
    }

    //public class TracesOffset

    public class ContrailsClient : IDisposable, IContrailsClient
    {
        private readonly ILog log;
        private readonly ICassandraDataScheme dataScheme;
        private readonly CassandraSessionKeeper cassandraSessionKeeper;
        private readonly JsonSerializer jsonSerializer;
        private readonly RetriableCallStrategy retriableCallStrategy;

        public ContrailsClient(ContrailsClientSettings settings, ILog log)
        {
            this.log = log;
            cassandraSessionKeeper = new CassandraSessionKeeper(settings.CassandraNodes, settings.Keyspace);
            var executionStrategySettings = settings.CassandraRetryExecutionStrategySettings;
            retriableCallStrategy = new RetriableCallStrategy(executionStrategySettings.CassandraSaveRetryMaxAttempts, executionStrategySettings.CassandraSaveRetryMinDelay, executionStrategySettings.CassandraSaveRetryMaxDelay);
            dataScheme = new CassandraDataScheme(cassandraSessionKeeper.Session);
            dataScheme.CreateTableIfNotExists();
            jsonSerializer = new JsonSerializer();
        }

        public async Task AddSpan(Span span)
        {
            var statement = dataScheme.GetInsertStatement(span);
            await retriableCallStrategy.CallAsync(
                () => cassandraSessionKeeper.Session.ExecuteAsync(statement),
                ex => ex is WriteTimeoutException || ex is NoHostAvailableException || ex is WriteFailureException,
                log);
        }

        public async Task<IEnumerable<Span>> GetTracesById(Guid traceId, DateTimeOffset? fromTimestamp, Guid? fromSpan, DateTimeOffset? toTimestamp, Guid? toSpan, bool ascending, int limit = 1000)
        {
            var query = dataScheme.Table.Where(x => x.TraceId == traceId);
            if (fromTimestamp != null)
            {
                query = query.Where(x => x.BeginTimestamp > fromTimestamp);
                if (fromSpan != null)
                    query = query.Where(x => x.SpanId.CompareTo(fromSpan) > 0);
            }
            if (toTimestamp != null)
            {
                query = query.Where(x => x.BeginTimestamp < toTimestamp);
                if (toSpan != null)
                    query = query.Where(x => x.SpanId.CompareTo(toSpan) < 0);
            }
            query = ascending ? query.OrderBy(x => x.BeginTimestamp).ThenBy(x => x.SpanId) : query.OrderByDescending(x => x.BeginTimestamp).ThenByDescending(x => x.SpanId);
            var spans = await query.Take(limit).ExecuteAsync();
            return spans.Select(x => new Span
            {
                SpanId = x.SpanId,
                TraceId = x.TraceId,
                BeginTimestamp = x.BeginTimestamp,
                EndTimestamp = x.EndTimestamp,
                ParentSpanId = x.ParentSpanId,
                Annotations = string.IsNullOrWhiteSpace(x.Annotations) ? null : jsonSerializer.Deserialize<Dictionary<string, string>>(new JsonTextReader(new StringReader(x.Annotations)))
            });
        }

        public void Dispose()
        {
            cassandraSessionKeeper.Dispose();
        }
    }
}