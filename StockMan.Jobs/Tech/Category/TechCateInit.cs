using System;
using System.Collections.Generic;
using System.Linq;
using Quartz;
using StockMan.EntityModel;
using StockMan.Jobs.Tech.Stock;
using StockMan.Service.Rds;
using StockMan.Jobs.Biz;

namespace StockMan.Jobs.Tech.Category
{
    public class TechCateInit : IJob
    {
        public TechCycle _cycleType = TechCycle.day;
        public TechCycle CycleType
        {
            get { return this._cycleType; }
            set { this._cycleType = value; }
        }
        public void Execute(IJobExecutionContext context)
        {
            var cateService = new StockCategoryService();



            var cycleType = this.CycleType;

            IList<stockcategory> cateList = cateService.GetCategoryList("tencent");

            foreach (var category in cateList)
            {
                var priceList = cateService.GetPriceInfo(category.code, cycleType);
                this.Log().Error("开始计算行业技术指数：" + category.name + "，Code：" + category.code);
                var tech = new TechCalculate(ObjectType.Category, cycleType, category.code, priceList);

                tech.Run();
                this.Log().Error("开始计算：" + category.name + "，Code：" + category.code);
            }

        }
    }
}
