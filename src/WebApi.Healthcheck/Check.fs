﻿namespace WebApi.HealthCheck

open Microsoft.Extensions.Diagnostics.HealthChecks
open WebRTC.Healthcheck.Model
open WebRTC.Healthcheck.Utils
open WebRTC.Healthcheck.Runner

type WebRTCHealthCheck (toCheck: Request, log: Log) =
    interface IHealthCheck with
        member __.CheckHealthAsync (context, cancellationToken) =
            async {
                let! result = run log toCheck
                let unhealthy =
                    result
                    |> Array.filter (fun x -> x.ResultCode <> 0)
                let isSuccess = unhealthy |> Array.isEmpty
                if isSuccess then
                    return HealthCheckResult.Healthy("All WebRTC servers are healthy")
                else
                    let msg =
                        unhealthy
                        |> Array.map (fun x -> x.Address)
                        |> String.concat ","
                        |> fun x -> $"An unhealthy result for: %s{x} servers"
                    return HealthCheckResult(context.Registration.FailureStatus, msg)
            } |> Async.StartAsTask
            
