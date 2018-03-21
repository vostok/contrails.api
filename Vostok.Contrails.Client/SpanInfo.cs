using System;

namespace Vstk.Contrails.Client
{
    [Cassandra.Mapping.Attributes.Table]
    public class SpanInfo
    {
        [Cassandra.Mapping.Attributes.Column("trace_id")]
        [Cassandra.Mapping.Attributes.PartitionKey]
        public Guid TraceId { get; set; }

        [Cassandra.Mapping.Attributes.ClusteringKey]
        [Cassandra.Mapping.Attributes.Column("begin_timestamp")]
        public DateTimeOffset BeginTimestamp { get; set; }

        [Cassandra.Mapping.Attributes.ClusteringKey(1)]
        [Cassandra.Mapping.Attributes.Column("span_id")]
        public Guid SpanId { get; set; }

        [Cassandra.Mapping.Attributes.Column("parent_span_id")]
        public Guid? ParentSpanId { get; set; }

        [Cassandra.Mapping.Attributes.Column("end_timestamp")]
        public DateTimeOffset? EndTimestamp { get; set; }

        [Cassandra.Mapping.Attributes.Column("annotations")]
        public string Annotations { get; set; }
    }
}