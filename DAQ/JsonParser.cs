using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAQ
{
    public static class JsonParser
    {

        private static readonly List<char> WHITESPACE = new List<char> { ' ', '\n', '\r', '\t', '\0' };
        private static readonly List<char> SYNTAX = new List<char> { ',', '[', ']', '{', '}', ':' };
        private static readonly List<string> KEYWORDS = new List<string> { "false", "true", "null" };

        public static string encodeJSON(Dictionary<string, object> data)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            bool first = true;
            foreach (KeyValuePair<string, object> kvpair in data)
            {
                if (!first)
                    sb.Append(',');
                if (kvpair.Value is string)
                    sb.Append(String.Format("\"{0}\":\"{1}\"", kvpair.Key, (string)kvpair.Value));
                else if (kvpair.Value is List<object>)
                    sb.Append(String.Format("\"{0}\":{1}", kvpair.Key, encodeJSONArray((List<object>)kvpair.Value)));
                else if (kvpair.Value is Dictionary<string,object>)
                    sb.Append(String.Format("\"{0}\":{1}", kvpair.Key, encodeJSON((Dictionary<string, object>)kvpair.Value)));
                else
                    sb.Append(String.Format("\"{0}\":{1}", kvpair.Key, kvpair.Value.ToString()));

                first = false;
            }
            sb.Append("}");
            return sb.ToString();
        }

        private static string encodeJSONArray(List<object> data)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[");
            bool first = true;
            foreach (object ob in data)
            {
                if (!first)
                    sb.Append(',');
                if (ob is string)
                    sb.Append(String.Format("\"{0}\"", (string)ob));
                else if (ob is List<object>)
                    sb.Append(String.Format("{0}", encodeJSONArray((List<object>)ob)));
                else if (ob is Dictionary<string, object>)
                    sb.Append(String.Format("{0}", encodeJSON((Dictionary<string, object>)ob)));
                else
                    sb.Append(String.Format("{0}", ob.ToString()));

                first = false;
            }
            sb.Append("]");
            return sb.ToString();
        }

        public static Dictionary<string, object> parseJSON(string json)
        {
            List<string> tokens = lexJSON(json);
            Dictionary<string, object> parsedJSON = new Dictionary<string, object> { };
            
            if (tokens[0]!="{")
                throw new ArgumentException("Invalid JSON to parse");

            int id = 1;

            parsedJSON = parseJSONDictionary(tokens, id).Item1;

            return parsedJSON;
        }

        private static object parseSingleToken(string token)
        {
            if (token[0] == '"')
            {
                return token.Substring(1, token.Length - 2);
            }
            else if (KEYWORDS.Contains(token))
            {
                switch (token)
                {
                    case "true":
                        return true;
                    case "false":
                        return false;
                    case "null":
                        return null;
                }
            }
            else if (!(token.Contains('.') || token.Contains('e') ||token.Contains('E')))
            {
                return Convert.ToInt32(token);
            }
            return Convert.ToDouble(token);
        }

        private static Tuple<Dictionary<string,object>,int> parseJSONDictionary(List<string> tokens, int offset)
        {
            Dictionary<string, object> parsedDict = new Dictionary<string, object> { };
            int len = 0;

            while (tokens[offset + len] != "}")
            {
                string key = tokens[offset + len++];
                if (key[0] != '"')
                    throw new ArgumentException("Invalid JSON to parse");
                if (tokens.Count <= offset + len)
                    throw new ArgumentException("Invalid JSON to parse");
                key = key.Substring(1, key.Length - 2);

                if (tokens[offset + len++] != ":")
                    throw new ArgumentException("Invalid JSON to parse");
                if (tokens.Count <= offset + len)
                    throw new ArgumentException("Invalid JSON to parse");

                string value = tokens[offset + len++];

                switch (value)
                {
                    case "{":
                        {
                            Tuple<Dictionary<string, object>, int> val = parseJSONDictionary(tokens, offset + len);
                            parsedDict.Add(key, val.Item1);
                            len += val.Item2;
                        }
                        break;

                    case "[":
                        {
                            Tuple<List<object>, int> val = parseJSONArray(tokens, offset + len);
                            parsedDict.Add(key, val.Item1);
                            len += val.Item2;
                        }
                        break;
                    default:
                        {
                            parsedDict.Add(key, parseSingleToken(value));
                        }
                        break;
                }

                if (tokens.Count <= offset + len)
                    throw new ArgumentException("Invalid JSON to parse");

                if (tokens[offset + len] == "}") break;
                if (tokens[offset + len++] != ",")
                    throw new ArgumentException("Invalid JSON to parse");

            }

            len++;
            return new Tuple<Dictionary<string, object>, int>(parsedDict, len);
        }

        private static Tuple<List<object>,int> parseJSONArray(List<string> tokens, int offset)
        {
            List<object> parsedArray = new List<object> { };
            int len = 0;

            while (tokens[offset + len] != "]")
            {
                string value = tokens[offset + len++];

                switch (value)
                {
                    case "{":
                        {
                            Tuple<Dictionary<string, object>, int> val = parseJSONDictionary(tokens, offset + len);
                            parsedArray.Add(val.Item1);
                            len += val.Item2;
                        }
                        break;

                    case "[":
                        {
                            Tuple<List<object>, int> val = parseJSONArray(tokens, offset + len);
                            parsedArray.Add(val.Item1);
                            len += val.Item2;
                        }
                        break;
                    default:
                        {
                            parsedArray.Add(parseSingleToken(value));
                        }
                        break;
                }

                if (tokens.Count <= offset + len)
                    throw new ArgumentException("Invalid JSON to parse");

                if (tokens[offset + len] == "]") break;
                if (tokens[offset + len++] != ",")
                    throw new ArgumentException("Invalid JSON to parse");

            }

            len++;

            return new Tuple<List<object>,int>(parsedArray, len);
        }

        private static List<string> lexJSON(string json)
        {
            List<string> tokens = new List<string> { };
            int offset = 0;
            while (offset < json.Length)
            {

                string res = lexString(json, offset);
                if (res != "")
                {
                    tokens.Add(res);
                    offset += res.Length;
                    continue;
                }

                res = lexKeyword(json, offset);
                if (res != "")
                {
                    tokens.Add(res);
                    offset += res.Length;
                    continue;
                }

                res = lexNumber(json, offset);
                if (res != "")
                {
                    tokens.Add(res);
                    offset += res.Length;
                    continue;
                }

                if (WHITESPACE.Contains(json[offset])) {
                    ++offset;
                    continue;
                }

                if (SYNTAX.Contains(json[offset]))
                {
                    tokens.Add(json[offset++].ToString());
                    continue;
                }

                throw new ArgumentException("Invalid JSON to parse");
            }

            return tokens;
        }

        private static string lexString(string json, int offset)
        {
            if (json[offset] != '"') return "";
            int len = 1;
            while (offset + len < json.Length)
            {
                if (json[offset + len++] == '"')
                    return json.Substring(offset, len);
            }

            throw new ArgumentException("Invalid JSON to parse");
        }

        private static string lexKeyword(string json, int offset)
        {
            foreach (string kw in KEYWORDS)
            {
                if (json.Length < offset + kw.Length) continue;
                if (json.Substring(offset, kw.Length) == kw) return json.Substring(offset, kw.Length)
                        ;
            }

            return "";
        }

        private static string lexNumber(string json, int offset)
        {
            // This is a bit more permissive than normal json, that being said misshapen numbers will cause crashes
            int len = 0;

            while (offset + len < json.Length)
            {
                if (!"+-eE0123456789.".Contains(json[offset + len]))
                    break;
                ++len;
            }
            return json.Substring(offset, len);

        }

    }
}
