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
        /// czy wiadomosc typu hello?
        /// </summary>
        public bool helloMessage;
        /// <summary>
        /// czy wiadomosc z systemu zarzadzajacego? //jeszcze zostawiam, ale przerobi sie w NodeCloudCommunication
        /// </summary>
        public bool managementMessage;

        public double lambda;

        public Command MMsgType;

        private NetworkPackage()
        {

        }

        // Wiadomość typu klient-klient
        public NetworkPackage(string sendingClientId, string sendingClientIP, int sendingClientPort, string receivingClientId, double lambda, string message)
        {
            this.sendingClientId = sendingClientId;
            this.currentIP = sendingClientIP;
            this.currentPort = sendingClientPort;
            this.receivingClientId = receivingClientId;
            this.receivingClientIP = null;
            this.message = message;
            this.helloMessage = false;
            this.lambda = lambda;
            this.managementMessage = false;
            this.MMsgType = Command.Client_To_Client;
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
            this.lambda = 0.0;
            this.managementMessage = false;
            this.MMsgType = Command.HELLO;
        }

        public NetworkPackage(string sendingClientId)
        {
            this.sendingClientId = sendingClientId;
            this.currentIP = "";
            this.message = "";
            this.receivingClientId = "Cloud";
            this.receivingClientIP = null;
            this.currentPort = 0;
            this.helloMessage = true;
            this.lambda = 0.0;
            this.managementMessage = true;
            this.MMsgType = Command.HELLO;
        }

        public NetworkPackage(string sendingClientId, string receivingClientId, Command commandType, string message)
        {
            this.sendingClientId = sendingClientId;
            this.currentIP = "";
            this.message = message;
            this.receivingClientId = receivingClientId;
            this.receivingClientIP = null;
            this.currentPort = 0;
            this.lambda = 0.0;
            this.helloMessage = false;
            this.managementMessage = true;
            this.MMsgType = commandType;
        }

        public NetworkPackage(string sendingClientId, string receivingClientId, Command commandType)
        {
            this.sendingClientId = sendingClientId;
            this.currentIP = "";
            this.message = "";
            this.receivingClientId = receivingClientId;
            this.receivingClientIP = null;
            this.currentPort = 0;
            this.lambda = 0.0;
            this.helloMessage = false;
            this.managementMessage = true;
            this.MMsgType = commandType;
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
            receivingClientIP = (string)serializationInfo.GetValue("recivingClientIP",typeof(string));
            MMsgType = (Command)serializationInfo.GetValue("MMsgType", typeof(Command));
            lambda = (double)serializationInfo.GetValue("lambda", typeof(double));
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
            serializationInfo.AddValue("recivingClientIP",receivingClientIP);
            serializationInfo.AddValue("MMsgType", MMsgType);
            serializationInfo.AddValue("lambda", lambda);
        }
    }
}
