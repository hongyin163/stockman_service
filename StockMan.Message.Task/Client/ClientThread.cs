using Quartz;
using Quartz.Impl;
using StockMan.Message.DataAccess;
using StockMan.Message.Model;
using StockMan.Message.Task;
using StockMan.Message.Task.Biz;
using StockMan.Message.Task.Client;
using StockMan.Message.Task.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StockMan.Message.Client
{
    public class ClientThread : IReceiveCommand
    {
        private TaskService taskService = new TaskService();
        private MessageService messageService = new MessageService();
        private Thread jobThread = null;
        private MessageClient client = null;
        private Thread senderThead = null;
        private Mutex mPause = new Mutex(false, null);
        public ClientThread()
        {
            //this.client = new MessageClient();
            this.jobThread = new Thread(Run);
            //this.senderThead = new Thread(RunSender);
        }

        public void Start()
        {
            this.jobThread.Start();
        }

        ISchedulerFactory sf = null;
        IScheduler sched = null;
        public void Run()
        {
          
            sf = new StdSchedulerFactory();
            sched = sf.GetScheduler();
            TaskService taskService = new TaskService();
            var list = taskService.GetTaskList();
            int i = 1;
            foreach (var task in list)
            {
                IJobDetail job = JobBuilder
                     .Create<SenderJob>()
                     .WithIdentity("job_" + task.code, "group_1")
                     .RequestRecovery() // ask scheduler to re-execute this job if it was in progress when the scheduler went down...
                     .Build();

                // tell the job to delay some small amount... to simulate work...

                job.JobDataMap.Put("assembly", task.assembly);
                job.JobDataMap.Put("type", task.type);
                job.JobDataMap.Put("taskCode", task.code);
                ITrigger trigger = TriggerBuilder.Create()
                    .WithIdentity("trigger_" + task.code, "group_1")
                    .StartAt(DateBuilder.FutureDate(1000*i++, IntervalUnit.Millisecond)) // space fire times a small bit
                    .Build();

                sched.ScheduleJob(job, trigger);

            }
            sched.Start();
            //var sender = new TaskSender();
            //sender.Load("StockMan.Message.TaskInstance", "StockMan.Message.TaskInstance.ThreeTask");
            //sender.Start();
            //var assembly = "StockMan.Message.TaskInstance";
            //var type = "StockMan.Message.TaskInstance.ThreeTask";
            //RemoteLoader rl = Loader.Instance.GetRemoteLoader("ThreeTask");
            //var taskSender = rl.GetTaskSender();
            ////taskSender.onStop += taskSender_onStop;
            //taskSender.Load(assembly, type);
            //taskSender.Start();
        }
        internal void Pause()
        {
            this.Log().Info("暂停");
            mPause.WaitOne();
        }
        internal void Resume()
        {
            this.Log().Info("恢复");
            mPause.ReleaseMutex();
        }
        public void Receive(Model.CmdMessage cmdMsg)
        {
            var cmd = cmdMsg.command;
            if (cmd == "start")
            {
                this.Start();
            }
            else if (cmd == "reload")
            {
                this.Log().Info("断开连接");
                this.Pause();
                //this.worklistener.Close();
                //this.workThread.Interrupt();
                this.Log().Info("重新初始化服务列表");

                this.Log().Info("重启连接");
                //this.workThread.Start();
                //this.startMessageListener();
                this.Resume();
            }
            else if (cmd == "pause")
            {
                this.Log().Info("暂停");
                this.Pause();
            }
            else if (cmd == "resume")
            {
                this.Log().Info("恢复");
                this.Resume();
            }
            else if (cmd == "upload")
            {
                var assembly = cmdMsg.Get("assembly");
                var type = cmdMsg.Get("type");
                Loader.Instance.CreateTaskAssembly("T0001", cmdMsg.attachment);
            }
            else if (cmd == "init")
            {
                this.Pause();

                var code = cmdMsg.Get("task_code");
                Loader.Instance.CreateTaskAssembly(code, cmdMsg.attachment);

                this.Resume();
            }
        }

        public void RunSender()
        {
            this.Log().Info("处理未完成消息开始");
            //没有可用消息时重试的次数，如果超过次数仍没有消息，结束任务
            int retryTotal = 20;
            int retryCount = 0;
            while (true)
            {
                mPause.WaitOne();
                //从数据库获取消息，发送消息，修改状态
                IList<TaskMessage> list = getMessage();
                if (list.Count == 0)
                {
                    this.Log().Info("消息处理完成，等待:" + retryCount * 500);
                    Thread.Sleep(retryCount++ * 500);

                    if (retryCount <= retryTotal)
                        continue;
                    else
                    {
                        this.Log().Info("消息处理全部结束！");
                  
                        break;
                    }
                }
                foreach (var msg in list)
                {
                    this.sendMessage(msg);
                    //更新状态
                    //this.updateMessageStatus(msg, MessageStatus.Wait);

                    this.Log().Info("发送消息:" + msg.code);
                }
                mPause.ReleaseMutex();
                Thread.Sleep(300);
            }

        }

        private void sendMessage(TaskMessage msg)
        {
            this.client.Send(msg);
        }


        private IList<TaskMessage> getMessage()
        {
            var taskList=taskService.GetTaskList();
            List<mq_message> list = new List<mq_message>(100);

            foreach (var task in taskList)
            {
                var tlist=  messageService.GetUnHandleMessage(task.code);
                if (tlist.Count > 0)
                {
                    list.AddRange(tlist);
                }               
            }
            return list.Select(p => new TaskMessage
            {
                code = p.code,
                task_code = p.task_code,
                values = p.values,
                description = ""
            }).ToList();
        }
    }

}
