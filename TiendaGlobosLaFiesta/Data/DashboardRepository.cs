using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using TiendaGlobosLaFiesta.Models;

namespace TiendaGlobosLaFiesta.Data
{
    public class DashboardRepository
    {
        public DashboardData ObtenerDatosDashboard()
        {
            var data = new DashboardData();

            try
            {
                using var conn = DbHelper.ObtenerConexion();
                using var cmd = new SqlCommand("sp_ObtenerKPIsDashboard", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                using var reader = cmd.ExecuteReader();

                // ======= RESULTADO 1: KPIs Numéricos =======
                if (reader.Read())
                {
                    data.VentasHoy = SafeDecimal(reader["VentasHoy"]);
                    data.Ventas7Dias = SafeDecimal(reader["Ventas7Dias"]);
                    data.VentasMes = SafeDecimal(reader["VentasMes"]);
                    data.TicketPromedioHoy = SafeDecimal(reader["TicketPromedioHoy"]);
                    data.TotalStockCritico = SafeInt(reader["TotalStockCritico"]);
                    data.TotalClientesFrecuentes = SafeInt(reader["TotalClientesFrecuentes"]);
                }

                // ======= RESULTADO 2: Top 3 Stock Crítico =======
                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        string nombre = reader["Nombre"]?.ToString() ?? "Desconocido";
                        int stock = SafeInt(reader["Stock"]);
                        data.NombresStockCritico.Add($"{nombre} ({stock})");
                    }
                }

                // ======= RESULTADO 3: Top 3 Clientes Frecuentes =======
                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        string nombreCliente = reader["NombreCliente"]?.ToString() ?? "Desconocido";
                        data.NombresClientesFrecuentes.Add(nombreCliente);
                    }
                }

                // ======= RESULTADO 4: Top Cliente del Mes =======
                if (reader.NextResult() && reader.Read())
                {
                    data.NombreTopCliente = reader["NombreTopCliente"]?.ToString() ?? "N/A";
                    data.TotalTopCliente = SafeDecimal(reader["TotalTopCliente"]);
                }

                // ======= RESULTADO 5: Productos Más Vendidos =======
                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        string periodo = reader["Periodo"]?.ToString() ?? "";
                        string nombre = reader["Nombre"]?.ToString() ?? "";
                        int cantidad = SafeInt(reader["CantidadTotal"]);
                        string texto = $"{nombre} ({cantidad})";

                        switch (periodo)
                        {
                            case "Dia": data.ProductoMasVendidoDia = texto; break;
                            case "Semana": data.ProductoMasVendidoSemana = texto; break;
                            case "Mes": data.ProductoMasVendidoMes = texto; break;
                        }
                    }
                }

                // ======= RESULTADO 6: Ventas Diarias Últimos 7 Días =======
                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        if (DateTime.TryParse(reader["Fecha"]?.ToString(), out DateTime fecha))
                        {
                            decimal total = SafeDecimal(reader["Total"]);
                            data.VentasDiarias7Dias[fecha.Date] = total;
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                // Manejo de errores de SQL
                Console.Error.WriteLine($"Error SQL: {ex.Message}");
                // Aquí se puede loguear en un sistema de logs real
            }
            catch (Exception ex)
            {
                // Manejo de errores generales
                Console.Error.WriteLine($"Error: {ex.Message}");
            }

            return data;
        }

        // ======= MÉTODOS AUXILIARES =======
        private decimal SafeDecimal(object value)
        {
            return value != DBNull.Value && decimal.TryParse(value.ToString(), out decimal result) ? result : 0m;
        }

        private int SafeInt(object value)
        {
            return value != DBNull.Value && int.TryParse(value.ToString(), out int result) ? result : 0;
        }
    }
}