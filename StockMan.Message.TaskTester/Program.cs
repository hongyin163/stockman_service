using StockMan.Message.Task;
using StockMan.Message.Task.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StockMan.Message.TaskTester
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("输入测试的TaskCode:");
                var code = Console.ReadLine();
                while (string.IsNullOrEmpty(code))
                {
                    code = Console.ReadLine();
                }

                TaskTest test = new TaskTest();

                var assembly = Assembly.Load("StockMan.Message.TaskInstance");
                var types = assembly.GetTypes()
                    .Where(t => t.GetInterfaces().Contains(typeof(ITask)))
                    .ToArray();

                foreach (var type in types)
                {
                    var task = (ITask)assembly.CreateInstance(type.FullName);
                    if (task.GetCode() == code)
                    {
                        test.Task = task;
                        break;
                    }
                    Console.WriteLine(type.Name);
                }
                test.Excute();
            }

        }

    }


    public class TaskTest
    {
        public ITask Task { get; set; }
        public void Excute()
        {
            var sender = new TestSener();
            sender.onReceive += Sender_onReceive;
            Task.Send(sender);
        }
        private void Sender_onReceive(string obj)
        {
            Task.Excute(obj);
        }
    }
    public class TestSener : IMessageSender
    {
        public event Action<string> onReceive;
        private int count = 0;
        public void Send(string message)
        {
            if (null != onReceive)
            {
                if (count++ == 0)
                    onReceive(message);
            }
        }
    }
}
