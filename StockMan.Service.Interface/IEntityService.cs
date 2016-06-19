using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StockMan.Service.Interface
{
    public interface IEntityService<T>:IDisposable
    {
        T Add(T data);
        void AddRange(IList<T> datas);
        void Update(T data);
        void Delete(T data);
        void DeleteRange(IList<T> datas);
        T Find(string id);
        IList<T> FindAll();
    }
}
