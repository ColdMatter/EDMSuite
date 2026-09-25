// Thin console wrapper around ScriptCompileCheck.cs (same folder, compiled in alongside
// this one by verify-scripts.ps1 rather than duplicated here), for checking every
// top-level pattern script with nothing but csc.exe - no .NET SDK or VS Code needed.
//
// Usage: ScriptSmokeTest.exe <scriptFolder> <refAssembly1> [refAssembly2 ...]
//   scriptFolder  - directory of top-level *.cs scripts to check (subfolders are not scanned,
//                   matching Controller.ScriptLookupAndDisplay's own behaviour)
//   refAssembly*  - assemblies to compile each script against (MOTMaster.exe, DAQ.dll,
//                   and any AdditionalMOTMasterAssemblies such as SpectrumDDS.dll),
//                   in the same order Controller.compileFromFile would reference them

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MOTMasterScriptCheck;

internal static class ScriptSmokeTest
{
    private static int Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.Error.WriteLine("Usage: ScriptSmokeTest.exe <scriptFolder> <refAssembly1> [refAssembly2 ...]");
            return 2;
        }

        string scriptFolder = args[0];
        string[] referencedAssemblies = args.Skip(1).ToArray();

        var scripts = Directory.GetFiles(scriptFolder, "*.cs", SearchOption.TopDirectoryOnly)
                                .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                                .ToArray();

        if (scripts.Length == 0)
        {
            Console.Error.WriteLine("No top-level .cs files found in " + scriptFolder);
            return 2;
        }

        int failCount = 0;
        foreach (string scriptPath in scripts)
        {
            string name = Path.GetFileName(scriptPath);
            string failure = ScriptCompileCheck.Check(scriptPath, referencedAssemblies);
            if (failure == null)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("OK    " + name);
                Console.ResetColor();
            }
            else
            {
                failCount++;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("FAIL  " + name);
                Console.ResetColor();
                Console.WriteLine("      " + failure.Replace("\n", "\n      "));
            }
        }

        Console.WriteLine();
        if (failCount == 0)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(string.Format("All {0} scripts compile and build a runnable pattern.", scripts.Length));
            Console.ResetColor();
            return 0;
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(string.Format("{0} of {1} scripts failed.", failCount, scripts.Length));
            Console.ResetColor();
            return 1;
        }
    }
}
