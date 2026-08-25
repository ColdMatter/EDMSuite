// Thin console wrapper around MOTMaster.ScriptTests/ScriptCompileCheck.cs (that file is
// compiled in alongside this one - see verify-scripts.ps1 - rather than duplicated here),
// for checking scripts without a full dotnet/VS Code setup. If you have the .NET SDK and
// VS Code's C# Dev Kit, prefer the MOTMaster.ScriptTests project itself: it gives one
// test per script in VS Code's Test Explorer (`dotnet test` from the repo root also
// works headless). This script is the fallback for CI or an agent session that only has
// csc.exe.
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
using MOTMaster.ScriptTests;

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
