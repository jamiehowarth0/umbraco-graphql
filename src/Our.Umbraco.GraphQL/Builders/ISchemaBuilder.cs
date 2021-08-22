using GraphQL.Types;

namespace Our.Umbraco.GraphQL.Builders
{
    public interface ISchemaBuilder
    {
        ISchema Build();
    }
}
