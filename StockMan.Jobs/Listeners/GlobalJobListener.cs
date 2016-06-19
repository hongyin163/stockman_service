using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quartz;

namespace StockMan.Jobs.Listeners
{
    public class GlobalJobListener : IJobListener
    {
        public void JobExecutionVetoed(IJobExecutionContext context)
        {
            this.Log().Info("否决：" + context.JobDetail.Description);
        }

        public void JobToBeExecuted(IJobExecutionContext context)
        {
            this.Log().Info("将要执行：" + context.JobDetail.Description);
        }

        public void JobWasExecuted(IJobExecutionContext context, JobExecutionException jobException)
        {
            this.Log().Info("执行完成：" + context.JobDetail.Description);
        }

        public string Name
        {
            get { return "GlobalJobListener"; }
        }
    }
}