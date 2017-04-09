using Quartz;
using StockMan.Message.DataAccess;
using StockMan.Message.Model;
using StockMan.Message.Task;
using StockMan.Message.Task.Biz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using thread = System.Threading.Tasks;
namespace StockMan.Message.Task.Client
{
    public class SenderJob : IJob
    {
        private string assembly;
        private string type;
        private string taskCode;
        public void Execute(IJobExecutionContext context)
        {
            assembly = context.JobDetail.JobDataMap.Get("assembly") + "";
            type = context.JobDetail.JobDataMap.Get("type") + "";
            taskCode = context.JobDetail.JobDataMap.Get("taskCode") + "";

            var host = new TaskSenderHost(taskCode, assembly, type);
            host.Start();

        }

    }


    public class TaskSenderHost
    {
        private MessageClient client = null;
        TaskSender taskSender = null;
        TaskService taskService = new TaskService();
        private Loader assembleloader = null;
        protected Loader Assembleloader
        {
            get
            {
                if (assembleloader == null)
                    assembleloader = new Loader();
                return assembleloader;
            }
        }
        public TaskSenderHost(string taskCode, string assembly, string type)
        {
            this.client = new MessageClient();
            this.assembly = assembly;
            this.type = type;
            this.taskCode = taskCode;
        }
        private string assembly;
        private string type;
        private string taskCode;
        public void Start()
        {

            RemoteLoader rl = Assembleloader.GetRemoteLoader(this.assembly,taskCode);
            taskSender = rl.GetTaskSender();
            taskSender.Load(assembly, type);
            taskSender.Start();

            //sendMessage();
        }

       
    }
}
