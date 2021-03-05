module Server

open System
open System.Web
open Candidates
open Credentials
open Microsoft.MixedReality.WebRTC
open Utils

type Address = Address of string

type TurnProperties = {
    Address: Address
    Credentials: Credentials
    ForcedProtocol: Protocol option
}

type Type =
    | Stun of Address
    | Turn of TurnProperties
    | Turns of TurnProperties
    
module Type =
    let parse addr =
        if String.IsNullOrWhiteSpace addr then
            None
        else
            let address = Address addr
            let rawType = addr.Split(":").[0].ToLower()
            let resolveTransport () =
                let parameters = HttpUtility.ParseQueryString addr
                try
                    match parameters.["transport"].ToLower () with
                    | "tcp" -> Some TCP
                    | "udp" -> Some UDP
                    | _ -> None
                with _ -> None
            match rawType with
            | "stun" ->
                address
                |> Stun
                |> Some
            | "turn" ->
                Credentials.generate ()
                |> Option.map (fun cred -> Turn { Address = address; Credentials = cred; ForcedProtocol = resolveTransport () })
            | "turns" ->
                Credentials.generate ()
                |> Option.map (fun cred -> Turns { Address = address; Credentials = cred; ForcedProtocol = resolveTransport () })
            | _ -> failwith $"%s{rawType} server type is not supported. Only stun and turn are supported."
 
let private protocolCheck protocol forcedProtocol =
    match forcedProtocol with
    | Some fProtocol -> protocol = fProtocol
    | _ -> protocol = UDP
 
let private stun (protocol, candidateType, forcedProtocol) =
    protocolCheck protocol forcedProtocol
    && candidateType = Srflx
    
let private turn (protocol, candidateType, forcedProtocol) =
    protocolCheck protocol forcedProtocol
    && candidateType = Relay

let private turns (protocol, candidateType, forcedProtocol) =
    protocolCheck protocol forcedProtocol
    && candidateType = Relay
    
let private serverCheck checkType results =
    let result =
        results
        |> Seq.tryFind checkType
    match result with
    | Some (protocol, candidateType, forcedProtocol) ->
        Log.info $"Found candidate of type: {candidateType} for protocol: {protocol}."
        true
    | None -> false
    
let checks =
    function
    | Stun _ ->  [ serverCheck stun ]
    | Turn _ ->  [ serverCheck stun; serverCheck turn ]
    | Turns _ ->  [ serverCheck turns ]
    
let asIceServer =
    function
    | Stun (Address address) -> IceServer(Urls = asList [ address ])
    | Turn { Address = Address address; Credentials = { Login = Login login; Password = Password pass }}
    | Turns { Address = Address address; Credentials = { Login = Login login; Password = Password pass }} ->
        IceServer(Urls = asList [ address ], TurnUserName = login, TurnPassword = pass)
    
let getForcedProtocol =
    function
    | Stun _ -> None
    | Turn { ForcedProtocol = forcedProtocol }
    | Turns { ForcedProtocol = forcedProtocol } ->
        forcedProtocol
    