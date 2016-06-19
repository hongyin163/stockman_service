using System;
using System.Web;

namespace StockMan.Web.RestService
{
    public class WebApiModule : IHttpModule
    {
        /// <summary>
        /// You will need to configure this module in the Web.config file of your
        /// web and register it with IIS before being able to use it. For more information
        /// see the following link: http://go.microsoft.com/?linkid=8101007
        /// </summary>
        #region IHttpModule Members

        public void Dispose()
        {
            //clean-up code here.
        }

        public void Init(HttpApplication context)
        {
            // Below is an example of how you can handle LogRequest event and provide 
            // custom logging implementation for it
            //context.LogRequest += new EventHandler(OnLogRequest);

            context.BeginRequest += new EventHandler(application_BeginRequest);
        }

        #endregion
        public void application_BeginRequest(object sender, EventArgs e)
        {
            HttpContext context = (sender as HttpApplication).Context;
            if (context.Request.HttpMethod == "OPTIONS")
            {
                context.Response.StatusCode = 200;
                context.Response.End();
            }
           

        }
        public void OnLogRequest(Object source, EventArgs e)
        {
            //custom logging logic can go here
        }
    }
}
