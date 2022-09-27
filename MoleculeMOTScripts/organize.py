import pathlib
import calendar
import shutil
from tqdm import tqdm

LOCAL_MASTER_PATH = "D:\mot_master_data"
REMOTE_MASTER_PATH = "D:\mot_master_data_organized"


local_master_path = pathlib.Path(LOCAL_MASTER_PATH)
remote_master_path = pathlib.Path(REMOTE_MASTER_PATH)
local_files = [x for x in local_master_path.glob('**/*') if x.is_file() and x.name[-3:] == "zip"]

for file in tqdm(local_files):
    file_name = file.name
    day = file_name[3:5]
    month_abbr = file_name[5:8]
    month_num = str(list(calendar.month_abbr).index(month_abbr)).zfill(2)
    year_abbr = file_name[8:10]
    remote_dir_path = remote_master_path.joinpath(f"20{year_abbr}", f"{month_num}{month_abbr}", day)
    remote_file = remote_dir_path.joinpath(file.name)
    pathlib.Path(remote_dir_path).mkdir(parents=True, exist_ok=True)
    shutil.copy(file, remote_file)
