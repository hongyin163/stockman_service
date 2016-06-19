using System.Collections.Generic;
using System.Collections.Specialized;
using StockMan.EntityModel;

namespace StockMan.Index
{
    public interface IIndex
    {
        /// <summary>
        /// 全量计算
        /// </summary>
        /// <param name="priceList"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        IList<IndexData> Calculate(IList<StockMan.EntityModel.PriceInfo> priceList, NameValueCollection parameter);
        /// <summary>
        /// 增量计算
        /// </summary>
        /// <param name="last">当前已经存在的数据，最新50条</param>
        /// <param name="priceList">获取的最新的股票价格数据</param>
        /// <returns>返回增量的技术指标计算结果</returns>
        IList<IndexData> Calculate(IList<IndexData> last, IList<StockMan.EntityModel.PriceInfo> priceList, NameValueCollection parameter);

        /// <summary>
        /// 返回当前指数的状态
        /// </summary>
        /// <param name="last"></param>
        /// <returns></returns>
        IndexState GetState(IList<IndexData> last);
    }
}