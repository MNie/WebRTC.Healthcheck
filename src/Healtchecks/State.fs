module State
open System
open System.Collections.Concurrent
open Candidates
open Utils

let private bag = ConcurrentBag<Protocol * CandidateType>()
let mutable private wait = true
let mutable private timer = null
let mutable private startTime = DateTimeOffset.UtcNow

let private elapsed server (ev: Timers.ElapsedEventArgs) =
    let result =
        server
        |> Server.checks
        |> List.map (fun x -> x bag)
        |> List.fold (fun x y -> x && y) true
    
    let tenSecOccurred =
        (DateTimeOffset.UtcNow - startTime).TotalSeconds > 10.
    
    if result then
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
    timer <- new System.Timers.Timer (Interval = 1_000.0)
    
    server
    |> elapsed
    |> timer.Elapsed.Add
    
    timer.Start ()
    startTime <- DateTimeOffset.UtcNow
    
let add protocol candidate =
    bag.Add (protocol, candidate)
    
let waitTillCandidatesWouldBeCreated () =
    while wait do
        ()
    
let stop () =
    bag.Clear ()
    wait <- true
    timer.Stop ()
    timer.Dispose ()