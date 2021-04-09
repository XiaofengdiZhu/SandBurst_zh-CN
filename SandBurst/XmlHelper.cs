using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SandBurst
{
    class XmlHelper
    {
        public static void Serialize<T>(string savePath, T graph)
        {
            using (var sw = new StreamWriter(savePath, false, Encoding.UTF8))
            {
                var ns = new XmlSerializerNamespaces();
                ns.Add(string.Empty, string.Empty);

                new XmlSerializer(typeof(T)).Serialize(sw, graph, ns);
            }
        }

        public static T Deserialize<T>(string loadPath)
        {
            using (var sr = new StreamReader(loadPath))
            {
                return (T)new XmlSerializer(typeof(T)).Deserialize(sr);
            }
        }
    }
}
