using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace StockMan.Index
{
    public class IndexDefinitionInfo
    {
        public IndexDefinitionInfo()
        {
            this.fields = new List<Field>();
        }
        public string name
        {
            get;
            set;
        }

        public string description
        {
            get;
            set;
        }

        public IList<Field> fields
        {
            get;
            set;
        }
     
        public string algorithm
        {
            get;
            set;
        }

        public string code
        {
            get;
            set;
        }
        public string server_algorithm_code { get; set; }
        public string client_algorithm_code { get; set; }
        public IList<Parameter> parameter { get; set; }

        public string table_name { get; set; }
    }
}
