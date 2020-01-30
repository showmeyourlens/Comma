using System;
using System.Collections.Generic;
using System.Linq;
using ToolsLibrary;

namespace Subnetwork
{
    public static class SignalParamFinder
    {
        private static readonly int numberOfAllCracks = 400;
        private static readonly double waveLength = 1550 * Math.Pow(10, -9);
        private static readonly double lightSpeed = 3 * Math.Pow(10, 8);

        public static string FindParams(int bandwidth, int distance, List<int> busyCracks)
        {
            double freq = lightSpeed / waveLength / Math.Pow(10, 9);
            double firstFreq = freq - 20 * 12.5;
            double MiddleLambda = 0.0;

            int maxNumberOfCracks = busyCracks.Count > 0 ? busyCracks.Max() : 0;
            int FirstCrack = 0;
            int LastCrack = 0;


            int value = SetModulationValue(distance);
            double bauds = (double)bandwidth / value;
            double band = 2 * bauds;
            int numberOfCracksNeeded = 2 + (int)Math.Ceiling(band / 12.5);


            if (maxNumberOfCracks + numberOfCracksNeeded <= numberOfAllCracks)
            {
                for (int i = 1; i <= numberOfCracksNeeded; i++)
                {
                    busyCracks.Add(maxNumberOfCracks + i);
                }

                int max = busyCracks.Max();

                FirstCrack = maxNumberOfCracks + 1;
                LastCrack = maxNumberOfCracks + numberOfCracksNeeded;


                double middleFreq = firstFreq + (((FirstCrack + LastCrack) * 12.5) / 2);

                MiddleLambda = lightSpeed / middleFreq;

            }
            Console.WriteLine(TimeStamp.TAB + String.Format(" Lambda: {0:N2}nm, First slot: {1}, Last slot: {2}", MiddleLambda, FirstCrack, LastCrack));
            return String.Format("{0} {1} {2}", MiddleLambda, FirstCrack, LastCrack);
        }


        private static int SetModulationValue(int distance)
        {
            string modulation = string.Empty;
            int value = 0;

            if (distance <= 100)
            {
                modulation = "64-QAM";
                value = 6;
            }
            else if (distance > 100 && distance <= 250)
            {
                modulation = "32-QAM";
                value = 5;
            }
            else if (distance > 250 && distance <= 500)
            {
                modulation = "16-QAM";
                value = 4;
            }
            else if (distance > 500 && distance <= 1000)
            {
                modulation = "8-QAM";
                value = 3;
            }
            else if (distance > 1000 && distance <= 1500)
            {
                modulation = "QPSK";
                value = 2;
            }
            else if (distance > 1500)
            {
                modulation = "BPSK";
                value = 1;
            }
            Console.WriteLine(TimeStamp.TAB + " Distnace: {0}, Modulation: {1}", distance, modulation);
            return value;
        }
    }
}