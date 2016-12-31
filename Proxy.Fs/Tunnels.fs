namespace Proxy.Fs.Tunnels

open System
open System.IO
open System.Net.Sockets
open System.Threading

type TcpTwoWayTunnel()=

    let rec tunnel (ctx:CancellationTokenSource) (source:Stream) (destination:Stream) (buffer:byte[]) = async {
        let! bytes = source.AsyncRead(buffer)
        if bytes > 0 && not ctx.IsCancellationRequested then
            do! destination.AsyncWrite(buffer, 0, bytes)
            return! tunnel ctx source destination buffer
    }

    member this._cancellationTokenSource = new CancellationTokenSource()

    interface IDisposable with

        member this.Dispose()=
            this._cancellationTokenSource.Dispose()
            GC.SuppressFinalize this

    member this.Run (client:NetworkStream) (host:NetworkStream) =
        let cTunnel = tunnel this._cancellationTokenSource

        let outbound = cTunnel client host (Array.zeroCreate 8192)
        let inbound = cTunnel host client (Array.zeroCreate 8192)

        [outbound; inbound]
        |> Async.Parallel
        |> Async.StartAsTask


type TcpOneWayTunnel()=

    let rec tunnel (ctx:CancellationTokenSource) (source:Stream) (destination:Stream) (buffer:byte[]) = async {
        let! bytes = source.AsyncRead(buffer)
        if bytes > 0 && not ctx.IsCancellationRequested then
            do! destination.AsyncWrite(buffer, 0, bytes)
            return! tunnel ctx source destination buffer
    }

    member this._cancellationTokenSource = new CancellationTokenSource()

    interface IDisposable with

        member this.Dispose()=
            this._cancellationTokenSource.Dispose()
            GC.SuppressFinalize this

    member this.Run (client:NetworkStream) (host:NetworkStream) =
        tunnel this._cancellationTokenSource host client (Array.zeroCreate 8192)
        |> Async.StartAsTask
