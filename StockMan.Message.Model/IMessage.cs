using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockMan.Message.Model
{
    public interface IMessage
    {
        string Description { get; set; }
    }
}
