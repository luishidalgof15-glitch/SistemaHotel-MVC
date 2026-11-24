using System.Text.Json;

namespace SistemaMVC.Helpers
{
    public static class SessionExtensions
    {
        public static void SetObjeto<T>(this ISession session, string clave, T valor)
        {
            session.SetString(clave, JsonSerializer.Serialize(valor));
        }

        public static T? GetObjeto<T>(this ISession session, string clave)
        {
            var json = session.GetString(clave);
            return json == null ? default : JsonSerializer.Deserialize<T>(json);
        }
    }
}
