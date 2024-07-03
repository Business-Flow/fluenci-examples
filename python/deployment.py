import os
import subprocess
import time
import zipfile

def create_zip(zip_name: str, files):
    print(f'Creating .zip file: {zip_name}')
    with zipfile.ZipFile(zip_name, 'w') as zipf:
        for local_file, file_within_zip, is_executable in files:
            print(f"Adding local file {local_file} to zip as {file_within_zip}")
            info = zipfile.ZipInfo(os.path.basename(file_within_zip))
            if is_executable:
                info.external_attr = 0x755 << 16 # Executable permissions
            else:
                info.external_attr = 0o644 << 16 # Read/write permissions
            with open(local_file, 'rb') as f:
                zipf.writestr(info, f.read(), compress_type=zipfile.ZIP_DEFLATED)
    


def get_env_variable(var_name: str):
    print(f"Getting environment variable: '{var_name}'")
    value=os.getenv(var_name)
    if value is None or value == '':
        raise EnvironmentError(f"Environment variable '{var_name}' is missing or not set.")
    return value


AZURE_CLIENT_ID = get_env_variable('AZURE_CLIENT_ID')
AZURE_SECRET = get_env_variable('AZURE_SECRET')
AZURE_TENANT = get_env_variable('AZURE_TENANT')
AZURE_SUBSCRIPTION_ID = get_env_variable('AZURE_SUBSCRIPTION_ID')

files_to_zip = [
    ('python/function_app.py', 'function_app.py', False),
    ('python/host.json', 'host.json', False),
    ('python/function.json', 'hello_world/function.json', False)
]

create_zip('deploy.zip', files_to_zip)        

print("Logging in to Azure.")
az_login = f"az login --service-principal -u {AZURE_CLIENT_ID} -p {AZURE_SECRET} --tenant {AZURE_TENANT}"
result = subprocess.run(["bash", "-c", az_login], capture_output=True, text=True)
print(result.stdout)
if result.returncode != 0:
    print(result.stderr)
    os._exit(1)

print("Setting Azure subscription.")
set_subscription = f"az account set --subscription {AZURE_SUBSCRIPTION_ID}"
result = subprocess.run(["bash", "-c", set_subscription], capture_output=True, text=True)
print(result.stdout)
if result.returncode != 0:
    print(result.stderr)
    os._exit(1)

start = time.time()
print("Deploying ZIP to Azure (Serverless) Function.")
deploy_command = "az functionapp deployment source config-zip --name fluenci-python-demo --resource-group fluenci-prod --src deploy.zip --timeout 1800"
result = subprocess.run(["bash", "-c", deploy_command], capture_output=True, text=True)
elapsed = time.time() - start
print(f"The operation took {elapsed} seconds")
print(result.stdout)
if result.returncode != 0:
    print(result.stderr)
    os._exit(1)