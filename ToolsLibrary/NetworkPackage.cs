using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ToolsLibrary
{
    /// <summary>
    /// Pakiecik
    /// </summary>
    [Serializable()]
    public class NetworkPackage : ISerializable
    {
        /// <summary>
        /// ID elementu wysyłającego pakiet
        /// </summary>
        public string sendingClientId;
        /// <summary>
        /// aktualny adres IP przez jaki przechodzi pakiet
        /// </summary>
        public string currentIP;
        /// <summary>
        /// aktualny mr  portu przez jaki przechodzi pakiet
        /// <summary>
        public int currentPort;
        /// ID klienta odbierającego pakiet
        /// </summary>
        public string receivingClientId;
        /// <summary>
        /// adres klienta odbierającego pakiet
        /// </summary>
        public string receivingClientAddress;

        public string receivingClientIP;

        /// <summary>
        /// zawartość wiadomosci
        /// </summary>
        public string message;
        /// <summary>
        ///czestotliwosc
        /// </summary>
        public short frequency { get; set; }
        /// <summary>
        /// pasmo zajete do przeslania pakietu
        /// </summary>
        public short band { get; set; }
        /// <summary>
        /// czy wiadomosc typu hello?
        /// </summary>
        public bool helloMessage;
        /// <summary>
        /// czy wiadomosc z systemu zarzadzajacego? //jeszcze zostawiam, ale przerobi sie w NodeCloudCommunication
        /// </summary>
        public bool managementMessage;
        /// <summary>
        /// modulacja {64QAM, 32QAM..., BPSK}
        /// </summary>
        public string modulation;
        /// <summary>
        /// slownik okreslajacy ile razy spadnie pasmo zaleznie od modulacji
        /// </summary>
        public Dictionary<string, int> modu = new Dictionary<string, int>()
        {
            {"64QAM", 6 },  {"32QAM", 5}, {"16QAM", 4 }, {"8QAM", 3 }, {"4QAM", 2}, {"BPSK", 1 }
        };
        public int capacity;

        public bool fromCPCC;

        public bool fromNcc;

        public string slots;

        public bool toSubnetwork;

        private NetworkPackage()
        {

        }


        public NetworkPackage(string message)
        {
            this.message = message;
        }

        // Wiadomość typu klient-klient
        public NetworkPackage(string sendingClientId, string sendingClientIP, int sendingClientPort, string receivingClientId, short frequency, short band, string message)
        {
            this.sendingClientId = sendingClientId;
            this.currentIP = sendingClientIP;
            this.currentPort = sendingClientPort;
            this.receivingClientId = receivingClientId;
            this.receivingClientIP = null;
            this.message = message;
            this.frequency = frequency;
            this.band = band;
            this.helloMessage = false;
            this.managementMessage = false;
            this.capacity = 0;
            this.fromCPCC = false;
            this.fromNcc = false;
            this.toSubnetwork = false;
        }

        // Wiadomość typu HELLO od węzła sieci
        public NetworkPackage(string sendingClientId, string sendingClientIP, int clientPort)
        {
            this.sendingClientId = sendingClientId;
            this.currentIP = sendingClientIP;
            this.message = "";
            this.receivingClientId = "Cloud";
            this.receivingClientIP = null;
            this.currentPort = clientPort;
            this.helloMessage = true;
            this.managementMessage = false;
            this.capacity = 0;
            this.fromCPCC = false;
            this.fromNcc = false;
            this.toSubnetwork = false;
        }

        // Wiadomość typu CallRequest_req od cpcc do ncc
        public NetworkPackage(string sendingClientId, string receivingClientId, int sendingClientPort, int capacity)
        {
            this.sendingClientId = sendingClientId;
            this.currentIP = null;
            this.message = "CallRequest_req";
            this.receivingClientId = receivingClientId;
            this.receivingClientIP = null;
            this.currentPort = sendingClientPort;
            this.helloMessage = false;
            this.managementMessage = false;
            this.capacity = capacity;
            this.fromCPCC = true;
            this.fromNcc = false;
            this.toSubnetwork = false;
        }
        //Wiadomość typu ConnectionRequest_req od pierwszego w kolejności ncc do subnetworku
        public NetworkPackage(string sendingClientIP, string receivingClientIP, int capacity, bool fromNcc)
        {
            this.sendingClientId = null;
            this.currentIP = sendingClientIP;
            this.message = "ConnectionRequest_req";
            this.receivingClientId = null;
            this.receivingClientIP = receivingClientIP;
            this.currentPort = 0;
            this.helloMessage = true;
            this.managementMessage = false;
            this.capacity = capacity;
            this.fromCPCC = false;
            this.fromNcc = fromNcc;
            this.toSubnetwork = true;
        }

        //Wiadomość typu ConnectionRequest_rsp subnetworku do pierwszego w kolejności ncc
        public NetworkPackage(string sendingClientIP, string receivingClientIP, string slots )
        {
            this.sendingClientId = null;
            this.currentIP = sendingClientIP;
            this.message = "ConnectionRequest_rsp";
            this.receivingClientId = null;
            this.receivingClientIP = receivingClientIP;
            this.currentPort = 0;
            this.helloMessage = true;
            this.managementMessage = false;
            this.capacity = 0;
            this.fromCPCC = false;
            this.fromNcc = false;
            this.slots = slots;
            this.toSubnetwork = false;
        }

        //Wiadomość typu CallCoordination_req od pierwszego w kolejności ncc do drugiego
        public NetworkPackage(string sendingClientIP, string receivingClientIP, string slots, bool fromNcc)
        {
            this.sendingClientId = null;
            this.currentIP = sendingClientIP;
            this.message = "CallCoordination_req";
            this.receivingClientId = null;
            this.receivingClientIP = receivingClientIP;
            this.currentPort = 0;
            this.helloMessage = true;
            this.managementMessage = false;
            this.capacity = 0;
            this.fromCPCC = false;
            this.fromNcc = fromNcc;
            this.slots = slots;
            this.toSubnetwork = false;
        }

        //Wiadomość typu ConnectionRequest_req od drugiego w kolejności ncc do subnetworku
        public NetworkPackage(string sendingClientIP, string receivingClientIP, string slots, bool fromNcc, bool toSubnetwork)
        {
            this.sendingClientId = null;
            this.currentIP = sendingClientIP;
            this.message = "ConnectionRequest_req";
            this.receivingClientId = null;
            this.receivingClientIP = receivingClientIP;
            this.currentPort = 0;
            this.helloMessage = true;
            this.managementMessage = false;
            this.capacity = 0;
            this.fromCPCC = false;
            this.fromNcc = fromNcc;
            this.slots = slots;
            this.toSubnetwork = toSubnetwork;
        }

        public NetworkPackage(short frequency, short band, string message)
        {
            this.message = "";
            this.frequency = frequency;
            this.band = band;
            this.helloMessage = true;
            this.managementMessage = true;
            this.capacity = 0;
            this.fromCPCC = false;
            this.fromNcc = false;
            this.receivingClientIP = null;
        }



        // Konstruktor do deserializatora
        public NetworkPackage(SerializationInfo serializationInfo, StreamingContext context)
        {
            sendingClientId = (string)serializationInfo.GetValue("sendingClientId", typeof(string));
            currentIP = (string)serializationInfo.GetValue("currentIP", typeof(string));
            currentPort = (int)serializationInfo.GetValue("currentPort", typeof(int));
            receivingClientId = (string)serializationInfo.GetValue("receivingClientId", typeof(string));
            message = (string)serializationInfo.GetValue("message", typeof(string));
            helloMessage = (bool)serializationInfo.GetValue("helloMessage", typeof(bool));
            managementMessage = (bool)serializationInfo.GetValue("managementMessage", typeof(bool));
            capacity = (int)serializationInfo.GetValue("capacity", typeof(int));
            fromCPCC = (bool)serializationInfo.GetValue("fromCPCC", typeof(bool));
            fromNcc = (bool)serializationInfo.GetValue("fromNcc",typeof(bool));
            receivingClientIP = (string)serializationInfo.GetValue("recivingClientIP",typeof(string));
            slots = (string)serializationInfo.GetValue("slots", typeof(string));
            toSubnetwork = (bool)serializationInfo.GetValue("toSubnetwork", typeof(bool));
        }


        public NetworkPackage CloneNetworkPackage()
        {
            NetworkPackage result = new NetworkPackage();

            result.sendingClientId = this.sendingClientId;
            result.currentIP = this.currentIP;
            result.currentPort = this.currentPort;
            result.receivingClientId = this.receivingClientId;
            result.message = this.message;
            result.frequency = this.frequency;
            result.band = this.band;
            result.helloMessage = false;
            result.managementMessage = false;
            result.capacity = 0;

            return result;
        }

        public void GetObjectData(SerializationInfo serializationInfo, StreamingContext context)
        {
            if (serializationInfo is null)
            {
                throw new ArgumentNullException(nameof(serializationInfo));
            }

            serializationInfo.AddValue("sendingClientId", sendingClientId);
            serializationInfo.AddValue("currentIP", currentIP);
            serializationInfo.AddValue("currentPort", currentPort);
            serializationInfo.AddValue("receivingClientId", receivingClientId);
            serializationInfo.AddValue("message", message);
            serializationInfo.AddValue("helloMessage", helloMessage);
            serializationInfo.AddValue("managementMessage", managementMessage);
            serializationInfo.AddValue("capacity", capacity);
            serializationInfo.AddValue("fromCPCC", fromCPCC);
            serializationInfo.AddValue("fromNcc", fromNcc);
            serializationInfo.AddValue("recivingClientIP",receivingClientIP);
            serializationInfo.AddValue("slots",slots);
            serializationInfo.AddValue("toSubnetwork", toSubnetwork);
        }
    }
}
