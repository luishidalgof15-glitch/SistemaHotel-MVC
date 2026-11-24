using Microsoft.Data.SqlClient;
using SistemaMVC.Models;

namespace SistemaMVC.Data
{
    public class UsuarioDAO
    {
        private readonly Conexion _cn;
        public UsuarioDAO(Conexion cn) => _cn = cn;

        public Usuario? Validar(string nombreUsuario, string clave)
        {
            const string sql = @"SELECT IdUsuario, NombreUsuario, NombreCompleto, Rol, CodOperario
                                 FROM Usuario WHERE NombreUsuario = @usuario AND Clave = @clave";
            using var con = _cn.Crear();
            using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@usuario", nombreUsuario);
            cmd.Parameters.AddWithValue("@clave", clave);
            con.Open();
            using var dr = cmd.ExecuteReader();
            if (!dr.Read()) return null;
            return new Usuario
            {
                IdUsuario = dr.GetInt32(0),
                NombreUsuario = dr.GetString(1),
                NombreCompleto = dr.GetString(2),
                Rol = dr.GetString(3),
                CodOperario = dr.IsDBNull(4) ? null : dr.GetString(4).Trim()
            };
        }
    }
}
