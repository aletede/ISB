using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISB
{
    class LogMessage
    {
        private string _msg;
        private DateTime _dt;

        public LogMessage(string msg, DateTime dt)
        {
            _msg = msg;
            _dt = dt;
        }

        public string Message
        {
            get { return _msg; }
            set { _msg = value; }
        }

        public DateTime Time
        {
            get { return _dt; }
            set { _dt = value; }
        }

        public override string ToString()
        {
            return _dt.ToLongTimeString() + ": " + _msg + "\r"; 
        }
    }
}
