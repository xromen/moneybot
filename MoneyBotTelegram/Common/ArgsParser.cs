using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using Telegram.Bot.Types;

namespace MoneyBotTelegram.Common;

public static class ArgsParser<TArgs> where TArgs : new()
{
    private static Dictionary<Type, PropertyInfo[]> _propertiesDict = new();

    public static TArgs ParseArgs(string text, string? prefix = null, char separator = GlobalConstants.Separator)
    {
        var splited = text.Split(separator).Where(c => !c.Equals(prefix, StringComparison.OrdinalIgnoreCase)).ToList();

        TArgs result = (TArgs)Activator.CreateInstance(typeof(TArgs))!;

        var properties = GetProperties(typeof(TArgs));

        for (int i = 0; i < splited.Count; i += 2)
        {
            PropertyInfo property = properties.SingleOrDefault(c => c.Name == splited[i]) ?? throw new Exception($"Cant mapped {splited[i]} argument");

            var converter = TypeDescriptor.GetConverter(property.PropertyType);

            if (property.PropertyType == typeof(bool))
            {
                property.SetValue(result, true, null);
                i--;
            }
            else
            {
                var value = converter.ConvertFromString(splited[i + 1]);

                property.SetValue(result, value, null);
            }

        }

        return result;
    }

    public static string BuildArgs(TArgs args, string? prefix = null, char separator = GlobalConstants.Separator)
    {
        var sb = new StringBuilder(prefix);

        var properties = GetProperties(typeof(TArgs));

        foreach (var prop in properties)
        {
            var value = prop.GetValue(args, null);

            if (value == null)
            {
                continue;
            }

            if (value.GetType() == typeof(bool))
            {
                sb.Append(GlobalConstants.Separator + prop.Name);
            }
            else
            {
                sb.Append(GlobalConstants.Separator + prop.Name + GlobalConstants.Separator + value.ToString());
            }
        }

        return sb.ToString();
    }

    private static PropertyInfo[] GetProperties(Type type)
    {
        if (!_propertiesDict.TryGetValue(typeof(TArgs), out var properties))
        {
            properties = typeof(TArgs).GetProperties();
            _propertiesDict.Add(typeof(TArgs), properties);
        }

        return properties;
    }
}
