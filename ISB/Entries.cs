using System;

namespace ISB
{
    public enum EntryType
    {
        Cartella,
        File
    }

    class LocalEntry
    {
        public string Name { get; set; }
        public string Fullpath { get; set; }
        private string _size;
        public EntryType Type { get; set; }
        public string Date { get; set; }
        public Uri Image { get; set; }

        public LocalEntry(string name, string fullpath, string size, string date, EntryType type, Uri image)
        {
            Name = name;
            Fullpath = fullpath;
            _size = size;
            Date = date;
            Type = type;
            Image = image;
        }

        public string Size
        {
            get { if (_size != null) return _size + " KB"; else return _size; }
            set { _size = value; }
        }
    }

    class RemoteEntry
    {
        //public string Name { get; set; }
        public string Date { get; set; }
        public string ID { get; set; }

        public RemoteEntry(string date, string id)
        {
            //Name = name;
            Date = date;
            ID = id;
        }
    }
}
