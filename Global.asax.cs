using System;
using System.Collections.Generic;
using System.Linq;
using System.Net; // Required for ServicePointManager
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Optimization;

namespace SchoolTimetable
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            // 1. FORCE TLS 1.2 - This fixes the "NetworkError" when connecting to Stripe
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}