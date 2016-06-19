using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockMan.Jobs.Object.tencent
{
    public static class StockPriceMap
    {
        public static int symbol = 2;
        public static int name = 1;
        public static int percent = 32;//涨跌幅
        public static int high = 33;//最高价                        
        public static int price = 3;//当前价
        public static int open = 5;//开盘价                       
        public static int low = 34;//最低价
        public static int turnoverrate = 38;
        public static int updown = 31;//涨跌额
        public static int yestclose = 4;//昨收                        
        public static int volume = 36;//成交量
        public static int turnover = 37;//成交额
        public static int pe = 39;//市盈率
        public static int pb = 46;//市净率
        public static int amplitude = 43;//~2.70振幅
        public static int fv = 44;//~161.20流通市值
        public static int mv = 45;//~161.20总市值
        public static int surgedprice = 47;//~8.95涨停价
        public static int declineprice = 48;//~7.33跌停价
        public static int ask1 = 9;//买                        
        public static int ask2 = 11;
        public static int ask3 = 13;
        public static int ask4 = 15;
        public static int ask5 = 17;
        public static int askvol1 = 10;
        public static int askvol2 = 12;
        public static int askvol3 = 14;
        public static int askvol4 = 16;
        public static int askvol5 = 18;
        public static int bid1 = 19;
        public static int bid2 = 21;
        public static int bid3 = 23;
        public static int bid4 = 25;
        public static int bid5 = 27;//卖    
        public static int bidvol1 = 20;
        public static int bidvol2 = 22;
        public static int bidvol3 = 24;
        public static int bidvol4 = 26;
        public static int bidvol5 = 28;
        public static int update = 30;
        public static int time = 30;
    }
}
