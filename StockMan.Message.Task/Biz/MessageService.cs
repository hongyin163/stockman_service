using StockMan.Message.DataAccess;
using StockMan.Message.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockMan.Message.Task.Biz
{
    public class MessageService : ServiceBase<mq_message>
    {
        public void UpdateStatus(string msg_code, MessageStatus status)
        {
            using (var entity = new messageEntities())
            {
                var m = entity.mq_message.FirstOrDefault(p => p.code == msg_code);
                if (m != null)
                {
                    m.status = (int)status;
                    m.updatetime = DateTime.Now;
                    entity.SaveChanges();
                }
            }
        }

        public IList<mq_message> GetUnHandleMessage(string task_code)
        {
            using (var entity = new messageEntities())
            {
                var code = task_code;
                return entity.mq_message
                    .Where(p => p.task_code == code && (p.status == (int)MessageStatus.UnHandle || p.status == (int)MessageStatus.Retry))
                   .ToList();
            }
        }

        public void RemoveAll()
        {
            using (var entity = new messageEntities())
            {
                entity.Database.ExecuteSqlCommand("delete from mq_message");
            }
        }
    }
}
