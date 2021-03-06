﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetMQ;
using System.Threading;
using System.IO;
using StockMan.Message.Model;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;
using StockMan.Message.Task.Biz;
using NetMQ.Sockets;
namespace StockMan.Message.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Control Center Start up");

            //using (NetMQ.NetMQContext context = NetMQContext.Create())
            using (var requester = new RequestSocket())
            {
                requester.Connect("tcp://127.0.0.1:5561");

                while (true)
                {
                    string cmd = Console.ReadLine();
                    IList<NetMQMessage> msgs = buildMessage(cmd);
                    foreach (var msg in msgs)
                    {
                        requester.SendMultipartMessage(msg);
                        string result = requester.ReceiveFrameString();
                        Console.WriteLine(result);
                    }
                }
            }



        }

        private static IList<NetMQMessage> buildMessage(string cmdText)
        {
            IList<NetMQMessage> msgList = new List<NetMQMessage>();
            //target command -p aaa -f dddd -d  
            var cmds = cmdText.Split('-');
            var mainCmd = cmds[0].Trim().Split(' ');
            var target = mainCmd[0];
            var command = mainCmd[1];


            CmdMessage cmd = new CmdMessage()
            {
                target = target,
                command = command
            };

            for (int i = 1; i < cmds.Length; i++)
            {
                var p = cmds[i].Split(' ');
                cmd.Put(p[0], p[1]);
            }

            switch (cmd.command)
            {
                case "upload":

                    if (string.IsNullOrEmpty(cmd.Get("assembly")))
                    {
                        cmd.Put("assembly", "StockMan.Message.TaskInstance");
                    }
                    if (string.IsNullOrEmpty(cmd.Get("type")))
                    {
                        cmd.Put("type", "StockMan.Message.TaskInstance.FirstTask");
                    }
                    //var assembly = cmd.Get("assembly");
                    //var type = cmd.Get("type");
                    //var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "tasks", assembly, type);

                    //string[] files = Directory.GetFiles(path);
                    //foreach (var file in files)
                    //{
                    //    if (File.Exists(file))
                    //    {
                    //        byte[] fileData = File.ReadAllBytes(file);
                    //        cmd.PutAttachment(Path.GetFileName(file), fileData);
                    //    }
                    //}
                    return msgList;
                case "init":
                    TaskService service = new TaskService();
                    var taskList = service.GetTaskList();
                    var assmeblys = taskList.Select(p => p.assembly).Distinct();
                    foreach (var assembly in assmeblys)
                    {
                        CmdMessage cmd0 = new CmdMessage()
                        {
                            target = target,
                            command = command
                        };

                        for (int i = 1; i < cmds.Length; i++)
                        {
                            var p = cmds[i].Split(' ');
                            cmd0.Put(p[0], p[1]);
                        }
                        //cmd0.Put("task_code", task.code);
                        //cmd0.Put("task_type", task.type);
                        cmd0.Put("task_assembly", assembly);
                        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tasks", assembly);
                        if (Directory.Exists(path))
                        {
                            var dics = GetFiles(path);
                            foreach (var file in dics.Keys)
                            {
                                cmd0.PutAttachment(file.Replace(path + "\\", ""), dics[file]);
                            }

                            NetMQMessage msg0 = GetMessage(cmd0);
                            msgList.Add(msg0);
                        }
                    }

                    return msgList;
                case "clear":
                    MessageService msgService = new MessageService();
                    msgService.RemoveAll();
                    return null;
                    break;
                default:
                    NetMQMessage msg = GetMessage(cmd);
                    msgList.Add(msg);
                    return msgList;
            }
        }
        public static Dictionary<string, byte[]> GetFiles(string path)
        {
            Dictionary<string, byte[]> dics = new Dictionary<string, byte[]>();
            string[] files = Directory.GetFiles(path);
            foreach (var file in files)
            {
                if (File.Exists(file))
                {
                    byte[] fileData = File.ReadAllBytes(file);
                    dics.Add(file, fileData);
                }
            }
            string[] subDirectory = Directory.GetDirectories(path);

            if (subDirectory.Length <= 0)
                return dics;

            foreach (var dir in subDirectory)
            {
                var result = GetFiles(dir);
                foreach (var f in result.Keys)
                {
                    dics.Add(f, result[f]);
                }
            }

            return dics;
        }

        private static NetMQMessage GetMessage(CmdMessage cmd)
        {
            MemoryStream ms = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(ms, cmd);
            byte[] data = ms.ToArray();
            ms.Close();

            //订阅标识
            NetMQMessage msg = new NetMQMessage();
            msg.Append(new NetMQFrame(cmd.target));
            msg.Append(new NetMQFrame(data));
            return msg;
        }

        //private static void push()
        //{
        //    using (NetMQ.NetMQContext context = NetMQContext.Create())
        //    using (var requester = context.CreateSocket(NetMQ.zmq.ZmqSocketType.Pub))
        //    {
        //        requester.Bind("tcp://127.0.0.1:5561");

        //        for (int n = 0; n < 100; ++n)
        //        {
        //            requester.Send("Hello" + n);

        //            //var reply = requester.ReceiveString();
        //            Console.WriteLine("Hello");
        //            //Thread.Sleep(100);
        //            //Console.WriteLine("Hello {0}!", reply);
        //        }
        //    }
        //}
    }
}
