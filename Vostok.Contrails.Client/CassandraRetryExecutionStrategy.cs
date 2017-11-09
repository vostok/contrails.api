using System;
using System.Threading.Tasks;
using Cassandra;
using Vostok.Commons.Utilities;
using Vostok.Logging;

namespace Vostok.Contrails.Client
{
    public interface ICassandraRetryExecutionStrategy
    {
        Task ExecuteAsync(Statement statement);
    }

    public class CassandraRetryExecutionStrategy : ICassandraRetryExecutionStrategy
    {
        private readonly CassandraRetryExecutionStrategySettings settings;
        private readonly ISession session;
        
        private readonly ILog log;
        private const double minDelayMultiplier = 1.7;
        private const double maxDelayMultiplier = 2.5;

        public CassandraRetryExecutionStrategy(CassandraRetryExecutionStrategySettings settings, ILog log, ISession session)
        {
            this.settings = settings;
            this.session = session;
            this.log = log.ForContext(this);
        }

        public async Task ExecuteAsync(Statement statement)
        {
            var maxAttemptsCount = 1 + Math.Max(0, settings.CassandraSaveRetryMaxAttempts);
            var delay = settings.CassandraSaveRetryMinDelay;

            for (var attempt = 1; attempt < maxAttemptsCount + 1; attempt++)
            {
                if (attempt != 1)
                {
                    log.Warn($"Will try to save again in {delay:g}");
                    await Task.Delay(delay);
                    delay = IncreaseDelay(delay);
                }

                try
                {
                    await session.ExecuteAsync(statement);
                    if (attempt != 1)
                    {
                        log.Info($"Save succeed after {attempt} attempts");
                    }
                    return;
                }
                catch (Exception ex)
                {
                    if (ex is WriteTimeoutException || ex is NoHostAvailableException || ex is WriteFailureException)
                        continue;
                    log.Error($"Save failed at {attempt} attempt. Will drop this span insert.", ex);
                    return;
                }
            }

            log.Error($"Exceeded max retry attempts limit (tried {maxAttemptsCount} times). Will drop this span insert.");
        }

        private TimeSpan IncreaseDelay(TimeSpan delay)
        {
            var multiplier = minDelayMultiplier + ThreadSafeRandom.NextDouble() * (maxDelayMultiplier - minDelayMultiplier);
            var increasedDelay = delay.Multiply(multiplier);
            return TimeSpanExtensions.Min(TimeSpanExtensions.Max(settings.CassandraSaveRetryMinDelay, increasedDelay), settings.CassandraSaveRetryMaxDelay);
        }
    }
}