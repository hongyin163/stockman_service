using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockMan.Message.Model
{
    [Serializable]
    public class CmdMessage
    {
        public Dictionary<string, string> _parameters = null;
        public Dictionary<string, string> parameters
        {
            get
            {
                if (_parameters == null)
                    _parameters = new Dictionary<string, string>();
                return _parameters;
            }
            set
            {
                _parameters = value;
            }
        }
        public string target { get; set; }
        public string command { get; set; }
        public Dictionary<string, byte[]> _attachment = null;
        public Dictionary<string, byte[]> attachment
        {
            get
            {
                if (_attachment == null)
                    _attachment = new Dictionary<string, byte[]>();
                return _attachment;

            }
            set {_attachment = value; }
        }
        public string Get(string key)
        {
            if (this.parameters.ContainsKey(key))
            {
                return this.parameters[key];
            }
            return string.Empty;
        }
        public void Put(string key, string value)
        {
            if (this.parameters == null)
                this.parameters = new Dictionary<string, string>();
            this.parameters.Add(key, value);
        }

        public byte[] GetAttachment(string key)
        {
            if (this.attachment.ContainsKey(key))
            {
                return this.attachment[key];
            }
            return new byte[0];
        }
        public void PutAttachment(string key, byte[] value)
        {
            if (this.attachment == null)
                this.attachment = new Dictionary<string, byte[]>();
            this.attachment.Add(key, value);
        }
    }
}
