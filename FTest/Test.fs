namespace FTest

open System
open System.Threading
open NUnit.Framework

open FServerLib
open FClientLib

[<TestFixture>]
type Test() = 

    [<Test>]
    member x.TestCase() =

        Console.WriteLine "test begin"

        let cts = new CancellationTokenSource ()

        let task = Async.StartAsTask (Server.start, cancellationToken = cts.Token)

////        seq { for i in [1..4] do yield Client.start }
//        Client.start
////        |> Async.Parallel
//        |> Async.RunSynchronously
//        |> ignore

        let clientTask = Async.StartAsTask (Client.start)

        clientTask.Wait ()

        Console.WriteLine "client task done"

        cts.Cancel ()

        Console.WriteLine "test end"

        ()

        Assert.IsTrue (true)

        ()

