using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ToolsLibrary;
using CableCloud;

namespace ClientNode
{
    //Calling Party Call Controller
    public class CPCC
    {
        ///<summary>
        ///Zmienna przechowująca IP aplikacji klienckiej, z którą połączony jest CPCC
        ///</summary>
        public static string ClientIP;
        ///<summary>
        ///Nazwa klienta
        ///</summary>
        public static string ClientName;
        ///<summary>
        ///IP na którym łączymy się z NCC;
        ///</summary>
        public string NCCip;
        ///<summary>
        ///Port na którym łaczymy się z NCC;
        ///</summary>
        public string NCCport;
        ///<summary>
        ///Zmienna przechowująca IP na którym będzie nasłuchiwał CPCC
        ///</summary>
        public static string CPCCListenerIP;
        ///<summary>
        ///adresy IP i porty NCC'ow
        ///</summary>
        public List<string> NCCinfo;
        ///<summary>
        ///Polaczenia ustanowione przez ten CPCC
        ///</summary>
        public List<string> establishedConnections;
        ///</summary>
        ///Obiekt klasy StreamReader służacy do oczytu linii z pliku tekstowego. 
        //</summary>
        public StreamReader sr;

        //zmienne do odczytu z pliku
        public List<string> CPCCConfig;
        public string line;

        //<summary>
        ///Konstruktor CPCC
        ///</summary>
        public CPCC(string OurClientIP, string name, string listener)
        {
            ClientIP = OurClientIP;
            ClientName = name;
            CPCCListenerIP = listener;
            establishedConnections = new List<string>();
            //establishedConnections.Add("...");

        }
        
        //uruchomienie CPCC (wczytanie z pliku adresow IP i portow NCC)
        public void runCPCC()
        {
            try
            {
                CPCCConfig = new List<string>();
                NCCinfo = new List<string>();

                using (sr = new StreamReader("CPCCconfig.txt"))
                {
                    while ((line = sr.ReadLine()) != null)
                    {
                        CPCCConfig.Add(line);
                    }
                }

                for (int i = 0; i < CPCCConfig.Count(); i++)
                {
                    string tmp = CPCCConfig[i];

                    //Znaki oddzielające poszczególne części żądania klienta.
                    char[] delimiterChars = { '#' };
                    //Podzielenie żądania na tablicę stringów.
                    string[] words = tmp.Split(delimiterChars);
                    //Dodanie do listy odpowiednich wpisów - klucz/wartość
                    NCCinfo.Add(words[0]);
                    NCCinfo.Add(words[1]);
                }

                  //client 1
                if (ClientIP == "121.0.0.1")
                {
                    NCCip = NCCinfo[NCCinfo.IndexOf("121.0.0.1")];
                    NCCport = NCCinfo[NCCinfo.IndexOf("121.0.0.1") + 1];
                    CPCCListenerIP = "121.0.0.11";
                } //client 2
                else if (ClientIP == "127.0.0.4")
                {
                    NCCip = NCCinfo[NCCinfo.IndexOf("171.0.0.2")];
                    NCCport = NCCinfo[NCCinfo.IndexOf("171.0.0.2") + 1];
                    CPCCListenerIP = "121.0.0.22";
                } //client 3
                else if (ClientIP == "148.0.0.3")
                {
                    NCCip = NCCinfo[NCCinfo.IndexOf("148.0.0.3")];
                    NCCport = NCCinfo[NCCinfo.IndexOf("148.0.0.3") + 1];
                    CPCCListenerIP = "148.0.0.33";
                } //client 4
                else
                {
                    NCCip = NCCinfo[NCCinfo.IndexOf("132.0.0.4")];
                    NCCport = NCCinfo[NCCinfo.IndexOf("132.0.0.4") + 1];
                    CPCCListenerIP = "132.0.0.44";
                }

                Listen(CPCCListenerIP, Convert.ToInt32(NCCport));

            }
            catch (Exception err)
            {

            }
        }

        //nasluchiwanie CPCC
        private void Listen(string CPCCListenerIP, int portNCC)
        {



        }

        //Wyslanie wiadomosci CallRequest
        public void sendCallRequest(string senderid, string receiverid, string demandedCapacity, string NCCip)
        {
            try
            {
                string message = null;
                message = "CALL_REQUEST" + "#" + senderid + "#" + receiverid + "#" + demandedCapacity + "#" + ClientIP + '#';
                

                NetworkPackage Call_req = new NetworkPackage(message);


                TimeStamp.WriteLine("Send CALL_REQUEST message to {0}", String.Concat(this.NCCip, ":", this.NCCport));

                /// do dokonczenia

            }
            catch (Exception err)
            {

            }


        }







    }
}
