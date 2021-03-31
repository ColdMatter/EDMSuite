from __future__ import print_function
import glob
import os
import shutil
from datetime import datetime

# Environment variables
LOCAL_PATH = "C:\Users\cafmot\Box Sync\CaF MOT\MOTData\MOTMasterData"
LOCAL_BACKUP_PATH = "D:\Data2021"
REMOTE_PATH = "Y:\live\MOTData\MOTMasterData"
LOG_PATH = os.path.join("C:\ControlPrograms\EDMSuite\MoleculeMOTScripts", "autouploader.log")

now = datetime.now()
local_files_list = glob.glob(LOCAL_PATH+'/*.zip')

# remote backup
if os.path.exists(REMOTE_PATH):
    for local_file in local_files_list:
        basename = os.path.basename(local_file)
        remote_file = os.path.join(REMOTE_PATH, basename)
        if not os.path.exists(remote_file):
            with open(LOG_PATH, 'a') as f:
                f.write('\n{} was not in remote backup location...copying now'.format(basename))
            shutil.copy(local_file, remote_file)
    with open(LOG_PATH, 'a') as f:
        f.write('\nlast updated: {}'.format(now.strftime("%m/%d/%Y, %H:%M:%S")))
else:
    with open(LOG_PATH, 'a') as f:
        f.write('\nlast checked: {}, remote backup location unavailable'.format(now.strftime("%m/%d/%Y, %H:%M:%S")))

# local backup
if os.path.exists(LOCAL_BACKUP_PATH):
    for local_file in local_files_list:
        basename = os.path.basename(local_file)
        backup_file = os.path.join(LOCAL_BACKUP_PATH, basename)
        if not os.path.exists(backup_file):
            with open(LOG_PATH, 'a') as f:
                f.write('\n{} was not in local backup location...copying now'.format(basename))
            shutil.copy(local_file, backup_file)
    with open(LOG_PATH, 'a') as f:
        f.write('\nlast updated: {}'.format(now.strftime("%m/%d/%Y, %H:%M:%S")))
else:
    with open(LOG_PATH, 'a') as f:
        f.write('\nlast checked: {}, local backup location unavailable'.format(now.strftime("%m/%d/%Y, %H:%M:%S")))
