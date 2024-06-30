package main

import (
	"archive/zip"
	"fmt"
	"io"
	"os"
	"os/exec"
	"time"
)

func main() {
	err := os.Setenv("CC", "musl-gcc")
	if err != nil {
		fmt.Println("Error setting environment variable:", err)
		os.Exit(1)
	}

	build_cmd := exec.Command("bash", "-c", "go build -o target_dir/my_project go/src/hello.go")

	fmt.Println("Building project.")
	output, err := build_cmd.CombinedOutput()
	fmt.Println(string(output))
	if err != nil {
		os.Exit(1)
	}
	fmt.Println("Project successfully built with output: ", output)

	// List of files to add to the zip file with their new names and executable flag
	filesToZip := []struct {
		localFileName     string
		fileNameWithinZip string
		executable        bool
	}{
		{"target_dir/my_project", "my_project", true},
		{"go/deployment/azure_function_host.json", "host.json", false},
		{"go/deployment/hello_world_function.json", "hello_world/function.json", false},
	}

	zipFile, err := os.Create("target_dir/deploy.zip")
	if err != nil {
		fmt.Println(err)
		os.Exit(1)
	}
	defer zipFile.Close()

	// Create a new zip writer
	zipWriter := zip.NewWriter(zipFile)
	defer zipWriter.Close()

	for _, file := range filesToZip {
		err := addFileToZip(zipWriter, file.localFileName, file.fileNameWithinZip, file.executable)
		if err != nil {
			fmt.Printf("Failed to add file %s to zip: %s\n", file.localFileName, err)
			os.Exit(1)
		}
	}

	azure_client_id := getEnvVarOrTerminate("AZURE_CLIENT_ID")
	azure_secret := getEnvVarOrTerminate("AZURE_SECRET")
	azure_tenant := getEnvVarOrTerminate("AZURE_TENANT")
	fmt.Println("Logging in to Azure.")
	az_login := fmt.Sprintf("az login --service-principal -u %s -p %s --tenant %s", azure_client_id, azure_secret, azure_tenant)
	login_command := exec.Command("bash", "-c", az_login)

	output, err = login_command.CombinedOutput()
	fmt.Println(string(output))
	if err != nil {
		os.Exit(1)
	}

	azure_subscription_id := getEnvVarOrTerminate("AZURE_SUBSCRIPTION_ID")
	fmt.Println("Setting Azure subscription.")
	set_subscription := fmt.Sprintf("az account set --subscription %s", azure_subscription_id)
	set_subscription_command := exec.Command("bash", "-c", set_subscription)

	output, err = set_subscription_command.CombinedOutput()
	fmt.Println(string(output))
	if err != nil {
		os.Exit(1)
	}

	start := time.Now()
	fmt.Println("Deploying ZIP to Azure (Serverless) Function.")
	deploy_command := exec.Command("bash", "-c", "az functionapp deployment source config-zip --name fluenci-go-demo --resource-group fluenci-prod --src target_dir/deploy.zip --timeout 1800")
	output, err = deploy_command.CombinedOutput()
	elapsed := time.Since(start)
	fmt.Printf("The operation took %v seconds\n", elapsed.Seconds())
	fmt.Println(string(output))
	if err != nil {
		os.Exit(1)
	}
	// let set_subscription_command = format!("az account set --subscription {}",
	//     get_env_var_or_err("AZURE_SUBSCRIPTION_ID").unwrap());

	// run_command("sh", &["-c", set_subscription_command.as_str()])?;

	// let deploy_command = "az functionapp deployment source config-zip --name fluenci-rust-demo --resource-group fluenci-prod --src target_dir/deploy.zip";

	// run_command("sh", &[
	//     "-c",
	//     deploy_command
	// ])?;

}

func getEnvVarOrTerminate(key string) string {
	value, exists := os.LookupEnv(key)
	if !exists {
		fmt.Printf("Environment variable %s is not set. Terminating the process.\n", key)
		os.Exit(1)
	}
	return value
}

func addFileToZip(zipWriter *zip.Writer, localFileName, fileNameWithinZip string, executable bool) error {
	fmt.Printf("Adding file '%s' to zip as '%s' (%t)\n", localFileName, fileNameWithinZip, executable)

	// Open the local file
	file, err := os.Open(localFileName)
	if err != nil {
		return err
	}
	defer file.Close()

	// Get the file info
	fileInfo, err := file.Stat()
	if err != nil {
		return err
	}

	// Create a zip file header based on the file info
	header, err := zip.FileInfoHeader(fileInfo)
	if err != nil {
		return err
	}

	// Set the name of the file inside the zip archive
	header.Name = fileNameWithinZip

	// Set executable permissions if needed
	if executable {
		header.SetMode(0755)
	} else {
		header.SetMode(0644)
	}

	// Create a writer for the file inside the zip archive
	zipWriter2, err := zipWriter.CreateHeader(header)
	if err != nil {
		return err
	}

	// Copy the file data to the zip archive
	_, err = io.Copy(zipWriter2, file)
	if err != nil {
		return err
	}

	return nil
}
