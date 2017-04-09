using Newtonsoft.Json;
using StockMan.EntityModel;
using StockMan.Facade.Models;
using StockMan.Service.Cache;
using StockMan.Service.Interface.Rds;
using StockMan.Service.Rds;
using StockMan.Web.RestService.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace StockMan.Web.RestService.Controllers
{
    [IdentityBasicAuthentication]
    public class TradeController : ApiController
    {
        private IUserTradeService tradeService = new UserTradeService();
        private IStockService stockService = new StockService();
        /// <summary>
        /// 获取用户持仓数据
        /// </summary>
        /// <returns></returns>
        public IHttpActionResult GetMyPosition()
        {
            var user_id = this.User.Identity.Name; //this.User.Identity.n
            var userPositions = tradeService.GetUserPosition(user_id);
            //转变成字典
            Dictionary<string, user_position> userPosDic = new Dictionary<string, user_position>();
            foreach (var pos in userPositions)
            {
                userPosDic.Add(pos.stock_code, pos);
            }

            var stockIds = userPositions.Select(p => p.stock_code).ToArray();

            //从缓存区获取股票当前的价格
            string[] results = results = CacheHelper.Get(stockIds.Select(p => "1_" + p).ToArray());

            IList<Stock> stockList = null;
            if (results != null && results.Length > 0)
            {
                stockList = results
                    .Where(p => !string.IsNullOrEmpty(p))
                    .Select(p => JsonConvert.DeserializeObject<Stock>(p)).ToList();
            }
            else
            {
                var list = stockService.GetStocksByIds(string.Join(",", stockIds));
                stockList = list.Select(p => new Stock
                {
                    code = p.code,
                    name = p.name,
                    price = p.price,
                    percent = p.percent
                }).ToList();
            }

            IList<Position> position = new List<Position>();
            foreach (var stock in stockList)
            {
                var userPos = userPosDic[stock.code];
                var percent = Math.Round((double)((stock.price - userPos.cost) / userPos.cost), 2);
                var amount = Math.Round((double)(stock.price - userPos.cost) * userPos.position ?? 0, 2);
                position.Add(new Position
                {
                    stock_code = stock.code,
                    stock_name = stock.name,
                    price = stock.price,
                    cost = userPos.cost,
                    count = userPos.position,
                    percent = (decimal)percent,
                    amount = (decimal)amount
                });
            }
            return Ok<IList<Position>>(position);
        }
        /// <summary>
        /// 获取交易记录
        /// </summary>
        /// <returns></returns>
        public IHttpActionResult GetTradeRecord()
        {
            var user_id = this.User.Identity.Name;
            var tradeList = tradeService.GetUserTrade(user_id);
            IList<TradeRecord> trades = new List<TradeRecord>();
            foreach (var trade in tradeList)
            {
                trades.Add(new TradeRecord
                {
                    code = trade.code,
                    count = trade.count,
                    date = DateTime.Now,
                    direact = (int)trade.direact,
                    price = trade.price,
                    stock_code = trade.stock_code,
                    stock_name = stockService.Find(trade.stock_code).name
                });
            }

            return Ok<IList<TradeRecord>>(trades);
        }
        public IHttpActionResult GetStratety()
        {
            var user_id = this.User.Identity.Name;
            var strategy = tradeService.GetStragegy(user_id);
            return Ok(strategy);
        }
        [HttpPost]
        public IHttpActionResult SetStrategy([FromBody] IList<string> strategy)
        {
            var user_id = this.User.Identity.Name;
            tradeService.SetStrategy(user_id, strategy);
            return Ok(new Message
            {
                code = "200",
                success = true,
                content = "保存成功"
            });
        }
        public IHttpActionResult GetUserAmount()
        {
            var user_id = this.User.Identity.Name;
            var user_amount = tradeService.GetAmount(user_id);
            if (user_amount != null)
            {
                return Ok(new MyAmount
                {
                    id = user_amount.id,
                    amount = user_amount.amount,
                    init = true
                });
            }
            return Ok(new MyAmount
            {
                id = user_id,
                amount = 0,
                init = false
            });
        }

    }
}
