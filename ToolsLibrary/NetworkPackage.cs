using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace TSST_EON
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
        public Dictionary<string, int> modu = new Dictionary<string, int>(){
            {"64QAM", 6 },  {"32QAM", 5}, {"16QAM", 4 }, {"8QAM", 3 }, {"4QAM", 2}, {"BPSK", 1 }
        };

        private NetworkPackage()
        {
        }

        // Wiadomość typu klient-klient
        public NetworkPackage(string sendingClientId, string sendingClientIP, int sendingClientPort, string receivingClientId, short frequency, short band, string message)
        {
            this.sendingClientId = sendingClientId;
            this.currentIP = sendingClientIP;
            this.currentPort = sendingClientPort;
            this.receivingClientId = receivingClientId;
            this.message = message;
            this.frequency = frequency;
            this.band = band;
            this.helloMessage = false;
            this.managementMessage = false;
        }

        // Wiadomość typu HELLO od węzła sieci
        public NetworkPackage(string sendingClientId, string sendingClientIP, int clientPort)
        {
            this.sendingClientId = sendingClientId;
            this.currentIP = sendingClientIP;
            this.message = "";
            this.receivingClientId = "Cloud";
            this.currentPort = clientPort;
            this.helloMessage = true;
            this.managementMessage = false;
        }

        public NetworkPackage(short frequency, short band, string message)
        {
            this.message = "";
            this.frequency = frequency;
            this.band = band;

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
        }
    }
}
