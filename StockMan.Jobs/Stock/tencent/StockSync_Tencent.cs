using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using data = StockMan.EntityModel;
using StockMan.Service.Interface.Rds;
using StockMan.Service.Rds;
using System.Threading;

namespace StockMan.Jobs.Stock.tencent
{
    public class StockSync_Tencent : IStockSync
    {


        public IList<data.stockcategory> GetCategorys()
        {
            string[] allCode = getAllCode();

            List<data.stockcategory> cateList = new List<data.stockcategory>();
            for (int i = 0; i < allCode.Length; i = i + 20)
            {
                List<string> codes = allCode.Skip(i).Take(20).Select(p => { return "bkhz" + p.Substring(4); }).ToList();

                cateList.AddRange(getCategory(codes));
            }

            return cateList;
        }

        private IList<data.stockcategory> getCategory(List<string> codes)
        {
            string url = "http://push1.gtimg.cn/q={0}&m=push&r={1}";
            url=string.Format(url,string.Join(",",codes),new Random().Next(99999999).ToString());
            string content = this.getRequestContent(new Uri(url));
            string[] list = content.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            //pv_bkhz011100="011100~军工~3~0~15~18~15.342~-0.193~-1.243~4073784~501102~sh600765~sh600855~125769.04~173182.36~-47413.32~-438130.63~-9.72";

            IList<data.stockcategory> cateList = new List<data.stockcategory>();
            foreach (string sub in list)
            {
                string code = sub.Substring(0, sub.IndexOf("="));
                Regex r = new Regex("\"[^\"]*\"");
                Match match = r.Match(sub);
                string dataStr = match.Value.Trim('\"');
                string[] datas = dataStr.Split('~');
                string id = datas[0];
                string name = datas[1];

                cateList.Add(new data.stockcategory
                {
                    code = id,
                    name = name,
                    group_code = "tencent"
                });
            }
            return cateList;
        }

        private string[] getAllCode()
        {
            var listUrl = "http://stock.gtimg.cn/data/view/bdrank.php?&t=01/averatio&p=1&o=0&l=80&v=list_data";
            var listContent = this.getRequestContent(new Uri(listUrl));
            Regex rlist = new Regex("data:\'([^\']*)\'");
            Match match0 = rlist.Match(listContent);

            string v = match0.Groups[1].Value;

            return v.Split(',');
        }



        public data.stock_category_group initCategoryGroup()
        {
            return new data.stock_category_group
            {
                code = "tencent",
                name = "腾讯行业"
            };
        }



        public IList<data.stock> GetStocks(data.stockcategory category)
        {
            IList<string> codes = getStockCodeList(category);
            IList<IList<string>> codeStr = new List<IList<string>>();
            int num = 10;
            if (codes.Count > 10)
            {
                for (int i = 0; i < codes.Count; i = i + num)
                {
                    codeStr.Add(codes.Skip(i).Take(num).ToList());
                }
            }
            else
            {
                codeStr.Add(codes);
            }

            IList<data.stock> stockList = new List<data.stock>();
            foreach (IList<string> sub in codeStr)
            {
                string url2 = "http://push3.gtimg.cn/q={0}&m=push&r={1}";
                Uri uri = new Uri(string.Format(url2, string.Join(",", sub), new Random().ToString()));
                //pv_sz000718="51~苏宁环球~000718~4.65~4.63~4.65~181492~70761~110731~4.64~1372~4.63~3972~4.62~3000~4.61~5423~4.60~1117~4.65~3246~4.66~1875~4.67~2492~4.68~3955~4.69~2481~15:00:20/4.65/3400/S/1581372/21958|14:56:56/4.65/50/S/23250/21784|14:56:53/4.66/20/B/9320/21779|14:56:47/4.65/29/S/13509/21768|14:56:38/4.65/14/S/6510/21756|14:56:38/4.66/24/B/11184/21750~20140704150411~0.02~0.43~4.69~4.58~4.65/178092/82605364~181492~8419~1.23~103.55~~4.69~4.58~2.38~68.35~95.01~2.19~5.09~4.17~";

                string content = getRequestContent(uri);
                string[] stockStrs = content.Split('\n');

                foreach (string stockStr in stockStrs)
                {
                    Regex reg = new Regex("\"[^\"]+\"");
                    var matchs = reg.Matches(stockStr);

                    foreach (Match match in matchs)
                    {
                        string[] vs = match.Value.Trim('\"').Split('~');
                        data.stock stock = new data.stock();
                        stock.code = (vs[0] == "51" ? "1" : "0") + vs[2];
                        //stock.cate_code = category.code;
                        stock.name = vs[1];
                        stock.price = decimal.Parse(vs[3]);
                        stock.yestclose = decimal.Parse(vs[4]);
                        stock.symbol = "";
                        stockList.Add(stock);
                        Console.WriteLine(stock.code + "-" + stock.name + "-" + stock.price);
                    }
                }
            }
            return stockList;
        }

