﻿namespace WebRTC.Healthcheck
module ConnectionState =
    open System
    open System.Collections.Concurrent
    open Server
    open Utils
    open SIPSorcery.Net

    type State (log, server: Type) =
        let server = server
        let log = log
        let bag = ConcurrentBag<RTCIceProtocol * RTCIceCandidateType * RTCIceProtocol option>()
        let mutable wait = true
        let mutable timer = null
        let mutable startTime = DateTimeOffset.UtcNow
        let mutable result = errorCode
        
        let elapsed server (ev: Timers.ElapsedEventArgs) =
            let res =
                server
                |> Server.checks log
                |> List.map (fun x -> x bag)
                |> List.fold (fun x y -> x && y) true
            
            let tenSecOccurred () =
                (DateTimeOffset.UtcNow - startTime).TotalSeconds > 10.
            
            if res then
                result <- successCode
                wait <- false
                bag.Clear ()
                log.Success $"Check of %A{server} succeed!"
            if tenSecOccurred () then
                log.Failure $"Check of %A{server} failed, generated candidates: %A{bag}."
                wait <- false
                bag.Clear ()
            ()
            
        member __.Start () =
            bag.Clear ()
            wait <- true
            result <- errorCode
            timer <- new System.Timers.Timer (Interval = 1_000.0)
            
            server
            |> elapsed
            |> timer.Elapsed.Add
            
            timer.Start ()
            startTime <- DateTimeOffset.UtcNow
            
        member __.Add protocol candidate forcedProtocol =
            bag.Add (protocol, candidate, forcedProtocol)
            
        member __.WaitForResult () =
            async {
                let rec checkAgain () =
                    async {
                        if wait then
                            do! Async.Sleep 500
                            return! checkAgain ()
                        else return ()
                    }
                    
                do! checkAgain ()
                return result
            }
            
        interface IDisposable with
            member __.Dispose () =
                bag.Clear ()
                wait <- true
                result <- errorCode
                timer.Stop ()
                timer.Dispose ()
