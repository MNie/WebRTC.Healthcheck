# WebRTC.Heathcheck

* NuGet Status [![NuGet](https://buildstats.info/nuget/WebRTC.Healthcheck?includePreReleases=true)](https://www.nuget.org/packages/WebRTC.Healthcheck)

Library with Healtchecks to check a health of WebRTC connection.

# How to add healthchecks

```fsharp
type Startup(configuration: IConfiguration) =
    let request = {
        Servers = [| Address "serverhere" |]
        CredentialsConfig = Some {
            OTP = Otp "otphere"
            UserPostfix = Some (UserPostfix "turn")
        }
    }
    
    member _.ConfigureServices(services: IServiceCollection) =
        ...
        services.AddHealthChecks()
            .AddCheck(
                "WebRTC",
                WebRTCHealthCheck(
                    request,
                    {   Success = Console.WriteLine
                        Failure = Console.WriteLine }
                )
            ) |> ignore
        services
            .AddHealthChecksUI(fun s -> s.AddHealthCheckEndpoint("WebRTC", "/health") |> ignore)
            .AddInMemoryStorage() |> ignore
        ...
        
    member _.Configure(app: IApplicationBuilder, env: IWebHostEnvironment) =
        ...
        app.
           .UseEndpoints(fun endpoints ->
                endpoints.MapControllers() |> ignore
                endpoints.MapHealthChecksUI() |> ignore
                endpoints.MapHealthChecks(
                    "/health",
                    HealthCheckOptions(
                        Predicate = (fun _ -> true),
                        ResponseWriter = Func<HttpContext, HealthReport, Task>(fun (context) (c: HealthReport) -> UIResponseWriter.WriteHealthCheckUIResponse(context, c))
                    )
                ) |> ignore
            ) |> ignore
        ...
```
