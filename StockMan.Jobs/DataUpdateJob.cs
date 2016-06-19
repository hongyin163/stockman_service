using Quartz;
using StockMan.Jobs.Category;
using StockMan.Jobs.Object;
using StockMan.Jobs.Stock;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockMan.Jobs
{
    public class DataUpdateJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            LoggingExtensions.Logging.Log.InitializeWith<LoggingExtensions.log4net.Log4NetLog>();

            this.Log().Info("开始更新大盘数据");
            var job2 = new ObjectDataUpdateJob();
            job2.Execute(null);
            this.Log().Info("结束更新大盘数据");

            this.Log().Info("开始更新个股数据");
            var job4 = new StockPriceUpdateJob();
            job4.Execute(null);
            this.Log().Info("结束更新个股数据");

            this.Log().Info("开始更新行业数据");
            var job6 = new CateIndexUpdateJob();
            job6.Execute(null);
            this.Log().Info("结束更新行业数据");
        }
    }
}
