module FClientLib.Client

open System
open System.IO
open System.Net
open System.Net.Sockets
open System.Collections.Generic

let host = "localhost"
let port = 9000

let byteIList (data : byte array) =
    let segment = new System.ArraySegment<byte>(data)
    let data = new List<System.ArraySegment<byte>>() :> IList<System.ArraySegment<byte>>
    data.Add(segment)
    data

let start = async {

    try
        let socket = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)

//        do! Async.Sleep 5000

//            socket.SetSocketOption (SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true)

        do! Async.FromBeginEnd (host, port, (fun (host, port, callback, state) ->
            socket.BeginConnect (host, port, callback, state)), socket.EndConnect)

        printfn "client connected: %A -> %A" socket.LocalEndPoint socket.RemoteEndPoint

//            printfn "Client %A Connected to %A %A..." idx host port

        let data = byteIList ("I'm so happy!!!"B)
        let! sentLength = Async.FromBeginEnd (data, SocketFlags.None, (fun (data, flags, callback, state) ->
            socket.BeginSend (data, flags, callback, state)), socket.EndSend)

        let data = Array.zeroCreate<byte> 256
        let! dataLength = Async.FromBeginEnd(byteIList data, SocketFlags.None, (fun (data, flags, callback, state) ->
            socket.BeginReceive(data, flags, callback, state)), socket.EndReceive)

        printfn "server said: %A" (Text.Encoding.ASCII.GetString (data))
//            do! Async.Sleep 5000
//            printfn "."

//        socket.Shutdown SocketShutdown.Both

        printfn "client disconnecting: %A" socket.LocalEndPoint

        do! Async.FromBeginEnd (true, (fun (reuseSocket, callback, state) ->
            socket.BeginDisconnect (reuseSocket, callback, state)), socket.EndDisconnect)

        socket.Close (0)

//        do! Async.Sleep 5000

//            socket.Disconnect (true)
//            socket.Close (0)

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
