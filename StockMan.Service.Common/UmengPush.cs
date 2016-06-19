using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;
using Newtonsoft.Json;
using System.Configuration;
using System.Security.Cryptography;

namespace StockMan.Service.Common
{
    public class UmengPush
    {

        /// <summary>
        /// 使用友盟API发送通知，
        /// </summary>
        /// <param name="tokens"></param>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="custommsg"></param>
        public void SendNotification(string title, string message, string custommsg)
        {
            string uploadUrl = "http://msg.umeng.com/upload";
            string apiSendUrl = "http://msg.umeng.com/api/send";
            string appkey = ConfigurationManager.AppSettings["UmengAppKey"]; //"54b614ddfd98c5cf7e001140";
            string app_master_secret = ConfigurationManager.AppSettings["UmengMasterSecret"]; //"jff9ach1iyri1lazbzmy8ebstqdloflh";
            int timestamp = ConvertDateTimeInt(DateTime.Now);

            string validationToken = GetMD5(appkey.ToLower() + app_master_secret.ToLower() + timestamp);


            //jsonObject.Add("appkey", appkey);
            //jsonObject.Add("timestamp", timestamp.ToString());
            //jsonObject.Add("validation_token", validationToken);
            //jsonObject.Add("type", "broadcast");
            //jsonObject.Add("alias_type", "");
            //jsonObject.Add("file_id", uMengUploadResult.data.file_id);
            var payloadObj = new
            {
                display_type = "notification",
                body = new
                {
                    ticker = title,
                    title = title,
                    text = message,
                    after_open = "go_app",
                    custom = custommsg
                }
            };


            //jsonObject.Add("payload", JsonConvert.SerializeObject(payload));
            var expire_time = new { expire_time = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd hh:mm:ss") };
            //jsonObject.Add("policy", JsonConvert.SerializeObject(expire_time));
            //jsonObject.Add("description", "慢牛提醒");

            var jsonObject = new
            {
                appkey = appkey,
                timestamp = timestamp.ToString(),
                //validation_token = validationToken,
                type = "broadcast",
                //device_tokens = "",
                //alias_type = "",
                //alias = "",               
                //production_mode = "true",               
                payload = new
                {
                    display_type = "notification",
                    body = new
                    {
                        ticker = title,
                        title = title,
                        text = message,
                        after_open = "go_app"
                    }
                },
                policy = new
                {
                    //start_time = "2013-10-29 12:00:00", //定时发送
                    expire_time = expire_time
                },
                description = "慢牛提醒"
            };


            string json = JsonConvert.SerializeObject(jsonObject);
            //byte[] bytpostData = Encoding.Default.GetBytes(json);
            String sign = GetMD5("POST" + apiSendUrl + json + app_master_secret);
            HttpPost(apiSendUrl + "?sign=" + sign, json);

        }

        private string HttpPost(string Url, string postDataStr)
        {

            HttpWebRequest request = HttpWebRequest.CreateHttp(Url);
            request.Method = "POST";
            //request.ContentType = "application/json";
            //request.UserAgent = "Mozilla/5.0";
            byte[] postdata = Encoding.UTF8.GetBytes(postDataStr);
            request.ContentLength = postdata.Length;

            using (Stream reqStream = request.GetRequestStream())
            {
                reqStream.Write(postdata, 0, postdata.Length);
                //reqStream.Close();
            }
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream myResponseStream = response.GetResponseStream();
                StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
                string retString = myStreamReader.ReadToEnd();
                myStreamReader.Close();
                myResponseStream.Close();

                return retString;
            }
            catch (WebException ex)
            {
                HttpWebResponse res = (HttpWebResponse)ex.Response;
                Stream myResponseStream = res.GetResponseStream();
                StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
                string retString = myStreamReader.ReadToEnd();
                return retString;
            }
        }

        /// <summary>
        /// 计算MD5
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns> 
        private string GetMD5(string paramsString)
        {
            MD5 md5 = MD5.Create();
            byte[] bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(paramsString));
            StringBuilder sb = new StringBuilder();
            foreach (byte b in bytes)
                sb.AppendFormat("{0:x2}", b);
            return sb.ToString();

        }

        /// <summary>  
        /// DateTime时间格式转换为Unix时间戳格式  
        /// </summary>  
        /// <param name="time"> DateTime时间格式</param>  
        /// <returns>Unix时间戳格式</returns>  
        private int ConvertDateTimeInt(System.DateTime time)
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
            return (int)(time - startTime).TotalSeconds;
        }
    }
}