using System;
using System.Collections.Generic;
using Vstk.Commons.Extensions.UnitConvertions;

namespace Vstk.Contrails.Client
{
    public class CassandraRetryExecutionStrategySettings
    {
        public int CassandraSaveRetryMaxAttempts { get; set; } = 3;
        public TimeSpan CassandraSaveRetryMinDelay { get; set; } = 1.Seconds();
        public TimeSpan CassandraSaveRetryMaxDelay { get; set; } = 3.Seconds();

        public CassandraRetryExecutionStrategySettings()
        {

        }

        public CassandraRetryExecutionStrategySettings(Dictionary<string, object> settings)
        {
            if (settings == null)
                return;
            if (settings["cassandra.save.retry.max.attempts"] != null)
            {
                CassandraSaveRetryMaxAttempts = int.Parse(settings["cassandra.save.retry.max.attempts"].ToString());
            }
            if (settings["cassandra.save.retry.min.delay"] != null)
            {
                CassandraSaveRetryMinDelay = TimeSpan.Parse(settings["cassandra.save.retry.min.delay"].ToString());
            }
            if (settings["cassandra.save.retry.max.delay"] != null)
            {
                CassandraSaveRetryMaxDelay = TimeSpan.Parse(settings["cassandra.save.retry.max.delay"].ToString());
            }
        }
    }
}