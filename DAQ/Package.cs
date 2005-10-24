using System;

namespace DAQ.Mathematica
{
	/// <summary>
	/// Represents a Mathematica package. At the moment, the package only has a name.
	/// There may be other information associated with a package that could go in here.
	/// </summary>
	public class Package
	{
		public Package(String name)
		{
			this.FullName = name;
		}

		private String fullName;

		public String FullName
		{
			get { return fullName; }
			set { fullName = value; }
		}


	}
}
