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
        private static ConnectionMultiplexer _clientsManager = ConnectionMultiplexer.Connect(RedisUri);
        private static ConnectionMultiplexer Client()
        {
            if (_clientsManager == null)
            {
                _clientsManager = ConnectionMultiplexer.Connect(RedisUri);
            }
            return _clientsManager;
        }

        public static void Set<T>(string key, T value)
        {
            var client = Client().GetDatabase();
            ;
            client.StringSet(key, JsonConvert.SerializeObject(value));

        }
        public static T Get<T>(string key)
        {
            var client = Client().GetDatabase();

            string str = client.StringGet(key);
            if (!string.IsNullOrEmpty(str))
            {
                return JsonConvert.DeserializeObject<T>(str);
            }
            return default(T);
        }

        public static void Set(string key, string value)
        {
            var client = Client().GetDatabase();
            client.StringSet(key, value);

        }
        public static string Get(string key)
        {
            var client = Client().GetDatabase();
            return client.StringGet(key);
        }

        public static string[] Get(string[] keys)
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
        public static T[] Get<T>(string[] keys)
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

        public static void Remove(string key)
        {
            var client = Client().GetDatabase();
            client.KeyDelete(key);
        }

    }
}
