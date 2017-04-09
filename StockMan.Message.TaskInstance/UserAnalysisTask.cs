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
        IIndexService indexService = new IndexService();
        IUserTradeService userTradeService = new UserTradeService();
        IStockService stockService = new StockService();
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
            //var objects = user.object_user_map.ToList();
            //
            //状态检查，objectstate，状态转换点是交易提醒
            //自动触发
            if (stocks.Count <= 0)
            {
                return;
            }
            //检查自选股，如果有合适的买入
            Buy(user.id, stocks);

            //检查持仓股票，如果有合适的状态，卖出
            Sell(user.id); 

        }

        private void Sell(string user_id)
        {
            //检查持仓股票状态
            var positionList = userTradeService.GetUserPosition(user_id);

            if (positionList.Count <= 0)
            {
                return;
            }

            var pcodes = positionList.Select(p => p.stock_code).ToArray();

            var pstateList = indexService.GetObjectStates(pcodes);

            var sellList = pstateList.Where(p => p.day < p.last_day).ToList();

            var stockCodes = sellList.Select(p => p.object_code).ToArray();

            var targetStocks = stockService.GetStocksByIds(string.Join(",", stockCodes));

            foreach (var pstock in sellList)
            {
                var price = targetStocks.First(p => p.code == pstock.object_code).price;

                userTradeService.Sell(user_id, pstock.object_code, (decimal)price, 1000);
            }
        }

        public void Buy(String user_id, List<stock_user_map> stocks)
        {
            string[] codes = stocks.Select(p => p.stock_code).ToArray();

            var stateList = indexService.GetObjectStates(codes);

            var upList = stateList.Where(p => p.day > 0 && p.week > 0).ToList();

            var targets = upList.Where(p => p.day > p.last_day && p.week > p.last_week).ToList();

            if (targets.Count <= 0)
            {
                return;
            }

            var stockCodes = targets.Select(p => p.object_code).ToArray();

            var targetStocks = stockService.GetStocksByIds(string.Join(",", stockCodes));

            foreach (var stock in targets)
            {
                var price = targetStocks.First(p => p.code == stock.object_code).price;

                userTradeService.Buy(user_id, stock.object_code, (decimal)price, 1000);

            }
        }
    }
}
