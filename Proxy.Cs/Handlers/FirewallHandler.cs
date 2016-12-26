﻿using System.Linq;
using System.Threading.Tasks;
using Proxy.Core.Configurations;
using Proxy.Core.Handlers;
using Proxy.Core.Sessions;

namespace Proxy.Cs.Handlers
{
    public class FirewallHandler : IHandler
    {
        private static readonly FirewallHandler Self = new FirewallHandler();

        private FirewallHandler()
        {
        }

        public Task<ExitReason> Run(SessionContext context)
        {
            if (!Configuration.Settings.Firewall.Enabled)
            {
                return Task.FromResult(ExitReason.NewHostConnectionRequired);
            }

            return IsAllowed(context)
                ? Task.FromResult(ExitReason.NewHostConnectionRequired)
                : Task.FromResult(ExitReason.TerminationRequired);
        }

        public static FirewallHandler Instance()
        {
            return Self;
        }

        private static bool IsAllowed(SessionContext context)
        {
            return !Configuration.Settings.Firewall.Rules
                .Any(r => r.Pattern.Match(context.Header.Host.Hostname).Success &&
                          r.Action == ActionEnum.Deny);
        }
    }
}