module Credentials

open System
open System.Security.Cryptography
open System.Text

let private secret =
    try
        let v = Environment.GetEnvironmentVariable "WEBRTC_SECRET"
        if String.IsNullOrWhiteSpace v then
            None
        else Some v
    with _ ->
        None
        
let private userPostfix =
    try
        let v = Environment.GetEnvironmentVariable "WEBRTC_USER_POSTFIX"
        if String.IsNullOrWhiteSpace v then
            "turn"
        else v
    with _ ->
        "turn"

type Login = Login of string
type Password = Password of string

type Credentials = {
    Login: Login
    Password: Password
}

let private generatePass (secret: string) (Login user) =
    use hmacsha1 = new HMACSHA1 ()
    let secretBytes = Encoding.UTF8.GetBytes secret
    let userBytes = Encoding.UTF8.GetBytes user
    hmacsha1.Key <- secretBytes
    hmacsha1.ComputeHash userBytes
    |> Convert.ToBase64String
    |> Password
    
let private generateLogin () =
    let timestamp =
        DateTimeOffset.Now.ToUnixTimeMilliseconds ()
        / 1000L
        + (24L * 3600L)
        
    Login $"%d{timestamp}:%s{userPostfix}"
    
let generate () =
    match secret with
    | Some s ->
        let login = generateLogin ()
        let pass = generatePass s login
        Some { Login = login; Password = pass }
    | None -> None
