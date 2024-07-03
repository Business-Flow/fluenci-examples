import logging
import os
import zipfile

AZURE_CLIENT_ID = get_env_variable('AZURE_CLIENT_ID')
AZURE_SECRET = get_env_variable('AZURE_SECRET')
AZURE_TENANT = get_env_variable('AZURE_TENANT')
AZURE_SUBSCRIPTION_ID = get_env_variable('AZURE_SUBSCRIPTION_ID')

files_to_zip = [
    ('function_app.py', false),
    ('host.json', false)
]

create_zip('deploy.zip', files_to_zip)

def create_zip(zip_name, files):
    logging.info(f'Creating .zip file: {zip_name}')
    with zipfile.ZipFile(zip_name, 'w') as zipf:
        for file, is_executable in files:
            logging.info(f"Adding file to zip: {file}")
            info = zipfile.ZipInfo(os.path.basename(file))
            if is_executable:
                info.external_attr = 0x755 << 16 # Executable permissions
            else:
                info.external_attr = 0o644 << 16 # Read/write permissions
            with open(file, 'rb' as f):
                zipf.writestr(info, f.read(), compress_type=zipfile.ZIP_DEFLATED)
    


def get_env_variable(var_name):
        value=os.getenv(var_name)
        if value is None or value == '':
            raise EnvironmentError(f"Environment variable '{var_name}' is missing or not set.")
        return value