using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace DeMasterProCloud.Common.Infrastructure
{
    public static class XMLHelper
    {
        public static string SerializeObjectToXml<T>(T obj, string rootElement = "Message")
        {
            try
            {
                var root = new XmlRootAttribute(rootElement);
                var serializer = new XmlSerializer(typeof(T), root);
                using (var stringWriter = new StringWriter())
                {
                    using (var xmlWriter = new XmlTextWriter((stringWriter)))
                    {
                        serializer.Serialize(xmlWriter, obj);
                    }

                    return stringWriter.ToString();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public static T DeserializeXmlToObject<T>(string xml, string rootElement = "Message")
        {
            try
            {
                var root = new XmlRootAttribute(rootElement);
                var serializer = new XmlSerializer(typeof(T), root);

                using (var reader = new StringReader(xml))
                {
                    var xmlReader = new XmlTextReader(reader) { DtdProcessing = DtdProcessing.Ignore };

                    return (T)serializer.Deserialize(xmlReader);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}