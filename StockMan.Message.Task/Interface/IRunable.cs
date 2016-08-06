using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockMan.Message.Task.Interface
{
    public interface IRunable
    {
        void Run(object state);
    }
}
