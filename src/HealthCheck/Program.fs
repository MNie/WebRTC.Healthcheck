open Server
open Utils
open ConnectionState

let run server =
    try
        match Type.parse server with
        | Some serv ->
            use state = new State (serv)
            state.Start ()
            
            let result = PeerConnection.connect serv state
            result
        | _ -> errorCode
    with er ->
        Log.failure $"Error occured while processing: %s{server}, error: %A{er}"
        errorCode

[<EntryPoint>]
let main argv =
    argv.[0].Split ","
    |> Array.map run
    |> Array.fold (fun x y -> x + y) successCode
