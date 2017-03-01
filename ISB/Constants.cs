using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISB
{
    static class Constants
    {
        public const int RECONNECT = 1000;
        public const int READTIMEOUT = 30000;
        public const int WRITETIMEOUT = 30000;
        public const int BUFFSIZE = 2048;
        public const int CLOSECONN = -1;
        public const int POS_ANS = 1;
        public const int INIT = 2;
        public const int SRVREADY = 3;
        public const int RCVENTRIES = 4;
        public const int SYNCFILE = 5;
        public const int RESTOREDIR = 6;
        public const int RCVDATAFILE = 7;

    }
}
