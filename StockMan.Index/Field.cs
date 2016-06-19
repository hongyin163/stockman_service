using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StockMan.Index
{
    public class Field
    {
        public int index { get; set; }
        public string name
        {
            get;set;
        }

        public string description
        {
            get;
            set;
        }

        public string color
        {
            get;
            set;
        }

        public string char_type
        {
            get;
            set;
        }
    }

    public class Parameter
    {
        public string key { get; set; }
        public string value { get; set; }
    }
}
