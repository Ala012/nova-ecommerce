using System.Text.Json;
namespace EcommerceApp.Extensions;
public static class SessionExtensions
{
    public static void SetJson<T>(this ISession s, string key, T value) => s.SetString(key, JsonSerializer.Serialize(value));
    public static T GetJson<T>(this ISession s, string key)
    {
        var v = s.GetString(key);
        return v == null ? default : JsonSerializer.Deserialize<T>(v);
    }
}
