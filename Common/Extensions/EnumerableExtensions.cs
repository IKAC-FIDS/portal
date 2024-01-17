using System.Collections.Generic;

namespace TES.Common.Extensions
{
    public static class EnumerableExtensions
    {
        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
        {
            return key == null ? defaultValue : dictionary.TryGetValue(key, out var value) ? value : defaultValue;
        }
    }
}