        private IList<string> getStockCodeList(data.stockcategory category)
        {
            string url = "http://stock.gtimg.cn/data/index.php?appn=rank&t=pt{0}/chr&p=1&o=0&l=100000&v=list_data";
            Uri uri = new Uri(string.Format(url, category.code));
            string content = getRequestContent(uri);

            Regex r = new Regex(@"s[zh]{1}\d{6}");
            MatchCollection matchs = r.Matches(content);
            //content.Substring(content.IndexOf("data:'")，
            IList<string> codes = new List<string>();
            foreach (Match match in matchs)
            {
                codes.Add(match.Value);
            }
            return codes;
        }

        private string getRequestContent(Uri uri)
        {
            try
            {
                HttpWebRequest request = HttpWebRequest.CreateHttp(uri);
                WebResponse response = request.GetResponse();
                Stream stream = response.GetResponseStream();
                StreamReader sr = new StreamReader(stream, Encoding.Default);
                string content = sr.ReadToEnd();
                return content;
            }
            catch (WebException ex)
            {
                if (ex.Response is HttpWebResponse)
                {
                    var response = ex.Response as HttpWebResponse;
                    if (response.StatusCode == HttpStatusCode.NotFound)
                        return string.Empty;
                    else
                        throw ex;
                }
                else
                {
                    throw ex;
                }
            }
        }



