using StockMan.Message.Task.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StockMan.Message.Task
{
    public static class TaskThread
    {
        public static void Post(IRunable task)
        {
            ThreadPool.QueueUserWorkItem(task.Run);
        }
    }
}
