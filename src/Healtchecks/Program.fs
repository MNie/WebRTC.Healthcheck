open Server
open Utils

let run server =
    try
        match Type.parse server with
        | Some serv ->
            State.start serv
            
            PeerConnection.connect serv
        
            State.stop ()
        | _ -> ()
    with er ->
        Log.failure $"Error occured while processing: %s{server}, error: %A{er}"

[<EntryPoint>]
let main argv =
    argv.[0].Split ","
    |> Array.iter run
    
    0 // return an integer exit code