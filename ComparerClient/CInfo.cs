using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComparerClient
{
    class CInfo
    {
        public string File1Name { get; set; }
        public string File2Name { get; set; }
        public double Similarity { get; set; }
        public CInfo(string file1Name, string file2Name, double similarity)
        {
            File1Name = file1Name;
            File2Name = file2Name;
            Similarity = similarity;
        }
    }
}
