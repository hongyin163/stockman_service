﻿using StockMan.Message.DataAccess;
using StockMan.Message.Task.Biz;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using m=StockMan.Message.Model;

namespace StockMan.Message.Task.Client
{
    public class TaskManager
    {
        private static TaskManager instance = null;
        public static TaskManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new TaskManager();
                }
                return instance;
            }
        }
        private TaskService taskService = new TaskService();

        private IDictionary<string, SenderTask> taskList = null;
        public IDictionary<string, SenderTask> TaskList
        {
            get
            {
                if (taskList == null)
                {
                    taskList = new Dictionary<string, SenderTask>();
                }
                return taskList;
            }
        }
        private ITask Build(mq_task task)
        {
            return Load(task.code, task.assembly, task.type);
        }
        private ITask Load(string taskCode, string assembleName, string typeName)
        {
            var basePath = AppDomain.CurrentDomain.BaseDirectory + "tasks";
            var binPath = Path.Combine(basePath, taskCode);
            var path = Path.Combine(binPath, assembleName + ".dll");

            if (!File.Exists(path))
                throw new Exception(string.Format("程序集文件不存在:{0},{1}", assembleName, typeName));

            var asseblyBytes = File.ReadAllBytes(path);

            Assembly assebly = Assembly.Load(asseblyBytes);
            ITask taskIns = assebly.CreateInstance(typeName) as ITask;
            if (taskIns != null)
            {
                return taskIns;
            }
            else
            {
                throw new Exception(string.Format("任务执行器创建失败:程序集{0}：类型：{1}", assembleName, typeName));
            }
        }
        private ITask GetNextTask()
        {
            var list = taskService.GetTaskList();
            foreach (var task in list)
            {
                if (task.status == (int)StockMan.Message.Model.TaskStatus.stop)
                {
                    var trigger = new TimeTrigger(task.time);
                    if (trigger.IsTrigger())
                    {
                        return this.Build(task);
                    }
                }
            }
            return null;
        }
        public void Run()
        {
            var task = this.GetNextTask();
            if (task == null)
            {
                return;
            }
            this.Log().Info("任务:" + task.GetCode() + "开始执行");

            var senderTask= new SenderTask(task);
            senderTask.onComplete += senderTask_onComplete;
            this.TaskList.Add(task.GetCode(), senderTask);
            TaskThread.Post(senderTask);

            try
            {
                this.Log().Info("更新任务状态:" + task.GetCode()+","+ m.TaskStatus.running);
                taskService.Update(task.GetCode(), (int)m.TaskStatus.running);
            }
            catch (Exception ex)
            {
                this.Log().Info("更新任务状态异常:"+ex.Message+ex.StackTrace);
                this.TaskList.Remove(task.GetCode());
            }           
        }

        private void senderTask_onComplete(string code)
        {
            this.Log().Info("更新任务状态:" + code + "," + m.TaskStatus.stop);
            this.TaskList.Remove(code);
            taskService.Update(code, (int)m.TaskStatus.stop);
        }
    }
}
