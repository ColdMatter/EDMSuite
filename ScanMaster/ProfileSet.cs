using System;
using System.Collections;
using System.Xml.Serialization;

namespace ScanMaster
{
	/// <summary>
	/// The class provides a convenient way to XmlSerialize and deserialize the set of profiles.
	/// That is it's only function.
	/// </summary>
	[Serializable]
	public class ProfileSet
	{
		public ProfileSet()
		{
		}
	
		private ArrayList profiles;

		[XmlArray]
		[XmlArrayItem(Type = typeof(Profile))]
		public ArrayList Profiles
		{
			get { return profiles; }
			set { profiles = value; }
		}
	}
	
}
