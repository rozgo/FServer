open FClientLib

[<EntryPoint>]
let main argv = 
    printfn "%A" argv

    seq { for i in [1..10000] do yield Client.start }
    |> Async.Parallel
    |> Async.RunSynchronously
    |> ignore

    0



