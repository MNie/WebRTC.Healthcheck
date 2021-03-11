namespace WebRTC.Healthcheck

module Utils =

    [<Literal>]
    let successCode = 0
    [<Literal>]
    let errorCode = 1

    let asList l =
        l
        |> List.toSeq
        |> System.Collections.Generic.List

    type Log = {
        Success: string -> unit
        Failure: string -> unit
    }
        