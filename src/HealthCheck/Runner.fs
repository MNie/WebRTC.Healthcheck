namespace WebRTC.Healthcheck

open Credentials
open Server
open Utils
open ConnectionState

module Model =
    type Request = {
        Servers: Address []
        CredentialsConfig: Config option
    }
    
    type Response = {
        Address: string
        ResultCode: int
    }

module Runner =
    open Model
    
    let runSingle log config (Address server) =
        try
            match Type.parse config server with
            | Some serv ->
                use state = new State (log, serv)
                state.Start ()
                
                let addr, resCode = PeerConnection.connect log serv state
                { Address = addr; ResultCode = resCode }
            | _ -> { Address = server; ResultCode = errorCode }
        with er ->
            log.Failure $"Error occured while processing: %s{server}, error: %A{er}"
            { Address = server; ResultCode = errorCode }
            
    let run log (model: Request) =
        model.Servers
        |> Array.map (runSingle log model.CredentialsConfig)

