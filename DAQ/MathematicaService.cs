using System;
using System.Collections;

using Wolfram.NETLink;

using DAQ.Environment;

namespace DAQ.Mathematica
{
	/// <summary>
	/// Provides a connection to a Mathematica Kernel. Call the getKernel() method to obtain a reference
	/// to the kernel
	/// </summary>
	public class MathematicaService
	{
		public MathematicaService()
		{
		}

		
		private static IKernelLink kernelLink = null;

		private static Hashtable packages = new Hashtable();

		
		public static IKernelLink GetKernel()
		{
			if (kernelLink == null)
			{
				String args = "-linkmode launch -linkname \""
										+ Environs.FileSystem.Paths["mathPath"] + "\"";
				if ((String)Environs.Info["mlArgs"] != null) args = (String)Environs.Info["mlArgs"];
				try
				{
					kernelLink = MathLinkFactory.CreateKernelLink(args);
					kernelLink.WaitAndDiscardAnswer();
				}
				catch(MathLinkException)
				{
					Console.WriteLine("Failed to open the link to Mathematica");
				}
			}

			return kernelLink;
		}

		public static void DisposeKernel()
		{
			if (kernelLink != null) kernelLink.Close();
			packages.Clear();
		}

		// Loads the named Mathematica package. Use the full name, including the ` at the end.
		// Setting the reload flag to true causes the package to be loaded irrespective of whether it has
		// already been loaded. If this flag is false, the package will only be loaded if it hasn't been already.
		public static void LoadPackage(String name, bool reload)
		{
			if (packages.Contains(name) && !reload)
			{
				//nothing to do
			}
			else
			{
				GetKernel(); // make sure the kernel exists
				kernelLink.EvaluateToOutputForm("<<" + name, 0);

				if (!packages.Contains(name))
				{
					packages.Add(name, new Package(name));
				}
			}												 
		}

		
	}
}
