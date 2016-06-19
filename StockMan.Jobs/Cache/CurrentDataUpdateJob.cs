using Newtonsoft.Json;
using Quartz;
using StockMan.EntityModel;
using StockMan.Jobs.Object.tencent;
using StockMan.Jobs.Stock;
using StockMan.Jobs.Stock.tencent;
using StockMan.MySqlAccess;
using StockMan.Service.Cache;
using StockMan.Service.Interface.Rds;
using StockMan.Service.Rds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using data = StockMan.EntityModel;

namespace StockMan.Jobs.Cache
{
    /// <summary>
    /// 更新大盘，个股，行业当前周期的数据，盘中执行
    /// </summary>
    public class CurrentDataUpdateJob : IJob
    {
        ICustomObjectService objService = new CustomObjectService();
        IStockCategoryService cateService = new StockCategoryService();
        IStockService stockService = new StockService();


        int batchNum = 40;

        private string _category = "-1";
        public string Category
        {
            get
            {
                return _category;
            }
            set
            {
                _category = value;
            }
        }
        public void Execute(IJobExecutionContext context)
        {
            LoggingExtensions.Logging.Log.InitializeWith<LoggingExtensions.log4net.Log4NetLog>();
            if (this.Category == "1" || this.Category == "-1")
            {
                UpdateStock();
            }
            if (this.Category == "2" || this.Category == "-1")
            {
                UpdateCategory();
            }
            if (this.Category == "3" || this.Category == "-1")
            {
                UpdateObject();
            }
        }

        private void UpdateCategory()
        {
            IList<data.stockcategory> list = cateService.GetCategoryList("tencent");

            //遍历列表，获取行业下的股票，计算指数，更新指数

            IList<data.PriceInfo> priceInfoList = new List<data.PriceInfo>();
            foreach (var item in list)
            {

                //IList<data.stock> stocks = stockService.GetStockByCategory(item.code);
                DateTime today = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                decimal yestclose = GetYestclose(item.code, today);

                //获取基期的值
                int i = -1;
                bool isOk = false;
                decimal baseValue = 0;
                while (!isOk)
                {
                    var priceList = stockService.GetStockPriceByDate(item.code, data.TechCycle.day, DateTime.Now.AddDays(i--));
                    if (priceList == null || priceList.Count == 0)
                        continue;
                    decimal total = 0, baseIndex = 100;
                    foreach (var priceInfo in priceList)
                    {
                        total += priceInfo.price ?? 1 * priceInfo.volume ?? 1;
                    }
                    baseValue = (total * 100) / yestclose;
                    isOk = true;
                }

                //index = (total / baseValue) * 100;


                var priceList0 = GetCurrentCategoryPrice(item.code);
                decimal vol = 0, turnover = 0;
                decimal total1 = 0;
                foreach (var priceInfo in priceList0)
                {
                    total1 += priceInfo.price ?? 1 * priceInfo.volume ?? 1;
                    vol += priceInfo.volume ?? 1;
                    turnover += priceInfo.turnover ?? 1;
                }

                decimal index = (total1 / baseValue) * 100;

                var info = new data.PriceInfo
                {
                    code = item.code,
                    date = today,
                    price = Math.Round(index, 2),
                    high = 0,
                    low = 0,
                    yestclose = yestclose,
                    volume = vol,
                    turnover = turnover
                };

                priceInfoList.Add(info);
                this.Log().Info(string.Format("行业：{0}，值：{1}", item.code, info.price));

            }

            foreach (var info in priceInfoList)
            {
                this.Log().Info("更新行业:" + string.Format("{0}_{1}", (int)ObjectType.Category, info.code) + ":" + info.code);

                UpdateRealTimePrice(ObjectType.Category, info);

                UpdateDayPrice(ObjectType.Category, info);

                //UpdatePrice(ObjectType.Category, TechCycle.Week, info);

                //UpdatePrice(ObjectType.Category, TechCycle.Month, info);
            }
            //cateService.UpdateCategoryPrice(priceInfoList);
            //cateService.AddPriceByDay<data.data_category_day_latest>(priceInfoList);
            //cateService.AddPriceByWeek<data.data_category_week_latest>(priceInfoList);
            //cateService.AddPriceByMonth<data.data_category_month_latest>(priceInfoList);
            this.Log().Info("计算结束");
        }

