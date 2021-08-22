using Umbraco.Core.Composing;

namespace Our.Umbraco.GraphQL.Extensions
{
    public static class CompositionExtensions
    {
        public static void RegisterAuto<T>(this Composition composition)
        {
            composition.RegisterAuto(typeof(T));
        }
    }
}
