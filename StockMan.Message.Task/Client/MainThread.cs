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
        private bool running = true;
        public void Start()
        {
            while (this.running)
            {
                TaskManager.Instance.Run();
                this.Log().Info(string.Format("{0}:任务检查运行中……",DateTime.Now.ToString("yy:MM:dd hh:mm:ss")));
                Thread.Sleep(1000*60);
            }
        }
        public void Receive(Model.CmdMessage cmdMsg)
        {
            Console.WriteLine(cmdMsg.command);
            var cmd = cmdMsg.command;
            if (cmd == "start")
            {
                this.running = true;
                this.Start();
            }
            else if (cmd == "pause")
            {
                this.Log().Info("暂停");
                this.running = false;
            }
            else if (cmd == "resume")
            {
                this.Log().Info("恢复");
                
                this.Start();
            }
            else if (cmd == "upload")
            {
                var assembly = cmdMsg.Get("assembly");
                var type = cmdMsg.Get("type");
                Loader.Instance.CreateTaskAssembly("T0001", cmdMsg.attachment);
            }
            else if (cmd == "init")
            {
                this.running = false;

                var code = cmdMsg.Get("task_code");
                Loader.Instance.CreateTaskAssembly(code, cmdMsg.attachment);

            }
        }
    }
}
