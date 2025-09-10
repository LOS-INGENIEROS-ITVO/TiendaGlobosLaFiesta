using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Collections.ObjectModel;
using TiendaGlobosLaFiesta.Models;

namespace TiendaGlobosLaFiesta.Data
{
    public class VentasRepository
    {
        // Obtener ventas totales
        public decimal ObtenerVentasHoy()
        {
            string query = "SELECT ISNULL(SUM(importeTotal),0) FROM Venta WHERE CAST(fechaVenta AS DATE) = CAST(GETDATE() AS DATE)";
            return Convert.ToDecimal(DbHelper.ExecuteScalar(query) ?? 0);
        }

        public decimal ObtenerVentasUltimos7DiasTotal()
        {
            string query = "SELECT ISNULL(SUM(importeTotal),0) FROM Venta WHERE fechaVenta >= DATEADD(DAY,-6,CAST(GETDATE() AS DATE))";
            return Convert.ToDecimal(DbHelper.ExecuteScalar(query) ?? 0);
        }

        public decimal ObtenerVentasMes()
        {
            string query = "SELECT ISNULL(SUM(importeTotal),0) FROM Venta WHERE MONTH(fechaVenta) = MONTH(GETDATE()) AND YEAR(fechaVenta) = YEAR(GETDATE())";
            return Convert.ToDecimal(DbHelper.ExecuteScalar(query) ?? 0);
        }

        public decimal ObtenerTicketPromedioHoy()
        {
            string query = "SELECT ISNULL(AVG(importeTotal),0) FROM Venta WHERE CAST(fechaVenta AS DATE) = CAST(GETDATE() AS DATE)";
            return Convert.ToDecimal(DbHelper.ExecuteScalar(query) ?? 0);
        }

        public Dictionary<DateTime, decimal> ObtenerVentasUltimos7DiasDetalle()
        {
            string query = @"
        SELECT CAST(fechaVenta AS DATE) AS Fecha, ISNULL(SUM(importeTotal),0) AS Total
        FROM Venta
        WHERE fechaVenta >= DATEADD(DAY,-6,CAST(GETDATE() AS DATE))
        GROUP BY CAST(fechaVenta AS DATE)
        ORDER BY CAST(fechaVenta AS DATE)";

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

        // Registrar venta
        public bool RegistrarVenta(Venta venta)
        {
            using var conn = DbHelper.ObtenerConexion();
            using var tran = conn.BeginTransaction();

            try
            {
                // Insertar venta
                string queryVenta = @"INSERT INTO Venta (ventaId, clienteId, empleadoId, fechaVenta, importeTotal)
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
                    string queryProd = @"INSERT INTO VentaDetalle (ventaId, productoId, cantidad, costo, importe)
                                 VALUES (@ventaId, @productoId, @cantidad, @costo, @importe)";
                    using var cmd = new SqlCommand(queryProd, conn, tran);
                    cmd.Parameters.AddWithValue("@ventaId", venta.VentaId);
                    cmd.Parameters.AddWithValue("@productoId", p.ProductoId);
                    cmd.Parameters.AddWithValue("@cantidad", p.Cantidad);
                    cmd.Parameters.AddWithValue("@costo", p.Costo);
                    cmd.Parameters.AddWithValue("@importe", p.Importe);
                    cmd.ExecuteNonQuery();
                }

                // Insertar globos de la venta
                foreach (var g in venta.Globos)
                {
                    string queryGlobo = @"INSERT INTO VentaDetalle (ventaId, globoId, cantidad, costo, importe)
                                  VALUES (@ventaId, @globoId, @cantidad, @costo, @importe)";
                    using var cmd = new SqlCommand(queryGlobo, conn, tran);
                    cmd.Parameters.AddWithValue("@ventaId", venta.VentaId);
                    cmd.Parameters.AddWithValue("@globoId", g.GloboId);
                    cmd.Parameters.AddWithValue("@cantidad", g.Cantidad);
                    cmd.Parameters.AddWithValue("@costo", g.Costo);
                    cmd.Parameters.AddWithValue("@importe", g.Importe);
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

        // Historial de ventas
        public List<VentaHistorial> ObtenerHistorialVentas()
        {
            string query = @"
    SELECT v.ventaId, v.clienteId, v.empleadoId, v.fechaVenta AS fecha, v.importeTotal AS total,
           c.primerNombre AS primerNombreCliente, c.segundoNombre AS segundoNombreCliente, 
           c.apellidoP AS apellidoPCliente, c.apellidoM AS apellidoMCliente,
           e.primerNombre AS primerNombreEmpleado, e.segundoNombre AS segundoNombreEmpleado,
           e.apellidoP AS apellidoPEmpleado, e.apellidoM AS apellidoMEmpleado
    FROM Venta v
    JOIN Cliente c ON v.clienteId = c.clienteId
    JOIN Empleado e ON v.empleadoId = e.empleadoId
    ORDER BY v.fechaVenta DESC";

            DataTable dt = DbHelper.ExecuteQuery(query);
            var lista = new List<VentaHistorial>();

            foreach (DataRow row in dt.Rows)
            {
                lista.Add(new VentaHistorial
                {
                    VentaId = row["ventaId"].ToString(),
                    ClienteId = row["clienteId"].ToString(),
                    ClienteNombre = $"{row["primerNombreCliente"]} {row["segundoNombreCliente"]} {row["apellidoPCliente"]} {row["apellidoMCliente"]}"
                                    .Replace("  ", " ").Trim(),
                    NombreEmpleado = $"{row["primerNombreEmpleado"]} {row["segundoNombreEmpleado"]} {row["apellidoPEmpleado"]} {row["apellidoMEmpleado"]}"
                                     .Replace("  ", " ").Trim(),
                    FechaVenta = Convert.ToDateTime(row["fecha"]),
                    Total = Convert.ToDecimal(row["total"]),
                    Productos = new ObservableCollection<ProductoVenta>(),
                    Globos = new ObservableCollection<GloboVenta>()
                });
            }

            return lista;
        }
    }
}
