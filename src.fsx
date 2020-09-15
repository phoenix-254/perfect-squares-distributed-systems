let main n k = 
    printfn "The N and K values are %d and %d respectively" n k

let toInt str =
    str |> int

let printError = 
    printfn "%s"

match fsi.CommandLineArgs with
    | [| _; nValue; kValue |] -> main (toInt nValue) (toInt kValue)
    | _ -> printError "Invalid Arguments"
    