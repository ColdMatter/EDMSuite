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

        public void AppendToZip(String filePath)
        {
            lock (this)
            {
                string[] bits = (filePath.Split('\\'));
                string entryName = bits[bits.Length - 1];
                string cleanedEntryName = ZipEntry.CleanName(entryName);
                ZipEntry entry = new ZipEntry(cleanedEntryName);
                FileInfo f = new FileInfo(filePath);
                entry.Size = f.Length;
                runningZipStream.PutNextEntry(entry);
                byte[] buffer = new byte[16384];
                using (FileStream streamReader = File.OpenRead(filePath))
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

