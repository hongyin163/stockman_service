using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockMan.Jobs
{
    public class JobTest:IJob
    {
        public void Execute(IJobExecutionContext context)
        {

            this.Log().Info("Job启动测试");
        }
    }
}
