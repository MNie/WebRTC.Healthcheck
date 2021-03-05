module Server

open System
open Candidates
open Credentials
open Microsoft.MixedReality.WebRTC
open Utils

type Address = Address of string

type Type =
    | Stun of Address
    | Turn of Address * Credentials
    
module Type =
    let parse addr =
        if String.IsNullOrWhiteSpace addr then
            None
        else
            let address = Address addr
            let rawType = addr.Split(":").[0].ToLower()
            match rawType with
            | "stun" ->
                address
                |> Stun
                |> Some
            | "turn" ->
                Credentials.generate ()
                |> Option.map (fun cred -> Turn (address, cred))
            | _ -> failwith $"%s{rawType} server type is not supported. Only stun and turn are supported."
 
let private stun (protocol, candidateType) =
    protocol = UDP
    && candidateType = Srflx
    
let private turn (protocol, candidateType) =
    protocol = UDP
    && candidateType = Relay
    
let private serverCheck checkType results =
    let result =
        results
        |> Seq.tryFind checkType
    match result with
    | Some (protocol, candidateType) ->
        Log.info $"Found candidate of type: {candidateType} for protocol: {protocol}."
        true
    | None -> false
    
let checks =
    function
    | Stun _ ->  [ serverCheck stun ]
    | Turn _ ->  [ serverCheck stun; serverCheck turn ]
    
let asIceServer =
    function
    | Stun (Address address) -> IceServer(Urls = asList [ address ])
    | Turn ((Address address), { Login = Login login; Password = Password pass }) -> IceServer(Urls = asList [ address ], TurnUserName = login, TurnPassword = pass)
    