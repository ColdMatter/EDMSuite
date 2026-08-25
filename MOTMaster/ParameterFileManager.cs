using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MOTMaster
{
    public static class ParameterFileManager
    {
        // Expected file format per line: Name  Value   Type
        private const char Delimiter = '\t';

        public static List<ParameterEntry> ReadFile(string filePath, Type fallbackType)
        {
            var entries = new List<ParameterEntry>();

            if (!File.Exists(filePath))
                return entries;

            foreach (var line in File.ReadAllLines(filePath))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                var parts = line.Split(Delimiter);
                if (parts.Length < 3) continue;

                Type resolvedType = Type.GetType(parts[2].Trim()) ?? fallbackType;

                entries.Add(new ParameterEntry(
                    parts[0].Trim(),
                    parts[1].Trim(),
                    resolvedType
                ));
            }

            return entries;
        }

        public static void WriteFile(string filePath, List<ParameterEntry> entries)
        {
            var lines = new List<string>();

            foreach (var entry in entries)
                lines.Add($"{entry.Name}{Delimiter}{entry.Value}{Delimiter}{entry.Type.FullName}");

            File.WriteAllLines(filePath, lines);
        }
    }
}
