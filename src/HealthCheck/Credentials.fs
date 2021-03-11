namespace WebRTC.Healthcheck

module Credentials =

    open System
    open System.Security.Cryptography
    open System.Text

    type OTP = Otp of string
    type UserPostfix = UserPostfix of string

    type Config = {
        OTP: OTP
        UserPostfix: UserPostfix option
    }

    type Login = Login of string
    type Password = Password of string

    type Credentials = {
        Login: Login
        Password: Password
    }

    let private generatePass (Otp secret) (Login user) =
        use hmacsha1 = new HMACSHA1 ()
        let secretBytes = Encoding.UTF8.GetBytes secret
        let userBytes = Encoding.UTF8.GetBytes user
        hmacsha1.Key <- secretBytes
        hmacsha1.ComputeHash userBytes
        |> Convert.ToBase64String
        |> Password
        
    let private generateLogin userPostfix =
        let (UserPostfix postfix) =
            userPostfix
            |> Option.defaultValue (UserPostfix "turn")
        let timestamp =
            DateTimeOffset.Now.ToUnixTimeMilliseconds ()
            / 1000L
            + (24L * 3600L)
            
        Login $"%d{timestamp}:%s{postfix}"
        
    let generate config =
        match config with
        | Some { OTP = secret; UserPostfix = userPostfix } ->
            let login = generateLogin userPostfix
            let pass = generatePass secret login
            Some { Login = login; Password = pass }
        | None -> None
