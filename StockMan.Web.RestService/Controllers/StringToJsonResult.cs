using StockMan.Service.Interface.Rds;
using StockMan.Service.Rds;
using StockMan.Web.RestService.Filters;
using StockMan.Facade.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Configuration;
using StockMan.Service.Cache;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using System.Threading;

namespace StockMan.Web
{
    public class StringToJsonResult : IHttpActionResult
    {
        public string Content { get; set; }
        public System.Net.Http.HttpRequestMessage Request { get; set; }
        public StringToJsonResult(String content, System.Net.Http.HttpRequestMessage request)
        {
            this.Content = content;
            this.Request = request;
        }

        System.Threading.Tasks.Task<System.Net.Http.HttpResponseMessage> IHttpActionResult.ExecuteAsync(System.Threading.CancellationToken cancellationToken)
        {
            var response = new System.Net.Http.HttpResponseMessage()
            {
                Content = new System.Net.Http.StringContent(this.Content, Encoding.UTF8, "application/json"),
                RequestMessage = this.Request,
                StatusCode = HttpStatusCode.OK
            };
            return Task.FromResult(response);
        }
    }

}