using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace TSST_EON
{
    /* Sprytna klasa żeby nie kleić stringa jak upośledzone dziecko z 1 semestru
     * Jak dodajecie jakieś pole, to musi się pojawić w klasie, we wszystkich konstruktorach, w GetObjectData, i w konstruktorze serializatora też
     * Może i dużo roboty, ale przynajmniej się nie zjebie.
     * 
     * Zróbcie konstruktor kopiujący na wszelki wypadek, zwłaszcza jak będą listy/tablice pakietów.
     * 
     * W sumie zamiast helloMessage i managementMessage powinien być enum, cała ta klasa mogłaby być trochę lepiej zrobiona...
     * ...ale już chuj.
     * 
     */

    [Serializable()]
    public class NetworkPackage : ISerializable//Pakiecik MPLSowy bardzo zdrowy
    {
        public string sendingClientId;
        public string currentIP;
        public int currentPort;
        public string receivingClientId;
        public string receivingClientAddress;
        public string message;
        public bool helloMessage;
        public bool managementMessage;
        public Stack<int> labelStack;
        public int capacity;

        private NetworkPackage()
        {
            this.labelStack = new Stack<int>();
        }

        // Wiadomość typu klient-klient
        public NetworkPackage(string sendingClientId, string sendingClientIP, int sendingClientPort, string receivingClientId, string message, int startLabel)
        {
            this.sendingClientId = sendingClientId;
            this.currentIP = sendingClientIP;
            this.currentPort = sendingClientPort;
            this.receivingClientId = receivingClientId;
            this.message = message;
            this.helloMessage = false;
            this.managementMessage = false;
            this.capacity = 0;
            this.labelStack = new Stack<int>();
            this.labelStack.Push(startLabel);
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
            this.capacity = 0;
            this.labelStack = new Stack<int>();
        }

        // Wiadomość HELLO od węzła zarządzającego
        public NetworkPackage(string sendingClientId)
        {
            this.sendingClientId = sendingClientId;
            this.currentIP = "";
            this.currentPort = 0;
            this.receivingClientId = "Cloud";
            this.message = "";
            this.helloMessage = true;
            this.managementMessage = true;
            this.capacity = 0;
            this.labelStack = new Stack<int>();
        }

        // Komunikacja z węzłem zarządzającym
        public NetworkPackage(string sendingClientId, string receivingClientId, string message)
        {
            this.sendingClientId = sendingClientId;
            this.currentIP = "";
            this.currentPort = 0;
            this.receivingClientId = receivingClientId;
            this.message = message;
            this.helloMessage = false;
            this.managementMessage = true;
            this.capacity = 0;
            this.labelStack = new Stack<int>();
        }

        //Wiadomość od Ncc do Subnetwork
        public NetworkPackage(string sendingClientId, string sendingClientIP, string receivingClientId, int capacity)
        {
            this.sendingClientId = sendingClientId;
            this.currentIP = sendingClientIP;
            this.currentPort = 0;
            this.receivingClientId = receivingClientId;
            this.message = "";
            this.helloMessage = false;
            this.managementMessage = false;
            this.labelStack = new Stack<int>();
            this.capacity = capacity;
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
            labelStack = (Stack<int>)serializationInfo.GetValue("labelStack", typeof(Stack<int>));
            capacity = (int)serializationInfo.GetValue("capacity", typeof(int));
        }

        public NetworkPackage CloneNetworkPackage()
        {
            NetworkPackage result = new NetworkPackage();

            result.sendingClientId = this.sendingClientId;
            result.currentIP = this.currentIP;
            result.currentPort = this.currentPort;
            result.receivingClientId = this.receivingClientId;
            result.message = this.message;
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
            serializationInfo.AddValue("labelStack", labelStack);
            serializationInfo.AddValue("capacity", capacity);
        }
    }
}
