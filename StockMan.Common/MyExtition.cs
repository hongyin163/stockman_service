using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TC.Util;

namespace StockMan.Common
{
    public static class MyExtition
    {
        public static void Each<T1>(this IList<T1> list, Action<T1> func)
        {
            foreach (T1 t in list)
            {
                func(t);
            }
        }

        public static string Format(this string source, params object[] args)
        {
            return string.Format(source, args);
        }

        public static string GetAllExptionMessage(this Exception source)
        {
            string msg = source.Message;
            Exception ex = source;
            while (ex.InnerException != null)
            {
                msg += "\r\n" + ex.InnerException.Message;
                ex = ex.InnerException;
            }
            return msg + "\r\n";
        }
    }


}
