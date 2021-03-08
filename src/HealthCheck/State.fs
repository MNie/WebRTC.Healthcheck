module State
open System
open System.Collections.Concurrent
open Candidates
open Utils

let private bag = ConcurrentBag<Protocol * CandidateType * Protocol option>()
let mutable private wait = true
let mutable private timer = null
let mutable private startTime = DateTimeOffset.UtcNow
let mutable private result = errorCode

let private elapsed server (ev: Timers.ElapsedEventArgs) =
    let res =
        server
        |> Server.checks
        |> List.map (fun x -> x bag)
        |> List.fold (fun x y -> x && y) true
    
    let tenSecOccurred =
        (DateTimeOffset.UtcNow - startTime).TotalSeconds > 10.
    
    if res then
        result <- successCode
        wait <- false
        bag.Clear ()
        Log.success $"Check of %A{server} succeed!"
    if tenSecOccurred then
        Log.failure $"Check of %A{server} failed, generated candidates: %A{bag}."
        wait <- false
        bag.Clear ()
    ()

let start server =
    bag.Clear ()
    wait <- true
    result <- errorCode
    timer <- new System.Timers.Timer (Interval = 1_000.0)
    
    server
    |> elapsed
    |> timer.Elapsed.Add
    
    timer.Start ()
    startTime <- DateTimeOffset.UtcNow
    
let add protocol candidate forcedProtocol =
    bag.Add (protocol, candidate, forcedProtocol)
    
let waitForResult () =
    while wait do
        ()
    result
    
let stop () =
    bag.Clear ()
    wait <- true
    result <- errorCode
    timer.Stop ()
    timer.Dispose ()