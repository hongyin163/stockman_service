using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace StockMan.Web.RestService
{
    /// <summary>
    /// Summary description for JsonProxy
    /// </summary>
    public class JsonProxy : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            //context.Response.ContentType = "text/json";
            //context.Response.Write("Hello World");

            string p1 = context.Request["url"];

            byte[] bytes = Convert.FromBase64String(p1);

            string url = Encoding.Default.GetString(bytes);

            var jsonString = GetJsonData(url);

            var cb = HttpContext.Current.Request["callback"];
            String responseString = "";
            if (!String.IsNullOrEmpty(cb))
            {
                responseString = cb + "(" + jsonString + ")";
            }
            else
            {
                responseString = jsonString;
            }
            context.Response.Write(responseString);
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
        public string GetJsonData(string reqUrl)
        {
            var url = HttpContext.Current.Server.UrlDecode(reqUrl);
            HttpWebRequest web = HttpWebRequest.CreateHttp(url);
            //web.Proxy = new WebProxy("proxy1.bj.petrochina", 8080);
            var rsp = web.GetResponse();
            var stream = rsp.GetResponseStream();

            if (stream != null)
            {
                var sr = new StreamReader(stream, System.Text.Encoding.Default);
                var content = sr.ReadToEnd();

                return content;
            }
            return string.Empty;
        }
    }
}