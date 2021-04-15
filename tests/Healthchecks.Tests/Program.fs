open Expecto

let tests = testList "Healthcheck" [
    ServerTests.tests
]

[<EntryPoint>]
let main argv =
    runTestsWithCLIArgs [] argv tests