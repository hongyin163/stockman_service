using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using data=StockMan.EntityModel;
namespace StockMan.Service.Interface.Rds
{
    public interface IRelatedDataService : IEntityService<data.related_object_define>, IDisposable
    {
        void InsertData(string defineCode, string jsonData);
        string GetData(string defineCode);

        IList<data.customobject> GetDataByCode(string codes);
    }
}
