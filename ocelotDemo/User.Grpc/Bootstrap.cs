using Grpc.Core;
using MagicOnion.Server;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace User.Grpc
{
    public class Bootstrap
    {
        static IConfiguration _config;
        static Server _rpcServer;

        public static void Initialize(IConfiguration config)
        {
            try
            {
                _config = config;
                InitalizeRpcSetting();
            }
            catch (Exception ex)
            {
                Console.WriteLine("service start failed.");
                throw;
            }
        }
        public static void Start()
        {
            try
            {
                _rpcServer.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine("service start failed.");
                throw;
            }
        }

        public static void Stop()
        {
            try
            {
                _rpcServer.ShutdownAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public static void InitalizeRpcSetting()
        {

            var service = MagicOnionEngine.BuildServerServiceDefinition(true);
            var serverAddresss = _config["RpcDiscovery:Host"];
            var port = _config["RpcDiscovery:Port"];
            _rpcServer = new Server
            {
                Services = { service },
                Ports = { new ServerPort(serverAddresss, int.Parse(port), ServerCredentials.Insecure) }
            };
        }
    }
}
