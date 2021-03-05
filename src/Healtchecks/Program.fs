open Server
open Utils

let run server =
    try
        match Type.parse server with
        | Some serv ->
            State.start serv
            
            let result = PeerConnection.connect serv
        
            State.stop ()
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
