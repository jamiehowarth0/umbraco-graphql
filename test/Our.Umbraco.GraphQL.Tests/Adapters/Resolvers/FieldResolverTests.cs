using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using FluentAssertions;
using GraphQL;
using GraphQL.Execution;
using GraphQL.Types;
using NSubstitute;
using Our.Umbraco.GraphQL.Adapters.Resolvers;
using Our.Umbraco.GraphQL.Attributes;
using Xunit;

namespace Our.Umbraco.GraphQL.Tests.Adapters.Resolvers
{
    public class FieldResolverTests
    {
        private FieldResolver CreateSUT(MemberInfo memberInfo, IServiceProvider serviceProvider = null,
            Action<FieldType> configureFieldType = null)
        {
            var fieldType = new FieldType {Metadata = {{nameof(MemberInfo), memberInfo}}};
            configureFieldType?.Invoke(fieldType);

            return new FieldResolver(fieldType, serviceProvider ?? new DefaultServiceProvider());
        }

        [Fact]
        public void Resolve_FieldDefinitionWithFieldInfo_ReturnsFieldValue()
        {
            var fieldInfo = typeof(Query).GetField(nameof(Query.Answer));
            var resolver = CreateSUT(fieldInfo);

            var result = resolver.Resolve(new ResolveFieldContext
            {
                Source = new Query()
            });

            result.Should().Be(42);
        }

        [Fact]
        public void Resolve_FieldDefinitionWithMethodInfo_ReturnsMethodValue()
        {
            var methodInfo = typeof(Query).GetMethod(nameof(Query.Ping));
            var resolver = CreateSUT(methodInfo);

            var result = resolver.Resolve(new ResolveFieldContext
            {
                Source = new Query()
            });

            result.Should().Be("Pong");
        }

        [Fact]
        public void Resolve_FieldDefinitionWithPropertyInfo_ReturnsPropertyValue()
        {
            var propertyInfo = typeof(Query).GetProperty(nameof(Query.Hello));
            var resolver = CreateSUT(propertyInfo);

            var result = resolver.Resolve(new ResolveFieldContext
            {
                Source = new Query()
            });

            result.Should().Be("World!");
        }

        [Fact]
        public void Resolve_FieldDefinitionWithMethodInfoWithArguments_ReturnsMethodValue()
        {
            var methodInfo = typeof(Query).GetMethod(nameof(Query.SayHello));
            var resolver = CreateSUT(methodInfo, configureFieldType: fieldType =>
            {
                fieldType.Arguments = new QueryArguments(new QueryArgument(typeof(StringGraphType))
                {
                    Name = "name"
                });
            });

            var result = resolver.Resolve(new ResolveFieldContext
            {
                Source = new Query(),
                Arguments = new Dictionary<string, ArgumentValue>
                {
                    {"name", new ArgumentValue("World!", ArgumentSource.Literal)}
                }
            });

            result.Should().Be("Hello World!");
        }

        [Fact]
        public void Resolve_FieldDefinitionWithMethodInfoWithDefaultValueArgument_UsesDefaultValue()
        {
            var methodInfo = typeof(Query).GetMethod(nameof(Query.SayHello));
            var resolver = CreateSUT(methodInfo, configureFieldType: fieldType =>
            {
                fieldType.Arguments = new QueryArguments(new QueryArgument(typeof(StringGraphType))
                {
                    Name = "name",
                    DefaultValue = "John"
                });
            });

            var result = resolver.Resolve(new ResolveFieldContext
            {
                Source = new Query()
            });

            result.Should().Be("Hello John");
        }

        [Fact]
        public void Resolve_FieldDefinitionWithNullableArgument_UsesDefaultValue()
        {
            var methodInfo = typeof(Query).GetMethod(nameof(Query.GetNullableArgumentValue));
            var resolver = CreateSUT(methodInfo, configureFieldType: fieldType =>
            {
                fieldType.Arguments = new QueryArguments(new QueryArgument(typeof(IntGraphType))
                {
                    Name = "value"
                });
            });
            var result = resolver.Resolve(new ResolveFieldContext
            {
                Source = new Query()
            });

            result.Should().BeNull();
        }

