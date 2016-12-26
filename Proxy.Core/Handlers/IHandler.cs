using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Proxy.Core.Sessions;

namespace Proxy.Core.Handlers
{
    public interface IHandler
    {
        Task<ExitReason> Run(SessionContext context);
    }
}
