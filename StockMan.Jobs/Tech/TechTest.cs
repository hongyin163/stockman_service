using Quartz;
using StockMan.EntityModel;
using StockMan.Index;
using StockMan.Service.Rds;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockMan.Jobs.Tech
{
    public class TechTest : IJob
    {
        public string TechName { get; set; }
        public string CycleType { get; set; }
        public string ObjectCode { get; set; }
        public void Execute(IJobExecutionContext context)
        {
            LoggingExtensions.Logging.Log.InitializeWith<LoggingExtensions.log4net.Log4NetLog>();

            var cateService = new StockCategoryService();
            var stockService = new StockService();

            string type = this.CycleType;

            if (string.IsNullOrEmpty(type))
            {
                this.Log().Error("周期类型cycle参数为设置");
                return;
            }
            var cycleType = (TechCycle)Enum.Parse(typeof(TechCycle), type, true);

            string code = string.IsNullOrEmpty(this.ObjectCode) ? "0600015" : this.ObjectCode;
            IList<PriceInfo> priceList = stockService.GetPriceInfo(code, cycleType);

            this.Log().Error("开始计算股票技术指数");

            string algorithm_script = File.ReadAllText(string.Format(@"Script\{0}.js", this.TechName.Trim()));

            string contextNew = "";

            IndexCalculate.IsDebugg = true;

            //计算技术数据
            IList<IndexData> result = IndexCalculate.GetIndexData(algorithm_script, "{}", "", out contextNew, priceList);

            //技术形态
            var stateResult = IndexCalculate.GetState(algorithm_script, result);
            
            var tagReuslt = IndexCalculate.GetTag(algorithm_script, result);

            this.Log().Info("计算结果：" + this.TechName + ":" + stateResult);
            this.Log().Error("计算结束");
        }
    }
}
