using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace StockMan.Message.Task.Biz
{
    public class SerializeHelper
    {
        public static byte[] BinarySerialize(object obj)
        {
            MemoryStream ms = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(ms, obj);
            byte[] data = ms.ToArray();
            ms.Close();
            return data;
        }
        public static object BinaryDeserialize(byte[] data)
        {
            MemoryStream ms = new MemoryStream(data);
            ms.Position = 0;
            BinaryFormatter formatter = new BinaryFormatter();
            object obj = formatter.Deserialize(ms);
            ms.Close();
            return obj;
        }
    }
}
