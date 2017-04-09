using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockMan.EntityModel;
using StockMan.Service.Interface.Rds;
using StockMan.MySqlAccess;
using StockMan.Common;
using Newtonsoft.Json;
namespace StockMan.Service.Rds
{
    public class UserTradeService : ServiceBase<user_trade>, IUserTradeService
    {

        public user_amount GetAmount(string user_id)
        {
            using (StockManDBEntities entity = new StockManDBEntities())
            {
                return entity.user_amount.FirstOrDefault(p => p.id == user_id);
            }
        }
        public void AddAmount(string user_id, decimal amount)
        {
            using (StockManDBEntities entity = new StockManDBEntities())
            {
                var um = entity
                    .user_amount
                    .FirstOrDefault(p => p.id == user_id);
                if (um != null)
                {
                    um.amount = um.amount + amount;
                    entity.SaveChanges();
                }
            }
        }
        public void SubAmount(string user_id, decimal amount)
        {
            using (StockManDBEntities entity = new StockManDBEntities())
            {
                var um = entity
                    .user_amount
                    .FirstOrDefault(p => p.id == user_id);
                if (um != null)
                {
                    if (um.amount >= amount)
                    {
                        um.amount = um.amount - amount;
                        entity.SaveChanges();
                    }
                    else
                    {
                        throw new TradeException("余额不足");
                    }
                }
            }
        }
        public IList<user_position> GetUserPosition(string user_id)
        {
            using (StockManDBEntities entity = new StockManDBEntities())
            {
                return entity.user_position.Where(p => p.user_id == user_id).ToList();
            }
        }

        public IList<user_trade> GetUserTrade(string user_id)
        {
            using (StockManDBEntities entity = new StockManDBEntities())
            {
                return entity.user_trade
                    .Where(p => p.user_id == user_id)
                    .OrderByDescending(p => p.date)
                    .Take(5)
                    .ToList();
            }
        }
        public void AddPosition(string user_id, string stock_code, int position)
        {
            using (StockManDBEntities entity = new StockManDBEntities())
            {

                var code = user_id + "_" + stock_code;
                var pos = entity.user_position.FirstOrDefault(p => p.code == code);
                if (pos != null)
                {
                    pos.position = pos.position + position;
                }
                else
                {
                    entity.user_position.Add(new user_position
                    {
                        code = code,
                        user_id = user_id,
                        position = position,
                        stock_code = stock_code
                    });
                }
                entity.SaveChanges();
            }
        }
        public void SubPosition(string user_id, string stock_code, int position)
        {
            using (StockManDBEntities entity = new StockManDBEntities())
            {
                var code = user_id + "_" + stock_code;
                var pos = entity.user_position.FirstOrDefault(p => p.code == code);
                if (pos != null)
                {
                    if (pos.position >= position)
                    {
                        pos.position = pos.position - position;
                        entity.SaveChanges();
                    }
                }
            }
        }
        public void AddTrade(string user_id, string stock_code, decimal price, int count, TradeDirection tradeDirection)
        {
            using (StockManDBEntities entity = new StockManDBEntities())
            {
                var code = user_id + "_" + stock_code + "_" + (int)tradeDirection + "_" + DateTime.Now.ToString("yyyyMMddhhmmss");
                entity.user_trade.Add(new user_trade
                {
                    code = code,
                    user_id = user_id,
                    stock_code = stock_code,
                    count = count,
                    direact = (int)tradeDirection,
                    price = price,
                    date = DateTime.Now
                });
                entity.SaveChanges();
            }
        }
        public void Buy(string user_id, string stock_code, decimal price, int count)
        {
            //确定是否余额是否足够
            //买入后新增一条交易记录
            //修改用户持仓 数据
            //修改用户账号余额和 市值
            var userAmount = GetAmount(user_id);
            var amount = userAmount.amount ?? 0;
            //获取股票当前价格
            var total = price * count;
            if (amount >= total)
            {
                //购买
                //新增一个持仓记录
                AddPosition(user_id, stock_code, count);

                //扣减余额
                SubAmount(user_id, total);

                //添加交易记录
                AddTrade(user_id, stock_code, price, count, TradeDirection.buy);

            }
            else
            {
                throw new TradeException("余额不足");
            }

        }
        public void Sell(string user_id, string stock_code, decimal price, int count)
        {
            //判断是否有持仓，能否卖出
            var posList = GetUserPosition(user_id);
            var pos = posList.FirstOrDefault(p => p.stock_code == stock_code);
            //总共获得的金额
            var amount = price * count;
            if (pos != null)
            {
                if (pos.position >= count)
                {
                    //可以卖
                    //扣减持仓数量
                    SubPosition(user_id, stock_code, count);

                    //增加余额
                    AddAmount(user_id, amount);

                    //添加交易记录
                    AddTrade(user_id, stock_code, price, count, TradeDirection.sell);
                }
                else
                {
                    throw new TradeException("没有持仓");
                }
            }
        }

        public void SetStrategy(string user_id, IList<string> strategry)
        {
            using (StockManDBEntities entity = new StockManDBEntities())
            {
                if (strategry==null)
                {
                    strategry = new List<string>();
                }

                var us = entity.user_strategy.FirstOrDefault(p => p.user_id == user_id);
                if (us == null)
                {
                    entity.user_strategy.Add(new user_strategy
                    {
                        user_id = user_id,
                        strategy =JsonConvert.SerializeObject(strategry)
                    });
                }
                else
                {
                    us.strategy = JsonConvert.SerializeObject(strategry);
                }
                entity.SaveChanges();
            }
        }
        public IList<string> GetStragegy(string user_id)
        {
            using (StockManDBEntities entity = new StockManDBEntities())
            {

                var data = entity.user_strategy.Where(p => p.user_id == user_id).FirstOrDefault();
                if (data == null)
                {
                    return new List<string>();
                }
                else
                {
                    if (string.IsNullOrEmpty(data.strategy))
                    {
                        return new List<string>();
                    }
                    return JsonConvert.DeserializeObject<IList<string>>(data.strategy);
                }
            }
        }
    }

    public enum TradeDirection
    {
        buy = 1,
        sell = 0
    }
}
