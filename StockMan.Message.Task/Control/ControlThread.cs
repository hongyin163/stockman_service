using StockMan.Message.Model;
using StockMan.Message.Task.Interface;
using StockMan.Message.Task.Worder;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StockMan.Message.Task.Control
{
    public class ControlThread
    { 
        private IReceiveCommand controlCommand = null;
        private Thread thread = null;
        private ControlListener controlListener = null;
        private string target = string.Empty;
        public ControlThread(string target,IReceiveCommand command)
        {
            this.target = target;
            this.controlCommand = command;
            this.thread = new Thread(Run);
        }
        public void Start()
        {
            this.Log().Info("Client Start");
            this.thread.Start();
        }

        public void Run()
        {
            if (controlListener != null) { controlListener.Dispose(); }
            controlListener = new ControlListener(this.target);
            controlListener.onReceive += this.HandleControlMessage;
            controlListener.onError += listener_onError;
            controlListener.connect();

        }

        private void listener_onError(string obj)
        {
            this.Log().Error(obj);
        }

        private void HandleControlMessage(CmdMessage cmdMsg)
        {
            this.controlCommand.Receive(cmdMsg); 
        }
    }
}
