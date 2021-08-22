using System.Configuration;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;
using GraphQL;
using GraphQL.Server;
using GraphQL.Server.Transports.AspNetCore;
using GraphQL.Server.Common;
using GraphQL.Types;
using Our.Umbraco.GraphQL.Adapters;
using Our.Umbraco.GraphQL.Adapters.Examine.Types;
using Our.Umbraco.GraphQL.Adapters.Examine.Visitors;
using Our.Umbraco.GraphQL.Adapters.PublishedContent.Types;
using Our.Umbraco.GraphQL.Adapters.PublishedContent.Visitors;
using Our.Umbraco.GraphQL.Adapters.Types;
using Our.Umbraco.GraphQL.Adapters.Types.Relay;
using Our.Umbraco.GraphQL.Adapters.Types.Resolution;
using Our.Umbraco.GraphQL.Adapters.Visitors;
using Our.Umbraco.GraphQL.Builders;
using Our.Umbraco.GraphQL.Composing;
using Our.Umbraco.GraphQL.Extensions;
using Our.Umbraco.GraphQL.Types.PublishedContent;
using Our.Umbraco.GraphQL.Web;
using Our.Umbraco.GraphQL.Web.Middleware;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Composing.CompositionExtensions;
using Umbraco.Core.Services.Implement;
using Umbraco.Web;
using PublishedContentQuery = Our.Umbraco.GraphQL.Types.PublishedContent.PublishedContentQuery;

namespace Our.Umbraco.GraphQL.Compose
{
    public class GraphQLComposer : ComponentComposer<GraphQLComponent>, IUserComposer
    {
        private BuiltSchema _schema;

        public override void Compose(Composition composition)
        {
            base.Compose(composition);

            composition.Register<ITypeRegistry, TypeRegistry>(Lifetime.Singleton);
            composition.RegisterFor<IGraphVisitor, CompositeGraphVisitor>(factory => new CompositeGraphVisitor(factory.GetAllInstances<GraphVisitorCollection>().Cast<IGraphVisitor>().ToArray()), Lifetime.Singleton);
            composition.Register<IGraphTypeAdapter, GraphTypeAdapter>(Lifetime.Singleton);
            composition.Register<ISchemaBuilder, SchemaBuilder>(Lifetime.Singleton);
            composition.Register<ISchema, BuiltSchema>(Lifetime.Singleton);

            composition.GraphVisitors()
                .Append<PublishedContentVisitor>()
                .Append<ExamineVisitor>();

            UmbracoDefaultOwinStartup.MiddlewareConfigured += UmbracoDefaultOwinStartupOnMiddlewareConfigured;

            if (bool.Parse(ConfigurationManager.AppSettings[Constants.EnableCorsKey]))
            {
                var corsPolicy = new EnableCorsAttribute(
            ConfigurationManager.AppSettings[Constants.AllowedOriginsKey],
            ConfigurationManager.AppSettings[Constants.AllowedHeadersKey],
            ConfigurationManager.AppSettings[Constants.AllowedMethodsKey]
                );
                GlobalConfiguration.Configuration.EnableCors(corsPolicy);
            }
            
            // Add all classes that need to be able to be resolved from the service provider
            composition.RegisterAuto<ExtendQueryWithUmbracoQuery>();
            composition.RegisterAuto<ExtendUmbracoQueryWithPublishedContentQuery>();
            composition.RegisterAuto<ExtendUmbracoQueryWithExamineQuery>();
            composition.RegisterAuto<ExamineQuery>();
            composition.RegisterAuto<PublishedContentAtRootQuery>();
            composition.RegisterAuto<PublishedContentByTypeQuery>();
            composition.RegisterAuto<PublishedContentQuery>();
            composition.RegisterAuto<UmbracoQuery>();
            composition.RegisterAuto<PublishedContentTypeGraphType>();
            composition.RegisterAuto<PublishedElementInterfaceGraphType>();
            composition.RegisterAuto<PublishedContentInterfaceGraphType>();
            composition.RegisterAuto<PublishedItemTypeGraphType>();
            composition.RegisterAuto<ContentVariationGraphType>();
            composition.RegisterAuto<BlockListItemGraphType>();
            composition.RegisterAuto<UrlModeGraphType>();
            composition.RegisterAuto<UdiGraphType>();
            composition.RegisterAuto<LinkGraphType>();
            composition.RegisterAuto<JsonGraphType>();
            composition.RegisterAuto<HtmlEncodedStringGraphType>();
            composition.RegisterAuto<SearchResultsInterfaceGraphType>();
            composition.RegisterAuto<SearchResultInterfaceGraphType>();
            composition.RegisterAuto<SearchResultFieldsGraphType>();
            composition.RegisterAuto<BooleanOperationGraphType>();
            composition.RegisterAuto<SortDirectionGraphType>();
            composition.RegisterAuto(typeof(ConnectionGraphType<>));
            composition.RegisterAuto(typeof(OrderByGraphType<>));
            composition.RegisterAuto<global::GraphQL.Types.IdGraphType>();

            ContentTypeService.Saved += (sender, args) => InvalidateSchema();
            ContentTypeService.SavedContainer += (sender, args) => InvalidateSchema();
            ContentTypeService.Moved += (sender, args) => InvalidateSchema();
            ContentTypeService.Deleted += (sender, args) => InvalidateSchema();
            ContentTypeService.DeletedContainer += (sender, args) => InvalidateSchema();

            MediaTypeService.Saved += (sender, args) => InvalidateSchema();
            MediaTypeService.SavedContainer += (sender, args) => InvalidateSchema();
            MediaTypeService.Moved += (sender, args) => InvalidateSchema();
            MediaTypeService.Deleted += (sender, args) => InvalidateSchema();
            MediaTypeService.DeletedContainer += (sender, args) => InvalidateSchema();

            MemberTypeService.Saved += (sender, args) => InvalidateSchema();
            MemberTypeService.SavedContainer += (sender, args) => InvalidateSchema();
            MemberTypeService.Moved += (sender, args) => InvalidateSchema();
            MemberTypeService.Deleted += (sender, args) => InvalidateSchema();
            MemberTypeService.DeletedContainer += (sender, args) => InvalidateSchema();

            DataTypeService.Saved += (sender, args) => InvalidateSchema();
            DataTypeService.SavedContainer += (sender, args) => InvalidateSchema();
            DataTypeService.Moved += (sender, args) => InvalidateSchema();
            DataTypeService.Deleted += (sender, args) => InvalidateSchema();
            DataTypeService.DeletedContainer += (sender, args) => InvalidateSchema();
        }

        private void UmbracoDefaultOwinStartupOnMiddlewareConfigured(object sender, OwinMiddlewareConfiguredEventArgs e)
        {
            e.AppBuilder.Use<GraphQLMiddleware>();
        }

        private void InvalidateSchema()
        {
            var container = global::Umbraco.Core.Composing.Current.Factory.Concrete as LightInject.ServiceContainer;
            _schema = container.GetInstance(typeof(BuiltSchema)) as BuiltSchema;
            _schema?.InvalidateSchema();
        }
    }
}
