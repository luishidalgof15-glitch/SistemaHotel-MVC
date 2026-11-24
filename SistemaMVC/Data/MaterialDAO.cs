using Microsoft.Data.SqlClient;
using SistemaMVC.Models;

namespace SistemaMVC.Data
{
    public class MaterialDAO
    {
        private readonly Conexion _cn;
        public MaterialDAO(Conexion cn) => _cn = cn;

        private static Material Mapear(SqlDataReader dr) => new Material
        {
            CodMaterial = dr.GetString(0).Trim(),
            Nombre = dr.GetString(1),
            Marca = dr.GetString(2),
            UnidadMedida = dr.GetString(3),
            Stock = dr.GetInt32(4)
        };

        /// <summary>CU incluido "Consultar Materiales".</summary>
        public List<Material> Listar()
        {
            const string sql = "SELECT CodMaterial, Nombre, Marca, UnidadMedida, Stock FROM Material ORDER BY CodMaterial";
            var lista = new List<Material>();
            using var con = _cn.Crear();
            using var cmd = new SqlCommand(sql, con);
            con.Open();
            using var dr = cmd.ExecuteReader();
            while (dr.Read()) lista.Add(Mapear(dr));
            return lista;
        }

        public Material? Buscar(string codMaterial)
        {
            const string sql = "SELECT CodMaterial, Nombre, Marca, UnidadMedida, Stock FROM Material WHERE CodMaterial = @cod";
            using var con = _cn.Crear();
            using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@cod", codMaterial);
            con.Open();
            using var dr = cmd.ExecuteReader();
            return dr.Read() ? Mapear(dr) : null;
        }
    }
}
