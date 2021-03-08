module Candidates

open System
open Utils

type Protocol =
    | TCP
    | UDP

type CandidateType =
    | Host
    | Prflx
    | Srflx
    | Relay
    | RelayTLS
    
// Parse manually candidate
// source: https://developer.mozilla.org/en-US/docs/Web/API/RTCIceCandidate/candidate
let parse (cand: string) =
    if String.IsNullOrWhiteSpace cand then
        None, None
    else
        let parts = cand.Split(' ')
        if parts.Length < 8 then
            None, None
        else
        let protocol =
            match parts.[2].ToLower() with
            | "udp" -> Some UDP
            | "tcp" -> Some TCP
            | _ ->
                Log.failure $"Unsupported protocol appears in candidate: %s{parts.[2]}."
                None
        let t =
            match parts.[7].ToLower() with
            | "host" -> Some Host
            | "prflx" -> Some Prflx
            | "relay" -> Some Relay
            | "srflx" -> Some Srflx
            | "relay(tls)" -> Some RelayTLS
            | _ ->
                Log.failure $"Unsupported type appears in candidate: %s{parts.[7]}."
                None
        protocol, t
