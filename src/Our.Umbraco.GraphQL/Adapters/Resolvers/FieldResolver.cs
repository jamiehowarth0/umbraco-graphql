using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Dependencies;
using GraphQL;
using GraphQL.Resolvers;
using GraphQL.Types;
using Our.Umbraco.GraphQL.Attributes;

namespace Our.Umbraco.GraphQL.Adapters.Resolvers
{
    public class FieldResolver : IFieldResolver
    {
        private readonly FieldType _fieldType;
        private readonly IServiceProvider _dependencyResolver;

        public FieldResolver(FieldType fieldType, IServiceProvider dependencyResolver)
        {
            _fieldType = fieldType ?? throw new ArgumentNullException(nameof(fieldType));
            _dependencyResolver = dependencyResolver ?? throw new ArgumentNullException(nameof(dependencyResolver));
        }

        public object Resolve(IResolveFieldContext context)
        {
            var memberInfo = _fieldType.GetMetadata<MemberInfo>(nameof(MemberInfo));
            var source = context.Source;
            if(source == null || memberInfo.DeclaringType.IsInstanceOfType(source) == false)
                source = _dependencyResolver.GetService(memberInfo.DeclaringType);

            switch (memberInfo)
            {
                case FieldInfo fieldInfo:
                    return fieldInfo.GetValue(source);
                case MethodInfo methodInfo:
                    return CallMethod(methodInfo, source, context);
                case PropertyInfo propertyInfo:
                    return propertyInfo.GetValue(source);
                default:
                    throw new ArgumentOutOfRangeException(nameof(context));
            }
        }

        private object CallMethod(MethodInfo methodInfo, object source, IResolveFieldContext context)
        {
            var parameters = methodInfo.GetParameters().ToList();
            var arguments = new object[parameters.Count];

            var argumentsIndex = 0;
            for (var i = 0; i < parameters.Count; i++)
            {
                var parameterInfo = parameters[i];
                var parameterType = parameterInfo.ParameterType;

                if (parameterInfo.GetCustomAttribute<InjectAttribute>() != null)
                {
                    arguments[i] = _dependencyResolver.GetService(parameterType);
                    continue;
                }

                if (parameterInfo.ParameterType == typeof(CancellationToken))
                {
                    arguments[i] = context.CancellationToken;
                    continue;
                }

                var argument = _fieldType.Arguments[argumentsIndex];
                argumentsIndex++;

                arguments[i] = context.GetArgument(parameterType, parameterInfo.Name, argument.DefaultValue);
            }

            return methodInfo.Invoke(source, arguments);
        }
    }
}
