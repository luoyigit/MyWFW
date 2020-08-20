using Grpc.Core;
using MagicOnion.Server;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ST.Common.MagicOnion
{
    public class Bootstrap
    {
        static Server _rpcServer;
        public static void Start()
        {
            try
            {
                _rpcServer.Start();
                Console.WriteLine("*********Rpc 服务已启动********");
            }
            catch (Exception ex)
            {
                //Console.WriteLine("service start failed.");
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
        public static void InitalizeRpcSetting(string host,int port)
        {
            var service = MagicOnionEngine.BuildServerServiceDefinition(true);
            _rpcServer = new Server
            {
                Services = { service },
                Ports = { new ServerPort(host, port, ServerCredentials.Insecure) }
            };
        }
    }
}
