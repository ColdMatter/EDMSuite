using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using ScanMaster.Acquire;
using ScanMaster.GUI;

namespace ScanMaster
{
	/// <summary>
	/// A profile is a complete description of how the program should take data.
	/// It has a name, an acquisitor configuration and a GUI configuration.
	/// It can belong to a group for bulk editing.
	/// </summary>
	[Serializable]
	public class Profile : ICloneable
	{
		public String Name = "New profile";
		public String Group;
		public AcquisitorConfiguration AcquisitorConfig = new AcquisitorConfiguration();
		public GUIConfiguration GUIConfig = new GUIConfiguration();

		public override String ToString()
		{
			return "Profile: " + Name + Environment.NewLine + "Group: " + Group +
				Environment.NewLine + AcquisitorConfig + Environment.NewLine;
		}

		public object Clone()
		{
			// Slightly odd way to clone an object: serialize it and deserialize straight away.
			MemoryStream m = new MemoryStream();
			BinaryFormatter bf = new BinaryFormatter();
			bf.Serialize(m,this);
			m.Position = 0;
			Profile newProfile = (Profile)bf.Deserialize(m);
			newProfile.Name += " copy";
			return newProfile;
		}
	}
}