        [Fact]
        public void Resolve_FieldDefinitionWithMethodInfoWithInjectedArgument_ResolvesInjectedFromDependencyResolver()
        {
            var serviceProvider = Substitute.For<IServiceProvider>();
            serviceProvider.GetService(Arg.Is(typeof(Injected)))
                .Returns(new Injected());
            var methodInfo = typeof(Query).GetMethod(nameof(Query.GetInjected));
            var resolver = CreateSUT(methodInfo, serviceProvider, fieldType =>
            {
                fieldType.Arguments = new QueryArguments(new QueryArgument(typeof(InputObjectGraphType))
                {
                    Name = "injected"
                });
            });

            var result = resolver.Resolve(new ResolveFieldContext
            {
                Source = new Query()
            });

            result.Should().Be("Injected Pong");
            serviceProvider.Received()
                .GetService(typeof(Injected));
        }

        [Fact]
        public void Resolve_SourceIsNull_ResolvesSourceFromDependencyResolver()
        {
            var serviceProvider = Substitute.For<IServiceProvider>();
            serviceProvider.GetService(Arg.Is(typeof(Query)))
                .Returns(new Query
                {
                    Answer = 17,
                });
            var methodInfo = typeof(Query).GetField(nameof(Query.Answer));
            var resolver = CreateSUT(methodInfo, serviceProvider);

            var result = resolver.Resolve(new ResolveFieldContext());

            result.Should().Be(17);
            serviceProvider.Received()
                .GetService(typeof(Query));
        }

        [Fact]
        public void Resolve_SourceIsWrongType_ResolvesCorrectTypeFromDependencyResolver()
        {
            var serviceProvider = Substitute.For<IServiceProvider>();
            serviceProvider.GetService(Arg.Is(typeof(Query)))
                .Returns(new Query
                {
                    Answer = 17,
                });
            var methodInfo = typeof(Query).GetField(nameof(Query.Answer));
            var resolver = CreateSUT(methodInfo, serviceProvider);

            var result = resolver.Resolve(new ResolveFieldContext
            {
                Source = new Injected()
            });

            result.Should().Be(17);
            serviceProvider.Received()
                .GetService(typeof(Query));
        }

        [Fact]
        public void Resolve_MemberInfoIsTypeInfo_ThrowsArgumentOutOfRangeException()
        {
            var resolver = CreateSUT(typeof(Query));

            Action action = () => resolver.Resolve(new ResolveFieldContext());

            action.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void Resolve_FieldDefinitionWithMethodInfoWithCancellationTokenArgument_ResolvesCancellationTokenFromContext()
        {
            var methodInfo = typeof(Query).GetMethod(nameof(Query.WithCancallationToken));
            var resolver = CreateSUT(methodInfo, configureFieldType: fieldType =>
            {
                fieldType.Arguments = new QueryArguments(new QueryArgument(typeof(InputObjectGraphType))
                {
                    Name = "cancellationToken"
                });
            });

            var cancellationToken = new CancellationToken(true);

            var result = resolver.Resolve(new ResolveFieldContext
            {
                Source = new Query(),
                CancellationToken = cancellationToken
            });

            result.Should().Be(cancellationToken);
        }

        private class Query
        {
            public int Answer = 42;
            public string Ping() => "Pong";
            public string Hello => "World!";
            public string SayHello(string name = "John") => $"Hello {name}";
            public string GetInjected([Inject]Injected injected) => injected.Ping;
            public int? GetNullableArgumentValue(int? value) => value;
            public CancellationToken WithCancallationToken(CancellationToken cancellationToken) => cancellationToken;
        }

        private class Injected
        {
            public string Ping => "Injected Pong";
        }
    }
}
