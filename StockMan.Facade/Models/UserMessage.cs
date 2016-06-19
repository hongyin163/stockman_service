using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockMan.Facade.Models
{
    public class UserMessage
    {
        public string code { get; set; }
        public string title { get; set; }
        public string content { get; set; }
        public IList<Hint> Hints { get; set; }
        public string createtime { get; set; }
    }
    /// <summary>
    /// 个性化推荐：周期成分个股，个人推荐，
    /// 系统消息：新技术上线，新数据上线，新股上市
    /// </summary>
    public class Hint
    {
        //path,button[type=dd] 
        public string type { get; set; }

        public string content { get; set; }
    }
}
