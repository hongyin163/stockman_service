using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockMan.EntityModel;
using StockMan.Service.Interface.Rds;
using StockMan.MySqlAccess;

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
        public IList<user_position> GetUserPosition(string user_id)
        {
            using (StockManDBEntities entity = new StockManDBEntities())
            {
                return entity.user_position.Where(p => p.user_id == user_id).ToList();
            }
        }
        public void Buy(string user_id, string stock_code, int count)
        {
            //确定是否余额是否足够
            //买入后新增一条交易记录
            //修改用户持仓 数据
            //修改用户账号余额和 市值
            var amount = GetAmount(user_id);

        }
        public void Sell(string user_id, string stock_code, int count)
        {

        }
    }
}
