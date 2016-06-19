using System.Collections.Generic;
using System.Collections.Specialized;
using StockMan.EntityModel;

namespace StockMan.Index
{
    public interface IIndex
    {
        /// <summary>
        /// ȫ������
        /// </summary>
        /// <param name="priceList"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        IList<IndexData> Calculate(IList<StockMan.EntityModel.PriceInfo> priceList, NameValueCollection parameter);
        /// <summary>
        /// ��������
        /// </summary>
        /// <param name="last">��ǰ�Ѿ����ڵ����ݣ�����50��</param>
        /// <param name="priceList">��ȡ�����µĹ�Ʊ�۸�����</param>
        /// <returns>���������ļ���ָ�������</returns>
        IList<IndexData> Calculate(IList<IndexData> last, IList<StockMan.EntityModel.PriceInfo> priceList, NameValueCollection parameter);

        /// <summary>
        /// ���ص�ǰָ����״̬
        /// </summary>
        /// <param name="last"></param>
        /// <returns></returns>
        IndexState GetState(IList<IndexData> last);
    }
}