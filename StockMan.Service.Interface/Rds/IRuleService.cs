using System;
using System.Collections.Generic;
using data=StockMan.EntityModel;

namespace StockMan.Service.Interface.Rds
{
    public interface IRuleService : IEntityService<data.rule>, IDisposable
    {
        IList<data.rule> GetMyRule(string userId);
        void RemoveByUserId(string userId);
    }
}
