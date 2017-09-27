using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;

namespace MOTMaster2.SequenceData
{
    [Serializable,JsonObject]
    public class Sequence
    {
        public List<SequenceStep> Steps { get; set; }
        [JsonConverter(typeof(DictionaryConverter))]
        public Dictionary<string,Parameter> Parameters { get; set; }

        public Dictionary<string,object> CreateParameterDictionary()
        {
            Dictionary<string,object> paramDict = new Dictionary<string,object>();
            foreach (Parameter p in Parameters.Values)
            {
                //Converts a 64-bit int to 32-bit
                if (p.Value.GetType() == typeof(Int64)) p.Value = Convert.ToInt32(p.Value);
                if (p.Value.GetType() == typeof(string))
                {
                    int pInt = 0;
                    double pDouble = 0.0;
                    if (int.TryParse((string)p.Value, out pInt)) p.Value = pInt;
                    else if (double.TryParse((string)p.Value, out pDouble)) p.Value = pDouble;
                    else throw new Exception(string.Format("Could not recast parameter {0} to integer or double",p.Name));
                }
                paramDict[p.Name] = p.Value;
            }
            return paramDict;
        }

        public void CreateParameterList(Dictionary<string,object> paramDict)
        {
            foreach (KeyValuePair<string, object> entry in paramDict)
                Parameters[entry.Key] = new Parameter(entry.Key, "", entry.Value);
        }

        public Sequence()
        {
            Steps = new List<SequenceStep>();
            Parameters = new Dictionary<string,Parameter>();
        }
    }

    public class DictionaryConverter:JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (typeof(IDictionary).IsAssignable(objectType) || TypeImplementsGenericInterface(objectType,typeof(Dictionary<,>)));
        }
         private static bool TypeImplementsGenericInterface(Type concreteType, Type interfaceType)
        {
        return concreteType.GetInterfaces()
               .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType);
     }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        Type type = value.GetType();
        IEnumerable keys = (IEnumerable)type.GetProperty("Keys").GetValue(value, null);
        IEnumerable values = (IEnumerable)type.GetProperty("Values").GetValue(value, null);
        IEnumerator valueEnumerator = values.GetEnumerator();

        writer.WriteStartArray();
        foreach (object val in values)
        {
            valueEnumerator.MoveNext();
            writer.WriteStartObject();
            serializer.Serialize(writer, val);
            writer.WriteEndObject();
        }
        writer.WriteEndArray();
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        Type type = existingValue.GetType();
        IEnumerable values = type.GetProperty("Values").GetValue(existingValue,null);
    }
}
