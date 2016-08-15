using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockMan.EntityModel;

namespace StockMan.Service.Interface.Rds
{
    public interface IPriceOperation
    {
        void AddPriceByDay<T>(IList<PriceInfo> price,bool check=true) where T : ObjectDataBase, new();
        void AddPriceByWeek<T>(IList<PriceInfo> price, bool check = true) where T : ObjectDataBase, new();
        void AddPriceByMonth<T>(IList<PriceInfo> price, bool check = true) where T : ObjectDataBase, new();
        string GetData<TM>(string code) where TM : ObjectDataBase, new();
        string GetData<TM>(string code,DateTime start,DateTime end) where TM : ObjectDataBase, new();
        PriceInfo GetCurrentData(ObjectType objType, TechCycle cycle, string code);  
        string GetHistoryData(ObjectType objType, TechCycle cycle, string code);
        DateTime GetLatestDate(ObjectType objType, string code);
    }
}
