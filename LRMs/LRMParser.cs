using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace LRMs
{
    public static class LRMParser
    {
        public static List<LRM> ReadLinks()
        {
            List<LRM> result = new List<LRM>();
            XmlDocument XmlDoc = new XmlDocument();
            try
            {
                XmlDoc.Load("Links.xml");
                Console.WriteLine("Links.xml załadowany!");
                int count = XmlDoc.GetElementsByTagName("Link").Count;
                for (int i = 0; i < count; i++)
                {
                    XmlAttributeCollection coll = XmlDoc.GetElementsByTagName("Link").Item(i).Attributes;
                    LRM readLink = new LRM(
                        Int32.Parse(XmlDoc.GetElementsByTagName("Link").Item(i).InnerText), // linkId
                        Int32.Parse(coll.Item(4).InnerText) //length <-----trzeba dodac dlugosci do XML !
                        );

                    result.Add(readLink);
                }
            }
            catch (XmlException exc)
            {
                Console.WriteLine(exc.Message);
            }

            return result;
        }
    }
}
