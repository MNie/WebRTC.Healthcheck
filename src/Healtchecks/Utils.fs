module Utils

open System

let asList l =
    l
    |> List.toSeq
    |> System.Collections.Generic.List

module Log =
    let success (msg: string) =
        Console.ForegroundColor <- ConsoleColor.Green
        Console.WriteLine msg
        Console.ResetColor ()
        

    let failure (msg: string) =
        Console.ForegroundColor <- ConsoleColor.Red
        Console.WriteLine msg
        Console.ResetColor ()
        
    let info (msg: string) =
        Console.WriteLine msg
    