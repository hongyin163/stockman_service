using StockMan.Message.Model;
using StockMan.Message.Task;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StockMan.Message.TaskInstance
{

    public class OneTask : ITask
    {
        public string GetCode()
        {
            return "OneTask";
        }
        public int count = 0;
        public int total = 500;
        public IList<Model.TaskMessage> GetMessage()
        {
            ;
            IList<Model.TaskMessage> msgList = new List<TaskMessage>();
            if (count < total)
            {
                int i = count;
                for (i = count; i < count + 500; i++)
                {
                    msgList.Add(new TaskMessage
                    {
                        code = this.GetCode() + "_" + i + "",
                        task_code = this.GetCode(),
                        description = "测试" + i,
                        values = "{data:1}"
                    });
                }
                count = i;
            }

            return msgList;
        }

        public void Excute(Model.TaskMessage message)
        {
            this.Log().Info("执行任务" + message.code);
            Thread.Sleep(500);
            this.Log().Info("任务完成" + message.code);
        }


        public void Send(Task.Interface.IMessageSender sender)
        {
            int i = 0;
            while (true)
            {
                sender.Send(new TaskMessage
                {
                    code = this.GetCode() + "_" + (i++) + "",
                    task_code = this.GetCode(),
                    description = "测试" + i,
                    values = "{data:1}"
                });
                if (i > 2)
                    break;
                ;
            }
        }
    }

    public class FourTask : ITask
    {
        public string GetCode()
        {
            return "FourTask";
        }
        public int count = 0;
        public int total = 500;
        public IList<Model.TaskMessage> GetMessage()
        {
            ;
            IList<Model.TaskMessage> msgList = new List<TaskMessage>();
            if (count < total)
            {
                int i = count;
                for (i = count; i < count + 500; i++)
                {
                    msgList.Add(new TaskMessage
                    {
                        code = this.GetCode() + "_" + i + "",
                        task_code = this.GetCode(),
                        description = "测试" + i,
                        values = "{data:1}"
                    });
                }
                count = i;
            }

            return msgList;
        }

        public void Excute(Model.TaskMessage message)
        {
            this.Log().Info("执行任务" + message.code);
            Thread.Sleep(500);
            this.Log().Info("任务完成" + message.code);
        }


        public void Send(Task.Interface.IMessageSender sender)
        {
            int i = 0;
            while (true)
            {
                sender.Send(new TaskMessage
                {
                    code = this.GetCode() + "_" + (i++) + "",
                    task_code = this.GetCode(),
                    description = "测试" + i,
                    values = "{data:1}"
                });
                if (i > 100)
                    break;
                ;
            }
        }
    }

    public class SecondTask : ITask
    {
        public string GetCode()
        {
            return "SecondTask";
        }
        public int count = 0;
        public int total = 500;
        public IList<Model.TaskMessage> GetMessage()
        {
            ;
            IList<Model.TaskMessage> msgList = new List<TaskMessage>();
            if (count < total)
            {
                int i = count;
                for (i = count; i < count + 500; i++)
                {
                    msgList.Add(new TaskMessage
                    {
                        code = this.GetCode() + "_" + i + "",
                        task_code = this.GetCode(),
                        description = "测试" + i,
                        values = "{data:1}"
                    });
                }
                count = i;
            }

            return msgList;
        }

        public void Excute(Model.TaskMessage message)
        {
            this.Log().Info("执行任务" + message.code);
            Thread.Sleep(500);
            this.Log().Info("任务完成" + message.code);
        }


        public void Send(Task.Interface.IMessageSender sender)
        {
            int i = 0;
            while (true)
            {
                sender.Send(new TaskMessage
                {
                    code = this.GetCode() + "_" + (i++) + "",
                    task_code = this.GetCode(),
                    description = "测试" + i,
                    values = "{data:1}"
                });
                if (i > 2)
                    break;
                ;
            }
        }
    }


    public class ThreeTask : ITask
    {
        public string GetCode()
        {
            return "ThreeTask";
        }
        public int count = 0;
        public int total = 500;
        public IList<Model.TaskMessage> GetMessage()
        {
            ;
            IList<Model.TaskMessage> msgList = new List<TaskMessage>();
            if (count < total)
            {
                int i = count;
                for (i = count; i < count + 500; i++)
                {
                    msgList.Add(new TaskMessage
                    {
                        code = this.GetCode() + "_" + i + "",
                        task_code = this.GetCode(),
                        description = "测试" + i,
                        values = "{data:1}"
                    });
                }
                count = i;
            }

            return msgList;
        }

        public void Excute(Model.TaskMessage message)
        {
            this.Log().Info("执行任务" + message.code);
            Thread.Sleep(500);
            this.Log().Info("任务完成" + message.code);
        }


        public void Send(Task.Interface.IMessageSender sender)
        {
            int i = 0;
            while (true)
            {
                sender.Send(new TaskMessage
                {
                    code = this.GetCode() + "_" + (i++) + "",
                    task_code = this.GetCode(),
                    description = "测试" + i,
                    values = "{data:1}"
                });
                if (i > 2)
                    break;
                ;
            }
        }
    }
    public class FiveTask : ITask
    {
        public string GetCode()
        {
            return "FiveTask";
        }
        public int count = 0;
        public int total = 500;
        public IList<Model.TaskMessage> GetMessage()
        {
            ;
            IList<Model.TaskMessage> msgList = new List<TaskMessage>();
            if (count < total)
            {
                int i = count;
                for (i = count; i < count + 500; i++)
                {
                    msgList.Add(new TaskMessage
                    {
                        code = this.GetCode() + "_" + i + "",
                        task_code = this.GetCode(),
                        description = "测试" + i,
                        values = "{data:1}"
                    });
                }
                count = i;
            }

            return msgList;
        }

        public void Excute(Model.TaskMessage message)
        {
            this.Log().Info("执行任务" + message.code);
            Thread.Sleep(500);
            this.Log().Info("任务完成" + message.code);
        }


        public void Send(Task.Interface.IMessageSender sender)
        {
            int i = 0;
            while (true)
            {
                sender.Send(new TaskMessage
                {
                    code = this.GetCode() + "_" + (i++) + "",
                    task_code = this.GetCode(),
                    description = "测试" + i,
                    values = "{data:1}"
                });
                if (i > 100)
                    break;
            }
        }
    }
}
