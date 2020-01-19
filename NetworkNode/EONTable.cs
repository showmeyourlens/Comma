using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace TSST_EON
{
    /// <summary>
    /// Tabela z zajętymi i wolnymi częstotliwościami.
    /// </summary>
    class EONTable
    {
        /// <summary>
        /// Tablica zawierajaca poczatkowe czestotliwosci zajete przy odbieraniu przez router. 
        /// Jeden rząd reprezentuje jeden tunel.
        /// -1 oznacza, że dana szczelina jest wolna. 1 oznacza wolną częstotliwość.
        /// </summary>
        public List<EONTableIN> TableIN { get; set; }

        /// <summary>
        /// Tablica zawierajaca poczatkowe czestotliwosci zajete przy wysyłaniu przez router. 
        /// </summary>
        public List<EONTableOUT> TableOUT { get; set; }
        /// <summary>
        /// Tablica zawierajaca wolne częstotliwości.
        /// </summary>
        public List<short> FreeFreqIN { get; set; }
        /// <summary>
        /// Tablica zawierajaca wolne częstotliwości.
        /// </summary>
        public List<short> FreeFreqOUT { get; set; }


        /// <summary>
        /// Domyślna liczba kanałów dostępnych w routerze. 64 szczeliny o szerokości 12.5 GHz
        /// </summary>
        public static int capacity = 64;

        /// <summary>
        /// Utworzenie listy częstotliwości obsługiwanych w routerze.
        /// 1 -> szczelina zajeta  / -1 -> szczelin wolna
        /// </summary>
        public EONTable()
        {
            FreeFreqIN = new List<short>(capacity);
            FreeFreqOUT = new List<short>(capacity);

            for (int i = 0; i < capacity; i++)
            {
                //na początku wszystkie częstotliwości wolne
                FreeFreqIN.Add(-1);
                FreeFreqOUT.Add(-1);
            }

            TableIN = new List<EONTableIN>();
            TableOUT = new List<EONTableOUT>();
        }

        /// <summary>
        /// Funkcja sprawdzajaca, czy pasmo na zadanej częstotliwości jest wolne w tym routerze. 
        /// in_or_out ma wartości "in" lub "out"
        /// </summary>
        /// <param name="frequency"></param>
        /// <param name="band"></param>
        /// <param name="in_or_out"></pacam>
        public bool CheckAvailability(short frequency, short band, string in_or_out)
        {
            bool result = true;
            try
            {
                if (in_or_out == "in")
                {
                    for (short i = frequency; i < frequency + band; i++)
                        if (FreeFreqIN[i] != -1)
                            result = false;
                }
                else if (in_or_out == "out")
                {
                    for (short i = frequency; i < frequency + band; i++)
                        if (FreeFreqOUT[i] != -1)
                            result = false;
                }
                else
                {
                    result = false;
                    throw new Exception("EONTable.ChackAvailability: bad input argument. in_or_out is " + in_or_out +
                                        ", but should be either \"in\" or \"out\".");
                }
            }
            catch (Exception E)
            {
                Console.WriteLine(E.Message);
            }
            return result;
        }

        /// <summary>
        /// Funkcja znajdujaca wolna czestotliwosc. Jezeli nie znajduje zadnej, zwraca -1
        /// </summary>
        /// <returns></returns>
        public short FindFreeFrequency(short band, string in_or_out)
        {
            for (short i = 0; i <= EONTable.capacity - band; i++)
            {
                if (CheckAvailability(i, band, in_or_out))
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// Dodanie rzędu do tablicy EON, wraz z aktualizacją tablic zajętości z częstotliwościami IN.
        /// </summary>
        public bool addRow(EONTableIN row)
        {
            try
            {
                //sprawdzenie, czy wpis nie bedzie kolidowal z juz istniejacymi
                if (CheckAvailability(row.busyFreqIN, row.busyBandIN, "in") && row.busyFreqIN >= 0 &&
                    row.busyFreqIN <= EONTable.capacity && row.busyBandIN > 0)
                {
                    //dodanie do tabeli
                    this.TableIN.Add(row);

                    //Ustawienie wartosci zajetych pol w tabeli
                    for (int i = row.busyFreqIN; i < row.busyFreqIN + row.busyBandIN; i++)
                        this.FreeFreqIN[i] = row.busyFreqIN;

                    return true;
                }
                else
                    throw new Exception("EONTable.addRow(in): failed to add a row! bandIn=" + row.busyBandIN + " frequency=" + row.busyFreqIN);
            }
            catch (Exception E)
            {
                if (row.busyBandIN != 0)
                    Console.WriteLine(E.Message);
                return false;
            }

        }

        /// <summary>
        /// Dodanie rzędu do tablicy EON, wraz z aktualizacją tablic zajętości z częstotliwościami OUT.
        /// </summary>
        /// <param name="row"></param>
        public bool addRow(EONTableOUT row)
        {
            try
            {
                //sprawdzenie, czy wpis nie bedzie kolidowal z juz istniejacymi
                if (CheckAvailability(row.busyFreqOUT, row.busyBandOUT, "out") && row.busyFreqOUT >= 0 &&
                    row.busyFreqOUT <= EONTable.capacity && row.busyBandOUT > 0)
                {
                    //dodanie do tabeli
                    this.TableOUT.Add(row);

                    //Ustawienie wartosci zajetych pol w tabeli
                    for (int i = row.busyFreqOUT; i < row.busyFreqOUT + row.busyBandOUT; i++)
                        this.FreeFreqOUT[i] = row.busyFreqOUT;

                    return true;
                }
                else
                    throw new Exception("EONTable.addRow(out): failed to add a row! bandOut=" + row.busyBandOUT + " frequency=" + row.busyFreqOUT);
            }
            catch (Exception E)
            {
                if (row.busyBandOUT != 0)
                    Console.WriteLine(E.Message);
                return false;
            }
        }


    }
}
