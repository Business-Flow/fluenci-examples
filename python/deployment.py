import logging
import os
import zipfile

def create_zip(zip_name: str, files):
    logging.info(f'Creating .zip file: {zip_name}')
    with zipfile.ZipFile(zip_name, 'w') as zipf:
        for local_file, file_within_zip, is_executable in files:
            logging.info(f"Adding local file {local_file} to zip as {file_within_zip}")
            info = zipfile.ZipInfo(os.path.basename(file_within_zip))
            if is_executable:
                info.external_attr = 0x755 << 16 # Executable permissions
            else:
                info.external_attr = 0o644 << 16 # Read/write permissions
            with open(local_file, 'rb') as f:
                zipf.writestr(info, f.read(), compress_type=zipfile.ZIP_DEFLATED)
    


def get_env_variable(var_name):
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
    ('python/host.json', 'host.json', False)
]

create_zip('deploy.zip', files_to_zip)        