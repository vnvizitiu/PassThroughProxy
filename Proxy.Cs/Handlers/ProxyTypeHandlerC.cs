using System.Threading.Tasks;
using Proxy.Core.Handlers;
using Proxy.Core.Sessions;

namespace Proxy.Cs.Handlers
{
    public class ProxyTypeHandlerC : IHandler
    {
        private static readonly ProxyTypeHandlerC Self = new ProxyTypeHandlerC();

        private ProxyTypeHandlerC()
        {
        }

        public Task<ExitReason> Run(SessionContext context)
        {
            return Task.FromResult(context.Header.Verb == "CONNECT" ? ExitReason.HttpsTunnelRequired : ExitReason.HttpProxyRequired);
        }

        public static ProxyTypeHandlerC Instance()
        {
            return Self;
        }
    }
}