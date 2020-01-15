using System;
using System.Collections.Generic;
using System.Text;
using System.Net;


namespace Subnetwork
{
    /// <summary>
    /// Mozliwe stany SubnetworkPointa: 
    /// AVAILABLE - istniejący w rzeczywistości;
    /// POTENTIAL - nieistniejący, ale może istnieć i wymaga stworzenia przez LRM
    /// </summary>
    public enum STATE { AVAILABLE = 1, POTENTIAL = 2 }

    /// <summary>
    /// Klasa reprezentujaca Subnetwork Point
    /// </summary>
    public class SubnetworkPoint
    {
        //public EONTable eonTable = new EONTable();
        public STATE state;
        public IPAddress ipaddress;
        /// <summary>
        /// ID Łącza wchodzącego do SNP. -1 oznacza, że nie ma łącza na wejściu.
        /// </summary>
        public int portIN;

        /// <summary>
        /// ID Łącza wychodzącego z SNP. -1 oznacza, że nie ma łącza na wyjściu.
        /// </summary>
        public int portOUT;

        public SubnetworkPoint()
        {
            //this.ipaddress = IPAddress.Parse("");
            this.state = STATE.AVAILABLE;
            this.portIN = -1;
            this.portOUT = -1;
            //this.eonTable = new EONTable();
        }

        public SubnetworkPoint(IPAddress ipaddress) : this()
        {
            this.ipaddress = ipaddress;
        }

        public SubnetworkPoint(IPAddress ipaddress, STATE state)
        {
            this.ipaddress = ipaddress;
            this.state = state;
        }

        public SubnetworkPoint(IPAddress ipaddress, STATE state, int port)
        {
            this.ipaddress = ipaddress;
            this.portIN = port;
            this.state = state;
        }

        public SubnetworkPoint(IPAddress ipaddress, int portIN, int portOUT, STATE state)
        {
            this.ipaddress = ipaddress;
            this.portIN = portIN;
            this.portOUT = portOUT;
            this.state = state;
        }

        public SubnetworkPoint(IPAddress ipaddress, int portIN, int portOUT)
        {
            this.ipaddress = ipaddress;
            this.portIN = portIN;
            this.portOUT = portOUT;
        }
    }
}
