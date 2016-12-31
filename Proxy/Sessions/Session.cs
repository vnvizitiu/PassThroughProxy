﻿using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using Proxy.Core.Handlers;
using Proxy.Core.Sessions;
using Proxy.Cs.Handlers;
using ProxyTypeHandler = Proxy.Fs.Handlers.ProxyTypeHandler;
using FirstRequestHandler = Proxy.Fs.Handlers.FirstRequestHandler;
using FirewallHandler = Proxy.Fs.Handlers.FirewallHandler;
using NewHostHandler = Proxy.Fs.Handlers.NewHostHandler;
using AuthenticationHandler = Proxy.Fs.Handlers.AuthenticationHandler;

namespace Proxy.Sessions
{
    public class Session
    {
        private static readonly Dictionary<ExitReason, IHandler> Handlers = new Dictionary<ExitReason, IHandler>
        {
            {ExitReason.InitializationRequired, FirstRequestHandler.Instance()},
            {ExitReason.Initialized, AuthenticationHandler.Instance()},
            {ExitReason.Authenticated, ProxyTypeHandler.Instance()},
            {ExitReason.AuthenticationNotRequired, ProxyTypeHandler.Instance()},
            {ExitReason.HttpProxyRequired, HttpProxyHandler.Instance()},
            {ExitReason.HttpsTunnelRequired, HttpsTunnelHandler.Instance()},
            {ExitReason.NewHostRequired, FirewallHandler.Instance()},
            {ExitReason.NewHostConnectionRequired, NewHostHandler.Instance()},
            {ExitReason.NewHostConnected, ProxyTypeHandler.Instance()}
        };

        public async Task Run(TcpClient client)
        {
            var result = ExitReason.InitializationRequired;

            using (var context = new SessionContext(client))
            {
                do
                {
                    try
                    {
                        result = await Handlers[result].Run(context);
                    }
                    catch (SocketException)
                    {
                        result = ExitReason.TerminationRequired;
                    }
                } while (result != ExitReason.TerminationRequired);
            }
        }
    }
}