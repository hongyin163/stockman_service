using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetMQ;
using Newtonsoft.Json;
using data = StockMan.EntityModel;
using StockMan.Jobs.Biz.Model;
using NetMQ.zmq;
using StockMan.Service.Rds;
using StockMan.EntityModel;
using StockMan.Service.Interface.Rds;
using System.Configuration;
using System.Threading;
using Quartz;
using StockMan.MySqlAccess;

namespace StockMan.Jobs.Tech
{
    public class TechMainJob : IJob
    {
        IStockCategoryService cateService = new StockCategoryService();
        IStockService stockService = new StockService();
        ICustomObjectService objService = new CustomObjectService();

        public string _stateType = "-1";
        public string StateType
        {
            get { return _stateType; }
            set { _stateType = value; }
        }
        public string _categoryCode = "-1";
        public string CategoryCode
        {
            get { return _categoryCode; }
            set { _categoryCode = value; }
        }



        public int MaxTaskNum = 3;
        public Queue<IndexTask> TaskQueue { get; set; }

        public IList<Thread> ThreadList { get; set; }

        public void Execute(IJobExecutionContext context)
        {

            LoggingExtensions.Logging.Log.InitializeWith<LoggingExtensions.log4net.Log4NetLog>();

            this.Log().Info("开始计算");

            Init();


            this.TaskQueue = new Queue<IndexTask>();


            if (context != null && context.Trigger.JobDataMap.ContainsKey("MaxTaskNum"))
            {
                var num = context.Trigger.JobDataMap["MaxTaskNum"] + "";
                if (!string.IsNullOrEmpty(num))
                {
                    this.MaxTaskNum = int.Parse(num);
                }
            }

            if (this.StateType == "-1" || this.StateType == "1")
            {
                ComputeStockState();
            }
            if (this.StateType == "-1" || this.StateType == "2")
            {
                ComputeCategoryState();
            }
            if (this.StateType == "-1" || this.StateType == "3")
            {
                ComputeMarketState();
            }

            ThreadList = new List<Thread>();
            for (int i = 0; i < this.MaxTaskNum; i++)
            {
                Thread th = new Thread(new ThreadStart(TechTask));
                th.Start();
                ThreadList.Add(th);

            }
            

            foreach (Thread t in ThreadList)
            {
                t.Join();
            }

            this.Log().Info("技术数据更新和状态分析任务结束。");
        }


        private void ComputeMarketState()
        {
            var objList = objService.FindAll();
            this.Log().Info("开始计算大盘形态");
            foreach (var obj in objList)
            {
                this.Log().Info("计算大盘：" + obj.name);


                //Send(ObjectType.Object, obj.code, TechCycle.Day);
                //Send(ObjectType.Object, obj.code, TechCycle.Week);
                //Send(ObjectType.Object, obj.code, TechCycle.Month);

                this.TaskQueue.Enqueue(new IndexTask
                {
                    type = ObjectType.Object,
                    code = obj.code,
                    cycle = TechCycle.day
                });
                this.TaskQueue.Enqueue(new IndexTask
                {
                    type = ObjectType.Object,
                    code = obj.code,
                    cycle = TechCycle.week
                });
                this.TaskQueue.Enqueue(new IndexTask
                {
                    type = ObjectType.Object,
                    code = obj.code,
                    cycle = TechCycle.month
                });
            }


            //Parallel.ForEach<customobject>(objList, (obj) =>
            //{
            //    this.Log().Info("计算大盘：" + obj.name);
            //    Send(ObjectType.Object, obj.code, TechCycle.Day);
            //    Send(ObjectType.Object, obj.code, TechCycle.Week);
            //    Send(ObjectType.Object, obj.code, TechCycle.Month);
            //});



        }

