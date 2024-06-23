use std::{fs::File, io, process::Command};
use anyhow::{anyhow, Context, Result};
use lazy_static::lazy_static;
use zip::{write::FileOptions, ZipWriter};

fn main() -> Result<()> {
    println!("Hello from the Deployment Script!");
    
    // Build the project
    run_command("cargo", &vec![
        "build", 
        "--release",
        "--manifest-path", "rust/src/my_project/Cargo.toml",
        "--target-dir", "target_dir"
    ])?;

    // Assemble deployment .zip file
    let output_file = File::create("target_dir/deploy.zip")?;
    let mut zip = ZipWriter::new(output_file);

    zip.start_file("my_project", *EXECUTABLE)?;
    let mut source_file = File::open("target_dir/release/my_project")?;
    io::copy(&mut source_file, &mut zip)?;

    zip.start_file("host.json", *NON_EXECUTABLE)?;
    let mut source_file = File::open("rust/deployment/src/azure_function_host.json")?;
    io::copy(&mut source_file, &mut zip)?;

    zip.start_file("hello_world/function.json", *NON_EXECUTABLE)?;
    let mut source_file = File::open("rust/deployment/src/hello_world_function.json")?;
    io::copy(&mut source_file, &mut zip)?;

    // Deploy .zip file
    deploy_zip_package()?;

    println!("Deployment Script Completed.");
    Ok(())
}

fn deploy_zip_package() -> Result<()> {
    let login_command = format!("az login --service-principal -u {} -p {} --tenant {}",
        // Environment variables/secret names are automatically prepended with `fluenci_`, for security:
        get_env_var_or_err("AZURE_CLIENT_ID")?,
        get_env_var_or_err("AZURE_SECRET")?,
        get_env_var_or_err("AZURE_TENANT")?);

    run_command("sh", &["-c", login_command.as_str()])?;

    let set_subscription_command = format!("az account set --subscription {}",
        get_env_var_or_err("AZURE_SUBSCRIPTION_ID").unwrap());

    run_command("sh", &["-c", set_subscription_command.as_str()])?;

    let deploy_command = "az functionapp deployment source config-zip --name fluenci-prod-linux --resource-group fluenci-prod --src target_dir/deploy.zip";

    run_command("sh", &[
        "-c",
        deploy_command
    ])?;
    Ok(())
}

fn get_env_var_or_err(name: &str) -> Result<String, anyhow::Error> {
    std::env::var(name)
        .map_err(|_| anyhow!("Missing environment variable: '{name}'."))
}


fn run_command(command: &str, args: &[&str]) -> Result<()> {
    println!("***** Executing command: {} {}", command, args.join(", "));

    let status = Command::new(command).args(args).status().with_context(|| format!("Failed to execute command: {}", command))?;

    if !status.success() {
        anyhow::bail!("Command {} failed with exit code: {}", command, status.code().unwrap_or(-1));
    }

    Ok(())
}

lazy_static! {
    pub(crate) static ref EXECUTABLE: zip::write::FileOptions = 
        FileOptions::default()
            .compression_method(zip::CompressionMethod::Deflated)
            .unix_permissions(0o755);

    pub(crate) static ref NON_EXECUTABLE: zip::write::FileOptions = 
        FileOptions::default()
            .compression_method(zip::CompressionMethod::Deflated)
            .unix_permissions(0o644);
}