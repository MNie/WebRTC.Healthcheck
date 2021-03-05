﻿module ServerTests

open System
open Expecto
open Server
open Candidates

let tests =
    testList "Server tests" [
        testCase "stun" <| fun _ ->
            let stun = "stun:10.10.10.10:443"
            let expected =
                "stun:10.10.10.10:443"
                |> Address
                |> Stun
                |> Some
            let server = Type.parse stun
            
            
            Expect.equal server expected ""
            
        testCase "turn" <| fun _ ->
            Environment.SetEnvironmentVariable("WEBRTC_SECRET", "placeholder")
            let turn = "turn:10.10.10.10:443"
            let server = Type.parse turn
            
            match server with
            | Some (Turn { Address = Address address; ForcedProtocol = forcedProtocol }) ->
                Expect.equal address turn ""
                Expect.equal forcedProtocol None ""
            | _ -> failwith "Server should be parsed as Turn server"
            
        testCase "turn with forced TCP" <| fun _ ->
            Environment.SetEnvironmentVariable("WEBRTC_SECRET", "placeholder")
            let turn = "turn:10.10.10.10:443?transport=TCP"
            let server = Type.parse turn
            
            match server with
            | Some (Turn { Address = Address address; ForcedProtocol = forcedProtocol }) ->
                Expect.equal address turn ""
                Expect.equal forcedProtocol (Some TCP) ""
            | _ -> failwith "Server should be parsed as Turn server"
            
        testCase "turn with forced UDP" <| fun _ ->
            Environment.SetEnvironmentVariable("WEBRTC_SECRET", "placeholder")
            let turn = "turn:10.10.10.10:443?transport=UDP"
            let server = Type.parse turn
            
            match server with
            | Some (Turn { Address = Address address; ForcedProtocol = forcedProtocol }) ->
                Expect.equal address turn ""
                Expect.equal forcedProtocol (Some UDP) ""
            | _ -> failwith "Server should be parsed as Turn server"
            
        testCase "turns" <| fun _ ->
            Environment.SetEnvironmentVariable("WEBRTC_SECRET", "placeholder")
            let turns = "turns:10.10.10.10:443"
            let server = Type.parse turns
            
            match server with
            | Some (Turns { Address = Address address; ForcedProtocol = forcedProtocol }) ->
                Expect.equal address turns ""
                Expect.equal forcedProtocol None ""
            | _ -> failwith "Server should be parsed as Turn server"
    ]