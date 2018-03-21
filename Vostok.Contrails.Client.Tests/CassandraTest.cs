using System;
using System.Collections.Generic;
using Cassandra;
using NUnit.Framework;
using Vostok.Tracing;

namespace Vostok.Contrails.Client.Tests
{
    public class CassandraTest
    {
        public static Lazy<ISession> Session = new Lazy<ISession>(
            () =>
            {
                var sessionKeeper = new CassandraSessionKeeper(new[] { "localhost:9042" }, "airlock");
                return sessionKeeper.Session;
            });

        public static CassandraDataScheme DataScheme
        {
            get
            {
                var dataScheme = new CassandraDataScheme(Session.Value);
                dataScheme.CreateTableIfNotExists();
                return dataScheme;
            }
        }

        [Test, Ignore("Manual")]
        public void InsertData()
        {
            var dataScheme = DataScheme;
            var traceId = Guid.NewGuid();
            var insertStatement = dataScheme.GetInsertStatement(
                new Span
                {
                    Annotations = new Dictionary<string, string> { ["key"] = "value" },
                    TraceId = traceId,
                    SpanId = Guid.NewGuid(),
                    BeginTimestamp = DateTimeOffset.UtcNow,
                    EndTimestamp = DateTimeOffset.UtcNow.AddMinutes(10),
                    ParentSpanId = Guid.NewGuid()
                });
            var insertStatement2 = dataScheme.GetInsertStatement(
                new Span
                {
                    Annotations = new Dictionary<string, string> { ["key"] = "value" },
                    TraceId = traceId,
                    SpanId = Guid.NewGuid(),
                    BeginTimestamp = DateTimeOffset.UtcNow.AddMinutes(1),
                    EndTimestamp = DateTimeOffset.UtcNow.AddMinutes(10),
                    ParentSpanId = Guid.NewGuid()
                });
            Session.Value.Execute(insertStatement);
            Session.Value.Execute(insertStatement2);
        }
    }
}