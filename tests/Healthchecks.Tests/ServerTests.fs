module ServerTests

open Expecto

open SIPSorcery.Net
open WebRTC.Healthcheck.Credentials
open WebRTC.Healthcheck.Server

let tests =
    testList "Server tests" [
        testCase "stun" <| fun _ ->
            let stun = "stun:10.10.10.10:443"
            let expected =
                "stun:10.10.10.10:443"
                |> Address
                |> Stun
                |> Some
            let server = Type.parse None stun
            
            
            Expect.equal server expected ""
            
        testCase "turn" <| fun _ ->
            let turn = "turn:10.10.10.10:443"
            let server = Type.parse (Some { OTP = Otp "placeholder"; UserPostfix = Some (UserPostfix "") }) turn
            
            match server with
            | Some (Turn { Address = Address address; ForcedProtocol = forcedProtocol }) ->
                Expect.equal address turn ""
                Expect.equal forcedProtocol None ""
            | _ -> failwith "Server should be parsed as Turn server"
            
        testCase "turn with forced TCP" <| fun _ ->
            let turn = "turn:10.10.10.10:443?transport=TCP"
            let server = Type.parse (Some { OTP = Otp "placeholder"; UserPostfix = Some (UserPostfix "") }) turn
            
            match server with
            | Some (Turn { Address = Address address; ForcedProtocol = forcedProtocol }) ->
                Expect.equal address turn ""
                Expect.equal forcedProtocol (Some RTCIceProtocol.tcp) ""
            | _ -> failwith "Server should be parsed as Turn server"
            
        testCase "turn with forced UDP" <| fun _ ->
            let turn = "turn:10.10.10.10:443?transport=UDP"
            let server = Type.parse (Some { OTP = Otp "placeholder"; UserPostfix = Some (UserPostfix "") }) turn
            
            match server with
            | Some (Turn { Address = Address address; ForcedProtocol = forcedProtocol }) ->
                Expect.equal address turn ""
                Expect.equal forcedProtocol (Some RTCIceProtocol.udp) ""
            | _ -> failwith "Server should be parsed as Turn server"
            
        testCase "turn with predefined long living user and pass" <| fun _ ->
            let usedAddress = "turn:10.10.10.10:443"
            let turn = $"user:pass@{usedAddress}"
            let server = Type.parse None turn
            
            match server with
            | Some (Turn { Address = Address address; ForcedProtocol = forcedProtocol; Credentials = { Login = Login login; Password = Password pass } }) ->
                Expect.equal address usedAddress ""
                Expect.equal forcedProtocol None ""
                Expect.equal login "user" ""
                Expect.equal pass "pass" ""
            | _ -> failwith "Server should be parsed as Turn server"
            
        testCase "turn with predefined long living user and pass and forced TCP" <| fun _ ->
            let usedAddress = "turn:10.10.10.10:443?transport=TCP"
            let turn = $"user:pass@{usedAddress}"
            let server = Type.parse None turn
            
            match server with
            | Some (Turn { Address = Address address; ForcedProtocol = forcedProtocol; Credentials = { Login = Login login; Password = Password pass } }) ->
                Expect.equal address usedAddress ""
                Expect.equal forcedProtocol (Some RTCIceProtocol.tcp) ""
                Expect.equal login "user" ""
                Expect.equal pass "pass" ""
            | _ -> failwith "Server should be parsed as Turn server"
            
        testCase "turns" <| fun _ ->
            let turns = "turns:10.10.10.10:443"
            let server = Type.parse (Some { OTP = Otp "placeholder"; UserPostfix = Some (UserPostfix "") }) turns
            
            match server with
            | Some (Turns { Address = Address address; ForcedProtocol = forcedProtocol }) ->
                Expect.equal address turns ""
                Expect.equal forcedProtocol None ""
            | _ -> failwith "Server should be parsed as Turn server"
    ]