using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;


namespace Data
{
    [Serializable]
    [XmlRoot("Dictionary")]
    public class SerializableDictionary<T1, T2> : Dictionary<T1, T2>, IXmlSerializable
    {

        private static readonly XmlSerializer keySerializer = new XmlSerializer(typeof(T1));
        private static readonly XmlSerializer valueSerializer = new XmlSerializer(typeof(T2));

        public SerializableDictionary() : base() { }

        protected SerializableDictionary(SerializationInfo info, StreamingContext context) : base(info, context) { }

        XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            bool wasEmpty = reader.IsEmptyElement;

            reader.Read();

            if (wasEmpty)
            {
                return;
            }

            try
            {
                while (reader.NodeType != XmlNodeType.EndElement)
                {
                    reader.ReadStartElement("Item");
                    try
                    {
                        T1 tKey;
                        T2 tValue;

                        reader.ReadStartElement("Key");
                        try
                        {
                            tKey = (T1)keySerializer.Deserialize(reader);
                        }
                        finally
                        {
                            reader.ReadEndElement();
                        }

                        reader.ReadStartElement("Value");
                        try
                        {
                            tValue = (T2)valueSerializer.Deserialize(reader);
                        }
                        finally
                        {
                            reader.ReadEndElement();
                        }

                        this.Add(tKey, tValue);
                    }
                    finally
                    {
                        reader.ReadEndElement();
                    }

                    reader.MoveToContent();
                }
            }
            finally
            {
                reader.ReadEndElement();
            }
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            foreach (KeyValuePair<T1, T2> kvpair in this)
            {
                writer.WriteStartElement("Item");
                try
                {
                    writer.WriteStartElement("Key");
                    try
                    {
                        keySerializer.Serialize(writer, kvpair.Key);
                    }
                    finally
                    {
                        writer.WriteEndElement();
                    }
                    writer.WriteStartElement("Value");
                    try
                    {
                        valueSerializer.Serialize(writer, kvpair.Value);
                    }
                    finally
                    {
                        writer.WriteEndElement();
                    }
                }
                finally
                {
                    writer.WriteEndElement();
                }
            }
        }

    }
}
