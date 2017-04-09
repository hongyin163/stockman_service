using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockMan.Facade.Models
{
    /// <summary>
    /// 持仓记录
    /// </summary>
    public class Position
    {
        /// <summary>
        /// 持仓数量
        /// </summary>
        public int? count { get; set; }
        /// <summary>
        /// 股票代码
        /// </summary>
        public string stock_code { get; set; }
        /// <summary>
        /// 股票名称
        /// </summary>
        public string stock_name { get; set; }
        /// <summary>
        /// 成本价
        /// </summary>
        public decimal? cost { get; set; }
        /// <summary>
        /// 当前价
        /// </summary>
        public decimal? price { get; set; }
        /// <summary>
        /// 盈亏比
        /// </summary>
        public decimal? percent { get; set; }
        /// <summary>
        /// 盈亏金额
        /// </summary>
        public decimal? amount { get; set; }
    }
    
    public class MyPosition
    {
        public string user_id { get; set; }
        public IList<Position> position { get; set; }
    }
}