        private IList<PriceInfo> GetCurrentCategoryPrice(string cateCode)
        {
            IList<PriceInfo> priceList = new List<PriceInfo>();
            IList<data.stock> stockList = stockService.GetStockByCategory(cateCode);

            foreach (var stock in stockList)
            {
                string pstr = CacheHelper.Get(string.Format("{0}_{1}", (int)ObjectType.Stock, stock.code));
                if (!string.IsNullOrEmpty(pstr))
                {
                    PriceInfo p = JsonConvert.DeserializeObject<PriceInfo>(pstr);
                    priceList.Add(p);
                }
            }
            return priceList;
        }
        private decimal GetYestclose(string p, DateTime today)
        {
            int i = -1;
            var list = cateService.GetPriceInfo(p, data.TechCycle.day, today.AddDays(i));
            while (list == null || list.Count == 0)
            {

                list = cateService.GetPriceInfo(p, data.TechCycle.day, today.AddDays(--i));

                if (list != null && list.Count > 0)
                {
                    return list[0].price ?? 0;
                }

                if (i > 10)
                    return 0;
            }
            return list[0].price ?? 1;
        }
        private void UpdateStock()
        {
            IList<data.stockcategory> cateList = cateService.GetCategoryList("tencent").ToList();
            IStockSync sync = new StockSync_Tencent();

            int total = 0;
            foreach (var category in cateList)
            {
                this.Log().Info("开始导入行业:" + category.code + "-" + category.name);
                IList<data.stock> stockList = stockService.GetStockByCategory(category.code);

                for (int i = 0; i < stockList.Count; i += batchNum)
                {
                    IList<data.StockInfo> spList = sync.GetPrice(stockList.Skip(i).Take(batchNum).ToList());


                    var stolist = spList.Select(p => new StockMan.Facade.Models.Stock
                    {
                        name = p.name,
                        code = p.stock_code,
                        price = p.price,
                        open = p.open,
                        yestclose = p.yestclose,
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
                        percent = p.percent,
                        date = p.date.ToString("yyyy-MM-dd")
                    });
                    foreach (var s in stolist)
                    {
                        this.Log().Info("更新股价:" + string.Format("{0}_{1}", (int)ObjectType.Stock, s.code) + ":" + s.name);

                        CacheHelper.Set(string.Format("{0}_{1}", (int)ObjectType.Stock, s.code), JsonConvert.SerializeObject(s));
                        UpdateDayPrice(ObjectType.Stock, new data.PriceInfo
                        {
                            code = s.code,
                            date = DateTime.Parse(s.date),
                            high = s.high,
                            low = s.low,
                            open = s.open,
                            percent = s.percent,
                            price = s.price,
                            updown = s.updown,
                            volume = s.volume,
                            yestclose = s.yestclose,
                            turnover = s.turnover
                        });
                    }

                }

            }
        }

        private void UpdateObject()
        {
            var objList = objService.FindAll();
            var sync = new OjbectSync_Tencent();
            var objInfoList = sync.GetPrice(objList);

            IList<data.PriceInfo> priceList = objInfoList.Select(p => new data.PriceInfo
            {
                code = p.object_code,
                date = p.date,
                high = p.high,
                low = p.low,
                open = p.open,
                percent = p.percent,
                price = p.price,
                updown = p.updown,
                volume = p.volume,
                yestclose = p.yestclose,
                turnover = p.turnover
            }).ToList();

            foreach (var info in priceList)
            {
                this.Log().Info("更新大盘:" + string.Format("{0}_{1}", (int)ObjectType.Object, info.code) + ":" + info.code);

                UpdateRealTimePrice(ObjectType.Object, info);
                UpdateDayPrice(ObjectType.Object, info);

                //UpdatePrice(ObjectType.Object, TechCycle.Week,info);

                //UpdatePrice(ObjectType.Object, TechCycle.Month,info);
            }

            //objService.UpdateObjectInfo(objInfoList);
            //objService.AddPriceByDay<data.data_object_day_latest>(priceList);
            //objService.AddPriceByWeek<data.data_object_week_latest>(priceList);
            //objService.AddPriceByMonth<data.data_object_month_latest>(priceList);
        }

        private void UpdateDayPrice(ObjectType objType, PriceInfo info)
        {

            CacheHelper.Set(string.Format("{0}_{1}_{2}_current", (int)objType, info.code, TechCycle.day.ToString().ToLower()), JsonConvert.SerializeObject(info));
        }

        private void UpdateRealTimePrice(ObjectType objType, PriceInfo info)
        {
            CacheHelper.Set(string.Format("{0}_{1}", (int)objType, info.code), JsonConvert.SerializeObject(info));

        }
    }
}
