using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockMan.Jobs.Weixin
{
    public class TokenUpdateJob:IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            string appid = "wx5778f915f510ee21", secret = "f53d64c15e31343d6736be89cbba2c0d ";
            
            string url =string.Format( "https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid={0}&secret={1}",appid,secret);



        }
    }
}
