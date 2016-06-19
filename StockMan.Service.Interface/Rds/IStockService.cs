using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockMan.EntityModel;
using StockMan.EntityModel.dto;
using System.Data;

namespace StockMan.Service.Interface.Rds
{
    public interface IStockService : IEntityService<stock>, IPriceOperation, IDisposable
    {
        /// <summary>
        /// 同步自选股
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="stocks"></param>
        /// <returns></returns>
        bool SyncMyStock(string userId, IList<stock_user_map> stockCodes);
        /// <summary>
        /// 获取我的自选股
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        IList<stock_user_map> GetMyStock(string userId);

        IList<stock> GetStocksByIds(string ids);

        IList<stock> GetStockByCategory(string cateCode);
        IList<stock> GetStockByCategory(string cateCode, int pageNum, int pageSize);

        decimal GetMyStockVersion(string user_id);

        void UpdateMyStockVersion(string user_id, decimal version);

        void UpdateStockPrice(string code, StockInfo price);
        void UpdateStockPrice(IList<StockInfo> spiList);
        //void AddPriceByDay(IList<PriceInfo> price);

        //void AddPriceByWeek(IList<PriceInfo> price);

        //void AddPriceByMonth(IList<PriceInfo> price);

        void AddPriceSyncLog(pricesynclog log);

        IList<pricesynclog> GetPriceSyncLog(Guid batch, int state);

        void UpdatePriceSyncLog(Guid batch, string stock_code, int state);

        IList<PriceInfo> GetStockPriceDayInfo(stock data);
        IList<PriceInfo> GetStockPriceWeekInfo(stock data);
        IList<PriceInfo> GetStockPriceMonthInfo(stock data);


        IList<PriceInfo> GetStockPriceByDate(string categoryCode, TechCycle cycle, DateTime datetime);

        IList<PriceInfo> GetPriceInfo(string code, TechCycle cycle);

        IList<StockQueryResult> FindStockBy(StockQueryCondition condition);
        IList<StockQueryResult> FindStockFromPoolBy(StockQueryCondition condition);
        IList<StockQueryCount> FindStockCountFromRankPoolBy(StockQueryCondition condition);
        IList<StockQueryResult> FindStockFromRankPoolBy(StockQueryCondition condition);

        IList<StockCrossQueryResult> FindCrossStockBy(StockQueryCondition condition);
        IList<StockQueryCount> FindCrossStockCountBy(StockQueryCondition condition);

        IList<StockCrossQueryResult> FindStateStockBy(StockQueryCondition condition);
        IList<StockQueryCount> FindStateStockCountBy(StockQueryCondition condition);

        void AddStockToCategory(string stockCode, string categoryCode);

        DataTable GetStockGroup(string[] ids);

        string GetCodeRandom();
    }
}
