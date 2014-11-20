open System
open System.Threading

#r @"./bin/Debug/Filbert.dll"
#r @"./bin/Debug/FServerLib.dll"
#r @"./bin/Debug/FClientLib.dll"


open FServerLib
open FClientLib

#time

printfn "test begin"

let cts = new CancellationTokenSource ()

let task = Async.StartAsTask (Server.start, cancellationToken = cts.Token)

seq { for i in [1..1] do yield Client.start }
//Client.start
|> Async.Parallel
|> Async.RunSynchronously
|> ignore

//let clientTask = Async.StartAsTask (Client.start)
//
//clientTask.Wait ()

printfn "client task done"

cts.Cancel ()

printfn "test end"
