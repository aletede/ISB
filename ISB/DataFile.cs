using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISB
{
    class DataFile
    {
        public string FullPath { get; set; }
        public string Checksum { get; set; }

        public DataFile() { }
        public DataFile(string fullpath, string checksum)
        {
            FullPath = fullpath;
            Checksum = checksum;
        }
    }
}
