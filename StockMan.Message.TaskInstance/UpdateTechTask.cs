using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using StockMan.Message.Model;
using StockMan.Message.Task;
using Newtonsoft.Json;
using StockMan.Jobs.Biz.Model;
using StockMan.Service.Interface.Rds;
using StockMan.Service.Rds;
using StockMan.Jobs.Tech;
using StockMan.EntityModel;

namespace StockMan.Message.TaskInstance
{
    public class UpdateTechTask : ITask
    {
        private int totol = 9999;
        IStockCategoryService cateService = new StockCategoryService();
        IStockService stockService = new StockService();
        ICustomObjectService objService = new CustomObjectService();
        public void Send(Task.Interface.IMessageSender sender)
        {
            try
            {
                ComputeStockState(sender);
            }
            catch (Exception ex)
            {
                this.Log().Info(ex.Message);
            }
        }

        private IList<TaskMessage> ComputeStockState(Task.Interface.IMessageSender sender)
        {
            this.Log().Info("开始计算个股形态");
            IList<stockcategory> cateList = cateService.FindAll();


            IList<TaskMessage> msgList = new List<TaskMessage>();
            TechCycle[] cycleList = new TechCycle[] { TechCycle.day, TechCycle.week, TechCycle.month };
            //foreach (var category in cateList)
            //{
            //    IList<stock> stockList = stockService.GetStockByCategory(category.code);

            //    foreach (var stock in stockList)
            //    {
            //        this.Log().Info("计算股票：" + stock.name);

            //        foreach (var cycle in cycleList)
            //        {
            //            var task1 = new IndexTask
            //            {
            //                type = ObjectType.Stock,
            //                code = stock.code,
            //                cycle = cycle
            //            };
            //            sender.Send(JsonConvert.SerializeObject(task1));
            //        }
            //    }
            //}

            string[] list = new string[] { "0600265" };
            foreach (var code in list)
            {
                foreach (var cycle in cycleList)
                {
                    var task1 = new IndexTask
                    {
                        type = ObjectType.Stock,
                        code = code,
                        cycle = cycle
                    };
                    sender.Send(JsonConvert.SerializeObject(task1));
                }
            }

            return msgList;
        }

        public void Excute(string message)
        {
            this.Log().Info("处理消息");
            var taskMesg = JsonConvert.DeserializeObject<IndexTask>(message);

            var objType = taskMesg.type;
            var objCode = taskMesg.code;
            var cycleType = taskMesg.cycle;// (TechCycle)Enum.Parse(typeof(TechCycle), taskMesg.cycle, true);
            //var date = taskMesg.date;
            IIndexService indexService = new IndexService();
            //获取数据
            var priceList = indexService.GetObjectData(objType.ToString(), cycleType, objCode);

            //计算指数结果                    
            //存储指数到MangoDB
            //更新状态数据到MangoDB
            if (priceList.Count > 0)
            {
                var tech = new TechCalculate(objType, cycleType, objCode, priceList);
                tech.Run();
            }
            this.Log().Info("处理完成");
        }

        public string GetCode()
        {
            return "T0001";
        }


    }
}
