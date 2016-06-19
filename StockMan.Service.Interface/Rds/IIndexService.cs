using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockMan.EntityModel;

namespace StockMan.Service.Interface.Rds
{
    public interface IIndexService : IEntityService<indexdefinition>, IDisposable
    {
        void Release(string code);

        string[][] GetIndexData(string entityType, string techName, TechCycle cycle, string dataId);
        IList<PriceInfo> GetObjectData(string entityType, TechCycle cycle, string objCode);
        IList<index_user_map> GetMyIndexs(string userId);

        bool SyncMyIndex(string userId, IList<index_user_map> indexCodeList);


        indexdefinegroup AddGroup(indexdefinegroup group);

        void DeleteGroup(string code);

        IList<indexdefinegroup> GetGroups();

        IList<objectstate> GetObjectStates(IList<string> codeList);

        void AddTechByDay(string table, IList<string> fields, string objectCode, IList<IndexData> result);

        void AddTechByWeek(string table, IList<string> fields, string objectCode, IList<IndexData> result);

        void AddTechByMonth(string table, IList<string> fields, string objectCode, IList<IndexData> result);

        IList<indexdefinition> GetIndexByCodes(string[] codes);
    }
}
