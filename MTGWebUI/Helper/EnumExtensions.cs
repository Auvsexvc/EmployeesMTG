using MTGWebUI.Enums;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace MTGWebUI.Helper
{
    public static class EnumExtensions
    {
        public static string GetDisplayName(this Operation value)
        {
            var dictionary = GetEnumValueNames(value.GetType());
            return dictionary[(int)value];
        }

        private static IDictionary<int, string> GetEnumValueNames(Type type)
        {
            var names = type.GetFields(BindingFlags.Public | BindingFlags.Static)
                .Select(f => f.GetCustomAttribute<DisplayAttribute>()?.Name ?? f.Name);

            var values = Enum.GetValues(type).Cast<int>();

            var dictionary = names.Zip(values, (n, v) => new KeyValuePair<int, string>(v, n))
                .ToDictionary(kv => kv.Key, kv => kv.Value);

            return dictionary;
        }
    }
}