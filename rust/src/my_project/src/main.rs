use std::{env, net::Ipv4Addr};

use axum::{response::Html, routing::get, Router};

#[tokio::main]
async fn main() {
    let app = Router::new()
        .route("/hello_world", get(hello_world_handler));

    let port: u16 = match env::var("FUNCTIONS_CUSTOMHANDLER_PORT") {
        Ok(val) => val.parse().expect("Custom Handler port is not a number!"),
        Err(_) => 3000,
    };

    let listen_on = format!("{}:{}", Ipv4Addr::LOCALHOST, port);

    let listener = tokio::net::TcpListener::bind(&listen_on).await.unwrap();

    println!("Service initialized on port {listen_on}.");
    axum::serve(listener, app).await.unwrap();
}

async fn hello_world_handler() -> Html<&'static str> {
    Html(r#"<h1>Hello from a <a href="https://fluenci.com">FluenCI.com</a>-deployed Rust app!</h1>
            <h3>(Click <a href="https://github.com/dave-biz/fluenci-examples/blob/main/rust/deployment/src/main.rs" target="_blank">here</a> to view the pipeline that deployed me.)</h3>
    "#)
}