using System;
using System.Collections.Generic;
using System.Text;

namespace LRMs
{
    public class LRM
    {
        public int linkId;
        public int length;
        public List<int> busyCracks;
        public List<string> domains;

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
            string[] cracks = message.Split(' ');
            foreach(string crack in cracks)
            {
                if(!busyCracks.Contains(Int32.Parse(crack)))
                {
                    busyCracks.Add(Int32.Parse(crack));
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
            foreach (string crack in cracks)
            {
                if (busyCracks.Contains(Int32.Parse(crack)))
                {
                    busyCracks.Remove(Int32.Parse(crack));
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
