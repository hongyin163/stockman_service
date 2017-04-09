using log4net;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockMan.Service.Cache
{
    public static class CacheHelper
    {
        private static string RedisUri = ConfigurationManager.AppSettings["RedisHost"];
        private static ConnectionMultiplexer _clientsManager = null;
        private static bool _available = true;

        private static DateTime _timestamp = DateTime.Now;
        private static ILog Log = LogManager.GetLogger("CacheHelper");

        private static ConnectionMultiplexer Client()
        {
            try
            {
                if (_clientsManager == null)
                {
                    _clientsManager = ConnectionMultiplexer.Connect(RedisUri);
                }
                return _clientsManager;
            }
            catch
            {
                throw new Exception("异常：缓存客户端创建异常");
            }
        }

        public static void Set<T>(string key, T value)
        {
            try
            {
                var client = Client().GetDatabase();
                client.StringSet(key, JsonConvert.SerializeObject(value));
            }
            catch (Exception ex)
            {
                Log.Error("异常：" + ex.Message + ",堆栈：" + ex.StackTrace);
            }

        }
        public static T Get<T>(string key)
        {
            try
            {
                var client = Client().GetDatabase();

                string str = client.StringGet(key);
                if (!string.IsNullOrEmpty(str))
                {
                    return JsonConvert.DeserializeObject<T>(str);
                }
                return default(T);
            }
            catch (Exception ex)
            {
                Log.Error("异常：" + ex.Message + ",堆栈：" + ex.StackTrace);
                return default(T);
            }
        }

        public static void Set(string key, string value)
        {
            try
            {
                var client = Client().GetDatabase();
                client.StringSet(key, value);
            }
            catch (Exception ex)
            {
                Log.Error("异常：" + ex.Message + ",堆栈：" + ex.StackTrace);
            }

        }
        public static string Get(string key)
        {
            try
            {
                var client = Client().GetDatabase();
                return client.StringGet(key);
            }
            catch (Exception ex)
            {
                Log.Error("异常：" + ex.Message + ",堆栈：" + ex.StackTrace);
                return null;
            }

        }

        public static string[] Get(string[] keys)
        {
            try
            {
                var client = Client().GetDatabase();
                RedisKey[] rkeys = keys.Select(k =>
                {
                    RedisKey rk = k;
                    return rk;
                }).ToArray();

                var values = client.StringGet(rkeys);
                var results = values
                    .Where(p => !p.IsNullOrEmpty)
                    .Select(p =>
                    {
                        string str = p;
                        return str;
                    }).ToArray();

                return results;
            }
            catch (Exception ex)
            {
                Log.Error("异常：" + ex.Message + ",堆栈：" + ex.StackTrace);
                return null;
            }
        }
        public static T[] Get<T>(string[] keys)
        {
            try
            {
                var client = Client().GetDatabase();

                RedisKey[] rkeys = keys.Select(k =>
                {
                    RedisKey rk = k;
                    return rk;
                }).ToArray();

                var values = client.StringGet(rkeys);
                var results = values
                    .Where(p => !p.IsNullOrEmpty)
                    .Select(p =>
                    {
                        return JsonConvert.DeserializeObject<T>(p);

                    }).ToArray();

                return results;
            }
            catch (Exception ex)
            {
                Log.Error("异常：" + ex.Message + ",堆栈：" + ex.StackTrace);
                return null;
            }
        }

        public static void Remove(string key)
        {
            try
            {
                var client = Client().GetDatabase();
                client.KeyDelete(key);
            }
            catch (Exception ex)
            {
                Log.Error("异常：" + ex.Message + ",堆栈：" + ex.StackTrace);
            }

        }

    }
}
