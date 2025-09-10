using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using TiendaGlobosLaFiesta.Models;

namespace TiendaGlobosLaFiesta.Data
{
    public class VentasRepository
    {
        // === MÉTODOS DE CONSULTA ===
        public decimal ObtenerVentasHoy()
        {
            string query = "SELECT ISNULL(SUM(total),0) FROM Venta WHERE CAST(fecha AS DATE) = CAST(GETDATE() AS DATE)";
            return Convert.ToDecimal(DbHelper.ExecuteScalar(query) ?? 0);
        }

        public decimal ObtenerVentasUltimos7DiasTotal()
        {
            string query = "SELECT ISNULL(SUM(total),0) FROM Venta WHERE fecha >= DATEADD(DAY,-6,CAST(GETDATE() AS DATE))";
            return Convert.ToDecimal(DbHelper.ExecuteScalar(query) ?? 0);
        }

        public decimal ObtenerVentasMes()
        {
            string query = "SELECT ISNULL(SUM(total),0) FROM Venta WHERE MONTH(fecha) = MONTH(GETDATE()) AND YEAR(fecha) = YEAR(GETDATE())";
            return Convert.ToDecimal(DbHelper.ExecuteScalar(query) ?? 0);
        }

        public decimal ObtenerTicketPromedioHoy()
        {
            string query = "SELECT ISNULL(AVG(total),0) FROM Venta WHERE CAST(fecha AS DATE) = CAST(GETDATE() AS DATE)";
            return Convert.ToDecimal(DbHelper.ExecuteScalar(query) ?? 0);
        }

        public Dictionary<DateTime, decimal> ObtenerVentasUltimos7DiasDetalle()
        {
            string query = @"
                SELECT CAST(fecha AS DATE) AS Fecha, ISNULL(SUM(total),0) AS Total
                FROM Venta
                WHERE fecha >= DATEADD(DAY,-6,CAST(GETDATE() AS DATE))
                GROUP BY CAST(fecha AS DATE)
                ORDER BY CAST(fecha AS DATE)";

            DataTable dt = DbHelper.ExecuteQuery(query);
            var resultado = new Dictionary<DateTime, decimal>();
            for (int i = 0; i < 7; i++)
                resultado[DateTime.Today.AddDays(-6 + i)] = 0m;

            foreach (DataRow row in dt.Rows)
            {
                DateTime fecha = Convert.ToDateTime(row["Fecha"]);
                decimal total = Convert.ToDecimal(row["Total"]);
                if (resultado.ContainsKey(fecha)) resultado[fecha] = total;
            }

            return resultado;
        }

        // === NUEVO MÉTODO: Registrar Venta ===
        public bool RegistrarVenta(Venta venta)
        {
            using var conn = DbHelper.ObtenerConexion();
            using var tran = conn.BeginTransaction();

            try
            {
                // Insertar venta
                string queryVenta = @"INSERT INTO Venta (ventaId, clienteId, empleadoId, fecha, total)
                                      VALUES (@ventaId, @clienteId, @empleadoId, @fecha, @total)";
                using (var cmd = new SqlCommand(queryVenta, conn, tran))
                {
                    cmd.Parameters.AddWithValue("@ventaId", venta.VentaId);
                    cmd.Parameters.AddWithValue("@clienteId", venta.ClienteId);
                    cmd.Parameters.AddWithValue("@empleadoId", venta.EmpleadoId);
                    cmd.Parameters.AddWithValue("@fecha", venta.FechaVenta);
                    cmd.Parameters.AddWithValue("@total", venta.ImporteTotal);
                    cmd.ExecuteNonQuery();
                }

                // Insertar productos de la venta
                foreach (var p in venta.Productos)
                {
                    string queryProd = @"INSERT INTO VentaDetalle (ventaId, productoId, cantidad, precio)
                                         VALUES (@ventaId, @productoId, @cantidad, @precio)";
                    using var cmd = new SqlCommand(queryProd, conn, tran);
                    cmd.Parameters.AddWithValue("@ventaId", venta.VentaId);
                    cmd.Parameters.AddWithValue("@productoId", p.ProductoId);
                    cmd.Parameters.AddWithValue("@cantidad", p.Cantidad);
                    cmd.Parameters.AddWithValue("@precio", p.Precio);
                    cmd.ExecuteNonQuery();
                }

                // Insertar globos de la venta
                foreach (var g in venta.Globos)
                {
                    string queryGlobo = @"INSERT INTO VentaDetalle (ventaId, globoId, cantidad, precio)
                                          VALUES (@ventaId, @globoId, @cantidad, @precio)";
                    using var cmd = new SqlCommand(queryGlobo, conn, tran);
                    cmd.Parameters.AddWithValue("@ventaId", venta.VentaId);
                    cmd.Parameters.AddWithValue("@globoId", g.GloboId);
                    cmd.Parameters.AddWithValue("@cantidad", g.Cantidad);
                    cmd.Parameters.AddWithValue("@precio", g.Precio);
                    cmd.ExecuteNonQuery();
                }

                tran.Commit();
                return true;
            }
            catch
            {
                tran.Rollback();
                return false;
            }
        }
    }
}