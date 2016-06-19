using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockMan.EntityModel;
using StockMan.EntityModel.dto;
namespace StockMan.Service.Interface.Rds
{
    public interface ICustomObjectService: IEntityService<customobject>,IPriceOperation, IDisposable
    {
        IList<PriceInfo> GetPriceInfo(string code, TechCycle type);
        void UpdateObjectInfo(IList<ObjectInfo> spiList);
        IList<customobject> GetObjectList(string categoryCode);
        //void AddPriceInfo(string objCode, PriceInfo info, TechCycle cycle);

        //void AddPriceByDay(IList<PriceInfo> price);
        //void AddPriceByWeek(IList<PriceInfo> price);
        //void AddPriceByMonth(IList<PriceInfo> price);

        IList<MyCycleObject> GetMyObject(string userId);

        void AddMyObject(List<object_user_map> objet_user);

        IList<customobject> GetDataByCode(string p1);
    }
}
