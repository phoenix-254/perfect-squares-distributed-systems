#r "nuget: Akka.FSharp"

let isValidInput n k = 
    n > 0 && k > 0 && k <= n

let toInt str =
    str |> int

let toFloat int = 
    int |> float

let rootOfN = 
    toFloat >> sqrt

let isSquare n =
    let rootN = rootOfN n
    rootN = (floor rootN)

let generateSquareValuesUpto n =
    [ for i in 1..n -> (i * i) ]

let extractStartNum (resultWindow: option<int[]>) = 
    (rootOfN resultWindow.Value.[0] |> int)

let main n k = 
    if not (isValidInput n k) then 
        printfn "Error: Invalid Values for N and/or K."
    else
        if k = 1 then
            printfn "1"
        else 
            let lastNum = n + k - 1
            let ans = generateSquareValuesUpto lastNum
                    |> Seq.windowed k 
                    |> Seq.tryFind (fun window -> isSquare (Seq.sum window))

            if ans.IsSome then 
                printfn "%d" (extractStartNum ans)
            else 
                printfn "No answer found!"

match fsi.CommandLineArgs with
    | [|_; n; k|] -> main (toInt n) (toInt k)
    | _ -> printfn "Error: Invalid Arguments. N and K values must be passed."
