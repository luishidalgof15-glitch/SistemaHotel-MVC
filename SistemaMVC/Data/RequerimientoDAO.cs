using Microsoft.Data.SqlClient;
using SistemaMVC.Models;

namespace SistemaMVC.Data
{
    public class RequerimientoDAO
    {
        private readonly Conexion _cn;
        public RequerimientoDAO(Conexion cn) => _cn = cn;

        /// <summary>Pasos 7 y 8 del CU: autogenera el número y graba el requerimiento con su detalle.</summary>
        public int Grabar(Requerimiento r)
        {
            using var con = _cn.Crear();
            con.Open();
            using var tx = con.BeginTransaction();
            try
            {
                int numero;
                using (var cmd = new SqlCommand(
                    "SELECT ISNULL(MAX(NumRequerimiento), 0) FROM Requerimiento WITH (UPDLOCK, HOLDLOCK)", con, tx))
                {
                    numero = (int)cmd.ExecuteScalar()! + 1;
                }

                using (var cmd = new SqlCommand(@"INSERT INTO Requerimiento (NumRequerimiento, CodOperario, Fecha, Estado)
                        VALUES (@num, @ope, GETDATE(), 'Pendiente')", con, tx))
                {
                    cmd.Parameters.AddWithValue("@num", numero);
                    cmd.Parameters.AddWithValue("@ope", r.CodOperario);
                    cmd.ExecuteNonQuery();
                }

                foreach (var d in r.Detalles)
                {
                    using var cmd = new SqlCommand(@"INSERT INTO DetalleRequerimiento (NumRequerimiento, CodMaterial, Cantidad)
                            VALUES (@num, @mat, @cant)", con, tx);
                    cmd.Parameters.AddWithValue("@num", numero);
                    cmd.Parameters.AddWithValue("@mat", d.CodMaterial);
                    cmd.Parameters.AddWithValue("@cant", d.Cantidad);
                    cmd.ExecuteNonQuery();
                }

                tx.Commit();
                return numero;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }
    }
}
