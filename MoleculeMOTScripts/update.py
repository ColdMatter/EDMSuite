import pathlib
from datetime import date
import shutil
import calendar


LOCAL_MASTER_PATH = "D:\mot_master_data"
REMOTE_MASTER_PATH = "Y:\live\mot_master_data"


def get_path_today(master_path):
    today = date.today()
    d2 = today.strftime("%Y,%B,%d")
    year, month, day = d2.split(",")
    month_abbr = month[:3]
    month_num = str(list(calendar.month_abbr).index(month_abbr)).zfill(2)
    _path = master_path.joinpath(year, f"{month_num}{month_abbr}", day)
    pathlib.Path(_path).mkdir(parents=True, exist_ok=True)
    return _path


def copy_data_to_remote_directory():
    local_master_path = pathlib.Path(LOCAL_MASTER_PATH)
    remote_master_path = pathlib.Path(REMOTE_MASTER_PATH)
    local_dir_path = get_path_today(local_master_path)
    remote_dir_path = get_path_today(remote_master_path)
    local_file_paths = [x for x in local_dir_path.glob('**/*') if x.is_file()]
    for local_file_path in local_file_paths:
        remote_file_path = remote_dir_path.joinpath(local_file_path.name)
        shutil.copy(local_file_path, remote_file_path)


if __name__ == "__main__":
    copy_data_to_remote_directory()
