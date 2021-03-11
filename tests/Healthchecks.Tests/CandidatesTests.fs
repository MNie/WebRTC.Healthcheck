module CandidateTests

open Expecto
open WebRTC.Healthcheck.Candidates
open WebRTC.Healthcheck.Utils

let log = { Success = (fun _ -> ()); Failure = (fun _ -> ()) }
let testParse = parse log

let tests =
    testList "Candidates tests" [
        testCase "local" <| fun _ ->
            let local = "candidate:2646563035 1 udp 2113937151 3a67faa1-3160-46fd-8eab-0b62281b09aa.local 63277 typ host generation 0 ufrag wieu network-cost 999"
            let protocol, transport = testParse local
            
            Expect.equal protocol (Some UDP) ""
            Expect.equal transport (Some Host) ""
            
        testCase "stun" <| fun _ ->
            let stun = "candidate:842163049 1 udp 1677729535 89.64.127.184 38243 typ srflx raddr 0.0.0.0 rport 0 generation 0 ufrag wieu network-cost 999"
            let protocol, transport = testParse stun
            
            Expect.equal protocol (Some UDP) ""
            Expect.equal transport (Some Srflx) ""
            
        testCase "turn" <| fun _ ->
            let turn = "candidate:842163049 1 udp 1677729535 89.64.127.184 38243 typ relay raddr 0.0.0.0 rport 0 generation 0 ufrag wieu network-cost 999"
            let protocol, transport = testParse turn
            
            Expect.equal protocol (Some UDP) ""
            Expect.equal transport (Some Relay) ""
            
        testCase "turn tcp" <| fun _ ->
            let turnTCP = "candidate:842163049 1 tcp 1677729535 89.64.127.184 38243 typ relay raddr 0.0.0.0 rport 0 generation 0 ufrag wieu network-cost 999"
            let protocol, transport = testParse turnTCP
            
            Expect.equal protocol (Some TCP) ""
            Expect.equal transport (Some Relay) ""
            
        testCase "turns" <| fun _ ->
            let turns = "candidate:842163049 1 tcp 1677729535 89.64.127.184 38243 typ relay(tls) raddr 0.0.0.0 rport 0 generation 0 ufrag wieu network-cost 999"
            let protocol, transport = testParse turns
            
            Expect.equal protocol (Some TCP) ""
            Expect.equal transport (Some RelayTLS) ""
    ]