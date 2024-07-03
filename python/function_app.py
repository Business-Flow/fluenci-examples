import azure.functions as func
import datetime
import json
import logging

app = func.FunctionApp()

@app.route(route="hello_world", auth_level=func.AuthLevel.ANONYMOUS)
def hello_world(req: func.HttpRequest) -> func.HttpResponse:
    logging.info('Python HTTP trigger function processed a request.')

    return func.HttpResponse(
            """<h1>Hello from a <a href="https://fluenci.co" target="_blank">FluenCI.co</a>-deployed Python app!</h1>
               <h3>(Click <a href="https://github.com/dave-biz/fluenci-examples/blob/main/python/deployment.py" target="_blank">here</a> to view the pipeline that deployed me.)</h3>""",
            status_code=200
    )