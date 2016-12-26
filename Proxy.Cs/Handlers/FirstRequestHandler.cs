using System.Threading.Tasks;
using Proxy.Core.Handlers;
using Proxy.Core.Sessions;
using Proxy.Cs.Headers;

namespace Proxy.Cs.Handlers
{
    public class FirstRequestHandler : IHandler
    {
        private static readonly FirstRequestHandler Self = new FirstRequestHandler();

        private FirstRequestHandler()
        {
        }

        public async Task<ExitReason> Run(SessionContext context)
        {
            context.Header = await HttpHeaderStream.Instance().GetHeader(context.ClientStream);

            return context.Header != null ? ExitReason.Initialized : ExitReason.TerminationRequired;
        }

        public static FirstRequestHandler Instance()
        {
            return Self;
        }
    }
}