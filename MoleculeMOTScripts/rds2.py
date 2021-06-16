from __future__ import print_function
import glob
import os
import shutil
from datetime import datetime

# Environment variables
LOCAL_MOTMASTERDATA_PATH = "D:\mot_master_data"
LOCAL_NOTEBOOK_PATH = "D:\ipython_notebooks"

REMOTE_MOTMASTERDATA_PATH = "Y:\live\mot_master_data"
REMOTE_NOTEBOOK_PATH = "Y:\live\ipython_notebooks"
LOG_PATH = os.path.join("C:\ControlPrograms\EDMSuite\MoleculeMOTScripts", "rds.log")

now = datetime.now()
local_motmasterdata_files_list = glob.glob(LOCAL_MOTMASTERDATA_PATH + '/*.zip')
local_notebook_files_list = glob.glob(LOCAL_NOTEBOOK_PATH + '/*.ipynb')

# motmaster backup
if os.path.exists(REMOTE_MOTMASTERDATA_PATH):
    for local_file in local_motmasterdata_files_list:
        basename = os.path.basename(local_file)
        remote_file = os.path.join(REMOTE_MOTMASTERDATA_PATH, basename)
        if not os.path.exists(remote_file):
            shutil.copy(local_file, remote_file)
    with open(LOG_PATH, 'a') as f:
        f.write('\nlast updated: {}'.format(now.strftime("%m/%d/%Y, %H:%M:%S")))
else:
    with open(LOG_PATH, 'a') as f:
        f.write('\nlast checked: {}, remote backup location unavailable'.format(now.strftime("%m/%d/%Y, %H:%M:%S")))

# notebook backup
if os.path.exists(REMOTE_NOTEBOOK_PATH):
    for local_file in local_notebook_files_list:
        basename = os.path.basename(local_file)
        backup_file = os.path.join(REMOTE_NOTEBOOK_PATH, basename)
        if not os.path.exists(backup_file):
            shutil.copy(local_file, backup_file)
    with open(LOG_PATH, 'a') as f:
        f.write('\nlast updated: {}'.format(now.strftime("%m/%d/%Y, %H:%M:%S")))
else:
    with open(LOG_PATH, 'a') as f:
        f.write('\nlast checked: {}, local backup location unavailable'.format(now.strftime("%m/%d/%Y, %H:%M:%S")))
