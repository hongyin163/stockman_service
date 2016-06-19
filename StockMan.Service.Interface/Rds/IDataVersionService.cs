using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockMan.EntityModel;

namespace StockMan.Service.Interface.Rds
{
    public interface IUserDataVersionService : IEntityService<userdataversion>, IDisposable
    {
        decimal GetUserDataVersion(string user_id, string version_code);
        void UpdateUserDataVersion(string user_id, string version_code,decimal version);
    }
}
