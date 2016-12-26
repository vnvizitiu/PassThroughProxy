namespace Proxy.Fs.Handlers

open System.Threading.Tasks;
open Proxy.Core.Handlers
open Proxy.Core.Sessions

type ProxyTypeHandler() =
    static let instance = new ProxyTypeHandler()

    interface IHandler with
        member this.Run context =
            match context.Header.Verb with
            | "CONNECT" -> ExitReason.HttpsTunnelRequired
            | _ ->  ExitReason.HttpProxyRequired
            |> Task.FromResult

    static member Instance() =
        instance