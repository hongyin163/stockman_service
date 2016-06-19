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
        string GetCode();
        /// <summary>
        /// 开线程轮训该方法，返回消息，存储到数据库
        /// </summary>
        /// <returns></returns>
        IList<TaskMessage> GetMessage();

        void Send(IMessageSender sender);
        /// <summary>
        /// 接收消息
        /// </summary>
        /// <param name="message"></param>
        void Excute(TaskMessage message);
    }
}
