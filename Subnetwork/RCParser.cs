using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Subnetwork
{
    public class RCParser
    {
        public static void ParseConfig(string rcName, List<RCContact> contacts, out Graph graph)
        {
            List<RCContact> result = new List<RCContact>();
            XmlDocument XmlDoc = new XmlDocument();
            List<Edge> edges = new List<Edge>();
            try
            {
                XmlDoc.Load("RCinfo.xml");
                Console.WriteLine("RCinfo.xml załadowany!");
                var allRCConfig = XmlDoc.GetElementsByTagName("RC");
                int count = XmlDoc.GetElementsByTagName("RC").Count;
                int index = 0;
                for(int i=0; i<count; i++)
                {
                    if (String.Equals(allRCConfig.Item(i).Attributes.Item(0).InnerText, rcName))
                    {
                        index = i;
                    }
                }
                var thisRcConfig = allRCConfig.Item(index);
                var rcContacts = thisRcConfig.ChildNodes.Item(0).ChildNodes;
                var rcLinks = thisRcConfig.ChildNodes.Item(1).ChildNodes;

                for (int i = 0; i < rcContacts.Count; i++)
                {
                    XmlAttributeCollection col1 = rcContacts.Item(i).Attributes;
                    contacts.Add(new RCContact(
                        col1.Item(0).InnerText,
                        col1.Item(1).InnerText
                        ));
                }

                for (int i=0; i<rcLinks.Count; i++)
                {
                    XmlAttributeCollection col2 = rcLinks.Item(i).Attributes;
                    Edge e = new Edge(
                        Int32.Parse(rcLinks.Item(i).InnerText),
                        col2.Item(0).InnerText,
                        col2.Item(1).InnerText,
                        Int32.Parse(col2.Item(2).InnerText)
                        );
                    if(Boolean.Parse(col2.Item(3).InnerText))
                    {
                        e.isDirect = true;
                        e.startPort = Int32.Parse(col2.Item(4).InnerText);
                        e.endPort = Int32.Parse(col2.Item(5).InnerText);
                    }
                    edges.Add(e);
                }


            }
            catch (XmlException exc)
            {
                Console.WriteLine(exc.Message);
            }
            graph = new Graph(edges);
        }
    }
}