        private void ComputeCategoryState()
        {
            IList<stockcategory> cateList = cateService.FindAll();
            this.Log().Info("开始计算行业形态");
            //foreach (var category in cateList)
            //{
            //    this.Log().Info("计算行业：" + category.name);
            //    Send(ObjectType.Category, category.code, TechCycle.Day);
            //    Send(ObjectType.Category, category.code, TechCycle.Week);
            //    Send(ObjectType.Category, category.code, TechCycle.Month);
            //}
            Parallel.ForEach<stockcategory>(cateList, (category) =>
            {
                this.Log().Info("计算行业：" + category.name);
                //Send(ObjectType.Category, category.code, TechCycle.Day);
                //Send(ObjectType.Category, category.code, TechCycle.Week);
                //Send(ObjectType.Category, category.code, TechCycle.Month);
                this.TaskQueue.Enqueue(new IndexTask
                {
                    type = ObjectType.Category,
                    code = category.code,
                    cycle = TechCycle.day
                });
                this.TaskQueue.Enqueue(new IndexTask
                {
                    type = ObjectType.Category,
                    code = category.code,
                    cycle = TechCycle.week
                });
                this.TaskQueue.Enqueue(new IndexTask
                {
                    type = ObjectType.Category,
                    code = category.code,
                    cycle = TechCycle.month
                });
            });
        }

        private IList<stockcategory> ComputeStockState()
        {
            IList<stockcategory> cateList = cateService.FindAll();
            if (string.IsNullOrEmpty(this.CategoryCode) || this.CategoryCode == "-1")
            {
                cateList = cateService.FindAll();
            }
            else
            {
                string[] cates = this.CategoryCode.Split(',');
                cateList = cateService.FindAll().Where(p => cates.Contains(p.code)).ToList();
            }
            this.Log().Info("开始计算个股形态");
            foreach (var category in cateList)
            {
                IList<stock> stockList = stockService.GetStockByCategory(category.code);

                foreach (var stock in stockList)
                {

                    this.Log().Info("计算股票：" + stock.name);
                    //Send(ObjectType.Stock, stock.code, TechCycle.Day);
                    //Send(ObjectType.Stock, stock.code, TechCycle.Week);
                    //Send(ObjectType.Stock, stock.code, TechCycle.Month);

                    this.TaskQueue.Enqueue(new IndexTask
                    {
                        type = ObjectType.Stock,
                        code = stock.code,
                        cycle = TechCycle.day
                    });
                    this.TaskQueue.Enqueue(new IndexTask
                    {
                        type = ObjectType.Stock,
                        code = stock.code,
                        cycle = TechCycle.week
                    });
                    this.TaskQueue.Enqueue(new IndexTask
                    {
                        type = ObjectType.Stock,
                        code = stock.code,
                        cycle = TechCycle.month
                    });
                }

                //Parallel.ForEach<stock>(stockList, (stock) =>
                //{
                //    this.Log().Info("计算股票：" + stock.name);
                //    Send(ObjectType.Stock, stock.code, TechCycle.Day);
                //    Send(ObjectType.Stock, stock.code, TechCycle.Week);
                //    Send(ObjectType.Stock, stock.code, TechCycle.Month);
                //});


            }
            return cateList;



        }
        /// <summary>
        /// 技术状态计算之前的初始化工作
        /// </summary>
        private void Init()
        {
            using (StockManDBEntities entity = new StockManDBEntities())
            {
                string sql = "DELETE FROM `object_tag_map`;";
                entity.Database.ExecuteSqlCommand(sql);            
            }
        }

        public void TechTask()
        {

            using (NetMQContext _context = NetMQContext.Create())
            {
                using (NetMQSocket _socket = _context.CreateSocket(ZmqSocketType.Req))
                {
                    _socket.Connect(ConfigurationManager.AppSettings["broker"]);
                    while (true)
                    {
                        IndexTask task = null;
                        lock (this.TaskQueue)
                        {
                            if (this.TaskQueue.Count > 0)
                            {
                                task = this.TaskQueue.Dequeue();
                                this.Log().Info("remain:" + this.TaskQueue.Count);
                            }
                        }

                        if (task == null)
                        {
                            Thread.Sleep(5000);
                            break;
                        }
                        string msg = JsonConvert.SerializeObject(task);
                        byte[] msgBytes = Encoding.UTF8.GetBytes(msg);
                        _socket.Send(msg);
                        this.Log().Info("send:{" + msg + "}");
                        string message = _socket.ReceiveString();
                        this.Log().Info("rcv:" + message);
                    }
                }
            }

        }


    }


}
