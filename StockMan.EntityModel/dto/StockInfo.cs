using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockMan.EntityModel
{
    public class StockInfo
    {
        public string code { get; set; }

        public string stock_code { get; set; }

        public string name { get; set; }
        /// <summary>
        /// 涨跌幅
        /// </summary>
        public decimal percent { get; set; }//涨跌幅
        /// <summary>
        /// 最高价
        /// </summary>
        public decimal high { get; set; }//最高价
        public string askvol3 { get; set; }
        public string askvol2 { get; set; }
        public string askvol5 { get; set; }
        public string askvol4 { get; set; }
        /// <summary>
        /// 当前价
        /// </summary>
        public decimal price { get; set; }//当前价
        /// <summary>
        /// 开盘价
        /// </summary>
        public decimal open { get; set; }//开盘价
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
        public decimal low { get; set; }//最低价
        /// <summary>
        /// 涨跌额
        /// </summary>
        public decimal updown { get; set; }//涨跌额
        public decimal turnoverrate { get; set; }//换手率
        public string type { get; set; }
        /// <summary>
        /// 卖量1
        /// </summary>
        public string bidvol1 { get; set; }
        public string bidvol3 { get; set; }
        public string bidvol2 { get; set; }
        public DateTime date { get; set; }
        public string bidvol5 { get; set; }
        public string bidvol4 { get; set; }
        /// <summary>
        /// 成就量
        /// </summary>
        public decimal volume { get; set; }//成交量
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
        public decimal yestclose { get; set; }//昨收
        /// <summary>
        /// 成交额
        /// </summary>
        public decimal turnover { get; set; }//成交额
        public string spell { get; set; }
        public string sort { get; set; }
        /// <summary>
        /// 市盈率
        /// </summary>
        public decimal pe { get; set; }//市盈率
        /// <summary>
        /// 市净率
        /// </summary>
        public decimal pb { get; set; }//市净率
        /// <summary>
        /// 振幅
        /// </summary>
        public decimal amplitude { get; set; }//~2.70振幅
        /// <summary>
        /// 总市值
        /// </summary>
        public decimal mv { get; set; }//~161.20总市值
        /// <summary>
        /// 流通市值
        /// </summary>
        public decimal fv { get; set; }//~161.20流通市值
        /// <summary>
        /// 涨停价
        /// </summary>
        public decimal surgedprice { get; set; }//~8.95涨停价
        /// <summary>
        /// 跌停价
        /// </summary>
        public decimal declineprice { get; set; }//~7.33跌停价
    }


    public class ObjectInfo
    {
        public string code { get; set; }

        public string object_code { get; set; }

        public string name { get; set; }
        /// <summary>
        /// 涨跌幅
        /// </summary>
        public decimal percent { get; set; }//涨跌幅
        /// <summary>
        /// 最高价
        /// </summary>
        public decimal high { get; set; }//最高价
        public string askvol3 { get; set; }
        public string askvol2 { get; set; }
        public string askvol5 { get; set; }
        public string askvol4 { get; set; }
        /// <summary>
        /// 当前价
        /// </summary>
        public decimal price { get; set; }//当前价
        /// <summary>
        /// 开盘价
        /// </summary>
        public decimal open { get; set; }//开盘价
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
        public decimal low { get; set; }//最低价
        /// <summary>
        /// 涨跌额
        /// </summary>
        public decimal updown { get; set; }//涨跌额
        public decimal turnoverrate { get; set; }//换手率
        public string type { get; set; }
        /// <summary>
        /// 卖量1
        /// </summary>
        public string bidvol1 { get; set; }
        public string bidvol3 { get; set; }
        public string bidvol2 { get; set; }
        public DateTime date { get; set; }
        public string bidvol5 { get; set; }
        public string bidvol4 { get; set; }
        /// <summary>
        /// 成就量
        /// </summary>
        public decimal volume { get; set; }//成交量
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
        public decimal yestclose { get; set; }//昨收
        /// <summary>
        /// 成交额
        /// </summary>
        public decimal turnover { get; set; }//成交额
        public string spell { get; set; }
        public string sort { get; set; }
        /// <summary>
        /// 市盈率
        /// </summary>
        public decimal pe { get; set; }//市盈率
        /// <summary>
        /// 市净率
        /// </summary>
        public decimal pb { get; set; }//市净率
        /// <summary>
        /// 振幅
        /// </summary>
        public decimal amplitude { get; set; }//~2.70振幅
        /// <summary>
        /// 总市值
        /// </summary>
        public decimal mv { get; set; }//~161.20总市值
        /// <summary>
        /// 流通市值
        /// </summary>
        public decimal fv { get; set; }//~161.20流通市值
        /// <summary>
        /// 涨停价
        /// </summary>
        public decimal surgedprice { get; set; }//~8.95涨停价
        /// <summary>
        /// 跌停价
        /// </summary>
        public decimal declineprice { get; set; }//~7.33跌停价
    }
}
