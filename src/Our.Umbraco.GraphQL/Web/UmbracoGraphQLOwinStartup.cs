using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GraphQL.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Owin;
using Our.Umbraco.GraphQL.Web;
using Umbraco.Web;

[assembly:OwinStartup(typeof(UmbracoGraphQLOwinStartup))]
namespace Our.Umbraco.GraphQL.Web
{
    public class UmbracoGraphQLOwinStartup : UmbracoDefaultOwinStartup
    {
        protected void ConfigureServices(IServiceCollection services)
        {
            services.AddGraphQL(opts =>
            {
                opts.EnableMetrics = true;
            });
        }
    }
}
