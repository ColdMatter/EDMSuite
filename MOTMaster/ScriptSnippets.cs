using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace MOTMaster
{
    /// <summary>
    /// Finds the snippet source files a pattern script uses, so that they can be compiled
    /// together with it.
    /// </summary>
    /// <remarks>
    /// A snippet folder holds one class per file, each file named after its class. A script
    /// uses a snippet if the snippet's class name appears in its code outside comments and
    /// string literals. Snippets that use other snippets are followed the same way. Only
    /// the files found are compiled, so a broken snippet only affects the scripts using it.
    ///
    /// Only used when the machine's FileSystem sets Paths["MOTMasterSnippetPath"]; other
    /// experiments compile scripts exactly as before.
    /// </remarks>
    public static class ScriptSnippets
    {
        public const string PathKey = "MOTMasterSnippetPath";

        private static readonly Regex Identifier = new Regex(@"[A-Za-z_][A-Za-z0-9_]*");

        /// <summary>
        /// The snippet files used by the script, directly or through other snippets, in the
        /// order they were found. Empty if the snippet folder does not exist.
        /// </summary>
        public static List<string> FilesUsedBy(string scriptPath, string snippetFolder)
        {
            List<string> used = new List<string>();
            if (!Directory.Exists(snippetFolder))
                return used;

            Dictionary<string, string> available = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (string file in Directory.GetFiles(snippetFolder, "*.cs", SearchOption.TopDirectoryOnly))
                available[Path.GetFileNameWithoutExtension(file)] = file;

            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);
            Queue<string> toRead = new Queue<string>();
            toRead.Enqueue(scriptPath);
            while (toRead.Count > 0)
            {
                string code = StripCommentsAndStrings(File.ReadAllText(toRead.Dequeue()));
                foreach (Match m in Identifier.Matches(code))
                {
                    string file;
                    if (available.TryGetValue(m.Value, out file) && seen.Add(m.Value))
                    {
                        used.Add(file);
                        toRead.Enqueue(file);
                    }
                }
            }
            return used;
        }

        /// <summary>
        /// The code with comments and string and character literals replaced by spaces, so
        /// that a snippet name mentioned in a comment or a channel name does not count.
        /// </summary>
        public static string StripCommentsAndStrings(string code)
        {
            StringBuilder sb = new StringBuilder(code.Length);
            int i = 0;
            while (i < code.Length)
            {
                char c = code[i];
                char next = i + 1 < code.Length ? code[i + 1] : '\0';
                if (c == '/' && next == '/')
                {
                    while (i < code.Length && code[i] != '\n') i++;
                }
                else if (c == '/' && next == '*')
                {
                    int end = code.IndexOf("*/", i + 2, StringComparison.Ordinal);
                    i = end < 0 ? code.Length : end + 2;
                }
                else if (c == '@' && next == '"')
                {
                    // Verbatim string: "" is an escaped quote, backslashes are literal.
                    i += 2;
                    while (i < code.Length)
                    {
                        if (code[i] == '"' && i + 1 < code.Length && code[i + 1] == '"') i += 2;
                        else if (code[i] == '"') { i++; break; }
                        else i++;
                    }
                }
                else if (c == '"' || c == '\'')
                {
                    i++;
                    while (i < code.Length && code[i] != c && code[i] != '\n')
                        i += code[i] == '\\' ? 2 : 1;
                    i++;
                }
                else
                {
                    sb.Append(c);
                    i++;
                    continue;
                }
                sb.Append(' ');
            }
            return sb.ToString();
        }
    }
}