        /// <summary>
        /// 获取当前股票的价格
        /// </summary>
        /// <param name="stocks"></param>
        /// <returns></returns>
        public IList<data.StockInfo> GetPrice(IList<data.stock> stocks)
        {
            string ids = string.Empty;
            IList<string> codeList = new List<string>();
            foreach (data.stock stock in stocks)
            {
                var code = GetStockCode(stock);
                codeList.Add(code);
            }
            IList<IList<string>> codeGroup = new List<IList<string>>();
            int num = 10;
            if (codeList.Count > 10)
            {
                for (int i = 0; i < codeList.Count; i = i + num)
                {
                    codeGroup.Add(codeList.Skip(i).Take(num).ToList());
                }
            }
            else
            {
                codeGroup.Add(codeList);
            }

            IList<data.StockInfo> stockList = new List<data.StockInfo>();
            string url2 = "http://push3.gtimg.cn/q={0}&m=push&r={1}";
            foreach (IList<string> sub in codeGroup)
            {
                Uri uri = new Uri(string.Format(url2, string.Join(",", sub), new Random().ToString()));
                //pv_sz000718="51~苏宁环球~000718~4.65~4.63~4.65~181492~70761~110731~4.64~1372~4.63~3972~4.62~3000~4.61~5423~4.60~1117~4.65~3246~4.66~1875~4.67~2492~4.68~3955~4.69~2481~15:00:20/4.65/3400/S/1581372/21958|14:56:56/4.65/50/S/23250/21784|14:56:53/4.66/20/B/9320/21779|14:56:47/4.65/29/S/13509/21768|14:56:38/4.65/14/S/6510/21756|14:56:38/4.66/24/B/11184/21750~20140704150411~0.02~0.43~4.69~4.58~4.65/178092/82605364~181492~8419~1.23~103.55~~4.69~4.58~2.38~68.35~95.01~2.19~5.09~4.17~";

                string content = getRequestContent(uri);
                string[] stockStrs = content.Split('\n');

                foreach (string stockStr in stockStrs)
                {
                    Regex reg = new Regex("\"[^\"]+\"");
                    var matchs = reg.Matches(stockStr);

                    foreach (Match match in matchs)
                    {
                        string[] vs = match.Value.Trim('\"').Split('~');
                        data.StockInfo stock = new data.StockInfo();
                        stock.stock_code = (vs[0] == "51" ? "1" : "0") + vs[2];
                        //stock.code = category.code;
                        stock.name = vs[1];
                        stock.price = decimal.Parse(vs[3]);
                        stock.yestclose = decimal.Parse(vs[4]);
                        stock.percent = decimal.Parse(vs[StockPriceMap.percent]);//涨跌幅
                        stock.high = decimal.Parse(vs[StockPriceMap.high]);//最高价                        
                        stock.price = decimal.Parse(vs[StockPriceMap.price]);//当前价
                        stock.open = decimal.Parse(vs[StockPriceMap.open]);//开盘价                       
                        stock.low = decimal.Parse(vs[StockPriceMap.low]);//最低价
                        stock.turnoverrate = decimal.Parse(vs[StockPriceMap.turnoverrate]);
                        stock.updown = decimal.Parse(vs[StockPriceMap.updown]);//涨跌额
                        stock.yestclose = decimal.Parse(vs[StockPriceMap.yestclose]);//昨收
                        stock.turnover = decimal.Parse(vs[StockPriceMap.turnover]);//成交额
                        stock.pe = decimal.Parse(string.IsNullOrEmpty(vs[StockPriceMap.pe]) ? "0" : vs[StockPriceMap.pe]);//市盈率
                        stock.pb = decimal.Parse(string.IsNullOrEmpty(vs[StockPriceMap.pb]) ? "0" : vs[StockPriceMap.pb]);//市净率
                        stock.amplitude = decimal.Parse(vs[StockPriceMap.amplitude]);//~2.70振幅
                        stock.mv = decimal.Parse(vs[StockPriceMap.mv]);//~161.20总市值
                        stock.fv = decimal.Parse(vs[StockPriceMap.fv]);//~161.20流通市值
                        stock.surgedprice = decimal.Parse(vs[StockPriceMap.surgedprice]);//~8.95涨停价
                        stock.declineprice = decimal.Parse(vs[StockPriceMap.declineprice]);//~7.33跌停价
                        stock.bid5 = vs[StockPriceMap.bid5];//卖
                        stock.bid4 = vs[StockPriceMap.bid4];
                        stock.bid3 = vs[StockPriceMap.bid3];
                        stock.bid2 = vs[StockPriceMap.bid2];
                        stock.bid1 = vs[StockPriceMap.bid1];
                        stock.bidvol1 = vs[StockPriceMap.bidvol1];
                        stock.bidvol2 = vs[StockPriceMap.bidvol2];
                        stock.bidvol3 = vs[StockPriceMap.bidvol3];
                        stock.bidvol4 = vs[StockPriceMap.bidvol4];
                        stock.bidvol5 = vs[StockPriceMap.bidvol5];

                        stock.date = getDate(vs[StockPriceMap.update].Substring(0, 8));
                        stock.volume = decimal.Parse(vs[StockPriceMap.volume]);//成交量
                        stock.ask1 = vs[StockPriceMap.ask1];//买                        
                        stock.ask2 = vs[StockPriceMap.ask2];
                        stock.ask3 = vs[StockPriceMap.ask3];
                        stock.ask4 = vs[StockPriceMap.ask4];
                        stock.ask5 = vs[StockPriceMap.ask5];
                        stock.askvol1 = vs[StockPriceMap.askvol1];
                        stock.askvol2 = vs[StockPriceMap.askvol2];
                        stock.askvol3 = vs[StockPriceMap.askvol3];
                        stock.askvol4 = vs[StockPriceMap.askvol4];
                        stock.askvol5 = vs[StockPriceMap.askvol5];
                        stock.time = vs[StockPriceMap.time].Substring(8);
                        stock.code = stock.stock_code + stock.date;
                        stockList.Add(stock);
                        Console.WriteLine(stock.code + "-" + stock.name + "-" + stock.price);
                    }
                }
            }
            return stockList;
        }

        private DateTime getDate(string p)
        {
            //20140718150257
            if (p.Length == 8)
            {
                return DateTime.ParseExact(p, "yyyyMMdd", null);
            }
            else
            {
                return DateTime.ParseExact(p, "yyyyMMddhhmmss", null);
            }

        }


