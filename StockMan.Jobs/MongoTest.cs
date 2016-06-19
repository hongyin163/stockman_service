using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver.Linq;
using StockMan.Jobs.Stock;
using Quartz;
using StockMan.Service.Interface.Rds;
using StockMan.Service.Rds;
using StockMan.Service.Cache;
using Newtonsoft.Json;
using m = StockMan.Facade.Models;
namespace StockMan.Jobs
{
    public class MongoTest : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            LoggingExtensions.Logging.Log.InitializeWith<LoggingExtensions.log4net.Log4NetLog>();

            DateTime start = DateTime.Now;
            for (int i = 0; i < 100; i++)
            {
                GetStocks("0600000,0600015,0600016,0600036,0601009,0601166,0601169,0601288,0601328,0601398", false);
            }
            DateTime end = DateTime.Now;
            this.Log().Info("耗时:" + (end - start).TotalMilliseconds);

            start = DateTime.Now;
            for (int i = 0; i < 100; i++)
            {
                GetStocks("0600000,0600015,0600016,0600036,0601009,0601166,0601169,0601288,0601328,0601398", true);
            }
            end = DateTime.Now;
            this.Log().Info("耗时:" + (end - start).TotalMilliseconds);

        }
        private IStockService service = new StockService();
        private IUserDataVersionService versionService = new UserDataVersionService();
        // GET api/Stock
        public IEnumerable<m.Stock> GetStocks(string id, bool useCache)
        {
            if (useCache)
            {
                var results = CacheHelper.Get(id.Split(',').Select(s => "1_" + s).ToArray());

                return results
                    .Where(p => !string.IsNullOrEmpty(p))
                    .Select(p => JsonConvert.DeserializeObject<m.Stock>(p));
            }
            else
            {
                var list = service.GetStocksByIds(id);
                return list.Select(p => new m.Stock
                {
                    name = p.name,
                    code = p.code,
                    price = p.price,
                    yestclose = p.yestclose,
                    symbol = p.symbol,
                    volume = p.volume,
                    turnover = p.turnover,
                    high = p.high,
                    updown = p.updown,
                    low = p.low,
                    turnoverrate = p.turnoverrate,
                    pe = p.pe,
                    pb = p.pb,
                    fv = p.fv,
                    mv = p.mv,
                    percent = p.percent
                });
            }
        }
    }
}
