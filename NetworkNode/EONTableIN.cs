using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkNode
{
    /// <summary>
    /// Klasa z rzedem tabeli zajetych pasm na wejsciu routera.
    /// </summary>
    public class EONTableIN
    {
        /// <summary>
        /// Czestotliwosc na jakiej zaczyna sie zajete pasmo.
        /// </summary>
        public short busyFreqIN { get; set; }

        /// <summary>
        /// Zajęte pasmo na wejściu węzła optycznego.
        /// </summary>
        public short busyBandIN { get; set; }

        /// <summary>
        /// Utworzenie tabeli zajetych pasm na wejsciu wezla.
        /// </summary>
        /// <param name="busyFrequency"></param>
        /// <param name="busyBandIn"></param>
        public EONTableIN(short busyFreq, short busyBandIn)
        {
            this.busyBandIN = busyBandIn;
            this.busyFreqIN = busyFreq;
        }
    }
}
