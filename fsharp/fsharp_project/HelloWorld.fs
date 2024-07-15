namespace Company.Function

open System
open System.IO
open Microsoft.AspNetCore.Mvc
open Microsoft.Azure.WebJobs
open Microsoft.Azure.WebJobs.Extensions.Http
open Microsoft.AspNetCore.Http
open Newtonsoft.Json
open Microsoft.Extensions.Logging

module fsharp_project =
    [<FunctionName("hello_world")>]
    let run ([<HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)>]req: HttpRequest) (log: ILogger) =
        async {
            log.LogInformation("F# HTTP trigger function processed a request.")
            
            let responseMessage = """
                <h1>Hello from a <a href="https://fluenci.co" target="_blank">FluenCI.co</a>-deployed F# app!</h1>
               <h3>(Click <a href="https://github.com/dave-biz/fluenci-examples/blob/main/fsharp/deployment/Program.fs" target="_blank">here</a> to view the F# pipeline that deployed me.)</h3>
            """

            let contentResult = ContentResult(Content = responseMessage, ContentType = "text/html", StatusCode = Nullable(200))
            return contentResult :> IActionResult
        } |> Async.StartAsTask