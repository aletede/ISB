using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISB
{
    class DataFile
    {
        private string _fullpath;
        private string _checksum;

        public DataFile(string fullpath, string checksum)
        {
            _fullpath = fullpath;
            _checksum = checksum;
        }

        public string FullPath
        {
            get { return _fullpath; }
            set { _fullpath = value; }
        }

        public string Checksum
        {
            get { return _checksum; }
            set { _checksum = value; }
        }
    }
}
