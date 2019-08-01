using System;
using System.Collections.Generic;
using GraphQL.Types;
using Umbraco.Core.Models.PublishedContent;

namespace Our.Umbraco.GraphQL.ValueResolvers
{
    [DefaultGraphQLValueResolver]
    public class BooleanValueResolver : GraphQLValueResolver
    {
        public override Type GetGraphQLType(IPublishedPropertyType propertyType)
        {
            return propertyType.ClrType == typeof(bool)
                ? typeof(BooleanGraphType)
                : typeof(ListGraphType<BooleanGraphType>);
        }

        public override bool IsResolver(IPublishedPropertyType propertyType)
        {
            return propertyType.ClrType == typeof(bool) ||
                   propertyType.ClrType == typeof(IEnumerable<bool>);
        }
    }
}
