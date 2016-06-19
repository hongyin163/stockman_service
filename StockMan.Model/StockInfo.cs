using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockMan.Model
{
    public class StockInfo_day : StockInfo
    {
 
    }

    public class Stock_month : StockInfo
    {
    }

    public class StockInfo_week : StockInfo
    {
    }

    public class StockInfo_minute : StockInfo
    {
 
    }

    public class StockInfo : Stock
    {
        public string level { get; set; }
        /// <summary>
        /// 涨跌幅
        /// </summary>
        public string percent { get; set; }//涨跌幅
        /// <summary>
        /// 最高价
        /// </summary>
        public string high { get; set; }//最高价
        public string askvol3 { get; set; }
        public string askvol2 { get; set; }
        public string askvol5 { get; set; }
        public string askvol4 { get; set; }
        /// <summary>
        /// 当前价
        /// </summary>
        public string price { get; set; }//当前价
        /// <summary>
        /// 开盘价
        /// </summary>
        public string open { get; set; }//开盘价
        public string bid5 { get; set; }//卖
        public string bid4 { get; set; }
        public string bid3 { get; set; }
        public string bid2 { get; set; }
        /// <summary>
        /// 卖价1
        /// </summary>
        public string bid1 { get; set; }
        /// <summary>
        /// 最低价
        /// </summary>
        public string low { get; set; }//最低价
        /// <summary>
        /// 涨跌额
        /// </summary>
        public string updown { get; set; }//涨跌额
        public string type { get; set; }
        /// <summary>
        /// 卖量1
        /// </summary>
        public string bidvol1 { get; set; }
        public string bidvol3 { get; set; }
        public string bidvol2 { get; set; }
        public string update { get; set; }
        public string bidvol5 { get; set; }
        public string bidvol4 { get; set; }
        /// <summary>
        /// 成就量
        /// </summary>
        public string volume { get; set; }//成交量
        /// <summary>
        /// 买量1
        /// </summary>
        public string askvol1 { get; set; }
        public string ask5 { get; set; }
        public string ask4 { get; set; }
        /// <summary>
        /// 买价1
        /// </summary>
        public string ask1 { get; set; }//买
        public string ask3 { get; set; }
        public string ask2 { get; set; }
        public string arrow { get; set; }//箭头
        public string time { get; set; }
        /// <summary>
        /// 昨收
        /// </summary>
        public string yestclose { get; set; }//昨收
        /// <summary>
        /// 成交额
        /// </summary>
        public string turnover { get; set; }//成交额
        public string spell { get; set; }
        public string sort { get; set; }
        /// <summary>
        /// 市盈率
        /// </summary>
        public string pe { get; set; }//市盈率
        /// <summary>
        /// 市净率
        /// </summary>
        public string pb { get; set; }//市净率
        /// <summary>
        /// 振幅
        /// </summary>
        public string amplitude { get; set; }//~2.70振幅
        /// <summary>
        /// 总市值
        /// </summary>
        public string mv { get; set; }//~161.20总市值
        /// <summary>
        /// 流通市值
        /// </summary>
        public string fv { get; set; }//~161.20流通市值
        /// <summary>
        /// 涨停价
        /// </summary>
        public string surgedprice { get; set; }//~8.95涨停价
        /// <summary>
        /// 跌停价
        /// </summary>
        public string declineprice { get; set; }//~7.33跌停价
    }
}
