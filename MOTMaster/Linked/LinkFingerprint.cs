using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace MOTMaster
{
    /// <summary>Canonical hashes used to prove both machines compiled the same script with the same parameters.</summary>
    public static class LinkFingerprint
    {
        public static string OfText(string text)
        {
            return Sha256Hex(text ?? "");
        }

        public static string OfParameters(IDictionary<string, object> p)
        {
            var lines = new List<string>();
            if (p != null)
            {
                foreach (string key in p.Keys.OrderBy(k => k, StringComparer.Ordinal))
                {
                    object value = p[key];
                    string typeName = value == null ? "null" : value.GetType().FullName;
                    string text;
                    if (value == null) text = "null";
                    else if (value is double) text = ((double)value).ToString("R", CultureInfo.InvariantCulture);
                    else if (value is float) text = ((float)value).ToString("R", CultureInfo.InvariantCulture);
                    else text = Convert.ToString(value, CultureInfo.InvariantCulture);
                    lines.Add(key + "\t" + typeName + "\t" + text);
                }
            }
            return Sha256Hex(string.Join("\n", lines));
        }

        static string Sha256Hex(string s)
        {
            using (var sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(s));
                var sb = new StringBuilder(hash.Length * 2);
                foreach (byte b in hash) sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }
    }
}
