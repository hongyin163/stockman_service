using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using data = StockMan.EntityModel;

namespace StockMan.Jobs.Stock
{
    interface IStockSync
    {
        data.stock_category_group initCategoryGroup();

        IList<data.stockcategory> GetCategorys();

        IList<data.stock> GetStocks(data.stockcategory category);

        IList<data.StockInfo> GetPrice(IList<data.stock> stock);

        IList<data.PriceInfo> GetPriceByDay(data.stock stock);
        IList<data.PriceInfo> GetPriceByWeek(data.stock stock);
        IList<data.PriceInfo> GetPriceByMonth(data.stock stock);
    }
}
