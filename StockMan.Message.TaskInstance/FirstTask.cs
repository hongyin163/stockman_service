using StockMan.Message.Model;
using StockMan.Message.Task;
using StockMan.Message.Task.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StockMan.Message.TaskInstance
{

    public class OneTask : Message.Task.ITask
    {
        public void Excute(string message)
        {
            throw new NotImplementedException();
        }

        public string GetCode()
        {
            throw new NotImplementedException();
        }

        public void Send(IMessageSender sender)
        {
            throw new NotImplementedException();
        }
    }


}
