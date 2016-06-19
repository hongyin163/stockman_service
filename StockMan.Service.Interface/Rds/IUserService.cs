using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StockMan.EntityModel;

namespace StockMan.Service.Interface.Rds
{
    public interface IUserService : IEntityService<users>,IDisposable
    {
        sys_userconfig GetUserConfig(string userId);

        void SaveUserConfig(sys_userconfig config);

        long GetUserCount();
    }
}