        public IList<data.PriceInfo> GetPriceByDay(data.stock stock)
        {
            //http://data.gtimg.cn/flashdata/hushen/daily/14/sh600050.js
            string url = "http://data.gtimg.cn/flashdata/hushen/daily/{0}/{1}.js";


            //daily_data_04="\n\
            //040102 3.93 4.12 4.17 3.91 1885527\n\
            //040105 4.13 4.53 4.53 4.13 3699816\n\
            //040106 4.59 4.88 4.98 4.56 4880455\n\
            //040107 4.88 4.78 4.97 4.73 3515511\n\
            //040108 4.75 4.94 5.05 4.72 2859660\n\
            //040109 4.92 4.87 5.11 4.84 2846707\n\
            //040112 4.89 5.02 5.04 4.78 2001276\n\
            //040113 5.04 4.89 5.08 4.82 1837009\n\
            //040114 4.88 4.74 4.90 4.72 1938512\n\
            //040115 4.74 4.74 4.82 4.72 1131030\n\
            //040116 4.76 4.74 4.79 4.61 1320374\n\
            //040129 4.78 4.86 4.95 4.78 1330404\n\
            //040130 4.89 4.87 5.03 4.84 1758014\n\
            //040202 5.36 5.17 5.36 5.03 4633113\n\
            //040203 5.18 5.15 5.19 5.07 1248059\n\

            string code = GetStockCode(stock);
            List<data.PriceInfo> stockList = new List<data.PriceInfo>();
            IList<string> urls = new List<string>();
            for (int i = 2010; i <= DateTime.Now.Year; i++)
            {
                string target = string.Format(url, (i.ToString()).Substring(2), code);

                var tempList = GetStockPriceFromServer(stock.code, target);
                if (tempList.Count <= 0)
                    continue;
                stockList.AddRange(tempList);
                Thread.Sleep(100);
            }
            return stockList;
        }


        public IList<data.PriceInfo> GetPriceByWeek(data.stock stock)
        {
            //http://data.gtimg.cn/flashdata/hushen/weekly/sh600050.js
            string url = "http://data.gtimg.cn/flashdata/hushen/weekly/{0}.js";
            string code = GetStockCode(stock);
            List<data.StockInfo> stockList = new List<data.StockInfo>();

            string target = string.Format(url, code);

            var tempList = GetStockPriceFromServer(stock.code, target);

            return tempList;
        }

        public IList<data.PriceInfo> GetPriceByMonth(data.stock stock)
        {
            //http://data.gtimg.cn/flashdata/hushen/monthly/sh600050.js
            string url = "http://data.gtimg.cn/flashdata/hushen/monthly/{0}.js";
            string code = GetStockCode(stock);
            List<data.StockInfo> stockList = new List<data.StockInfo>();

            string target = string.Format(url, code);

            var tempList = GetStockPriceFromServer(stock.code, target);

            return tempList;
        }

        private IList<data.PriceInfo> GetStockPriceFromServer(string code, string targetUrl)
        {
            IList<data.PriceInfo> stockList = new List<data.PriceInfo>();
            string content = this.getRequestContent(new Uri(targetUrl));
            if (content.Contains("<html>") || content.Contains("404 Not Found"))
                return new List<data.PriceInfo>();


            Regex reg = new Regex("\"[^\"]+\"");
            var matchs = reg.Matches(content);
            foreach (Match match in matchs)
            {
                string[] vs = match.Value.Trim('\"').Split(new string[] { "\\n\\\n" }, StringSplitOptions.RemoveEmptyEntries);
                decimal yestclose = 0;
                foreach (string v in vs)
                {
                    string[] sps = v.Trim('\"').Split(' ');

                    string date = string.Empty;

                    if (sps[0].StartsWith("8") || sps[0].StartsWith("9"))
                    {
                        date = "19" + sps[0];
                    }
                    else
                    {
                        date = "20" + sps[0];
                    }

                    DateTime datetime = new DateTime(int.Parse(date.Substring(0, 4)), int.Parse(date.Substring(4, 2)), int.Parse(date.Substring(6)));

                    data.PriceInfo sp = new data.PriceInfo();
                    sp.code = code;
                    sp.date = datetime;
                    sp.open = decimal.Parse(sps[1]);
                    sp.price = decimal.Parse(sps[2]);//
                    sp.high = decimal.Parse(sps[3]);
                    sp.low = decimal.Parse(sps[4]);
                    sp.volume = decimal.Parse(sps[5]);
                    stockList.Add(sp);
                }
            }
            return stockList;
        }

        /// <summary>
        /// 或者针对腾讯网站的编码
        /// </summary>
        /// <param name="stock"></param>
        /// <returns></returns>
        private string GetStockCode(data.stock stock)
        {
            string prifix = string.Empty;
            if (stock.code.Substring(0, 1) == "0")
            {
                prifix = "sh";
            }
            else if (stock.code.Substring(0, 1) == "1")
            {
                prifix = "sz";
            }
            return prifix + stock.code.Substring(1);
        }
    }


}
