using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vostok.Logging;
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
    }

    public class ContrailsClient : IDisposable, IContrailsClient
    {
        private readonly ICassandraDataScheme dataScheme;
        private readonly ICassandraRetryExecutionStrategy retryExecutionStrategy;
        private readonly CassandraSessionKeeper cassandraSessionKeeper;

        public ContrailsClient(ContrailsClientSettings settings, ILog log)
        {
            cassandraSessionKeeper = new CassandraSessionKeeper(settings.CassandraNodes, settings.Keyspace);
            retryExecutionStrategy = new CassandraRetryExecutionStrategy(settings.CassandraRetryExecutionStrategySettings, log, cassandraSessionKeeper.Session);
            dataScheme = new CassandraDataScheme(cassandraSessionKeeper.Session);
            dataScheme.CreateTableIfNotExists();
        }

        public async Task AddSpan(Span span)
        {
            await retryExecutionStrategy.ExecuteAsync(dataScheme.GetInsertStatement(span));
        }

        public void Dispose()
        {
            cassandraSessionKeeper.Dispose();
        }
    }
}