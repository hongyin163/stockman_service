using StockMan.Message.Model;
using StockMan.Message.Task.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using thread = System.Threading.Tasks;

namespace StockMan.Message.Task
{
    public interface ITask
    {
        /// <summary>
        /// code
        /// </summary>
        /// <returns></returns>
        string GetCode();

        void Send(IMessageSender sender);
        /// <summary>
        /// 接收消息
        /// </summary>
        /// <param name="message"></param>
        void Excute(string message);
    }
}
