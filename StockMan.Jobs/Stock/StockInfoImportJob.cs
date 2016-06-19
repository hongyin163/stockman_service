using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using Quartz.Job;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using data = StockMan.EntityModel;
using StockMan.Service.Interface.Rds;
using StockMan.Service.Rds;
using StockMan.Jobs.Stock;
using StockMan.Jobs.Stock.tencent;
using StockMan.EntityModel;
namespace StockMan.Jobs
{
    public class StockInfoImportJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            IStockCategoryService categoryService = new StockCategoryService();
            //导入行情分类的组
            IStockSync sync = new StockSync_Tencent();
            data.stock_category_group categoryGroup = sync.initCategoryGroup();
            categoryService.AddCategoryGroup(categoryGroup);

            //导入行情分类
            IList<data.stockcategory> cateList = sync.GetCategorys();
            foreach (data.stockcategory sc in cateList)
            {
                if (string.IsNullOrEmpty(sc.code) || string.IsNullOrEmpty(sc.name))
                    continue;
                if (categoryService.Find(sc.code) == null)
                {
                    categoryService.Add(sc);
                    this.Log().Info("导入行业:" + sc.name);

                }
            }
            //遍历类别，导入股票
            IStockService stockService = new StockService();
            IList<data.stockcategory> cateList2 = categoryService.GetCategoryList(categoryGroup.code);
            foreach (data.stockcategory cate in cateList2)
            {
                IList<data.stock> stockLIst = sync.GetStocks(cate);
                if (stockLIst == null || stockLIst.Count == 0)
                    continue;
                foreach (data.stock s in stockLIst)
                {
                    s.stock_category_map.Add(new stock_category_map
                    {
                        cate_code = cate.code,
                        stock_code = s.code,
                        stock_name = s.name
                    });
                    if (stockService.Find(s.code) == null)
                    {
                        stockService.Add(s);
                    }
                    else
                    {
                        stockService.Update(s);
                    }
                    this.Log().Info("导入股票:" + s.name);
                }
            }

            this.Log().Info("导入完成");
        }
    }
}
