using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using data = StockMan.EntityModel;
namespace StockMan.Jobs.Object
{
    public interface IObjectSync
    {
        IList<data.customobject> GetAllObjects();

        IList<data.ObjectInfo> GetPrice(IList<data.customobject> objList);


        IList<data.PriceInfo> GetPriceByDay(data.customobject obj);

        IList<data.PriceInfo> GetPriceByWeek(data.customobject obj);

        IList<data.PriceInfo> GetPriceByMonth(data.customobject obj);
    }
}
