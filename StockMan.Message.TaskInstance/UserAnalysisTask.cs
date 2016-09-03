using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockMan.Message.Task.Interface;
using StockMan.Service.Rds;
using StockMan.Service.Interface.Rds;
using StockMan.EntityModel;
using Newtonsoft.Json;

namespace StockMan.Message.TaskInstance
{
    /// <summary>
    /// 用户个股分析
    /// </summary>
    class UserAnalysisTask : Message.Task.ITask
    {
        private const string CODE = "T0007";
        IUserService userService = new UserService();
        public string GetCode()
        {
            return CODE;
        }
        public void Send(IMessageSender sender)
        {
            int pageSize = 10, pageIndex = 0;
            IList<users> userList = userService.GetUserWithDataList(pageSize, pageIndex);
            while (userList.Count() > 0)
            {
                foreach (var user in userList)
                {
                    UserInfo info = new UserInfo
                    {
                        id = user.id
                    };
                    sender.Send(JsonConvert.SerializeObject(info));
                }

                userList = userService.GetUserWithDataList(pageSize, ++pageIndex);
            }
        }
        public void Excute(string message)
        {
            var msg = JsonConvert.DeserializeObject<UserInfo>(message);
            //计算自选股状态，自动交易，产生交易通知
            //什么状态买入，卖出？
            //自选股列表
            var user = userService.GetUserWithDataList("13570723981");
            var stocks = user.stock_user_map.ToList();
            var objects = user.object_user_map.ToList();
            //


        }

    }
}
