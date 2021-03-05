open Expecto

let tests = testList "Healthcheck" [
    CandidateTests.tests
    ServerTests.tests
]

[<EntryPoint>]
let main argv =
    runTestsWithCLIArgs [] argv tests