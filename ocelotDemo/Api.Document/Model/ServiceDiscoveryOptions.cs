﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Api.Document.Model
{
    public class ServiceDiscoveryOptions
    {
        public string ServiceName { get; set; }
        public ConsulOptions Consul { get; set; }
    }

    public class ConsulOptions
    {
        public string HttpEndpoint { get; set; }
        public DnsEndPoint DnsEndPoint { get; set; }
    }

    public class DnsEndPoint
    {
        public string Address { get; set; }
        public int Port { get; set; }

        public IPEndPoint ToIpEndPoint()
        {
            return new IPEndPoint(IPAddress.Parse(Address), Port);
        }
    }
}