using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockMan.Message.Task.Interface;

namespace StockMan.Message.TaskInstance
{
    /// <summary>
    /// 用户个股分析
    /// </summary>
    class UserAnalysisTask : Message.Task.ITask
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
