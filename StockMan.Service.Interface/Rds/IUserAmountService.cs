using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StockMan.EntityModel;

namespace StockMan.Service.Interface.Rds
{
    public interface IUserAmountService : IEntityService<user_trade>,IDisposable
    {

    }
}
