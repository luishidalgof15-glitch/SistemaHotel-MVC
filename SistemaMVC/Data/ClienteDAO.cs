using Microsoft.Data.SqlClient;
using SistemaMVC.Models;

namespace SistemaMVC.Data
{
    public class ClienteDAO
    {
        private readonly Conexion _cn;
        public ClienteDAO(Conexion cn) => _cn = cn;

        private static Cliente Mapear(SqlDataReader dr) => new Cliente
        {
            CodCliente = dr.GetString(0).Trim(),
            Nombres = dr.GetString(1),
            Apellidos = dr.GetString(2),
            DNI = dr.IsDBNull(3) ? null : dr.GetString(3).Trim(),
            Telefono = dr.IsDBNull(4) ? null : dr.GetString(4)
        };

        /// <summary>CU incluido "Buscar Cliente": busca por código.</summary>
        public Cliente? Buscar(string codCliente)
        {
            const string sql = "SELECT CodCliente, Nombres, Apellidos, DNI, Telefono FROM Cliente WHERE CodCliente = @cod";
            using var con = _cn.Crear();
            using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@cod", codCliente);
            con.Open();
            using var dr = cmd.ExecuteReader();
            return dr.Read() ? Mapear(dr) : null;
        }

        public List<Cliente> Listar()
        {
            const string sql = "SELECT CodCliente, Nombres, Apellidos, DNI, Telefono FROM Cliente ORDER BY Apellidos, Nombres";
            var lista = new List<Cliente>();
            using var con = _cn.Crear();
            using var cmd = new SqlCommand(sql, con);
            con.Open();
            using var dr = cmd.ExecuteReader();
            while (dr.Read()) lista.Add(Mapear(dr));
            return lista;
        }

        /// <summary>Punto de extensión "Agregar Cliente": registra y devuelve el código generado (C0001...).</summary>
        public string Registrar(Cliente c)
        {
            const string sqlCodigo = @"SELECT 'C' + RIGHT('0000' + CAST(ISNULL(MAX(CAST(SUBSTRING(CodCliente, 2, 4) AS INT)), 0) + 1 AS VARCHAR(4)), 4)
                                       FROM Cliente WITH (UPDLOCK, HOLDLOCK)";
            const string sqlInsert = @"INSERT INTO Cliente (CodCliente, Nombres, Apellidos, DNI, Telefono)
                                       VALUES (@cod, @nom, @ape, @dni, @tel)";
            using var con = _cn.Crear();
            con.Open();
            using var tx = con.BeginTransaction();
            try
            {
                string codigo;
                using (var cmd = new SqlCommand(sqlCodigo, con, tx))
                    codigo = (string)cmd.ExecuteScalar()!;

                using (var cmd = new SqlCommand(sqlInsert, con, tx))
                {
                    cmd.Parameters.AddWithValue("@cod", codigo);
                    cmd.Parameters.AddWithValue("@nom", c.Nombres.Trim());
                    cmd.Parameters.AddWithValue("@ape", c.Apellidos.Trim());
                    cmd.Parameters.AddWithValue("@dni", (object?)c.DNI ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@tel", (object?)c.Telefono ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
                tx.Commit();
                return codigo;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }
    }
}
