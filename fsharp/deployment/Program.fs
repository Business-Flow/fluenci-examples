open System
open System.Diagnostics
open System.IO;

let getOrFail failMsg opt =
    match opt with
    | Some x -> x
    | None -> failwith failMsg



let getEnvironmentVariableOrFail name =
    name 
    |> Environment.GetEnvironmentVariable
    |> Option.ofObj
    |> getOrFail (sprintf "Environment variable '%s' missing." name)

let runCommand cmd args =
    let myProcess = new Process(
            StartInfo = new ProcessStartInfo(
                FileName=cmd,
                Arguments=args,
                RedirectStandardOutput=false,
                RedirectStandardError=false,
                UseShellExecute=false,
                CreateNoWindow=true
        )
    )

    myProcess.OutputDataReceived.Add(fun e ->
        if not (String.IsNullOrEmpty(e.Data)) then
            printfn "Output: %s" e.Data
            Console.Out.Flush()
    )

    myProcess.ErrorDataReceived.Add(fun e ->
        if not (String.IsNullOrEmpty(e.Data)) then
            printfn "Error: %s" e.Data
            Console.Out.Flush()
    )

    myProcess.Start() |> ignore
    myProcess.WaitForExit()

    if myProcess.ExitCode <> 0 then
        printfn $"Command '{cmd}' failed with exit code {myProcess.ExitCode}.  Terminating."
        Environment.Exit myProcess.ExitCode
    ()

// ***** Start the Actual Pipeline Steps ******

printfn "Starting F# pipeline."

let AZURE_CLIENT_ID = getEnvironmentVariableOrFail "AZURE_CLIENT_ID"
let AZURE_SECRET = getEnvironmentVariableOrFail("AZURE_SECRET")
let AZURE_TENANT= getEnvironmentVariableOrFail("AZURE_TENANT")
let AZURE_SUBSCRIPTION_ID = getEnvironmentVariableOrFail("AZURE_SUBSCRIPTION_ID")

let projectCodeDirectory = "fsharp/fsharp_project"
printfn $"Changing to directory: {projectCodeDirectory}"
Directory.SetCurrentDirectory(projectCodeDirectory);

printfn $"Current directory is {Directory.GetCurrentDirectory()}"

printfn "Logging in to Azure."
runCommand "az" $"login --service-principal -u {AZURE_CLIENT_ID} -p {AZURE_SECRET} --tenant {AZURE_TENANT}"

printfn "Setting Azure Subscription."
runCommand "az" $"account set --subscription {AZURE_SUBSCRIPTION_ID}"

printfn "Publishing Function App ..."
runCommand "func" "azure functionapp publish fluenci-fsharp-demo  --dotnet-isolated --dotnet-version 8.0"