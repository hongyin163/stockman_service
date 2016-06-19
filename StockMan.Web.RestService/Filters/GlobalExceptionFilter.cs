using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http.Filters;

namespace StockMan.Web.RestService.Filters
{
    public class GlobalExceptionFilterAttribute : ExceptionFilterAttribute 
    {
        public override void OnException(HttpActionExecutedContext context)
        {
            //if (context.Exception is NotImplementedException)
            //{
            //    context.Response = new HttpResponseMessage(HttpStatusCode.NotImplemented);

            //    this.Log().Error(context.Exception.Message);
                
            //}

            Exception ex = GetInnerException(context.Exception);
            this.Log().Error(ex.Message+"\r\n"+ex.StackTrace);

        }

        public Exception GetInnerException(Exception ex)
        {
            var extpion=ex;
            while (extpion.InnerException != null)
            {
                extpion = extpion.InnerException;
            }
            return extpion;
        }

    }

    
}