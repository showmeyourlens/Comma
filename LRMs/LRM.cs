using System;
using System.Collections.Generic;
using System.Text;

namespace LRMs
{
    public class LRM
    {
        public int linkId;
        public int length;
        public bool isLinkWorking;
        public List<int> busyCracks;
        public List<string> contacts;

        public LRM(int id, int length)
        {
            linkId = id;
            this.length = length;
            isLinkWorking = true;
            busyCracks = new List<int>();
            contacts = new List<string>();
        }

        public string GetBusyCracksMessage()
        {
            StringBuilder sb = new StringBuilder();
            foreach(int busyCrack in busyCracks)
            {
                if (sb.Length != 0)
                {
                    sb.Append(" ");
                }
                sb.Append(busyCrack);
            }

            return sb.ToString();
        }

        public bool AddCracks(string message)
        {
            //FirstCrack LastCrack
            string[] cracks = message.Split(' ');
            for (int i = Int32.Parse(cracks[0]); i <= Int32.Parse(cracks[1]); i++)
            {
                if (!busyCracks.Contains(i))
                {
                    busyCracks.Add(i);
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        public bool ReleaseCracks(string message)
        {
            string[] cracks = message.Split(' ');
            for (int i = Int32.Parse(cracks[0]); i <= Int32.Parse(cracks[1]); i++)
            {
                if (busyCracks.Contains(i))
                {
                    busyCracks.Remove(i);
                }
                else
                {
                    return false;
                }
            }
            return true;
        }
    }
}
