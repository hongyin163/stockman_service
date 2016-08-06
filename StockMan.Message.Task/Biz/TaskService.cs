using StockMan.Message.DataAccess;
using StockMan.Message.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StockMan.Message.Task.Biz
{

    public interface IEntityService<T> : IDisposable
    {
        T Add(T data);
        void AddRange(IList<T> datas);
        void Update(T data);
        void Delete(T data);
        void DeleteRange(IList<T> datas);
        T Find(string id);
        IList<T> FindAll();
    }
    public class ServiceBase<T> : IEntityService<T>
       where T : EntityBase
    {

        public ServiceBase()
        {

        }
        public virtual T Add(T data)
        {
            using (messageEntities entity = new messageEntities())
            {
                entity.Set<T>().Add(data);
                try
                {
                    entity.SaveChanges();
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException ex)
                {
                    throw ex;
                }
                return data;
            }
        }

        public virtual void Update(T data)
        {
            using (messageEntities db = new messageEntities())
            {
                db.Entry<T>(data).State = EntityState.Modified;
                db.SaveChanges();
            }

        }

        public virtual void Delete(T data)
        {
            using (messageEntities db = new messageEntities())
            {
                Type type = data.GetType();
                PropertyInfo[] members = type.GetProperties();
                List<string> ids = new List<string>();
                foreach (PropertyInfo m in members)
                {
                    var temp = m.GetCustomAttribute<KeyAttribute>();
                    if (temp != null)
                    {
                        object obj = m.GetValue(data);
                        ids.Add(obj.ToString());
                        continue;
                    }
                }
                T entity = db.Set<T>().Find(ids.ToArray());
                if (entity != default(T))
                {
                    db.Set<T>().Remove(entity);
                    db.SaveChanges();
                }
            }
        }

        public virtual T Find(string id)
        {
            using (messageEntities db = new messageEntities())
            {
                T entity = db.Set<T>().Find(id);
                return entity;
            }
        }

        public void Dispose()
        {
            //if (Context != null)
            //    Context.Dispose();
            //if (Client != null)
            //    Client.Dispose();
        }

        public virtual IList<T> FindAll()
        {
            using (messageEntities entity = new messageEntities())
            {
                return entity.Set<T>().ToList();
            }
        }


        public virtual void AddRange(IList<T> datas)
        {
            using (messageEntities entity = new messageEntities())
            {
                foreach (var data in datas)
                {
                    entity.Set<T>().Add(data);
                }

                try
                {
                    entity.SaveChanges();
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException ex)
                {
                    throw ex;
                }
            }
        }

        public virtual void DeleteRange(IList<T> datas)
        {
            using (messageEntities db = new messageEntities())
            {
                IList<T> list = new List<T>();
                foreach (T data in datas)
                {
                    Type type = data.GetType();
                    PropertyInfo[] members = type.GetProperties();
                    List<string> ids = new List<string>();
                    foreach (PropertyInfo m in members)
                    {
                        var temp = m.GetCustomAttribute<KeyAttribute>();
                        if (temp != null)
                        {
                            object obj = m.GetValue(data);
                            ids.Add(obj.ToString());
                            continue;
                        }
                    }
                    T entity = db.Set<T>().Find(ids.ToArray());
                    list.Add(entity);
                }

                if (list.Count > 0)
                {
                    foreach (var l in list)
                    {
                        db.Set<T>().Remove(l);
                    }

                    db.SaveChanges();
                }
            }
        }
    }
    public class TaskService : ServiceBase<mq_task>
    {
        public IList<mq_task> GetTaskList()
        {
            using (DataAccess.messageEntities entity = new messageEntities())
            {
                return entity.mq_task.Where(p => p.enable == 1).ToList();
            }
        }
        public void Update(string taskCode, int status)
        {
            using (DataAccess.messageEntities entity = new messageEntities())
            {
                var task= entity.mq_task.FirstOrDefault(p => p.enable == 1 && p.code == taskCode);
                if (task != null)
                {
                    task.status = status;
                    entity.SaveChanges();
                }
            }
        }
    }
}
