using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subnetwork
{
    class Edge
    {   // ID lacza
        private int id;
        // pierwszy wierzcholek
        Node start;
        // drugi wierzcholek
        Node end;
        // dlugosc w km
        int length;
        /// <summary>
        /// Tablica zawierajaca czestotliwosci (szczeliny) w linku
        /// Jedna komórka reprezentuje jeden tunel.
        /// -1 oznacza, że dana szczelina jest wolna. 1 oznacza zajeta częstotliwość.
        /// </summary>
        public List<int> channels { get; set; }
        // liczba wszystkich szczelin na laczu
        public static int capacity = 64;
        // waga lacza
        public int weight;
        /// <summary>
        /// indeksy wolnych szczelin ktore sa juz znalezione i gotowe do zajecia przez sygnał
        /// </summary>
        public static List<int> readyChannels { get; set; }

        // szczeliny na laczu
        //public SortedSet<int> channels;
        // public List<short> channels { get; set; }

        public Edge(int id, Node begin, Node end, int length)
        {
            this.start = begin;
            this.end = end;
            this.id = id;
            this.channels = new List<int>(40);              // 40 szczelin w laczu
            for (int i = 0; i < this.channels.Count(); i++)
            {
                // 1 -> szczelina zajeta  / -1 -> szczelin wolna
                //na początku wszystkie częstotliwości wolne
                this.channels[i] = -1;
            }
            this.length = length;
            this.weight = length + this.CountBusyChannels();
            readyChannels = new List <int>();

        }

        public Edge()
        {
            this.channels = new List<int>(40);
            for (int i = 0; i < this.channels.Count(); i++)
            {
                // 1 -> szczelina zajeta  / -1 -> szczelin wolna
                //na początku wszystkie częstotliwości wolne
                this.channels[i] = -1;
            }
        }

        public int GetStart()
        {
            return start.GetId();
        }

        public int GetEnd()
        {
            return end.GetId();
        }

        public int GetWeight()
        {
            return weight;
        }
        public List<int> GetChannels()
        {
            return this.channels;
        }

        public int GetId()
        {
            return id;
        }

        public int CompareTo(Edge druga)
        {
            if (this.weight > druga.weight)
                return 1;
            else if (this.weight < druga.weight)
                return -1;
            else return 0;
        }

        /// <summary>
        /// funkcja zwracajaca liczbe wolnych szczelin na laczu
        /// </summary>
        /// <param name="channels"></param>
        /// <returns></returns>
        public int CountBusyChannels()
        {
            int result = 0;
            for (int i = 0; i < this.channels.Count(); i++)
            {
                if(this.channels[i] != -1)
                {
                    result++;
                }
            }
            return result;
        }

        /// <summary>
        /// szuka NumberChannels kolejnych wolnych szczelin wsrod szczelin na laczu
        /// </summary>
        /// <param name="NumberChannels"></param>
        /// <returns></returns>
        public bool FindFreeChannels(int NumberChannels)
        {
            //this.readyChannels.Clear();
            bool found = false;
            for (int i = 0; i < this.channels.Count(); i++)
            {

                if (this.channels[i] == -1)                 //szukam jakiejs wolnej szczeliny
                {
                    int licznik = 0;
                    for(int k = i; k < i + NumberChannels; k++)     //sprawdzam nastepne szczeliny po tej wolnej znalezionej
                    {
                        if(this.channels[k] != -1) { break; }   //jesli znalazlem zajeta szczeline to wracam do szukania pojedynczej wolnej
                        licznik++;
                        readyChannels.Add(k);              //dodaje index do indeksow gotowych szczelin
                    }
                    if (licznik == NumberChannels) { return true; } //jesli znaleziono odpowiednia liczbe wolnych szczelin obok siebie to calosc true
                }
                if (found == true) { return true; }

            }
            if(found == false) { readyChannels.Clear(); }
            return found;
        }

        /// <summary>
        /// funkcja sprawdzajaca czy szczelina o danym indeksie jest wolna
        /// </summary>
        /// <param name="slot"></param>
        /// <returns></returns>
        public bool IsThisSlotFree(int slot)
        {
            if (this.channels[slot] == -1) { return true; } else return false;
        }

        public bool AreSlotsFree(List <int> slots)
        {
            for(int i = 0; i < this.channels.Count(); i++)
            {
                foreach(int slot in slots)
                {
                    if( i == slot ) 
                    {
                        if (IsThisSlotFree(i) == false) { return false; } 
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// zajecie szczelin
        /// </summary>
        /// <param name="readyChannels"></param>
        public void useChannels(List<int> readyChannels)
        {
                foreach (int slot in this.channels)
                {
                this.channels[slot] = 1;                
                }
        }

    }
}
