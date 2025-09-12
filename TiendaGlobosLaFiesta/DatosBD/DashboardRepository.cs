// Archivo: Data/DashboardRepository.cs
using System;
using System.Collections.Generic;
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

            // Asume que DbHelper.ObtenerConexion() devuelve una SqlConnection abierta
            using var conn = DbHelper.ObtenerConexion();
            using var cmd = new SqlCommand("sp_ObtenerKPIsDashboard", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            // Abre la conexión si no está abierta (es una buena práctica asegurar)
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }

            using var reader = cmd.ExecuteReader();

            // 1. Resultado: KPIs Numéricos Principales
            if (reader.Read())
            {
                data.VentasHoy = (decimal)reader["VentasHoy"];
                data.Ventas7Dias = (decimal)reader["Ventas7Dias"];
                data.VentasMes = (decimal)reader["VentasMes"];
                data.TicketPromedioHoy = (decimal)reader["TicketPromedioHoy"];
                data.TotalStockCritico = (int)reader["TotalStockCritico"];
                data.TotalClientesFrecuentes = (int)reader["TotalClientesFrecuentes"];
            }

            // 2. Resultado: Top 3 Stock Crítico
            if (reader.NextResult())
            {
                while (reader.Read())
                {
                    data.NombresStockCritico.Add($"{reader["Nombre"]} ({reader["Stock"]})");
                }
            }

            // 3. Resultado: Top 3 Clientes Frecuentes
            if (reader.NextResult())
            {
                while (reader.Read())
                {
                    data.NombresClientesFrecuentes.Add(reader["NombreCliente"].ToString());
                }
            }

            // 4. Resultado: Top Cliente del Mes
            if (reader.NextResult() && reader.Read())
            {
                data.NombreTopCliente = reader["NombreTopCliente"].ToString();
                data.TotalTopCliente = (decimal)reader["TotalTopCliente"];
            }

            // 5. Resultado: Productos Más Vendidos (Día, Semana, Mes)
            if (reader.NextResult())
            {
                while (reader.Read())
                {
                    var periodo = reader["Periodo"].ToString();
                    var nombre = reader["Nombre"].ToString();
                    var cantidad = (int)reader["CantidadTotal"];

                    if (periodo == "Dia") data.ProductoMasVendidoDia = $"{nombre} ({cantidad})";
                    if (periodo == "Semana") data.ProductoMasVendidoSemana = $"{nombre} ({cantidad})";
                    if (periodo == "Mes") data.ProductoMasVendidoMes = $"{nombre} ({cantidad})";
                }
            }

            // 6. Resultado: Ventas Diarias para la Gráfica (Últimos 7 Días)
            if (reader.NextResult())
            {
                while (reader.Read())
                {
                    data.VentasDiarias7Dias[((DateTime)reader["Fecha"]).Date] = (decimal)reader["Total"];
                }
            }

            return data;
        }
    }
}