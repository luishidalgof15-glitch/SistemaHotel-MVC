using Microsoft.Data.SqlClient;
using SistemaMVC.Models;

namespace SistemaMVC.Data
{
    public class ReservaDAO
    {
        private readonly Conexion _cn;
        public ReservaDAO(Conexion cn) => _cn = cn;

        /// <summary>
        /// Pasos 16 y 17 del CU: obtiene el último número, autogenera el nuevo, graba la reserva
        /// con su detalle y cambia las habitaciones a "Reservado". Todo en una transacción.
        /// </summary>
        public int Grabar(Reserva r)
        {
            using var con = _cn.Crear();
            con.Open();
            using var tx = con.BeginTransaction();
            try
            {
                int numero;
                using (var cmd = new SqlCommand(
                    "SELECT ISNULL(MAX(NumReserva), 0) FROM Reserva WITH (UPDLOCK, HOLDLOCK)", con, tx))
                {
                    numero = (int)cmd.ExecuteScalar()! + 1;
                }

                using (var cmd = new SqlCommand(@"INSERT INTO Reserva
                        (NumReserva, CodCliente, FechaRegistro, FechaLlegada, FechaSalida, CantidadDias, SubTotal, IGV, MontoTotal)
                        VALUES (@num, @cli, GETDATE(), @lle, @sal, @dias, @sub, @igv, @tot)", con, tx))
                {
                    cmd.Parameters.AddWithValue("@num", numero);
                    cmd.Parameters.AddWithValue("@cli", r.CodCliente);
                    cmd.Parameters.AddWithValue("@lle", r.FechaLlegada.Date);
                    cmd.Parameters.AddWithValue("@sal", r.FechaSalida.Date);
                    cmd.Parameters.AddWithValue("@dias", r.CantidadDias);
                    cmd.Parameters.AddWithValue("@sub", r.SubTotal);
                    cmd.Parameters.AddWithValue("@igv", r.IGV);
                    cmd.Parameters.AddWithValue("@tot", r.MontoTotal);
                    cmd.ExecuteNonQuery();
                }

                foreach (var d in r.Detalles)
                {
                    using (var cmd = new SqlCommand(@"INSERT INTO DetalleReserva
                            (NumReserva, NumHabitacion, NombreHuesped, CostoDia, Dias, MontoPagar)
                            VALUES (@num, @hab, @hue, @costo, @dias, @monto)", con, tx))
                    {
                        cmd.Parameters.AddWithValue("@num", numero);
                        cmd.Parameters.AddWithValue("@hab", d.NumHabitacion);
                        cmd.Parameters.AddWithValue("@hue", d.NombreHuesped);
                        cmd.Parameters.AddWithValue("@costo", d.CostoDia);
                        cmd.Parameters.AddWithValue("@dias", d.Dias);
                        cmd.Parameters.AddWithValue("@monto", d.MontoPagar);
                        cmd.ExecuteNonQuery();
                    }

                    using (var cmd = new SqlCommand(@"UPDATE Habitacion SET Estado = 'Reservado'
                            WHERE NumHabitacion = @hab AND Estado = 'Disponible'", con, tx))
                    {
                        cmd.Parameters.AddWithValue("@hab", d.NumHabitacion);
                        if (cmd.ExecuteNonQuery() == 0)
                            throw new InvalidOperationException(
                                $"La habitación {d.NumHabitacion} ya no está disponible. Elimínela de la cuadrícula.");
                    }
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
