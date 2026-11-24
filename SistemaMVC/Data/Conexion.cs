using Microsoft.Data.SqlClient;

namespace SistemaMVC.Data
{
    /// <summary>Entrega conexiones a la BD BDSistemaMVC (cadena en appsettings.json).</summary>
    public class Conexion
    {
        private readonly string _cadena;

        public Conexion(IConfiguration configuracion)
        {
            _cadena = configuracion.GetConnectionString("BDSistemaMVC")
                      ?? throw new InvalidOperationException("Falta la cadena de conexión 'BDSistemaMVC'.");
        }

        public SqlConnection Crear() => new SqlConnection(_cadena);
    }
}
