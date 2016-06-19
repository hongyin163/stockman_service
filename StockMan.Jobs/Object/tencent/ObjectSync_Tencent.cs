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

namespace StockMan.Jobs.Object.tencent
{
    public class OjbectSync_Tencent : IObjectSync
    {

        public IList<data.customobject> GetAllObjects()
        {
            string url = "http://qt.gtimg.cn/r=0.9269603732973337q=s_sh000001,s_sz399001,s_sz399300,s_sh000016,s_sz399004,s_sz399006,s_sz399005";


            string content = this.getRequestContent(new Uri(url));


            string[] list = content.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

            IList<data.customobject> cateList = new List<data.customobject>();
            foreach (string sub in list)
            {
                string code = sub.Substring(0, sub.IndexOf("="));
                Regex r = new Regex("\"[^\"]*\"");
                Match match = r.Match(sub);
                string dataStr = match.Value.Trim('\"');
                string[] datas = dataStr.Split('~');
                string id = datas[0];
                string name = datas[1];

                string prifix = id == "1" ? "0" : "1";//0上海，1深圳
                cateList.Add(new data.customobject
                {
                    code = prifix + datas[2],
                    name = name,
                    date = new DateTime(DateTime.Now.Year,DateTime.Now.Month,DateTime.Now.Day)
                });
            }

            return cateList;
        }

        public IList<data.ObjectInfo> GetPrice(IList<data.customobject> objList)
        {
            //http://qt.gtimg.cn/r={1}q=sh000001,sz399001,sz399300,sh000016,sz399004,sz399006,sz399005
            //
            string url2 = "http://qt.gtimg.cn/r={1}q={0}";

            var codeList = objList.Select(GetCode).ToList();

            Uri uri = new Uri(string.Format(url2, string.Join(",", codeList), new Random().ToString()));
            //v_s_sh000001="1~上证指数~000001~2320.47~8.79~0.38~140337395~11700020~~";
            // 0分类  1名称 2编号 3当前值 4涨跌额 5涨跌幅 6成交量 7成交额
            string content = getRequestContent(uri);
            string[] stockStrs = content.Split('\n');

            IList<data.ObjectInfo> priceList = new List<data.ObjectInfo>();
            foreach (string stockStr in stockStrs)
            {
                Regex reg = new Regex("\"[^\"]+\"");
                var matchs = reg.Matches(stockStr);

                foreach (Match match in matchs)
                {
                    string[] vs = match.Value.Trim('\"').Split('~');
                    data.ObjectInfo objectInfo = new data.ObjectInfo();
                    objectInfo.object_code = (vs[0] == "51" ? "1" : "0") + vs[2];
                    objectInfo.name = vs[1];
                    objectInfo.price = decimal.Parse(vs[3]);
                    objectInfo.yestclose = decimal.Parse(vs[4]);
                    objectInfo.percent = decimal.Parse(vs[Stock.tencent.StockPriceMap.percent]);//涨跌幅
                    objectInfo.high = decimal.Parse(vs[Stock.tencent.StockPriceMap.high]);//最高价                        
                    objectInfo.price = decimal.Parse(vs[Stock.tencent.StockPriceMap.price]);//当前价
                    objectInfo.open = decimal.Parse(vs[Stock.tencent.StockPriceMap.open]);//开盘价                       
                    objectInfo.low = decimal.Parse(vs[Stock.tencent.StockPriceMap.low]);//最低价
                    //objectInfo.turnoverrate = decimal.Parse(vs[Stock.tencent.StockPriceMap.turnoverrate]);
                    objectInfo.updown = decimal.Parse(vs[Stock.tencent.StockPriceMap.updown]);//涨跌额
                    objectInfo.yestclose = decimal.Parse(vs[Stock.tencent.StockPriceMap.yestclose]);//昨收
                    objectInfo.turnover = decimal.Parse(vs[Stock.tencent.StockPriceMap.turnover]);//成交额
                    objectInfo.pe = decimal.Parse(string.IsNullOrEmpty(vs[Stock.tencent.StockPriceMap.pe]) ? "0" : vs[Stock.tencent.StockPriceMap.pe]);//市盈率
                    objectInfo.pb = decimal.Parse(string.IsNullOrEmpty(vs[Stock.tencent.StockPriceMap.pb]) ? "0" : vs[Stock.tencent.StockPriceMap.pb]);//市净率
                    objectInfo.amplitude = decimal.Parse(vs[Stock.tencent.StockPriceMap.amplitude]);//~2.70振幅
                    //objectInfo.mv = decimal.Parse(vs[Stock.tencent.StockPriceMap.mv]);//~161.20总市值
                    //objectInfo.fv = decimal.Parse(vs[Stock.tencent.StockPriceMap.fv]);//~161.20流通市值
                    //objectInfo.surgedprice = decimal.Parse(vs[Stock.tencent.StockPriceMap.surgedprice]);//~8.95涨停价
                    //objectInfo.declineprice = decimal.Parse(vs[Stock.tencent.StockPriceMap.declineprice]);//~7.33跌停价
                    //objectInfo.bid5 = vs[Stock.tencent.StockPriceMap.bid5];//卖
                    //objectInfo.bid4 = vs[Stock.tencent.StockPriceMap.bid4];
                    //objectInfo.bid3 = vs[Stock.tencent.StockPriceMap.bid3];
                    //objectInfo.bid2 = vs[Stock.tencent.StockPriceMap.bid2];
                    //objectInfo.bid1 = vs[Stock.tencent.StockPriceMap.bid1];
                    //objectInfo.bidvol1 = vs[Stock.tencent.StockPriceMap.bidvol1];
                    //objectInfo.bidvol2 = vs[Stock.tencent.StockPriceMap.bidvol2];
                    //objectInfo.bidvol3 = vs[Stock.tencent.StockPriceMap.bidvol3];
                    //objectInfo.bidvol4 = vs[Stock.tencent.StockPriceMap.bidvol4];
                    //objectInfo.bidvol5 = vs[Stock.tencent.StockPriceMap.bidvol5];

                    objectInfo.date = getDate(vs[Stock.tencent.StockPriceMap.update].Substring(0, 8));
                    objectInfo.volume = decimal.Parse(vs[Stock.tencent.StockPriceMap.volume]);//成交量
                    //objectInfo.ask1 = vs[Stock.tencent.StockPriceMap.ask1];//买                        
                    //objectInfo.ask2 = vs[Stock.tencent.StockPriceMap.ask2];
                    //objectInfo.ask3 = vs[Stock.tencent.StockPriceMap.ask3];
                    //objectInfo.ask4 = vs[Stock.tencent.StockPriceMap.ask4];
                    //objectInfo.ask5 = vs[Stock.tencent.StockPriceMap.ask5];
                    //objectInfo.askvol1 = vs[Stock.tencent.StockPriceMap.askvol1];
                    //objectInfo.askvol2 = vs[Stock.tencent.StockPriceMap.askvol2];
                    //objectInfo.askvol3 = vs[Stock.tencent.StockPriceMap.askvol3];
                    //objectInfo.askvol4 = vs[Stock.tencent.StockPriceMap.askvol4];
                    //objectInfo.askvol5 = vs[Stock.tencent.StockPriceMap.askvol5];
                    objectInfo.time = vs[Stock.tencent.StockPriceMap.time].Substring(8);
                    objectInfo.code = objectInfo.object_code + objectInfo.date;
                    priceList.Add(objectInfo);
                }
            }

            return priceList;
        }

