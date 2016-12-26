namespace Proxy.Fs.Handlers

open System.Threading.Tasks;
open Proxy.Core.Handlers
open Proxy.Core.Sessions
open Proxy.Core.Headers

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