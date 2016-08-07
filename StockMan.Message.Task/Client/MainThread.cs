using StockMan.Message.Task.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using StockMan.Message.Model;

namespace StockMan.Message.Task.Client
{
    public class MainThread : IReceiveCommand
    {
        private bool running = false;

        public void Start()
        {
            if (!this.running)
            {
                this.running = true;

                Thread taskThread = new Thread(Run);
                taskThread.Start();               
            }
        }

        private void Run()
        {
            while (this.running)
            {
                TaskManager.Instance.Run();
                this.Log().Info(string.Format("{0}:任务检查运行中……", DateTime.Now.ToString("yy:MM:dd hh:mm:ss")));
                Thread.Sleep(1000 * 60);
            }
            this.Log().Info("任务线程结束");
        }

        public void Receive(Model.CmdMessage cmdMsg)
        {
            var cmd = cmdMsg.command;
            if (cmd == "start")
            {
                this.Log().Info("任务开始");
                this.Start();
            }
            else if (cmd == "stop")
            {
                this.Log().Info("任务停止");
                this.running = false;
            }
            else if (cmd == "init")
            {
                this.Log().Info("初始化程序集");

                this.running = false;

                var assembly = cmdMsg.Get("task_assembly");
                Loader.Instance.CreateTaskAssembly(assembly, cmdMsg.attachment);

            }
        }
    }
}
