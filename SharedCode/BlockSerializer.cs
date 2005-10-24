using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;

using ICSharpCode.SharpZipLib.Zip;

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
	}
}
