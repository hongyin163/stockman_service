using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz.Impl;
using Quartz.Server;
using StockMan.Jobs.Category;
using StockMan.Jobs.Object;
using StockMan.Jobs.State;
using StockMan.Jobs.Stock;
using StockMan.Jobs.Tech;
using StockMan.Jobs.Tech.Stock;
using Topshelf;
using Quartz;
using StockMan.Service.Rds;
using StockMan.Jobs.Recommend;
using StockMan.Jobs.Tech.Object;
using StockMan.EntityModel;
using StockMan.Jobs.Tech.Category;
using StockMan.Jobs.Cache;
using StockMan.Service.Interface.Rds;
using StockMan.Service.Cache;

namespace StockMan.Jobs
{
    class Program
    {
        static void Main(string[] args)
        {

            LoggingExtensions.Logging.Log.InitializeWith<LoggingExtensions.log4net.Log4NetLog>();
            string comStr =
             "0：初始化大盘\r\n" +
             "1：初始化行业和个股\r\n" +
             "2：更新大盘数据\r\n" +
             "3：初始导入个股价格数据\r\n" +
             "4：更新个股价格数据\r\n" +
             "5：初始计算行业数据\r\n" +
             "6：更新行业数据\r\n" +
             "7：行业个股筛选\r\n" +
             "8：用户个股推荐\r\n" +
             "9：发送推荐和状态消息\r\n" +
             "10，11，12：大盘，个股，行业技术数据状态计算，单线程。\r\n" +
             "13：大盘，个股，行业数据更新\r\n" +
             "20：技术数据计算，主控服务\r\n" +
             "21：更新到缓存数据，把当前价格写入历史数据，收盘执行\r\n" +
             "22：更新缓存的股票价格，盘中执行\r\n" +
             "23：个股全指标排名推荐\r\n" +
             "30：状态算法测试\r\n" +
             "31：导入个股价格数据\r\n" +
             "32：挂接个股到行业\r\n" +
             "98：清空技术指标数据\r\n" +
             "99：测试\r\n" +
             "-1:清空数据表\r\n" +
             "exit：退出\r\n";

            bool run = true;
            while (run)
            {
                Console.Write(comStr);

                var p = Console.ReadLine();
                var startTime = DateTime.Now;
                switch (p)
                {
                    case "0":
                        var job0 = new ObjectDataInitJob();
                        job0.Execute(null);
                        break;
                    case "1":
                        var job1 = new StockInfoImportJob();
                        job1.Execute(null);
                        break;
                    case "2":
                        var job2 = new ObjectDataUpdateJob();
                        Console.WriteLine("输入开始日期：0表示今天，-1表示昨天……或者输入：yyyy-MM-dd");
                        job2.StartDate = GetStartDate(Console.ReadLine());
                        job2.Execute(null);
                        break;
                    case "3":
                        var job3 = new StockPriceImportJob();
                        Console.WriteLine("输入行业编码：-1：全部");
                        job3.CategoryCode = Console.ReadLine();
                        job3.Execute(null);
                        break;
                    case "4":
                        var job4 = new StockPriceUpdateJob();
                        Console.WriteLine("输入行业编码：-1：全部");
                        job4.CategoryCode = Console.ReadLine();
                        Console.WriteLine("输入开始日期：0表示今天，-1表示昨天……或者输入：yyyy-MM-dd");
                        job4.StartDate = GetStartDate(Console.ReadLine());
                        job4.Execute(null);
                        break;
                    case "5":
                        var job5 = new CateIndexInitJob();
                        Console.WriteLine("输入行业编码：-1：全部");
                        job5.CategoryCode = Console.ReadLine();
                        job5.Execute(null);
                        break;
                    case "6":
                        var job6 = new CateIndexUpdateJob();
                        Console.WriteLine("输入行业编码：-1：全部");
                        job6.CategoryCode = Console.ReadLine();
                        job6.Execute(null);
                        break;
                    case "7":
                        var job7 = new CategoryRecoJob();
                        Console.WriteLine("输入行业编码：-1：全部");
                        job7.CategoryCode = Console.ReadLine();
                        job7.Execute(null);
                        break;
                    case "8":
                        var job8 = new PersonalRecoJob();
                        job8.Execute(null);

                        break;
                    case "9":
                        var job9 = new SendNotificationJob();
                        job9.Execute(null);
                        break;
                    case "10":
                        Console.WriteLine("大盘数据和状态计算开始");
                        var job10 = new TechObjectInit();
                        job10.CycleType = TechCycle.day;
                        job10.Execute(null);
                        Console.WriteLine("日线完成");
                        job10.CycleType = TechCycle.week;
                        job10.Execute(null);
                        Console.WriteLine("周线完成");
                        job10.CycleType = TechCycle.month;
                        job10.Execute(null);
                        Console.WriteLine("月线完成");
                        break;
                    case "11":
                        Console.WriteLine("个股数据和状态计算开始");
                        var job11 = new TechStockInit();
                        job11.CycleType = TechCycle.day;
                        job11.Execute(null);
                        Console.WriteLine("日线完成");
                        job11.CycleType = TechCycle.week;
                        job11.Execute(null);
                        Console.WriteLine("周线完成");
                        job11.CycleType = TechCycle.month;
                        job11.Execute(null);
                        Console.WriteLine("月线完成");
                        break;
                    case "12":
                        Console.WriteLine("行业数据和状态计算开始");
                        var job12 = new TechCateInit();
                        job12.CycleType = TechCycle.day;
                        job12.Execute(null);
                        Console.WriteLine("日线完成");
                        job12.CycleType = TechCycle.week;
                        job12.Execute(null);
                        Console.WriteLine("周线完成");
                        job12.CycleType = TechCycle.month;
                        job12.Execute(null);
                        Console.WriteLine("月线完成");
                        break;
                    case "13":
                        Console.WriteLine("大盘，个股，行业数据更新");
                        var job13 = new DataUpdateJob();
                        job13.Execute(null);
                        Console.WriteLine("更新完成");
                        break;
                    case "20":
                        Console.WriteLine("大盘数据和状态计算开始");
                        var job20 = new TechMainJob();
                        Console.WriteLine("输入计算的类别编码：1：个股，2：行业，3：大盘，-1：全部");
                        var cate = Console.ReadLine();
                        job20.StateType = cate;
                        if (cate == "1")
                        {
                            Console.WriteLine("输入行业编码：-1：全部");
                            job20.CategoryCode = Console.ReadLine();
                        }
                        Console.WriteLine("输入并行任务数");
                        int num0 = 1;
                        int.TryParse(Console.ReadLine(), out num0);
                        job20.MaxTaskNum = num0;
                        job20.Execute(null);
                        break;
                    case "21":
                        Console.WriteLine("更新到缓存数据，把当前价格写入历史数据，收盘执行。");
                        var job21 = new LastDataUpdateJob();
                        Console.WriteLine("输入更新缓存的类别编码：1：个股，2：行业，3：大盘，-1：全部");
                        job21.Category = Console.ReadLine();
                        job21.Execute(null);
                        break;
                    case "22":
                        Console.WriteLine("更新缓存价格数据。包括大盘，个股，行业");
                        var job22 = new CurrentDataUpdateJob();
                        Console.WriteLine("输入更新缓存的类别编码：1：个股，2：行业，3：大盘，-1：全部");
                        job22.Category = Console.ReadLine();
                        job22.Execute(null);
                        break;

                    case "23":
                        Console.WriteLine("个股全指标排名推荐");
                        var job23 = new RankRecoJob();
                        Console.WriteLine("输入行业编码：-1：全部");
                        job23.CategoryCode = Console.ReadLine();
                        job23.Execute(null);
                        break;
                    case "30":
                        var job30 = new TechTest();
                        Console.WriteLine("输入技术和周期，格式：macd,day,code");
                        string[] ps = Console.ReadLine().Split(',');
                        if (ps.Length == 1)
                        {
                            job30.TechName = ps[0];
                            job30.CycleType = "day";
                        }
                        else if (ps.Length == 2)
                        {
                            job30.TechName = ps[0];
                            job30.CycleType = ps[1];
                        }
                        else if (ps.Length == 3)
                        {
                            job30.TechName = ps[0];
                            job30.CycleType = ps[1];
                            job30.ObjectCode = ps[2];
                        }
                        else
                        {
                            continue;
                        }
                        job30.Execute(null);
                        break;
                    case "31":
                        var job31 = new StockPriceMaintanceJob();
                        Console.WriteLine("输入股票编码");
                        job31.StockCode = Console.ReadLine();
                        job31.Execute(null);
                        break;
                    case "32":
                        IStockService service = new StockService();
                        Console.WriteLine("输入股票编码");
                        var stockCode = Console.ReadLine();
                        if (service.Find(stockCode) == null)
                        {
                            Console.WriteLine("输入股票名称");
                            var stockName = Console.ReadLine();
                            service.Add(new stock
                            {
                                code = stockCode,
                                name = stockName,
                                symbol = ""
                            });
                        }
                        Console.WriteLine("输入行业编码");
                        var cateCode = Console.ReadLine();
                        service.AddStockToCategory(stockCode, cateCode);

                        var job32 = new StockPriceMaintanceJob();
                        job32.StockCode = stockCode;
                        job32.Execute(null);

                        break;
                    case "33":
                        Console.WriteLine("输入缓存Key");
                        var key = Console.ReadLine();
                        var val=CacheHelper.Get(key);
                        Console.WriteLine("缓存值:");
                        Console.WriteLine(val);
                        break;
                    case "98":
                        Console.WriteLine("清空技术数据，技术上下文数据");
                        var job98 = new TechResetJob();
                        job98.Execute(null);
                        break;
                    case "99":
                        Console.WriteLine("更新缓存价格数据。包括大盘，个股，行业");
                        var job99 = new MongoTest();
                        job99.Execute(null);
                        break;
                    case "-1":
                        Console.WriteLine("输入表名");
                        CommonService.ClearTable(Console.ReadLine());
                        Console.WriteLine("清空完成");
                        break;
                    case "exit":
                        run = false;
                        break;
                }

                var endTime = DateTime.Now;
                TimeSpan ts = endTime - startTime;
                Console.Write(string.Format("任务结束，耗时统计：小时：{0}，分钟：{1}。\r\n", ts.TotalHours, ts.TotalMinutes));


            }


            //Host host = HostFactory.New(x =>
            //{
            //    x.Service<IQuartzServer>(s =>
            //    {

            //        s.ConstructUsing(builder =>
            //        {
            //            QuartzServer server = new QuartzServer();
            //            server.Initialize();
            //            return server;
            //        });
            //        s.WhenStarted(server => server.Start());
            //        s.WhenPaused(server => server.Pause());
            //        s.WhenContinued(server => server.Resume());
            //        s.WhenStopped(server => server.Stop());
            //    });

            //    //x.RunAs("administrator","password.1");
            //    x.RunAsLocalSystem();
            //    x.SetDescription(Configuration.ServiceDescription);
            //    x.SetDisplayName(Configuration.ServiceDisplayName);
            //    x.SetServiceName(Configuration.ServiceName);
            //});

            //host.Run();

        }

        public static DateTime GetStartDate(string param)
        {
            int s;
            DateTime from = DateTime.Now;
            if (int.TryParse(param, out s))
            {
                if (s == 0)
                {
                    from = DateTime.Now;
                }
                else
                {
                    from = DateTime.Now.AddDays(s - 2);
                }
            }
            else
            {
                from = DateTime.Parse(param);
            }
            return from;
        }
    }

}
