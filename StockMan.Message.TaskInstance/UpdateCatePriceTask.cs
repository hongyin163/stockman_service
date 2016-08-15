using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockMan.Message.Task.Interface;
using data = StockMan.EntityModel;
using StockMan.Service.Interface.Rds;
using StockMan.Service.Rds;
using Newtonsoft.Json;

namespace StockMan.Message.TaskInstance
{
    /// <summary>
    /// 更新行业
    /// </summary>
    public class UpdateCatePriceTask : Message.Task.ITask
    {
        IStockCategoryService cateService = new StockCategoryService();
        IStockService stockService = new StockService();
        public string GetCode()
        {
            return "T0004";
        }

        public void Send(IMessageSender sender)
        {
            this.Log().Info("消息发送开始");
            //获取行业列表
            IList<data.stockcategory> list = cateService.GetCategoryList("tencent");
            foreach (var item in list)
            {
                var task1 = new CatePriceUpdate
                {
                    Code = item.code
                };
                sender.Send(JsonConvert.SerializeObject(task1));
            }

            this.Log().Info("消息发送开始");
        }


        public void Excute(string message)
        {
            this.Log().Info("计算开始");

            var msg = JsonConvert.DeserializeObject<CatePriceUpdate>(message);

            IList<data.PriceInfo> priceInfoList = GetPriceList(msg.Code);

            cateService.UpdateCategoryPrice(priceInfoList);
            cateService.AddPriceByDay<data.data_category_day_latest>(priceInfoList);
            cateService.AddPriceByWeek<data.data_category_week_latest>(priceInfoList);
            cateService.AddPriceByMonth<data.data_category_month_latest>(priceInfoList);

            this.Log().Info("计算结束");
        }

        private IList<data.PriceInfo> GetPriceList(string code)
        {
            IList<data.PriceInfo> priceInfoList = new List<data.PriceInfo>();

            DateTime today = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            decimal yestclose = GetYestclose(code, today);

            //获取基期的值
            int i = -1;
            bool isOk = false;
            decimal baseValue = 0;
            while (!isOk)
            {
                var priceList = stockService.GetStockPriceByDate(code, data.TechCycle.day, DateTime.Now.AddDays(i--));
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


            var priceList0 = stockService.GetStockPriceByDate(code, data.TechCycle.day, today);
            decimal vol = 0, turnover = 0;
            decimal total1 = 0;
            foreach (var priceInfo in priceList0)
            {
                total1 += priceInfo.price ?? 1 * priceInfo.volume ?? 1;
                vol += priceInfo.volume ?? 1;
                turnover += priceInfo.turnover ?? 1;
            }

            decimal index = (total1 / baseValue) * 100;

            var percent = (index - yestclose) * 100 / yestclose;
            var info = new data.PriceInfo
            {
                code = code,
                date = today,
                price = Math.Round(index, 2),
                high = 0,
                low = 0,
                yestclose = yestclose,
                volume = vol,
                turnover = turnover,
                percent = percent,
                open = Math.Round(index, 2),
            };

            priceInfoList.Add(info);
            this.Log().Info(string.Format("行业：{0}，值：{1}", code, info.price));
            return priceInfoList;
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
            }
            return list[0].price ?? 1;
        }


    }
    public class CatePriceUpdate
    {
        public string Code { get; set; }
    }
}
