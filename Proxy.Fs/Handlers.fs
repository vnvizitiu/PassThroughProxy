namespace Proxy.Fs.Handlers

open Proxy.Core.Configurations;
open Proxy.Core.Handlers
open Proxy.Core.Headers
open Proxy.Core.Sessions
open System;
open System.Linq;
open System.Text;
open System.Net.Sockets
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


type NewHostHandler() =
    static let instance = new NewHostHandler()

    static member Instance() =
        instance

    interface IHandler with
        member this.Run context =
            context.RemoveHost()

            let host = async {
                let client = new TcpClient()
                do! Async.AwaitTask (client.ConnectAsync (context.Header.Host.Hostname, context.Header.Host.Port))
                return client
            }
            
            context.AddHost (Async.RunSynchronously host)
            context.CurrentHostAddress <- context.Header.Host
            ExitReason.NewHostConnected |> Task.FromResult


type AuthenticationHandler() =
    static let instance = new AuthenticationHandler()

    static member Instance() =
        instance
        
    interface IHandler with
        member this.Run context =
            match this.IsAuthenticationEnabled() with
            | true -> this.Authenticate context
            | false -> ExitReason.AuthenticationNotRequired
            |> Task.FromResult

    member private this.IsAuthenticationEnabled()=
        Configuration.Settings.Authentication.Enabled

    member private this.Authenticate(context:SessionContext)=
        match this.IsProxyAuthorizationHeaderPresent(context.Header) with
        | true -> this.Validate context
        | false -> this.SendProxyAuthenticationRequired context.ClientStream
        
    member private this.IsProxyAuthorizationHeaderPresent(header:HttpHeader)=
        header.ArrayList.Any(fun s -> s.StartsWith("Proxy-Authorization: Basic", StringComparison.OrdinalIgnoreCase))

    member private this.SendProxyAuthenticationRequired(stream:NetworkStream)=
        let bytes = Encoding.ASCII.GetBytes("HTTP/1.1 407 Proxy Authentication Required\r\nProxy-Authenticate: Basic realm=\"Pass Through Proxy\"\r\nConnection: close\r\n\r\n")
        Async.RunSynchronously (Async.AwaitTask (stream.WriteAsync(bytes, 0, bytes.Length)))
        ExitReason.TerminationRequired

    member private this.Validate(context:SessionContext)=
        match this.IsCredentialsCorrect(context.Header) with
        | true -> ExitReason.Authenticated
        | false -> this.SendProxyAuthenticationInvalid context.ClientStream

    member private this.IsCredentialsCorrect(header:HttpHeader)=
        let key = "Proxy-Authorization: Basic"
        let value = header.ArrayList.First(fun s -> s.StartsWith(key, StringComparison.OrdinalIgnoreCase)).Substring(key.Length).Trim()
        let credentials = Configuration.Settings.Authentication.Username + ":" + Configuration.Settings.Authentication.Password
        let encoded = Convert.ToBase64String(Encoding.ASCII.GetBytes(credentials))
        value = encoded  

    member private this.SendProxyAuthenticationInvalid(stream:NetworkStream)=
        let bytes = Encoding.ASCII.GetBytes("HTTP/1.1 407 Proxy Authentication Invalid\r\nProxy-Authenticate: Basic realm=\"Pass Through Proxy\"\r\nConnection: close\r\n\r\n")
        Async.RunSynchronously (Async.AwaitTask (stream.WriteAsync(bytes, 0, bytes.Length)))
        ExitReason.TerminationRequired
