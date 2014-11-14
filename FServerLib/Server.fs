module FServerLib.Server

open System
open System.IO
open System.Net
open System.Net.Sockets
open System.Collections.Generic

let start : Async<unit> =

    let byteIList (data : byte array) =
        let segment = new System.ArraySegment<byte>(data)
        let data = new List<System.ArraySegment<byte>>() :> IList<System.ArraySegment<byte>>
        data.Add(segment)
        data

    System.Threading.ThreadPool.SetMaxThreads (10240, 10240) |> ignore

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

//            let stream = new NetworkStream (socket, false)



            let rec client () = async {

    //            let! response = stream.AsyncRead (14)
    //            printfn "RE: %A" (Text.Encoding.ASCII.GetString (response))
    //            do! stream.AsyncWrite (response)

                printfn "Waiting on client"

                try

                    let data = Array.zeroCreate<byte> 256
                    let! dataLength = Async.FromBeginEnd(byteIList data, SocketFlags.None, (fun (data, flags, callback, state) ->
                        socket.BeginReceive(data, flags, callback, state)), socket.EndReceive)

                    printfn "RE: %A" (Text.Encoding.ASCII.GetString (data))

                    let data = byteIList ("OK"B)
                    let! sentLength = Async.FromBeginEnd (data, SocketFlags.None, (fun (data, flags, callback, state) ->
                        socket.BeginSend (data, flags, callback, state)), socket.EndSend)

                    return! client ()

                with e ->
//                    printfn "An error occurred: %s" e.Message
//                    stream.Close ()
                    //socket.Shutdown (SocketShutdown.Both)
                    socket.Close ()

                
            }
//            do! client ()
            Async.Start (client ())

        with e ->
            printfn "An error occurred: %s" e.Message

        return! loop (count + 1)
    }

    loop 0
