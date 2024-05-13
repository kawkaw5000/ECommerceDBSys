using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace EcommerceShop
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            // Get the exception object
            Exception exception = Server.GetLastError();

            // Check if the exception is an HTTP exception and if its status code is 404
            if (exception is HttpException && ((HttpException)exception).GetHttpCode() == 404)
            {
                // Clear the error to prevent the Yellow Screen of Death (YSOD)
                Server.ClearError();

                // Redirect to the PageNotFound action
                Response.RedirectToRoute("PageNotFound");
            }
        }

    }
}
