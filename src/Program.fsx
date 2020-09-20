#r "nuget: Akka.FSharp"

open Akka.Actor
open Akka.FSharp


// Utility Functions - Start
// -------------------------
let isValidInput (n: int64, k: int64) = 
    n > 0L && k > 0L

let strToInt64 str =
    str |> int64

let dblToInt64 dbl = 
    dbl |> int64

let toDouble (int: int64) = 
    int |> double

let rootOf = 
    toDouble >> sqrt

let squareOf n  = 
    n * n

let isSquare n =
    let rootN = rootOf n
    let floorRootN = floor rootN
    (rootN = floorRootN) && (dblToInt64 floorRootN * dblToInt64 floorRootN = n)
// -----------------------
// Utility Functions - End


// Actor System Logic - Start
// --------------------------
// Create a root actor
let system = ActorSystem.Create("system")

type TaskDetails = {
    StartNumber: int64;
    EndNumber: int64;
    WindowLength: int64;
}

type JobInfo = {
    TotalTaskCount: int64;
    WindowLength: int64;
    TaskCountPerWorker: int64;
    WorkerCount: int64;
}

let Worker (mailbox: Actor<'a>) =
    let rec loop() = 
        actor {
            let! message = mailbox.Receive()
            let sender = mailbox.Sender()

            match box message with
                | :? TaskDetails as input ->
                    let first = input.StartNumber
                    let last = input.EndNumber
                    let windowLen = input.WindowLength

                    // let mutable sumOfSquares = [ for i in first .. first + windowLen - 1L -> squareOf i ] |> Seq.sum

                    let mutable sumOfSquares = 0L
                    for i in first .. first + windowLen - 1L do
                        sumOfSquares <- sumOfSquares + squareOf i
            
                    if isSquare sumOfSquares then
                        sender <! first

                    for i in (first + 1L) .. last do
                        sumOfSquares <- sumOfSquares - squareOf (i - 1L) + squareOf (i + windowLen - 1L)

                        if isSquare sumOfSquares then
                            sender <! i
                | _ -> failwith "Failed! Computation actor failed."
            
            sender <! "Finished execution."
            return! loop()
        }
    loop()


// Helper to distribute tasks among the workers
let rec workDistributionHelper (jobInfo: JobInfo, first: int64, workers: List<IActorRef>, workerIndex: int) = 
    let last = 
        if first + jobInfo.TaskCountPerWorker < jobInfo.TotalTaskCount then first + jobInfo.TaskCountPerWorker - 1L
        else jobInfo.TotalTaskCount
    
    let taskDetails: TaskDetails = {
        StartNumber = first;
        EndNumber = last;
        WindowLength = jobInfo.WindowLength;
    }
    
    let index = if workerIndex |> int64 >= jobInfo.TaskCountPerWorker then 0 else workerIndex
    workers |> List.item index <! taskDetails

    if last < jobInfo.TotalTaskCount then
        workDistributionHelper (jobInfo, last + 1L, workers, workerIndex + 1)


let Supervisor (mailbox: Actor<'a>) = 
    let mutable finishedWorkerCount = 0L
    let mutable totalWorkerCount = 0L
    let mutable parent: IActorRef = null

    let rec loop() = 
        actor {
            let! message = mailbox.Receive()
            let sender = mailbox.Sender()

            match box message with
                | :? JobInfo as input -> 
                    totalWorkerCount <- input.WorkerCount
                    parent <- sender

                    // Create worker handles
                    let workers = 
                        [1L .. input.WorkerCount]
                        |> List.map(fun id ->   let name = "worker_" + (id |> string)
                                                spawn system name Worker)
                    workDistributionHelper (input, 1L, workers, 0)
                | :? int64 as result -> 
                    printfn "%A" result
                | :? string -> 
                    sender <! PoisonPill.Instance
                    finishedWorkerCount <- finishedWorkerCount + 1L
                    if finishedWorkerCount = totalWorkerCount then
                        parent <! "Finish"
                | _ -> failwith "Failed! Supervisor actor failed." 

            return! loop()
        }
    loop()
// ------------------------
// Actor System Logic - End


// Main driver function
let main (n: int64, k: int64) =
    // Input validation
    if not (isValidInput (n, k)) then 
        printfn "Error: Invalid Values for N and/or K."
    else 
        // Configuration for total number of workers
        let numberOfWorkers = 128L
        
        // Amount of work to be done by a single worker
        let taskCountPerWorker = 
            if n <= numberOfWorkers then 1L 
            else n / numberOfWorkers

        let supervisor = spawn system "supervisor" Supervisor

        let jobInfo: JobInfo = {
            TotalTaskCount = n;
            WindowLength = k;
            TaskCountPerWorker = taskCountPerWorker;
            WorkerCount = numberOfWorkers;
        }
        
        let task = supervisor <? jobInfo
        let response = Async.RunSynchronously(task)
        printfn "%A" response
        supervisor <! PoisonPill.Instance
        system.Terminate() |> ignore


// Read command line inputs and pass on to the driver function
match fsi.CommandLineArgs with
    | [|_; n; k|] -> 
        let nVal = strToInt64 n
        let kVal = strToInt64 k
        main (nVal, kVal)
    | _ -> printfn "Error: Invalid Arguments. N and K values must be passed."
