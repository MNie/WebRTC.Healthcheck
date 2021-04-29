namespace WebApi

open System
open System.Threading.Tasks
open HealthChecks.UI.Client
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Diagnostics.HealthChecks
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Diagnostics.HealthChecks
open Microsoft.Extensions.Hosting
open WebApi.HealthCheck
open WebRTC.Healthcheck.Credentials
open WebRTC.Healthcheck.Model
open WebRTC.Healthcheck.Server

type Startup(configuration: IConfiguration) =
    let request = {
        Servers = [| Address "serverhere" |]
        CredentialsConfig = Some {
            OTP = Otp "otphere"
            UserPostfix = Some (UserPostfix "turn")
        }
    }
    
    member _.Configuration = configuration

    // This method gets called by the runtime. Use this method to add services to the container.
    member _.ConfigureServices(services: IServiceCollection) =
        // Add framework services.
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
        services.AddControllers() |> ignore
        
    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    member _.Configure(app: IApplicationBuilder, env: IWebHostEnvironment) =
        if (env.IsDevelopment()) then
            app.UseDeveloperExceptionPage() |> ignore
        app.UseHttpsRedirection()
           .UseRouting()
           .UseAuthorization()
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
