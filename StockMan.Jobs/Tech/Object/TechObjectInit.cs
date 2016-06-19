using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using StockMan.EntityModel;
using StockMan.Service.Rds;
using StockMan.Jobs.Biz;

namespace StockMan.Jobs.Tech.Object
{
    public class TechObjectInit : IJob
    {
        public TechCycle _cycleType = TechCycle.day;
        public TechCycle CycleType
        {
            get { return this._cycleType; }
            set { this._cycleType = value; }
        }
        public void Execute(IJobExecutionContext context)
        {
            var cateService = new CustomObjectService();

            //var type = "Day"; //context.Trigger.JobDataMap["cycle"] + "";

            //if (string.IsNullOrEmpty(type))
            //    return;

            var cycleType = this.CycleType;// (TechCycle)Enum.Parse(typeof(TechCycle), type, true);

            IList<customobject> objectList = cateService.FindAll();

            foreach (var item in objectList)
            {
                var priceList = cateService.GetPriceInfo(item.code, cycleType);

                this.Log().Error("开始计算大盘技术指数：" + item.name + "，Code：" + item.code);
                var tech = new TechCalculate(ObjectType.Object, cycleType, item.code, priceList);

                tech.Run();
                this.Log().Error("开始计算：" + item.name + "，Code：" + item.code);
            }

        }
    }
}
