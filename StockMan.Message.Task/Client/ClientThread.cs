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
        private Thread moniterThread = null;
        private MessageClient client = null;
        private bool running = true;
        private Mutex mPause = new Mutex(false, null);
        Dictionary<string, TaskSender> taskSenders = new Dictionary<string, TaskSender>();
        public ClientThread()
        {
            //this.client = new MessageClient();
            this.jobThread = new Thread(Run);
            //this.senderThead = new Thread(RunSender);
            this.moniterThread = new Thread(Moniter);
        }

        public void Start()
        {
            this.jobThread.Start();
        }
        public void Moniter()
        {
            while (true)
            {
                foreach (var taskType in taskSenders.Keys)
                {
                    if (!taskSenders[taskType].IsBusy())
                    {
                        this.Log().Info(string.Format("任务服务:{0}空闲，卸载", taskType));
                        Loader.Instance.UnLoad(taskType);
                    }
                    this.Log().Info(taskSenders[taskType].GetStatus());
                }

                Thread.Sleep(1000 * 60);
            }
        }
        //ISchedulerFactory sf = null;
        //IScheduler sched = null;
        public void Run()
        {
           
            //获取Trigger，如果出发，加载，执行
            //任务状态，时间设置，什么执行，设置一个时间点，
            //时间点设置表达式：day:1-5,time:15:20   1-5,15:20
            //w:6-7,t:1:00
            while (this.running)
            {
                var list = taskService.GetTaskList();
                foreach (var task in list)
                {
                    if (MatchTrigger(task.time))
                    {
                        if (task.status == (int)StockMan.Message.Model.TaskStatus.stop)
                        {
                            //触发，
                            RemoteLoader rl = Loader.Instance.GetRemoteLoader(task.code);
                            var taskSender = rl.GetTaskSender();
                            taskSender.Load(task.assembly, task.type);
                            taskSender.Start();
                            this.taskSenders.Add(task.code, taskSender);
                        }
                    }
                }
                Thread.Sleep(60 * 1000);
            }

            //sf = new StdSchedulerFactory();
            //sched = sf.GetScheduler();
            //TaskService taskService = new TaskService();
            //var list = taskService.GetTaskList();
            //int i = 1;
            //foreach (var task in list)
            //{
            //    IJobDetail job = JobBuilder
            //         .Create<SenderJob>()
            //         .WithIdentity("job_" + task.code, "group_1")
            //         .RequestRecovery() // ask scheduler to re-execute this job if it was in progress when the scheduler went down...
            //         .Build();

            //    // tell the job to delay some small amount... to simulate work...

            //    job.JobDataMap.Put("assembly", task.assembly);
            //    job.JobDataMap.Put("type", task.type);
            //    job.JobDataMap.Put("taskCode", task.code);
            //    ITrigger trigger = TriggerBuilder.Create()
            //        .WithIdentity("trigger_" + task.code, "group_1")
            //        .StartAt(DateBuilder.FutureDate(1000*i++, IntervalUnit.Millisecond)) // space fire times a small bit
            //        .Build();

            //    sched.ScheduleJob(job, trigger);

            //}
            //sched.Start();


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

        private bool MatchTrigger(string time)
        {
            DateTime now = DateTime.Now;
            //执行时间|时间间隔多个时间间隔用|分割开 ，时间间隔单分钟

            //w:1-5,t:9:30-11:30|13:00-15:00 15
            string[] timeArry = time.Split(',');
            bool pass = true;
            foreach (var item in timeArry)
            {
                int sindex = item.IndexOf(':');
                string type = item.Substring(0, sindex).Trim();
                string val = item.Substring(sindex + 1).Trim();
                if (type == "w")
                {
                    //区间
                    if (MatchWeek(val))
                    {
                        continue;
                    }
                    else
                    {
                        pass = false;
                        break;
                    }

                }
                else if (type == "t")
                {
                    if (MatchTime(val))
                    {
                        continue;
                    }
                    else
                    {
                        pass = false;
                        break;
                    }
                }
            }
            return pass;
        }

        private bool MatchTime(string val)
        {
            //9:30-11:30|13:00-15:00 15
            string[] ts = val.Split(' ');
            string timeSection = ts[0];
            string timeInterval = ts.Length > 1 ? ts[1] : "";
            //时间点 时间区间 
            //多少时间区间
            if (timeSection.IndexOf('|') > 0)
            {
                //多个时间区间
                string[] timeSecArry = timeSection.Split('|');
                foreach (var sec in timeSecArry)
                {
                    if (MatchTimeSection(sec, timeInterval))
                    {
                        return true;
                    }
                }
                return false;
            }
            else if (timeSection.IndexOf('-') > 0)
            {
                //单个时间区间
                if (MatchTimeSection(timeSection, timeInterval))
                {
                    return true;
                }
                return false;
            }
            else
            {
                //时间点
                var now = DateTime.Now;
                var startTime = string.Format(now.Year + "-" + now.Month + "-" + now.Day + " {0}", timeSection);
                DateTime startT = DateTime.Parse(startTime);
                if (now.Hour == startT.Hour && now.Minute == startT.Minute)
                {
                    return true;
                }
                return false;
            }

        }

        private bool MatchTimeSection(string timeSection, string timeInterval)
        {
            var start = timeSection.Split('-')[0];
            var end = timeSection.Split('-')[1];
            var now = DateTime.Now;
            var startTime = string.Format(now.Year + "-" + now.Month + "-" + now.Day + " ", start);
            var endTime = string.Format(now.Year + "-" + now.Month + "-" + now.Day + " ", end);
            DateTime startT = DateTime.Parse(startTime);
            DateTime endT = DateTime.Parse(endTime);

            if (now.CompareTo(startT) >= 0 && now.CompareTo(endT) <= 0)
            {
                if (string.IsNullOrEmpty(timeInterval))
                    return true;

                int interval = int.Parse(timeInterval);
                TimeSpan ts = now - startT;
                var minutes = ts.TotalMinutes;
                if (minutes % interval == 0)
                {
                    return true;
                }
            }
            return false;
        }

        private bool MatchWeek(string val)
        {
            int day = (int)DateTime.Now.DayOfWeek;
            if (day == 0) day = 7;
            DateTime now = DateTime.Now;
            if (val.IndexOf('-') > 0)
            {
                int start = int.Parse(val.Split('-')[0]);
                int end = int.Parse(val.Split('-')[1]);
                if (day >= start && day <= end)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if (int.Parse(val) == day)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
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
            var taskList = taskService.GetTaskList();
            List<mq_message> list = new List<mq_message>(100);

            foreach (var task in taskList)
            {
                var tlist = messageService.GetUnHandleMessage(task.code);
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
