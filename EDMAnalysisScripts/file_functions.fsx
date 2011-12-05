module file_functions

#load "constants.fsx"

let GetDirectoryForCluster(cName: string) : string = 
    let monthMap = dict ["Jan","January";
    "Feb","February";
    "Mar","March";
    "Apr","April";
    "May","May";
    "Jun","June";
    "Jul","July";
    "Aug","August";
    "Sep","September";
    "Oct","October";
    "Nov","November";
    "Dec","December"]

    let cMonth = cName.Substring(2,3)
    let cYear = "20" + cName.Substring(5,2)

    let filePath = constants.dataRoot + @"\sedm\v3\" + cYear + @"\" + monthMap.Item(cMonth) + cYear
    filePath

let GetFilesForCluster(cName: string) =
    let filePath = GetDirectoryForCluster(cName)
    Seq.toList(System.IO.Directory.GetFiles(filePath, cName + "*.zip"))

