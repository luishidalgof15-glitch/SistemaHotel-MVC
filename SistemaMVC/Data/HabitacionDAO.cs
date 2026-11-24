using Microsoft.Data.SqlClient;
using SistemaMVC.Models;

namespace SistemaMVC.Data
{
    public class HabitacionDAO
    {
        private readonly Conexion _cn;
        public HabitacionDAO(Conexion cn) => _cn = cn;

        private static Habitacion Mapear(SqlDataReader dr) => new Habitacion
        {
            NumHabitacion = dr.GetString(0),
            Tipo = dr.GetString(1),
            CostoDia = dr.GetDecimal(2),
            Estado = dr.GetString(3)
        };

        public Habitacion? Buscar(string numHabitacion)
        {
            const string sql = "SELECT NumHabitacion, Tipo, CostoDia, Estado FROM Habitacion WHERE NumHabitacion = @num";
            using var con = _cn.Crear();
            using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@num", numHabitacion);
            con.Open();
            using var dr = cmd.ExecuteReader();
            return dr.Read() ? Mapear(dr) : null;
        }

        /// <summary>CU incluido "Buscar Habitación": solo las que están en estado Disponible.</summary>
        public List<Habitacion> ListarDisponibles()
        {
            const string sql = @"SELECT NumHabitacion, Tipo, CostoDia, Estado FROM Habitacion
                                 WHERE Estado = 'Disponible' ORDER BY NumHabitacion";
            var lista = new List<Habitacion>();
            using var con = _cn.Crear();
            using var cmd = new SqlCommand(sql, con);
            con.Open();
            using var dr = cmd.ExecuteReader();
            while (dr.Read()) lista.Add(Mapear(dr));
            return lista;
        }
    }
}
