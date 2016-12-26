namespace Proxy.Fs.Handlers

open Proxy.Core.Configurations;
open Proxy.Core.Handlers
open Proxy.Core.Headers
open Proxy.Core.Sessions
open System.Linq;
open System.Threading.Tasks;

type FirstRequestHandler() =
    static let instance = new FirstRequestHandler()

    static member Instance() =
        instance

    interface IHandler with
        member this.Run context =
            let async = async {
                let! result = Async.AwaitTask (HttpHeaderStream.Instance().GetHeader context.ClientStream)
                return result
            }

            context.Header <- Async.RunSynchronously async

            match context.Header with
            | null -> ExitReason.TerminationRequired
            | _ -> ExitReason.Initialized
            |> Task.FromResult


type ProxyTypeHandler() =
    static let instance = new ProxyTypeHandler()

    static member Instance() =
        instance

    interface IHandler with
        member this.Run context =
            match context.Header.Verb with
            | "CONNECT" -> ExitReason.HttpsTunnelRequired
            | _ ->  ExitReason.HttpProxyRequired
            |> Task.FromResult


type FirewallHandler() =
    static let instance = new FirewallHandler()

    static member Instance() =
        instance

    interface IHandler with
        member this.Run context =
            match Configuration.Settings.Firewall.Enabled with
            | false -> ExitReason.NewHostConnectionRequired
            | true -> this.Check context
            |> Task.FromResult

    member private this.Check(context:SessionContext) =
        match this.IsAllowed context with
            | true -> ExitReason.NewHostConnectionRequired
            | false -> ExitReason.TerminationRequired

    member private this.IsAllowed(context:SessionContext) =
        let a = Configuration.Settings.Firewall.Rules.Any(fun r -> r.Pattern.Match(context.Header.Host.Hostname).Success && r.Action = ActionEnum.Deny)
        not a
