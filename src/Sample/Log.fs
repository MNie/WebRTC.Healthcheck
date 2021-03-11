module Log

    open System

    let success (msg: string) =
        Console.ForegroundColor <- ConsoleColor.Green
        Console.WriteLine msg
        Console.ResetColor ()
        

    let failure (msg: string) =
        Console.ForegroundColor <- ConsoleColor.Red
        Console.WriteLine msg
        Console.ResetColor ()
        