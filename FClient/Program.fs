open System
open System.Net.Sockets
open System.Collections.Generic

[<EntryPoint>]
let main argv = 
    printfn "%A" argv

    let host = "localhost"
    let port = 9000

    let byteIList (data : byte array) =
        let segment = new System.ArraySegment<byte>(data)
        let data = new List<System.ArraySegment<byte>>() :> IList<System.ArraySegment<byte>>
        data.Add(segment)
        data

    let client idx = async {

        try
            let socket = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)

            socket.SetSocketOption (SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true)

            do! Async.FromBeginEnd (host, port, (fun (host, port, callback, state) ->
                socket.BeginConnect (host, port, callback, state)), socket.EndConnect)

//            printfn "port: %A" socket.LocalEndPoint

//            printfn "Client %A Connected to %A %A..." idx host port

            let data = byteIList ("Hello world!!!"B)
            let! sentLength = Async.FromBeginEnd (data, SocketFlags.None, (fun (data, flags, callback, state) ->
                socket.BeginSend (data, flags, callback, state)), socket.EndSend)

            let data = Array.zeroCreate<byte> 256
            let! dataLength = Async.FromBeginEnd(byteIList data, SocketFlags.None, (fun (data, flags, callback, state) ->
                socket.BeginReceive(data, flags, callback, state)), socket.EndReceive)

//            printfn "RE: %A" (Text.Encoding.ASCII.GetString (data))
//            do! Async.Sleep 5000
//            printfn "."

            do! Async.FromBeginEnd (true, (fun (reuseSocket, callback, state) ->
                socket.BeginDisconnect (reuseSocket, callback, state)), socket.EndDisconnect)

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

    seq { for i in [1..10000] do yield client i }
    |> Async.Parallel
    |> Async.RunSynchronously
    |> ignore

    0



