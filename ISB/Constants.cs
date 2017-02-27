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
        public const int READTIMEOUT = 60000;
        public const int WRITETIMEOUT = 60000;
        public const int BUFFSIZE = 2048;
        public const int CLOSECONN = -1;
        public const int POS_ANS = 1;
        public const int INIT = 2;
        public const int RCVENTRIES = 3;
        public const int NOTIFYDIR = 4;
        public const int NOTIFYFILE = 5;
    }
}
