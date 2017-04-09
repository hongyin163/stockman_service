using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StockMan.EntityModel;

namespace StockMan.Service.Interface.Rds
{
    public interface IUserTradeService : IEntityService<user_trade>, IDisposable
    {
        void Buy(string user_id, string stock_code, decimal price, int count);
        void Sell(string user_id, string stock_code, decimal price, int count);
        IList<user_position> GetUserPosition(string user_id);
        IList<user_trade> GetUserTrade(string user_id);
        user_amount GetAmount(string user_id);
        void SetStrategy(string user_id, IList<string> strategy);
        IList<string> GetStragegy(string user_id);
    }
}
