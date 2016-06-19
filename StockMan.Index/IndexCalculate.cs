using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.ClearScript.Windows;
using StockMan.EntityModel;
namespace StockMan.Index
{
    public class IndexCalculate
    {
        public static bool IsDebugg = false;
        public static IList<IndexData> GetIndexData(string jscript, string paramater, string contextOld, out string contextNew, IList<PriceInfo> priceList)
        {
            var log = log4net.LogManager.GetLogger(typeof(IndexCalculate));
            JScriptEngine engine = null;
            if (IsDebugg)
            {
                engine = new JScriptEngine(typeof(IndexCalculate).Name, WindowsScriptEngineFlags.EnableDebugging);
            }
            else
            {
                engine = new JScriptEngine(typeof(IndexCalculate).Name);
            }
            using (engine )
            {
                //log.Info("开始计算：" );
                engine.Execute(File.ReadAllText(@"Script\json2.js"));
                engine.Execute(File.ReadAllText(@"Script\util.js"));

                engine.Execute("var context=" + jscript);

                engine.Execute("var config=" + paramater);

                engine.Execute("var context=Ext.apply(context,config)");

                if (!string.IsNullOrEmpty(contextOld))
                {
                    engine.Execute("var context_old=" + contextOld);
                    engine.Execute("var context=Ext.apply(context,context_old)");
                }


                //data格式，
                //e[0],//日期
                //e[1],//开盘 
                //e[2],//收盘
                //e[3],//最高
                //e[4],//最低   
                //e[5],//成交量
                //e[6],//涨跌额
                //e[7],//涨跌幅

                priceList = Filter(contextOld, priceList);

                string dataStr = "";
                foreach (var priceInfo in priceList)
                {
                    if (dataStr.Length == 0)
                    {
                        dataStr = string.Format("['{0}',{1},{2},{3},{4},{5},{6},{7}]",
                            priceInfo.date.ToString("yyyyMMdd"),
                            priceInfo.open,
                            priceInfo.price,
                            priceInfo.high,
                            priceInfo.low,
                            priceInfo.volume,
                            priceInfo.updown,
                            priceInfo.percent);
                    }
                    else
                    {
                        dataStr += "," + string.Format("['{0}',{1},{2},{3},{4},{5},{6},{7}]",
                           priceInfo.date.ToString("yyyyMMdd"),
                           priceInfo.open,
                           priceInfo.price,
                           priceInfo.high,
                           priceInfo.low,
                           priceInfo.volume,
                           priceInfo.updown,
                           priceInfo.percent);
                    }

                }

                engine.Execute("var data=" + '[' + dataStr + ']');

                engine.Execute(@" 
                                  var results=context.calculate(data);
                                  context.cutdownContext(50);
                                  var contextStr=JSON.stringify(context);
                                  ");

                var results = engine.Script.results;

                IList<IndexData> idataList = new List<IndexData>();

                for (int i = 0; i < results.length; i++)
                {
                    string idataStr = results[i][0] + "";
                    var idata = new IndexData(results[i][0]);
                    for (int j = 1; j < results[i].length; j++)
                    {
                        idataStr += "-" + results[i][j];
                        double value = 0;
                        double.TryParse(results[i][j] + "", out value);
                        idata.Add(value);
                    }
                    //log.Info(idataStr);
                    idataList.Add(idata);
                }

                contextNew = engine.Script.contextStr;
                //log.Info("计算完成" );
                return idataList;
            }

        }

        private static IList<PriceInfo> Filter(string contextOld, IList<PriceInfo> priceList)
        {
            JScriptEngine engine = null;
            if (IsDebugg)
            {
                engine = new JScriptEngine("Filter", WindowsScriptEngineFlags.EnableDebugging);
            }
            else
            {
                engine = new JScriptEngine("Filter");
            }
            using (engine)
            {
                if (string.IsNullOrEmpty(contextOld))
                {
                    return priceList;
                }
                engine.Execute("var context=" + contextOld);

                engine.Execute(@"var data=context.data;  
                                var date;
                                if(data&&data.length>0){
	                                date=data[data.length-1][0];
                                }else{
	                                date='';
                                }");
                var date = engine.Script.date;

                var dateTime = DateTime.ParseExact(date, "yyyyMMdd", null);
                if (string.IsNullOrEmpty(date))
                {
                    return priceList;
                }
                else
                {
                    return priceList.Where(p => p.date > dateTime).ToList();
                }

            }
        }

        public static IndexState GetState(string jscript, IList<IndexData> indexDataList)
        {
            var log = log4net.LogManager.GetLogger(typeof(IndexCalculate));
            JScriptEngine engine = null;
            if (IsDebugg)
            {
                engine = new JScriptEngine("GetState", WindowsScriptEngineFlags.EnableDebugging);
            }
            else
            {
                engine = new JScriptEngine("GetState");
            }
            using (engine)
            {
                engine.Execute(File.ReadAllText(@"Script\json2.js"));
                engine.Execute(File.ReadAllText(@"Script\util.js"));

                engine.Execute("var context=" + jscript);

                string dataStr = "";
                foreach (var priceInfo in indexDataList)
                {
                    if (dataStr.Length == 0)
                    {
                        string str = priceInfo.Aggregate("", (current, price) => current + ("," + price));

                        dataStr = string.Format("['{0}'{1}]", priceInfo.date.ToString("yyyyMMdd"), str);
                    }
                    else
                    {
                        string str = priceInfo.Aggregate("", (current, price) => current + ("," + price));

                        dataStr += "," + string.Format("['{0}'{1}]", priceInfo.date.ToString("yyyyMMdd"), str);
                    }

                }

                engine.Execute("var data=" + '[' + dataStr + ']');
                if (IsDebugg)
                {
                    engine.Execute("debugger;var state=context.getState(data);");
                }
                else
                {
                    engine.Execute("var state=context.getState(data);");
                }
                int state = engine.Script.state;

                if (state == 9)
                {
                    return IndexState.Down;
                }
                else
                {
                    return (IndexState)Enum.Parse(typeof(IndexState), state + "", true);
                }

            }
        }


        public static string GetTag(string jscript, IList<IndexData> indexDataList)
        {
            var log = log4net.LogManager.GetLogger(typeof(IndexCalculate));
            JScriptEngine engine = null;
            if (IsDebugg)
            {
                engine = new JScriptEngine("GetTag", WindowsScriptEngineFlags.EnableDebugging);
            }
            else
            {
                engine = new JScriptEngine("GetTag");
            }
            using (engine)
            {
                engine.Execute(File.ReadAllText(@"Script\json2.js"));
                engine.Execute(File.ReadAllText(@"Script\util.js"));

                engine.Execute("var context=" + jscript);

                string dataStr = "";
                foreach (var priceInfo in indexDataList)
                {
                    if (dataStr.Length == 0)
                    {
                        string str = priceInfo.Aggregate("", (current, price) => current + ("," + price));

                        dataStr = string.Format("['{0}'{1}]", priceInfo.date.ToString("yyyyMMdd"), str);
                    }
                    else
                    {
                        string str = priceInfo.Aggregate("", (current, price) => current + ("," + price));

                        dataStr += "," + string.Format("['{0}'{1}]", priceInfo.date.ToString("yyyyMMdd"), str);
                    }

                }

                engine.Execute("var data=" + '[' + dataStr + ']');
                if (IsDebugg)
                {
                    engine.Execute(@"debugger;
                                    var tag='';
                                    if(context.getTag)  
                                        tag=context.getTag(data);");
                }
                else
                {
                    engine.Execute(@"var tag='';
                                    if(context.getTag)  
                                        tag=context.getTag(data);");
                }
                string tag = engine.Script.tag;
                return tag;
            }
        }
    }
}