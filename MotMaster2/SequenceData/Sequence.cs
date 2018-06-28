using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Collections.ObjectModel;
using dotMath;

namespace MOTMaster2.SequenceData
{
    [Serializable,JsonObject]
    public class Sequence
    {
        public ObservableCollection<SequenceStep> Steps { get; set; }
        [JsonConverter(typeof(DictionaryConverter))]
        public ObservableDictionary<string,Parameter> Parameters { get; set; }

        // for all names Controller.sequenceData.Parameters.Keys
        public List<string> ScannableParams(bool scannables = true) // scanables = false is for all non-scanables (derivative) params
        {
            List<string> ls = new List<string>();
            if (Parameters == null) return ls;
            foreach (KeyValuePair<string, Parameter> entry in Parameters)
                if (scannables)
                {
                    if (entry.Value.IsScannable()) ls.Add(entry.Key);
                }
                else
                {
                    if (!entry.Value.IsScannable()) ls.Add(entry.Key);
                }
            ls.Sort();
            return ls;
        }
        public List<string> DependableParams(string param) // the list of non-scan parameters dependable on "param" 
        {
            List<string> ls = new List<string>();
            if (!Parameters.Keys.Contains(param)) return ls;
            if (!Parameters[param].IsScannable()) return ls;
            List<string> lt = ScannableParams(false);
            foreach(string prm in lt)
            {
                string func = Parameters[prm].Description.TrimStart('=');
                EqCompiler compiler = new EqCompiler(func, true);
                compiler.Compile();
                List<string> vl = compiler.GetVariableList().ToList();
                if (vl.IndexOf(param) > -1) ls.Add(prm);
            }
            return ls;
        }
     
        public List<MMscan> ScanningParams { get; set; }
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
            Steps = new ObservableCollection<SequenceStep>();
            Parameters = new ObservableDictionary<string,Parameter>();
        }

    }

    
    public class DictionaryConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (typeof(IDictionary).IsAssignableFrom(objectType) || TypeImplementsGenericInterface(objectType, typeof(Dictionary<,>)));
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
              //  writer.WriteStartObject();
                serializer.Serialize(writer, val);
              //  writer.WriteEndObject();
            }
            writer.WriteEndArray();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            Type[] types = existingValue.GetType().GetGenericArguments();
            Type keyType = types[0];
            Type valType = types[1];
            IDictionary dict = (IDictionary)existingValue;
            object key;
            object value;
            reader.Read();
            while (reader.TokenType != JsonToken.EndArray)
            {
                value = serializer.Deserialize(reader,valType);
                key = valType.GetProperty("Name").GetValue(value);
                dict.Add(key, value);
                reader.Read();
            }
            return dict;
        }
    }
}
