using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;


using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Core;

namespace MOTMaster
{
    /// <summary>
    /// Summary description for MOTMasterScriptSerializer
    /// </summary>
    public class MMDataZipper
    {

        private ZipOutputStream runningZipStream;
        private ZipInputStream zipInputStream;

        public void PrepareZip(Stream stream)
        {
            runningZipStream = new ZipOutputStream(stream);
            runningZipStream.SetLevel(5);
        }

        public void AppendToZip(String folder, String name)
        {
            lock (this)
            {
                string entryName = ZipEntry.CleanName(name);
                ZipEntry entry = new ZipEntry(entryName);
                runningZipStream.PutNextEntry(entry);
                byte[] buffer = new byte[4096];
                using (FileStream streamReader = File.OpenRead(folder + name))
                {
                    StreamUtils.Copy(streamReader, runningZipStream, buffer);
                }
                runningZipStream.CloseEntry();

            }
        }

        public void CloseZip()
        {
            if (runningZipStream != null)
            {
                runningZipStream.Finish();
                runningZipStream.Close();
            }
        }

        public void Unzip(string sourceFile)
        {
            zipInputStream = new ZipInputStream(File.OpenRead(sourceFile));
            ZipEntry entry;
            string tmpEntry = String.Empty;
            while ((entry = zipInputStream.GetNextEntry()) != null)
            {
                string fileName = Path.GetFileName(entry.Name);
                // create directory 
                string outputFolder = Path.GetDirectoryName(sourceFile) + "\\" +
                    Path.GetFileNameWithoutExtension(sourceFile);
                
                if (fileName != String.Empty)
                {
                    string fullPath = outputFolder + "\\" + entry.Name;
                    fullPath = fullPath.Replace("\\ ", "\\");
                    string fullDirPath = Path.GetDirectoryName(fullPath);
                    if (!Directory.Exists(fullDirPath)) Directory.CreateDirectory(fullDirPath);
                    FileStream streamWriter = File.Create(fullPath);
                    int size = 2048;
                    byte[] data = new byte[2048];
                    while (true)
                    {
                        size = zipInputStream.Read(data, 0, data.Length);
                        if (size > 0)
                        {
                            streamWriter.Write(data, 0, size);
                        }
                        else
                        {
                            break;
                        }
                    }
                    streamWriter.Close();
                }
            }
            zipInputStream.Close();
        }
    }
}

