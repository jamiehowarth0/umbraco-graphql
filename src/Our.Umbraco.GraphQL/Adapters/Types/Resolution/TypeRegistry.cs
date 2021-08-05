using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using GraphQL;
using GraphQL.Types;
using GraphQL.Types.Relay;
using GraphQL.Types.Relay.DataObjects;
using Newtonsoft.Json.Linq;
using Our.Umbraco.GraphQL.Types;

namespace Our.Umbraco.GraphQL.Adapters.Types.Resolution
{
    public class TypeRegistry : ITypeRegistry
    {
        private readonly Dictionary<TypeInfo, TypeInfo> _types = new Dictionary<TypeInfo, TypeInfo>();
        private readonly Dictionary<TypeInfo, List<TypeInfo>> _extends = new Dictionary<TypeInfo, List<TypeInfo>>();

        public TypeRegistry()
        {
            Add<string, StringGraphType>();
            Add<byte, IntGraphType>();
            Add<short, IntGraphType>();
            Add<ushort, IntGraphType>();
            Add<int, IntGraphType>();
            Add<uint, IntGraphType>();
            Add<long, IntGraphType>();
            Add<ulong, IntGraphType>();
            Add<decimal, DecimalGraphType>();
            Add<double, FloatGraphType>();
            Add<float, FloatGraphType>();
            Add<bool, BooleanGraphType>();
            Add<Guid, GuidGraphType>();
            Add<DateTime, DateTimeGraphType>();
            Add<DateTimeOffset, DateTimeOffsetGraphType>();
            Add<TimeSpan, TimeSpanMillisecondsGraphType>();
            Add<IHtmlString, HtmlGraphType>();
            Add<Uri, UriGraphType>();
            Add<Id, IdGraphType>();
            Add<PageInfo, PageInfoType>();
            Add<JToken, JsonGraphType>();
            Add<JObject, JsonGraphType>();
            Add<JArray, JsonGraphType>();
        }

        public void Add<TType, TGraphType>() where TGraphType : IGraphType =>
            _types.Add(typeof(TType).GetTypeInfo(), typeof(TGraphType).GetTypeInfo());

        public TypeInfo Get(TypeInfo type)
        {
            if (type.IsNullable() && type.IsGenericType)
                type = type.GenericTypeArguments[0].GetTypeInfo();

            return _types.TryGetValue(type, out var graphType) ? graphType : null;
        }

        public void Extend<TExtend, TWith>()
        {
            var extend = typeof(TExtend).GetTypeInfo();
            var with = typeof(TWith).GetTypeInfo();

            if (_extends.TryGetValue(extend, out var list) == false)
            {
                list = new List<TypeInfo>();
                _extends.Add(extend, list);
            }

            list.Add(with);
        }

        public IEnumerable<TypeInfo> GetExtending(TypeInfo type) =>
            _extends.TryGetValue(type, out var list)
                ? list.AsReadOnly()
                : Enumerable.Empty<TypeInfo>();
    }
}
