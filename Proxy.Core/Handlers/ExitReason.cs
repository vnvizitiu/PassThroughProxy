﻿namespace Proxy.Core.Handlers
{
    public enum ExitReason
    {
        InitializationRequired,
        Initialized,
        NewHostRequired,
        NewHostConnectionRequired,
        NewHostConnected,
        Authenticated,
        AuthenticationNotRequired,
        HttpProxyRequired,
        HttpsTunnelRequired,
        TerminationRequired
    }
}