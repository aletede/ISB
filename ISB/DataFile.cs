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
        private byte[] _checksum;

        public DataFile(string fullpath, byte[] checksum)
        {
            _fullpath = fullpath;
            _checksum = checksum;
        }

        public string FullPath
        {
            get { return _fullpath; }
            set { _fullpath = value; }
        }

        public byte[] Checksum
        {
            get { return _checksum; }
            set { _checksum = value; }
        }
    }
}
