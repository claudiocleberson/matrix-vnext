/*
 * Copyright (c) 2003-2017 by AG-Software <info@ag-software.de>
 *
 * All Rights Reserved.
 * See the COPYING file for more information.
 *
 * This file is part of the MatriX project.
 *
 * NOTICE: All information contained herein is, and remains the property
 * of AG-Software and its suppliers, if any.
 * The intellectual and technical concepts contained herein are proprietary
 * to AG-Software and its suppliers and may be covered by German and Foreign Patents,
 * patents in process, and are protected by trade secret or copyright law.
 *
 * Dissemination of this information or reproduction of this material
 * is strictly forbidden unless prior written permission is obtained
 * from AG-Software.
 *
 * Contact information for AG-Software is available at http://www.ag-software.de
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DnsClient;
using DnsClient.Protocol;
using DotNetty.Transport.Bootstrapping;

namespace Matrix.Srv
{
    public class SrvNameResolver : INameResolver
    {
        const string PrefixSrvClient = "_xmpp-client._tcp.";
        const string PrefixSrvServer = "_xmpp-server._tcp.";
        
        public SrvNameResolver()
        {
        }

        public SrvNameResolver(bool isClient) : this()
        {
            IsClient = isClient;
        }

        public bool IsResolved(EndPoint address) => false;

        /// <summary>
        /// Decides wheter we lookup Xmpp Server or Client records
        /// </summary>
        public bool IsClient { get; set; } = true;

        public async Task<EndPoint> ResolveAsync(EndPoint address)
        {
            var asDns = address as DnsEndPoint;


            var srvRecord = await LookupSrvRecords(asDns.Host);
            if (srvRecord != null)
            {
                var dnsEndPoint = new DnsEndPoint(srvRecord.Target, srvRecord.Port);
                return await new DefaultNameResolver().ResolveAsync(dnsEndPoint);
            }
            
            return await new DefaultNameResolver().ResolveAsync(address);
        }

        private async Task<SrvRecord> LookupSrvRecords(string host)
        {
            try
            {
                var lookup = new LookupClient();
                var prefix = IsClient ? PrefixSrvClient : PrefixSrvServer;
                var result = await lookup.QueryAsync(prefix + host, QueryType.SRV);
                var srvRecords = result.Answers.SrvRecords().ToList();

                if (srvRecords.Count > 0)
                    return PickSrvRecord(srvRecords);
            }
            catch (Exception)
            {
                // ignored
            }

            return null;
        }

        /// <summary>
        /// Picks all records which the lowest priority. There can be more than just one.
        /// </summary>
        /// <param name="records"></param>
        /// <returns></returns>
        private IEnumerable<SrvRecord> GetMinServers(List<SrvRecord> records)
        {
            var minPrio = records.Min(s => s.Priority);
            return records
                        .Where(s1 => s1.Priority == minPrio);
        }

        private SrvRecord PickSrvRecord(List<SrvRecord> records)
        {
            SrvRecord ret;
            
            List<SrvRecord> minServers = GetMinServers(records).ToList();

            int minServersCount = minServers.Count;
            if (minServersCount > 1)
            {
                int rnd;
                // summarize the weight of all min servers
                int sumWeight = minServers.Sum(s => s.Weight);

                // eg collecta has a weight on 0 on all servers
                if (sumWeight == 0)
                {
                    // no weight, pick random
                    rnd = new Random().Next(0, minServersCount);
                    ret = minServers.ElementAt(rnd);
                }
                else
                {

                    // Create a random value between 1 - total Weight
                    rnd = new Random().Next(1, sumWeight);

                    int count = 0;
                    SrvRecord result =
                        minServers.First(s2 => rnd > (count += s2.Weight) - s2.Weight && rnd <= count);

                    ret = result;
                }
            }
            else
                ret = minServers.First();

            return ret;
        }
    }
}
