using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockMan.Service.Common
{
    public class PushHelper
    {
        public static void Push(string title, string message, bool isMessage = false)
        {

            UmengPush push = new UmengPush();
            push.SendNotification(title, message, string.Empty);

        }
    }
}
