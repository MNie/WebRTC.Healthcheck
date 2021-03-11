namespace WebApi.Controllers

open System
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open WebRTC.Healthcheck.Credentials
open WebRTC.Healthcheck.Runner
open WebRTC.Healthcheck.Server
open WebRTC.Healthcheck.Utils
open WebRTC.Healthcheck.Model

type DtoRequest = {
    Servers: string []
    Otp: string
    UserPostfix: string
}

[<ApiController>]
[<Route("[controller]")>]
type HCController (logger : ILogger<HCController >) =
    inherit ControllerBase()
    
    let log = {
        Success = logger.LogInformation
        Failure = logger.LogError
    }

    [<HttpPost>]
    member _.Check(req: DtoRequest) =
        let config =
            if String.IsNullOrWhiteSpace req.Otp then
                None
            else
                let postfix =
                    if String.IsNullOrWhiteSpace req.UserPostfix then None
                    else req.UserPostfix |> UserPostfix |> Some
                Some { OTP = Otp req.Otp; UserPostfix = postfix }
        
        req.Servers
        |> Array.map Address
        |> fun servers -> {
            Servers = servers
            CredentialsConfig = config
        }
        |> run log

