open FServerLib


[<EntryPoint>]
let main argv = 
    printfn "%A" argv
    //Server.start |> Async.RunSynchronously

    let taskServer = Async.StartAsTask (Server.start)
    taskServer.Wait()

    0

