import azure.functions as func
import logging

app = func.FunctionApp(http_auth_level=func.AuthLevel.FUNCTION)

@app.route(route="hello_world")
def hello_world(req: func.HttpRequest) -> func.HttpResponse:
    logging.info('Python HTTP trigger function processed a request.')

    return func.HttpResponse(
            """<h1>Hello from a <a href="https://fluenci.co" target="_blank">FluenCI.co</a>-deployed Go app!</h1>
               <h3>(Click <a href="https://github.com/dave-biz/fluenci-examples/blob/main/go/deployment/main.go" target="_blank">here</a> to view the pipeline that deployed me.)</h3>""",
            status_code=200
    )