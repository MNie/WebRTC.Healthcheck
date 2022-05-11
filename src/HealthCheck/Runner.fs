namespace WebRTC.Healthcheck

open Credentials
open Server
open Utils
open ConnectionState
open System.Threading.Tasks

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
        task {
            try
                match Type.parse config server with
                | Some serv ->
                    use state = new State (log, serv)
                    state.Start ()
                    
                    let! addr, resCode = PeerConnection.connect log serv state
                    return { Address = addr; ResultCode = resCode }
                | _ -> return { Address = server; ResultCode = errorCode }
            with er ->
                log.Failure $"Error occured while processing: %s{server}, error: %A{er}"
                return { Address = server; ResultCode = errorCode }
        }
        
    let run log (model: Request) =
        task {
            return!
                model.Servers
                |> Array.map (runSingle log model.CredentialsConfig)
                |> Task.WhenAll
        }

