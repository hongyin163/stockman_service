using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using data = StockMan.EntityModel;

namespace StockMan.Jobs.Stock
{
    public interface IStockSync
    {
        data.stock_category_group initCategoryGroup();

        IList<data.stockcategory> GetCategorys();

        IList<data.stock> GetStocks(data.stockcategory category);

        IList<data.StockInfo> GetPrice(IList<data.stock> stock);
        /// <summary>
        /// 废弃
        /// </summary>
        /// <param name="stock"></param>
        /// <returns></returns>
        IList<data.PriceInfo> GetPriceByDay(data.stock stock);
        /// <summary>
        /// 废弃
        /// </summary>
        /// <param name="stock"></param>
        /// <returns></returns>
        IList<data.PriceInfo> GetPriceByWeek(data.stock stock);
        /// <summary>
        /// 废弃
        /// </summary>
        /// <param name="stock"></param>
        /// <returns></returns>
        IList<data.PriceInfo> GetPriceByMonth(data.stock stock);

        IList<data.PriceInfo> GetPriceByDay(string code);
        IList<data.PriceInfo> GetPriceByWeek(string code);
        IList<data.PriceInfo> GetPriceByMonth(string code);
    }
}
