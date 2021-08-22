using System;
using FluentAssertions;
using GraphQL.Types;
using GraphQL.Types.Relay;
using Our.Umbraco.GraphQL.Adapters.PublishedContent.Types;
using Xunit;
using IdGraphType = Our.Umbraco.GraphQL.Adapters.Types.IdGraphType;

namespace Our.Umbraco.GraphQL.Tests.Adapters.PublishedContent.Types
{
    public class PublishedContentInterfaceGraphTypeTests
    {
        [Fact]
        public void Ctor_SetsName()
        {
            var graphType = new PublishedContentInterfaceGraphType();

            graphType.Name.Should().Be("PublishedContent");
        }

        [Theory]
        [InlineData("_ancestors", typeof(ConnectionType<PublishedContentInterfaceGraphType, EdgeType<PublishedContentInterfaceGraphType>>))]
        [InlineData("_children", typeof(ConnectionType<PublishedContentInterfaceGraphType, EdgeType<PublishedContentInterfaceGraphType>>))]
        [InlineData("_createDate", typeof(NonNullGraphType<DateTimeGraphType>))]
        [InlineData("_creatorName", typeof(NonNullGraphType<StringGraphType>))]
        [InlineData("_contentType", typeof(NonNullGraphType<PublishedContentTypeGraphType>))]
        [InlineData("_id", typeof(NonNullGraphType<IdGraphType>))]
        [InlineData("_level", typeof(NonNullGraphType<IntGraphType>))]
        [InlineData("_name", typeof(StringGraphType))]
        [InlineData("_parent", typeof(PublishedContentInterfaceGraphType))]
        [InlineData("_sortOrder", typeof(NonNullGraphType<IntGraphType>))]
        [InlineData("_url", typeof(StringGraphType))]
        [InlineData("_updateDate", typeof(DateTimeGraphType))]
        [InlineData("_writerName", typeof(NonNullGraphType<StringGraphType>))]
        public void Fields_Type_ShouldBeOfExpectedType(string field, Type type)
        {
            var graphType = new PublishedContentInterfaceGraphType();

            graphType.Fields.Should().Contain(x => x.Name == field)
                .Which.Type.Should().Be(type);
        }

        [Theory]
        [InlineData("_name", "culture", typeof(StringGraphType))]
        [InlineData("_url", "culture", typeof(StringGraphType))]
        [InlineData("_url", "mode", typeof(UrlModeGraphType))]
        [InlineData("_updateDate", "culture", typeof(StringGraphType))]
        public void Fields_Arguments_ShouldBeOfExpectedType(string field, string argument, Type type)
        {
            var graphType = new PublishedContentInterfaceGraphType();

            graphType.Fields.Should().Contain(x => x.Name == field)
                .Which.Arguments.Should().Contain(x => x.Name == argument)
                .Which.Type.Should().Be(type);
        }
    }
}
