using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockMan.Message.Model
{
    [Serializable]
    public class TaskMessage
    {
        public string code { get; set; }
        public string task_code { get; set; }
        public string values { get; set; }
        public string description { get; set; }

        public T GetData<T>()
        {
            return JsonConvert.DeserializeObject<T>(this.values);
        }
        public void SetData<T>(T data)
        {
            this.values = JsonConvert.SerializeObject(data);
        }

    }

    public class TaskMessageBuilder
    {
        public static TaskMessage Build<T>(string code, string task_code, string description, T data)
        {
            var msg = new TaskMessage()
            {
                code = code,
                task_code = task_code,
                description = description
            };
            msg.SetData<T>(data);
            return msg;
        }

    }
    //消息状态，0
    public enum MessageStatus
    {
        UnHandle = 0,//已入库，未处理
        Wait = 1,//已出库，待处理
        Running = 2,//处理中
        Retry = 3,//等待重新处理
        Success = 4,//处理成功
        Failed = 5//处理失败
    }
}
