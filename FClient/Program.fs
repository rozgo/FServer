open System
open System.Net.Sockets
open System.Collections.Generic

[<EntryPoint>]
let main argv = 
    printfn "%A" argv

//    Server.start |> Async.Start

    let host = "localhost"
    let port = 9000

//    System.Threading.ThreadPool.SetMaxThreads (10240, 10240) |> ignore
//
//    let m = System.Threading.ThreadPool.GetMaxThreads ()
//    printfn "GetMaxThreads: %A" m

    let byteIList (data : byte array) =
        let segment = new System.ArraySegment<byte>(data)
        let data = new List<System.ArraySegment<byte>>() :> IList<System.ArraySegment<byte>>
        data.Add(segment)
        data

    let client idx = async {

        try
            let socket = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            do! Async.FromBeginEnd (host, port, (fun (host, port, callback, state) -> 
                socket.BeginConnect (host, port, callback, state)), socket.EndConnect)

//            printfn "Client %A Connected to %A %A..." idx host port

            let data = byteIList ("Hello world!!!"B)
            let! sentLength = Async.FromBeginEnd (data, SocketFlags.None, (fun (data, flags, callback, state) ->
                socket.BeginSend (data, flags, callback, state)), socket.EndSend)

            let data = Array.zeroCreate<byte> 256
            let! dataLength = Async.FromBeginEnd(byteIList data, SocketFlags.None, (fun (data, flags, callback, state) ->
                socket.BeginReceive(data, flags, callback, state)), socket.EndReceive)

//            printfn "RE: %A" (Text.Encoding.ASCII.GetString (data))
//            do! Async.Sleep 50000
            printfn "."

            socket.Close ()

        with e ->

            printfn "An error occurred: %s" e.Message

    }

    seq { for i in [0..1020] do yield client i }
    |> Async.Parallel
    |> Async.RunSynchronously
    |> ignore

    0