        public IList<data.PriceInfo> GetPriceByDay(data.customobject obj)
        {
            //http://data.gtimg.cn/flashdata/hushen/daily/14/sh000001.js
            string url = "http://data.gtimg.cn/flashdata/hushen/daily/{0}/{1}.js";

            //daily_data_04="\n\
            //140102 2112.13 2109.39 2113.11 2101.02 68485486\n\


            var code = GetCode(obj);
            var priceList = new List<data.PriceInfo>();
            IList<string> urls = new List<string>();

            for (int i = DateTime.Now.Year - 3; i <= DateTime.Now.Year; i++)
            {
                string target = string.Format(url, (i.ToString()).Substring(2), code);

                var tempList = GetStockPriceFromServer(obj.code, target);
                if (tempList.Count <= 0)
                    continue;
                priceList.AddRange(tempList);
            }
            return priceList;
        }
        public IList<data.PriceInfo> GetPriceByWeek(data.customobject obj)
        {
            //////////////http://data.gtimg.cn/flashdata/hushen/weekly/sh000001.js
            string url = "http://data.gtimg.cn/flashdata/hushen/weekly/{0}.js";
            var code = GetCode(obj);
            var tempList = GetStockPriceFromServer(obj.code, string.Format(url, code));

            return tempList;
        }
        public IList<data.PriceInfo> GetPriceByMonth(data.customobject obj)
        {
            //////////////http://data.gtimg.cn/flashdata/hushen/monthly/sh000001.js
            string url = "http://data.gtimg.cn/flashdata/hushen/monthly/{0}.js";
            var code = GetCode(obj);
            return GetStockPriceFromServer(obj.code, string.Format(url, code));
        }
        private IList<data.PriceInfo> GetStockPriceFromServer(string code, string targetUrl)
        {
            IList<data.PriceInfo> stockList = new List<data.PriceInfo>();
            string content = this.getRequestContent(new Uri(targetUrl));
            if (string.IsNullOrEmpty(content) || content.Contains("<html>") || content.Contains("404 Not Found"))
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

                    //public string code { get; set; }1
                    //public DateTime date { get; set; }1
                    //public decimal? open { get; set; }1
                    //public decimal? price { get; set; }1
                    //public decimal? yestclose { get; set; }
                    //public decimal? high { get; set; }1
                    //public decimal? low { get; set; }1
                    //public decimal? percent { get; set; }
                    //public decimal? updown { get; set; }
                    //public decimal? volume { get; set; }1
                    //public decimal? turnover { get; set; }

                    //140102 2112.13 2109.39 2113.11 2101.02 68485486\n\
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
        /// 或者针对腾讯网站的编码
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private string GetCode(data.customobject obj)
        {
            string prifix = string.Empty;
            if (obj.code.Substring(0, 1) == "0")
            {
                prifix = "sh";
            }
            else if (obj.code.Substring(0, 1) == "1")
            {
                prifix = "sz";
            }
            return prifix + obj.code.Substring(1);
        }
        private DateTime getDate(string p)
        {
            //20140718150257
            if (p.Length == 8) {
                return DateTime.ParseExact(p, "yyyyMMdd", null);
            }
            return DateTime.ParseExact(p, "yyyyMMddhhmmss", null);

        }
    }

}
