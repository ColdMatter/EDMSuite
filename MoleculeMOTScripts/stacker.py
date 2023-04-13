from __future__ import print_function
import glob
import os
import shutil
from datetime import datetime

# Environment variables
LOCAL_MOTMASTERDATA_PATH = "D:\\backup\Data2020"
local_motmasterdata_files_list = glob.glob(LOCAL_MOTMASTERDATA_PATH + '/*.zip')

for local_file in local_motmasterdata_files_list:
    basename = os.path.basename(local_file)
    dirname = basename[5:10]
    dir_file = os.path.join(LOCAL_MOTMASTERDATA_PATH, dirname, basename) 
    shutil.copy(local_file, dir_file)
