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
        private const string CODE = "T0000";
        public void Excute(string message)
        {
            throw new NotImplementedException();
        }

        public string GetCode()
        {
            return CODE;
        }

        public void Send(IMessageSender sender)
        {
            throw new NotImplementedException();
        }
    }


}
