using System;
using System.Collections.Generic;
using System.Text;

namespace CableCloud
{
    /// <summary>
    /// Klasa z rzedem tabeli zajetych pasm na wyjsciu routera.
    /// </summary>
    public class EONTableOUT
    {
        /// <summary>
        /// Czestotliwosc na jakiej zaczyna sie pasmo.
        /// </summary>
        public short busyFreqOUT { get; set; }

        /// <summary>
        /// Zajęte pasmo na wyjściu węzła optycznego.
        /// </summary>
        public short busyBandOUT { get; set; }

        public EONTableOUT(short busyFrequency, short busyBandOut)
        {
            this.busyBandOUT = busyBandOut;
            this.busyFreqOUT = busyFrequency;
        }
    }
}
