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
        "--target-dir", "target_dir",
        "--target", "x86_64-unknown-linux-gnu"
    ])?;

    run_command("sh", &["-c", "ls", "-lFAR"])?;

    // Assemble deployment .zip file
    let output_file = File::create("target_dir/deploy.zip")?;
    let mut zip = ZipWriter::new(output_file);

    add_file_to_zip(&mut zip, "target_dir/release/my_project", "my_project", *EXECUTABLE)?;
    add_file_to_zip(&mut zip, "rust/deployment/src/azure_function_host.json", "host.json", *NON_EXECUTABLE)?;
    add_file_to_zip(&mut zip, "rust/deployment/src/hello_world_function.json", "hello_world/function.json", *NON_EXECUTABLE)?;

    // Deploy .zip file
    deploy_zip_package()?;

    println!("Deployment Script Completed.");
    Ok(())
}

fn add_file_to_zip(zip: &mut ZipWriter<File>, local_file: &str, zip_file: &str, options: FileOptions) -> Result<()> {
    println!("Adding local file {local_file} to zip as {zip_file}");
    zip.start_file(zip_file, options)?;
    let mut source_file = File::open(local_file)?;
    io::copy(&mut source_file, zip)?;
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

    let deploy_command = "az functionapp deployment source config-zip --name fluenci-rust-demo --resource-group fluenci-prod --src target_dir/deploy.zip";

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