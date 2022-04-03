using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoutubeRandomVideoLibrary
{ 
    public static class RandomSelector
    {
        public static string Select(List<string> list)
        {
            Random rand = new Random();
            int index = rand.Next(list.Count);
            string randomString = list[index];

            return randomString;
        }
    }
}

