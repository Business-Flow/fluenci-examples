using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace csharp_project;

    public class hello_world
    {
        private readonly ILogger _logger;

        public hello_world(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<hello_world>();
        }

        [Function("hello_world")]
        public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/html; charset=utf-8");

            response.WriteString("""
               <h1>Hello from a <a href="https://fluenci.co" target="_blank">FluenCI.co</a>-deployed C# app!</h1>
               <h3>(Click <a href="https://github.com/dave-biz/fluenci-examples/blob/main/csharp/deployment/Program.cs" target="_blank">here</a> to view the C# pipeline that deployed me.)</h3>
            """);

            return response;
        }
    }

