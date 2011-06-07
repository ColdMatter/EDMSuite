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
	public class MOTMasterScriptSerializer
	{

        private ZipOutputStream runningZipStream;

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

	}
}

