module FClientLib.Client

open System
open System.IO
open System.Net
open System.Net.Sockets
open System.Collections.Generic

open Filbert.Core
open Filbert.Encoder
open Filbert.Decoder

let host = "localhost"
let port = 9000

let start = async {

    try

        let socket = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)

//            socket.SetSocketOption (SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true)

        do! Async.FromBeginEnd (host, port, (fun (host, port, callback, state) ->
            socket.BeginConnect (host, port, callback, state)), socket.EndConnect)

        let stream = new NetworkStream (socket, false)

        printfn "client connected: %A -> %A" socket.LocalEndPoint socket.RemoteEndPoint

        let mysterWord = [| 131uy; 107uy; 0uy; 8uy; 104uy; 97uy; 122uy; 101uy; 108uy; 110uy; 117uy; 116uy |]
        let bert = Dictionary (Map.ofList [(Atom "Filbert", Atom "means"); (ByteList mysterWord, Atom "!")])

        use ms = new MemoryStream ()
        encode bert ms
        do! stream.AsyncWrite (ms.ToArray ())

        let bert = decode stream
        printfn "server said: %A" bert

        printfn "client disconnecting: %A" socket.LocalEndPoint

        do! Async.FromBeginEnd (true, (fun (reuseSocket, callback, state) ->
            socket.BeginDisconnect (reuseSocket, callback, state)), socket.EndDisconnect)

        socket.Close (0)

    with
    | :? SocketException as e ->
        Console.ForegroundColor <- ConsoleColor.Red
        printfn "%A: %A" e e.ErrorCode
        Console.ResetColor ()            

    | e ->
        Console.ForegroundColor <- ConsoleColor.Red
        printfn "%s" e.Message
        Console.ResetColor ()

}
