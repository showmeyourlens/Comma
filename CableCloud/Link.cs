using System;
using System.Collections.Generic;
using System.Text;

namespace CableCloud
{
    public class Link
    {
        public int LinkId { get; private set; }
        public string[] ConnectedNodes { get; private set; }
        public int[] ConnectedPorts { get; private set; }
        /// <summary>
        /// dlugosc w km
        /// </summary>
        public float length;
        /// <summary>
        /// Tablica zawierajaca czestotliwosci (szczeliny) w linku
        /// Jedna komórka reprezentuje jeden tunel.
        /// -1 oznacza, że dana szczelina jest wolna. 1 oznacza zajeta częstotliwość.
        /// </summary>
        public List<int> EONchannels { get; set; }
        /// Tablica zawierajaca zajete częstotliwości.
        /// </summary>
        public List<string> BusyFreq { get; set; } ///// raczej sie juz nie przyda
        /// <summary>
        /// Domyślna liczba kanałów dostępnych w laczu. 64 szczeliny o szerokości 12.5 GHz
        /// </summary>
        public int capacity = 64;

        public Link(int linkId, string firstNode, string secondNode, int firstNodePort, int secondNodePort, int length)
        {
            this.LinkId = linkId;
            this.ConnectedNodes = new string[2];
            this.ConnectedNodes[0] = firstNode;
            this.ConnectedNodes[1] = secondNode;
            this.ConnectedPorts = new int[2];
            this.ConnectedPorts[0] = firstNodePort;
            this.ConnectedPorts[1] = secondNodePort;
            this.length = length;

            this.EONchannels = new List<int>();
            for (int i = 0; i < capacity; i++)
            {
                // 1 -> szczelina zajeta  / -1 -> szczelin wolna
                //na początku wszystkie częstotliwości wolne
                this.EONchannels.Add(-1);
            }

        }

        /// <summary>
        /// Funkcja sprawdzajaca, czy pasmo band zaczynajace się na zadanej częstotliwości frequency jest wolne na tym linku 
        /// </summary>
        /// <param name="frequency"></param>
        /// <param name="band"></param>
        public bool CheckAvailability(short frequency, short band)
        {
            bool result = true;
            try
            {
                for (short i = frequency; i < frequency + band; i++)
                {
                    if (this.EONchannels[i] != -1)
                        result = false;
                }

            }
            catch (Exception E)
            {
                Console.WriteLine(E.Message);
            }
            return result;
        }

        /// <summary>
        /// Funkcja znajdujaca wolna czestotliwosc. Jezeli nie znajduje zadnej, zwraca -1.
        /// </summary>
        /// <returns></returns>
        public short FindFreeFrequency(short band)
        {
            for (short i = 0; i <= this.capacity - band; i++)
            {
                if (CheckAvailability(i, band))
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// Zajecie danych szczelin. Funkcja zwraca true jesli operacja powiodła się
        /// </summary>
        public bool AddFreq(short frequency, short band)
        {
            try
            {
                //sprawdzenie, czy wpis nie bedzie kolidowal z juz istniejacymi
                if (CheckAvailability(frequency, band) && frequency >= 0 &&
                    frequency <= capacity && band > 0 && (frequency + band) < capacity)
                {
                    // dodanie wpisu
                    for (short i = frequency; i <= frequency + band; i++)
                    {
                        this.EONchannels[i] = 1;
                    }

                    return true;
                }
                else
                    throw new Exception("EONTable.addRow(in): failed to add a row! bandIn=" + band + " frequency=" + frequency);
            }
            catch (Exception E)
            {
                if (band != 0)
                    Console.WriteLine(E.Message);
                return false;
            }

        }


    }
}
