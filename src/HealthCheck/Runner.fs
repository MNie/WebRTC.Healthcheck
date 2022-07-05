namespace WebRTC.Healthcheck

open System.Collections.Generic
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
        async {
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
        async {
            let finalResult = List<Response> model.Servers.Length
            
            for i in [0..model.Servers.Length - 1] do
                let! result = runSingle log model.CredentialsConfig model.Servers[i]
                
                finalResult.Add result
                
            return finalResult |> Seq.toArray
        }

