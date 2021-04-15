namespace WebRTC.Healthcheck

open SIPSorcery.Net

module Server =

    open System
    open Credentials
    open Utils

    type Address = Address of string

    type TurnProperties = {
        Address: Address
        Credentials: Credentials
        ForcedProtocol: RTCIceProtocol option
    }

    type Type =
        | Stun of Address
        | Turn of TurnProperties
        | Turns of TurnProperties
        
    module Type =
        let private parseAddressPart addr potentialCredentials =
            let address = Address addr
            let rawType = addr.Split(":").[0].ToLower()
            let resolveTransport () =
                if addr.EndsWith("?transport=tcp", StringComparison.OrdinalIgnoreCase) then
                    Some RTCIceProtocol.tcp
                elif addr.EndsWith("?transport=udp", StringComparison.OrdinalIgnoreCase) then
                    Some RTCIceProtocol.udp
                else None
            match rawType with
            | "stun" ->
                address
                |> Stun
                |> Some
            | "turn" ->
                potentialCredentials
                |> Option.map (fun cred -> Turn { Address = address; Credentials = cred; ForcedProtocol = resolveTransport () })
            | "turns" ->
                potentialCredentials
                |> Option.map (fun cred -> Turns { Address = address; Credentials = cred; ForcedProtocol = resolveTransport () })
            | _ -> failwith $"%s{rawType} server type is not supported. Only stun and turn are supported."
        let parse config addr =
            if String.IsNullOrWhiteSpace addr then
                None
            else
                let parts = addr.Split "@"
                if parts.Length = 2 then
                    let loginAndPass = parts.[0].Split ":"
                    let login = Login (loginAndPass.[0])
                    let pass = Password (loginAndPass.[1])
                    Some { Login = login; Password = pass }
                    |> parseAddressPart parts.[1]
                elif parts.Length = 1 then
                    Credentials.generate config
                    |> parseAddressPart addr
                else
                    failwith $"%A{parts} stun/turn server should contain 1 or 2 segments of address."
     
    let private protocolCheck protocol forcedProtocol =
        match forcedProtocol with
        | Some fProtocol -> protocol = fProtocol
        | _ -> protocol = RTCIceProtocol.udp
     
    let private stun (protocol, candidateType, forcedProtocol) =
        protocolCheck protocol forcedProtocol
        && candidateType = RTCIceCandidateType.srflx
        
    let private turn (protocol, candidateType, forcedProtocol) =
        protocolCheck protocol forcedProtocol
        && candidateType = RTCIceCandidateType.relay

    let private turns (protocol, candidateType, forcedProtocol) =
        protocolCheck protocol forcedProtocol
        && candidateType = RTCIceCandidateType.relay
        
    let private serverCheck log checkType results =
        let result =
            results
            |> Seq.tryFind checkType
        match result with
        | Some (protocol, candidateType, _) ->
            log.Success $"Found candidate of type: {candidateType} for protocol: {protocol}."
            true
        | None -> false
        
    let checks log =
        function
        | Stun _ ->  [ serverCheck log stun ]
        | Turn _ ->  [ serverCheck log stun; serverCheck log turn ]
        | Turns _ ->  [ serverCheck log turns ]
        
    let asIceServer =
        function
        | Stun (Address address) -> RTCIceServer(urls = address)
        | Turn { Address = Address address; Credentials = { Login = Login login; Password = Password pass }}
        | Turns { Address = Address address; Credentials = { Login = Login login; Password = Password pass }} ->
            RTCIceServer(urls = address, username = login, credential = pass)
        
    let getForcedProtocol =
        function
        | Stun _ -> None
        | Turn { ForcedProtocol = forcedProtocol }
        | Turns { ForcedProtocol = forcedProtocol } ->
            forcedProtocol
        