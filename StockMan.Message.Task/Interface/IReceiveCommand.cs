using StockMan.Message.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockMan.Message.Task.Interface
{
    public interface IReceiveCommand
    {
        void Receive(CmdMessage cmd);
    }
}
