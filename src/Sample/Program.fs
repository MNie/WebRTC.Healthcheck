
open System

open WebRTC.Healthcheck.Runner
open WebRTC.Healthcheck.Credentials
open WebRTC.Healthcheck.Server
open WebRTC.Healthcheck.Model
open WebRTC.Healthcheck.Utils

let private secret =
    try
        let v = Environment.GetEnvironmentVariable "WEBRTC_SECRET"
        if String.IsNullOrWhiteSpace v then
            None
        else
            v
            |> Otp
            |> Some
    with _ ->
        None
        
let private userPostfix =
    try
        let v = Environment.GetEnvironmentVariable "WEBRTC_USER_POSTFIX"
        if String.IsNullOrWhiteSpace v then
            UserPostfix "turn"
        else UserPostfix v
    with _ ->
        UserPostfix "turn"
        
let log = {
    Success = Log.success
    Failure = Log.failure
}

[<EntryPoint>]
let main argv =
    
    let config =
        secret
        |> Option.map (fun s -> {
            OTP = s
            UserPostfix = Some userPostfix
        })
    
    argv.[0].Split ","
    |> Array.map Address
    |> fun servers -> {
        Servers = servers
        CredentialsConfig = config
    }
    |> run log
    // Do something with a result

    0