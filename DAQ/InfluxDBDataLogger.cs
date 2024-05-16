using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DAQ
{
    public class InfluxDBDataLogger
    {
        private string measurement = "";
        private Dictionary<string, string> tags = new Dictionary<string, string> { };
        private Dictionary<string, object> fields = new Dictionary<string, object> { };
        private UInt64 timestamp = (uint)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        private string prec = "s";

        private InfluxDBDataLogger(string _meas)
        {
            measurement = _meas;
        }

        public static InfluxDBDataLogger Measurement(string _meas)
        {
            return new InfluxDBDataLogger(_meas);
        }
    
        public InfluxDBDataLogger Tag(string tag, string value)
        {
            tags.Add(tag, value);
            return this;
        }

        public InfluxDBDataLogger Field(string tag, object value)
        {
            fields.Add(tag, value);
            return this;
        }

        public InfluxDBDataLogger Timestamp(DateTime date)
        {
            timestamp = (UInt64)date.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            prec = "s";
            return this;
        }

        public InfluxDBDataLogger TimestampNS(DateTime date)
        {
            timestamp = (UInt64)date.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds*1000000;
            prec = "ns";
            return this;
        }

        public void Write(string url, string bucket, string org)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(measurement.Replace(" ", @"\ "));
            foreach (KeyValuePair<string, string> kv in tags)
            {
                sb.Append(",");
                sb.Append(kv.Key.Replace(" ", @"\ "));
                sb.Append('=');
                sb.Append(kv.Value.Replace(" ", @"\ "));
            }
            sb.Append(' ');
            bool first = true;
            foreach (KeyValuePair<string, object> kv in fields)
            {
                if (!first)
                {
                    sb.Append(",");
                }
                sb.Append(kv.Key.Replace(" ", @"\ "));
                sb.Append('=');
                if (kv.Value is string)
                {
                    sb.Append("\"");
                }
                sb.Append(kv.Value.ToString());
                if (kv.Value is string)
                {
                    sb.Append("\"");
                }
                first = false;
            }

            sb.Append(" ");
            sb.Append(timestamp.ToString());

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Token", System.Environment.GetEnvironmentVariable("INFLUX_TOKEN"));
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            StringContent content = new StringContent(sb.ToString());
            try
            {
                client.PostAsync(url + "/api/v2/write?org=" + org + "&bucket=" + bucket + "&precision=" + prec, content);
            }
            catch (Exception e) when (e is ArgumentNullException || e is HttpRequestException)
            {

            }
        }
    }
}
