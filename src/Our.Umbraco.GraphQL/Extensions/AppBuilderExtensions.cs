using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Owin;

namespace Our.Umbraco.GraphQL.Extensions
{
    public static class AppBuilderExtensions
    {
        public static IAppBuilder Use<T>(this IAppBuilder builder)
        {
            return builder.Use(typeof(T));
        }
    }
}
