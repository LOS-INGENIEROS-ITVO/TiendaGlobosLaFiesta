using System.Data;
using System.Data.SqlClient;
using TiendaGlobosLaFiesta.Modelos;

namespace TiendaGlobosLaFiesta.Data
{
    public class DashboardRepository
    {
        public DashboardData ObtenerDatosDashboard()
        {
            var data = new DashboardData();

            using var conn = DbHelper.ObtenerConexion();
            using var cmd = new SqlCommand("sp_ObtenerKPIsDashboard", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            using var reader = cmd.ExecuteReader();

            // Resultado 1: KPIs Numéricos
            if (reader.Read())
            {
                data.VentasHoy = reader["VentasHoy"] != DBNull.Value ? Convert.ToDecimal(reader["VentasHoy"]) : 0;
                data.Ventas7Dias = reader["Ventas7Dias"] != DBNull.Value ? Convert.ToDecimal(reader["Ventas7Dias"]) : 0;
                data.VentasMes = reader["VentasMes"] != DBNull.Value ? Convert.ToDecimal(reader["VentasMes"]) : 0;
                data.TicketPromedioHoy = reader["TicketPromedioHoy"] != DBNull.Value ? Convert.ToDecimal(reader["TicketPromedioHoy"]) : 0;
                data.TotalStockCritico = reader["TotalStockCritico"] != DBNull.Value ? Convert.ToInt32(reader["TotalStockCritico"]) : 0;
                data.TotalClientesFrecuentes = reader["TotalClientesFrecuentes"] != DBNull.Value ? Convert.ToInt32(reader["TotalClientesFrecuentes"]) : 0;
            }

            // Resultado 2: Top 3 Stock Crítico
            if (reader.NextResult())
            {
                while (reader.Read())
                {
                    data.NombresStockCritico.Add($"{reader["Nombre"]} ({reader["Stock"]})");
                }
            }

            // Resultado 3: Top 3 Clientes Frecuentes
            if (reader.NextResult())
            {
                while (reader.Read())
                {
                    data.NombresClientesFrecuentes.Add(reader["NombreCliente"].ToString());
                }
            }

            // Resultado 4: Top Cliente Mes
            if (reader.NextResult() && reader.Read())
            {
                data.NombreTopCliente = reader["NombreTopCliente"].ToString();
                data.TotalTopCliente = Convert.ToDecimal(reader["TotalTopCliente"]);
            }

            // Resultado 5: Productos Más Vendidos
            if (reader.NextResult())
            {
                while (reader.Read())
                {
                    var periodo = reader["Periodo"].ToString();
                    var nombre = reader["Nombre"].ToString();
                    var cantidad = Convert.ToInt32(reader["CantidadTotal"]);
                    string texto = $"{nombre} ({cantidad})";

                    switch (periodo)
                    {
                        case "Dia": data.ProductoMasVendidoDia = texto; break;
                        case "Semana": data.ProductoMasVendidoSemana = texto; break;
                        case "Mes": data.ProductoMasVendidoMes = texto; break;
                    }
                }
            }

            // Resultado 6: Ventas diarias para la gráfica
            if (reader.NextResult())
            {
                while (reader.Read())
                {
                    data.VentasDiarias7Dias[Convert.ToDateTime(reader["Fecha"]).Date] = Convert.ToDecimal(reader["Total"]);
                }
            }

            return data;
        }
    }
}