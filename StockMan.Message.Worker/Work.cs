using StockMan.Message.DataAccess;
using StockMan.Message.Model;
using StockMan.Message.Task;
using StockMan.Message.Task.Control;
using StockMan.Message.Task.Worder;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StockMan.Message.Worker
{
    public class Worker
    {

        ControlThread controlThread = null;
        WorkerThread workerThread = null;
        public void Start()
        {
            this.workerThread = new WorkerThread();
            //this.workerThread.Start();

            this.controlThread = new ControlThread("worker", this.workerThread);
            this.controlThread.Start();
            
        }
    }
}
