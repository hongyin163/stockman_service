using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using data = StockMan.EntityModel;
namespace StockMan.Service.Interface.Rds
{
    public interface IGoodIdeaService : IEntityService<data.sys_goodidea>, IDisposable
    {
    }
}
