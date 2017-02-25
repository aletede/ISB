using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISB
{
    class DirectoryStatus
    {
        private Dictionary<string, DataFile> status = new Dictionary<string, DataFile>();

        public void Insert(DataFile df)
        {
            this.status.Add(df.FullPath, df);
        }

        public string[] GetDiff(DirectoryStatus ds)
        {
            if (ds.status.Keys != null)
            {
                var keyDiff = this.status.Keys.Except(ds.status.Keys);
                if (keyDiff.Any())
                    return keyDiff.ToArray();
                else
                    return null;
            }
            else return null;
        }
    }
}
