#r "nuget: Akka.FSharp"
#r "nuget: Akka.Remote"

open Akka.Actor
open Akka.FSharp
open Akka.Configuration

let configuration = 
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
                    port = 9001
                    hostname = localhost
                }
            }
        }")

let system = ActorSystem.Create ("System", configuration)


// Utility Functions - Start
// -------------------------
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


type TaskDetails = {
    StartNumber: uint64;
    EndNumber: uint64;
    WindowLength: uint64;
}

let Worker (mailbox: Actor<_>) =
    let rec loop()= actor{
        let! message = mailbox.Receive();
        let sender = mailbox.Sender()

        printfn "Received message %A" message

        match box message with 
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
                let failureMessage = "Remote Worker Failed!"
                failwith failureMessage
        return! loop()
    }            
    loop()

spawn system "worker" Worker

System.Console.ReadLine() |> ignore
system.Terminate() |> ignore
