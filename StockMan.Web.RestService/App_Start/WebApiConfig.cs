using StockMan.EntityModel;
using StockMan.Web.RestService.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Extensions;
namespace StockMan.Web.RestService
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.EnableCors();
        
            //config.Routes.MapHttpRoute(
            //    name: "DefaultApi",
            //    routeTemplate: "api/{controller}/{id}",
            //    defaults: new { id = RouteParameter.Optional }
            //);
            config.Routes.MapHttpRoute(
                "ActionApi",
                "api/{controller}/{action}/{id}",
                new { id = RouteParameter.Optional });

            config.Routes.MapHttpRoute(
                "MethodApi",
                "api/{controller}/{action}/{p1}/{p2}/{p3}/{p4}",
                new { p1 = RouteParameter.Optional, p2 = RouteParameter.Optional, p3 = RouteParameter.Optional, p4 = RouteParameter.Optional });


            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<sys_goodidea>("GoodIdea");
            builder.EntitySet<sys_comments>("Comments");
            builder.EntitySet<stock_new>("StockNew");
            builder.EntitySet<reco_stock_category_index>("Recommend");
            builder.EntitySet<user_message>("UserMessage");
            config.Routes.MapODataServiceRoute("odata", "odata", builder.GetEdmModel());

            // Uncomment the following line of code to enable query support for actions with an IQueryable or IQueryable<T> return type.
            // To avoid processing unexpected or malicious queries, use the validation settings on QueryableAttribute to validate incoming queries.
            // For more information, visit http://go.microsoft.com/fwlink/?LinkId=279712.
            config.EnableQuerySupport();

            // To disable tracing in your application, please comment out or remove the following line of code
            // For more information, refer to: http://www.asp.net/web-api
            config.MapHttpAttributeRoutes();

            config.Filters.Add(new GlobalExceptionFilterAttribute());
        }
    }
}
