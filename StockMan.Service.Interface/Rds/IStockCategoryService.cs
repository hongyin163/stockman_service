using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockMan.EntityModel;

namespace StockMan.Service.Interface.Rds
{
    public interface IStockCategoryService : IEntityService<stockcategory>, IPriceOperation,IDisposable
    {
        void UpdateCategoryPrice(IList<PriceInfo> spiList);
        void AddCategoryGroup(stock_category_group group);
        IList<stockcategory> GetCategoryList(string group_code);

        IList<stockcategory> GetMyCategory(string user_id, string group_code);
        void AddMyCategory(stockcategory_user_map cate_user);
        void AddMyCategory(IList<stockcategory_user_map> cate_user);
        IList<stockcategory> GetCategoryByCode(string codes);
        IList<PriceInfo> GetPriceInfo(string code, TechCycle type);
        IList<PriceInfo> GetPriceInfo(string categoryCode, TechCycle type, DateTime date);
        void AddPriceInfo(string categoryCode, PriceInfo info, TechCycle cycle);

        //void AddPriceByDay(IList<PriceInfo> priceList);
        //void AddPriceByWeek(IList<PriceInfo> priceList);
        //void AddPriceByMonth(IList<PriceInfo> priceList);
    }
}
