using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Cassandra;

namespace Vstk.Contrails.Client
{
    public class CassandraSessionKeeper : IDisposable
    {
        public ISession Session { get; }
        private readonly ICluster cluster;

        public CassandraSessionKeeper(IEnumerable<string> nodes, string keyspace)
        {
            cluster = Cluster
                .Builder()
                .AddContactPoints(nodes.SelectMany(ToIpEndpoints))
                //.WithPoolingOptions(connectionConfig.GetPoolingOptions())
                //.WithSocketOptions(connectionConfig.GetSocketOptions())
                //.WithQueryOptions(connectionConfig.GetQueryOptions())
                .Build();
            Session = cluster.Connect();
            Session.CreateKeyspaceIfNotExists(keyspace);
            Session.ChangeKeyspace(keyspace);
        }

        private static IEnumerable<IPEndPoint> ToIpEndpoints(string nodeAddr)
        {
            var splitted = nodeAddr.Split(':');
            if (!(splitted.Length == 2 && int.TryParse(splitted[1], out var port)))
            {
                return Enumerable.Empty<IPEndPoint>();
            }

            if (IPAddress.TryParse(splitted[0], out var address))
            {
                return Enumerable.Repeat(new IPEndPoint(address, port), 1);
            }

            return Dns.GetHostEntry(splitted[0]).AddressList.Select(a => new IPEndPoint(a, port));
        }

        public void Dispose()
        {
            Session.Dispose();
            cluster.Dispose();
        }
    }
}