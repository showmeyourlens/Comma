using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace NetworkNode
{
    public class FIBXMLReader
    {
        public RoutingInfo ReadFIB(string FileName, string routerName)
        {
            RoutingInfo result = new RoutingInfo();
            XmlDocument XmlDoc = new XmlDocument();
            try
            {
                XmlDoc.Load(FileName);
               // Console.WriteLine(FileName + " załadowany!");
                XmlNodeList Node = XmlDoc.GetElementsByTagName(routerName);
                
                int countRouterLabels = Node.Item(0).ChildNodes[0].ChildNodes.Count;
                //Console.WriteLine(count);
                for (int i = 0; i < countRouterLabels; i++)
                {
                    XmlAttributeCollection xmlRouterLabel = Node.Item(0).ChildNodes[0].ChildNodes[i].Attributes;
                    RouterLabel routerLabel = new RouterLabel(
                        Int32.Parse(xmlRouterLabel.Item(0).InnerText),
                        Int32.Parse(xmlRouterLabel.Item(1).InnerText),
                        Int32.Parse(xmlRouterLabel.Item(2).InnerText),
                        Int32.Parse(xmlRouterLabel.Item(3).InnerText)
                        );

                    result.routerLabels.Add(routerLabel);
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

