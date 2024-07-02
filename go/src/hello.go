package main

import (
	"fmt"
	"log"
	"net/http"
	"os"
)

func helloHandler(w http.ResponseWriter, r *http.Request) {
	message := `<h1>Hello from a <a href="https://fluenci.co" target="_blank">FluenCI.co</a>-deployed Go app!</h1>
                <h3>(Click <a href="https://github.com/dave-biz/fluenci-examples/blob/main/go/deployment/main.go" target="_blank">here</a> to view the pipeline that deployed me.)</h3>`

	fmt.Fprint(w, message)
}

func main() {
	listenAddr := ":8081"
	if val, ok := os.LookupEnv("FUNCTIONS_CUSTOMHANDLER_PORT"); ok {
		listenAddr = ":" + val
	}
	http.HandleFunc("/hello_world", helloHandler)
	log.Printf("About to listen on %s. Go to https://127.0.0.1%s/", listenAddr, listenAddr)
	log.Fatal(http.ListenAndServe(listenAddr, nil))
}
