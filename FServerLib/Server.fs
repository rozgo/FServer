module FServerLib.Server

open System
open System.IO
open System.Net
open System.Net.Sockets
open System.Collections.Generic

open Filbert.Core
open Filbert.Encoder
open Filbert.Decoder

let start : Async<unit> =

    let port = 9000
    let endpoint = IPEndPoint (IPAddress.Any, port)

    let listener = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
    listener.Bind (endpoint)
    listener.Listen (10)

    printfn "Listening on port %d" port

    let rec loop count = async {

        printfn "Waiting for request %A ..." count

        try

            let! socket = Async.FromBeginEnd (listener.BeginAccept, listener.EndAccept)

            let stream = new NetworkStream (socket, false)

            printfn "server accepted: %A -> %A" socket.LocalEndPoint socket.RemoteEndPoint

            let rec client () = async {

                try

                    let bert = decode stream

                    printfn "client said: %A" bert

                    let bert = Atom "ok" 

                    use ms = new MemoryStream ()
                    encode bert ms
                    do! stream.AsyncWrite (ms.ToArray ())

                    printfn "server is disconnecting from remote: %A" socket.RemoteEndPoint

                    do! Async.FromBeginEnd (true, (fun (reuseSocket, callback, state) ->
                        socket.BeginDisconnect (reuseSocket, callback, state)), socket.EndDisconnect)

                    printfn "server disconnected from remote: %A" socket.RemoteEndPoint

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

                    socket.Close ()

                
            }

            Async.Start (client ())

        with
        | :? SocketException as e ->
            Console.ForegroundColor <- ConsoleColor.Red
            printfn "%A: %A" e e.ErrorCode
            Console.ResetColor ()            

        | e ->
            Console.ForegroundColor <- ConsoleColor.Red
            printfn "%s" e.Message
            Console.ResetColor ()

        return! loop (count + 1)
    }

    loop 0
