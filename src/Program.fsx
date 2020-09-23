#r "nuget: Akka.FSharp"
#r "nuget: Akka.Remote"

open Akka.Actor
open Akka.Configuration
open Akka.FSharp

open System


// Configuration for Remote 
let remoteWorkersIPAddresses = [|"192.168.0.167:9001"; "192.168.0.85:9001"|]
let remoteConfig = 
    ConfigurationFactory.ParseString(
        @"akka {
            log-config-on-start : on
            stdout-loglevel : DEBUG
            loglevel : ERROR
            actor {
                provider = ""Akka.Remote.RemoteActorRefProvider, Akka.Remote""
                debug : {
                    receive : on
                    autoreceive : on
                    lifecycle : on
                    event-stream : on
                    unhandled : on
                }
            }
            remote {
                helios.tcp {
                    port = 9000
                    hostname = localhost
                }
            }
        }")


// Utility Functions - Start
// -------------------------
let isValidInput (n: uint64, k: uint64) = 
    n > 0UL && k > 0UL

let strToUInt64 str =
    str |> uint64

let strToBool (str: string) =
    str.ToLower() = "true" 

let dblToUInt64 dbl = 
    dbl |> uint64

let toDouble (int: uint64) = 
    int |> double

let rootOf = 
    toDouble >> sqrt

let squareOf n  = 
    n * n

let isSquare n =
    let rootN = rootOf n
    let floorRootN = floor rootN
    (rootN = floorRootN) && (dblToUInt64 floorRootN * dblToUInt64 floorRootN = n)
// -----------------------
// Utility Functions - End


// Actor System Logic - Start
// --------------------------
type TaskDetails = {
    StartNumber: uint64;
    EndNumber: uint64;
    WindowLength: uint64;
}

type JobInfo = {
    ActorSystemRef: IActorRef;
    TotalTaskCount: uint64;
    WindowLength: uint64;
    TaskCountPerWorker: uint64;
    WorkerCount: uint64;
    ShouldRunRemotely: bool;
}

type Worker (name) =
    inherit Actor () 
    override x.OnReceive(message: obj) = 
        let sender = x.Sender

        match message with
            | :? TaskDetails as input ->
                let first = input.StartNumber
                let last = input.EndNumber
                let windowLen = input.WindowLength

                let mutable sumOfSquares = 0UL
                for number in first .. first + windowLen - 1UL do
                    sumOfSquares <- sumOfSquares + squareOf number

                if isSquare sumOfSquares then
                    sender <! first

                for number in (first + 1UL) .. last do
                    sumOfSquares <- sumOfSquares - squareOf (number - 1UL) + squareOf (number + windowLen - 1UL)
                    if isSquare sumOfSquares then
                        sender <! number
                
                sender <! "Done"
            | _ -> 
                let failureMessage = name + " Failed!"
                failwith failureMessage


// Helper to distribute tasks among the workers
let rec distributeWork (jobInfo: JobInfo, first: uint64, workers: List<IActorRef>, workerIndex: int) = 
    let last = 
        if first + jobInfo.TaskCountPerWorker < jobInfo.TotalTaskCount then 
            first + jobInfo.TaskCountPerWorker - 1UL
        else 
            jobInfo.TotalTaskCount
    
    let taskDetails: TaskDetails = {
        StartNumber = first;
        EndNumber = last;
        WindowLength = jobInfo.WindowLength;
    }
    
    let index = if workerIndex |> uint64 >= jobInfo.WorkerCount then 0 else workerIndex
    workers |> List.item index <! taskDetails

    if last < jobInfo.TotalTaskCount then
        distributeWork (jobInfo, last + 1UL, workers, workerIndex + 1)


let getWorkerInstance (jobInfo: JobInfo, id: uint64) = 
    if jobInfo.ShouldRunRemotely then
        let actorSelStr = "akka.tcp://System@" + remoteWorkersIPAddresses.[id-1UL] + "/user/worker"
        jobInfo.ActorSystemRef.ActorSelection(actorSelStr)
    else 
        let properties = [| "worker_" + (id |> string) :> obj |]
        jobInfo.ActorSystemRef.ActorOf(Props(typedefof<Worker>, properties))


type Supervisor (name) = 
    inherit Actor()
    let mutable finishedWorkerCount = 0UL
    let mutable totalWorkerCount = 0UL
    let mutable parent: IActorRef = null
    
    override x.OnReceive(message: obj) = 
        let sender = x.Sender

        match message with
            | :? JobInfo as input -> 
                totalWorkerCount <- input.WorkerCount
                parent <- sender

                let workers = 
                    [1UL .. input.WorkerCount]
                    |> List.map(fun id -> getWorkerInstance(jobInfo, id))

                distributeWork (input, 1UL, workers, 0)
            | :? int64 as result -> 
                printfn "%A" result
            | :? uint64 as result -> 
                printfn "%A" result
            | :? string -> 
                sender <! PoisonPill.Instance
                finishedWorkerCount <- finishedWorkerCount + 1UL
                if finishedWorkerCount = totalWorkerCount then
                    parent <! "Finish!"
            | _ -> 
                let failureMessage = name + " Failed!"
                failwith failureMessage
// ------------------------
// Actor System Logic - End


// Main driver function
let main (n: uint64, k: uint64, shouldRunRemotely: bool) =
    // Input validation
    if not (isValidInput (n, k)) then 
        printfn "Error: Invalid Values for N and/or K."
    else 
        // Configuration for total number of workers
        let numberOfWorkers = 
            if shouldRunRemotely then 
                remoteWorkersIPAddresses.Length |> uint64
            else 
                Environment.ProcessorCount |> uint64
        
        // Create a root actor
        let system = 
            if shouldRunRemotely then
                ActorSystem.Create ("System", remoteConfig)
            else 
                ActorSystem.Create "System"

        // Amount of work to be done by a single worker
        let taskCountPerWorker = 
            if n <= numberOfWorkers then 1UL 
            else n / numberOfWorkers

        let supervisor = system.ActorOf(Props(typedefof<Supervisor>, [| "supervisor" :> obj |]))

        let jobInfo: JobInfo = {
            TotalTaskCount = n;
            WindowLength = k;
            TaskCountPerWorker = taskCountPerWorker;
            WorkerCount = numberOfWorkers;
            ShouldRunRemotely = shouldRunRemotely;
        }

        let task = supervisor <? jobInfo
        Async.RunSynchronously(task) |> ignore
        supervisor <! PoisonPill.Instance
        system.Terminate() |> ignore


// Read command line inputs and pass on to the driver function
match fsi.CommandLineArgs with
    | [|_; n; k|] -> 
        let nVal = strToUInt64 n
        let kVal = strToUInt64 k
        main (nVal, kVal, false)
    | [|_; n; k; runRemotely|] -> 
        let nVal = strToUInt64 n
        let kVal = strToUInt64 k
        let shouldRunRemotely = strToBool runRemotely
        main (nVal, kVal, shouldRunRemotely)
    | _ -> printfn "Error: Invalid Arguments. N and K values must be passed."
