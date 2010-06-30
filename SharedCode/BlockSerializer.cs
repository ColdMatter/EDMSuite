using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;

using ICSharpCode.SharpZipLib.Zip;

using Newtonsoft.Json;

namespace Data.EDM
{
	/// <summary>
	/// 
	/// </summary>
	public class BlockSerializer
	{

		private XmlSerializer xmls;
		
		public BlockSerializer()
		{
			xmls = new XmlSerializer(typeof(Block));
		}

		public void SerializeBlockAsZippedXML(String filePath, Block block)
		{
			Stream blockStream = new FileStream(filePath, FileMode.Create);
			SerializeBlockAsZippedXML(blockStream, block);
			blockStream.Close();
		}

		public void SerializeBlockAsZippedXML(Stream stream, Block block)
		{
			ZipOutputStream zippedStream = new ZipOutputStream(stream);
			zippedStream.SetLevel(5);
			ZipEntry entry = new ZipEntry("block.xml");
			zippedStream.PutNextEntry(entry);
			xmls.Serialize(zippedStream, block);
			zippedStream.Finish();
			zippedStream.Close();
			stream.Close();
		}


		public Block DeserializeBlockFromZippedXML(String zipFilePath, String blockFileName)
		{
			FileStream zipFileStream = new FileStream(zipFilePath, FileMode.Open);
			ZipInputStream zippedStream = new ZipInputStream(zipFileStream);
			ZipEntry zipEntry;
			Block block = null;
			while ((zipEntry = zippedStream.GetNextEntry()) != null) 
			{
				if (zipEntry.Name == blockFileName) 
				{
					block = (Block)xmls.Deserialize(zippedStream);
					break;
				}
			}
			zippedStream.Close();
            // the following is a workaround. We took a lot of data before we started recording
            // the rfState. Luckily, almost all of this data was taken in the same rf state.
            // Here, if no rfState is found in the block it is assigned the default, true.
            try
            {
                bool rfState = (bool)block.Config.Settings["rfState"];
            }
            catch (Exception)
            {
                block.Config.Settings.Add("rfState", true);
            }
			return block;
		}

        //public Block DeserializeBlockFromZippedXML2(String zipFilePath, String blockFileName)
        //{
        //    Ionic.Zip.ZipFile file = Ionic.Zip.ZipFile.Read(zipFilePath);
        //    Ionic.Zip.ZipEntry zipEntry = file.Entries[0];
        //    MemoryStream s = new MemoryStream();
        //    zipEntry.Extract(s);
        //    Block block;
        //    block = (Block)xmls.Deserialize(s);
        //    return block;
        //}

        public Block DeserializeBlockFromXML(String filePath)
        {
            FileStream fileStream = new FileStream(filePath, FileMode.Open);
            Block block = null;
            block = (Block)xmls.Deserialize(fileStream);
            fileStream.Close();
            return block;
        }

		public void SerializeBlockAsBinary(String filePath, Block block)
		{
			Stream blockStream = new FileStream(filePath, FileMode.Create);
			(new BinaryFormatter()).Serialize(blockStream, block);
			blockStream.Close();
		}

		public Block DeserializeBlockFromBinary(String filePath)
		{
			Stream blockStream = new FileStream(filePath, FileMode.Open);
			Block block = (Block)(new BinaryFormatter()).Deserialize(blockStream);
			blockStream.Close();
			return block;
		}

        public void SerializeBlockAsJSON(String filePath, Block block)
        {
            JsonSerializer serializer = new JsonSerializer();
            using (StreamWriter sw = new StreamWriter(filePath))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, block);
            }
        }
	}
}
