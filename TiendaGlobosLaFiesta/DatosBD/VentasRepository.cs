using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Collections.ObjectModel;
using TiendaGlobosLaFiesta.Models;
using System.Linq; // Necesario para FirstOrDefault

namespace TiendaGlobosLaFiesta.Data
{
    public class VentasRepository
    {
        // ===================================================================
        // ===== MÉTODOS DE CONSULTA PARA KPIs (LOS QUE FALTABAN) =====
        // ===================================================================

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
            // Inicializar todos los días con 0 para que la gráfica no tenga huecos
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

        // ===================================================================
        // ===== MÉTODOS SIMPLES PARA TRANSACCIONES (LOS NUEVOS) =====
        // ===================================================================

        /// <summary>
        /// Inserta el registro maestro de una venta en la base de datos.
        /// Debe ser llamado dentro de una transacción existente.
        /// </summary>
        public void InsertarVentaMaestro(Venta venta, SqlConnection conn, SqlTransaction tran)
        {
            string queryVenta = @"INSERT INTO Venta (ventaId, clienteId, empleadoId, fechaVenta, importeTotal)
                                  VALUES (@ventaId, @clienteId, @empleadoId, @fecha, @total)";
            using var cmd = new SqlCommand(queryVenta, conn, tran);
            cmd.Parameters.AddWithValue("@ventaId", venta.VentaId);
            cmd.Parameters.AddWithValue("@clienteId", venta.ClienteId);
            cmd.Parameters.AddWithValue("@empleadoId", venta.EmpleadoId);
            cmd.Parameters.AddWithValue("@fecha", venta.FechaVenta);
            cmd.Parameters.AddWithValue("@total", venta.ImporteTotal);
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Inserta una línea de detalle para un producto vendido.
        /// Debe ser llamado dentro de una transacción existente.
        /// </summary>
        public void InsertarDetalleProducto(string ventaId, ProductoVenta p, SqlConnection conn, SqlTransaction tran)
        {
            string queryProd = @"INSERT INTO Detalle_Venta_Producto (ventaId, productoId, cantidad, costo, importe)
                                 VALUES (@ventaId, @productoId, @cantidad, @costo, @importe)";
            using var cmd = new SqlCommand(queryProd, conn, tran);
            cmd.Parameters.AddWithValue("@ventaId", ventaId);
            cmd.Parameters.AddWithValue("@productoId", p.ProductoId);
            cmd.Parameters.AddWithValue("@cantidad", p.Cantidad);
            cmd.Parameters.AddWithValue("@costo", p.Costo);
            cmd.Parameters.AddWithValue("@importe", p.Importe);
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Inserta una línea de detalle para un globo vendido.
        /// Debe ser llamado dentro de una transacción existente.
        /// </summary>
        public void InsertarDetalleGlobo(string ventaId, GloboVenta g, SqlConnection conn, SqlTransaction tran)
        {
            string queryGlobo = @"INSERT INTO Detalle_Venta_Globo (ventaId, globoId, cantidad, costo, importe)
                                  VALUES (@ventaId, @globoId, @cantidad, @costo, @importe)";
            using var cmd = new SqlCommand(queryGlobo, conn, tran);
            cmd.Parameters.AddWithValue("@ventaId", ventaId);
            cmd.Parameters.AddWithValue("@globoId", g.GloboId);
            cmd.Parameters.AddWithValue("@cantidad", g.Cantidad);
            cmd.Parameters.AddWithValue("@costo", g.Costo);
            cmd.Parameters.AddWithValue("@importe", g.Importe);
            cmd.ExecuteNonQuery();
        }

        // ===================================================================
        // ===== MÉTODO PARA HISTORIAL DE VENTAS (SIN CAMBIOS) =====
        // ===================================================================


        public List<VentaHistorial> ObtenerHistorialVentas()
        {
            // 1. Obtener las ventas principales (esta parte estaba bien)
            string queryVentas = @"
    SELECT v.ventaId, v.clienteId, v.empleadoId, v.fechaVenta, v.importeTotal,
           c.primerNombre AS primerNombreCliente, c.segundoNombre AS segundoNombreCliente,
           c.apellidoP AS apellidoPCliente, c.apellidoM AS apellidoMCliente,
           e.primerNombre AS primerNombreEmpleado, e.segundoNombre AS segundoNombreEmpleado,
           e.apellidoP AS apellidoPEmpleado, e.apellidoM AS apellidoMEmpleado
    FROM Venta v
    JOIN Cliente c ON v.clienteId = c.clienteId
    JOIN Empleado e ON v.empleadoId = e.empleadoId
    ORDER BY v.fechaVenta DESC";

            DataTable dtVentas = DbHelper.ExecuteQuery(queryVentas);
            var historial = new List<VentaHistorial>();

            foreach (DataRow row in dtVentas.Rows)
            {
                historial.Add(new VentaHistorial
                {
                    VentaId = row["ventaId"].ToString(),
                    ClienteId = row["clienteId"].ToString(),
                    ClienteNombre = $"{row["primerNombreCliente"]} {row["segundoNombreCliente"]} {row["apellidoPCliente"]} {row["apellidoMCliente"]}".Replace("  ", " ").Trim(),
                    NombreEmpleado = $"{row["primerNombreEmpleado"]} {row["segundoNombreEmpleado"]} {row["apellidoPEmpleado"]} {row["apellidoMEmpleado"]}".Replace("  ", " ").Trim(),
                    FechaVenta = Convert.ToDateTime(row["fechaVenta"]),
                    Total = Convert.ToDecimal(row["importeTotal"]),
                    Productos = new ObservableCollection<ProductoVenta>(),
                    Globos = new ObservableCollection<GloboVenta>()
                });
            }

            // 🔹 CÓDIGO RESTAURADO PARA CARGAR DETALLES DE GLOBOS 🔹
            string queryGlobos = @"
    SELECT dvg.ventaId, g.globoId, g.material, g.color, g.unidad, g.costo,
           gt.tamanio, gf.forma, t.nombre AS tematica, dvg.cantidad, dvg.importe
    FROM Detalle_Venta_Globo dvg
    JOIN Globo g ON dvg.globoId = g.globoId
    LEFT JOIN Globo_Tamanio gt ON g.globoId = gt.globoId
    LEFT JOIN Globo_Forma gf ON g.globoId = gf.globoId
    LEFT JOIN Tematica t ON g.globoId = t.globoId";

            DataTable dtGlobos = DbHelper.ExecuteQuery(queryGlobos);

            foreach (DataRow row in dtGlobos.Rows)
            {
                var venta = historial.FirstOrDefault(v => v.VentaId == row["ventaId"].ToString());
                if (venta == null) continue;

                venta.Globos.Add(new GloboVenta
                {
                    GloboId = row["globoId"].ToString(),
                    Material = row["material"].ToString(),
                    Color = row["color"].ToString(),
                    Unidad = row["unidad"].ToString(),
                    Costo = Convert.ToDecimal(row["costo"]),
                    Tamano = row["tamanio"]?.ToString(),
                    Forma = row["forma"]?.ToString(),
                    Tematica = row["tematica"]?.ToString(),
                    Cantidad = Convert.ToInt32(row["cantidad"]),
                    // El importe se calcula automáticamente en la clase base
                });
            }

            // 🔹 CÓDIGO RESTAURADO PARA CARGAR DETALLES DE PRODUCTOS 🔹
            string queryProductos = @"
    SELECT dvp.ventaId, p.productoId, p.nombre, p.unidad, p.costo, dvp.cantidad, dvp.importe
    FROM Detalle_Venta_Producto dvp
    JOIN Producto p ON dvp.productoId = p.productoId";

            DataTable dtProductos = DbHelper.ExecuteQuery(queryProductos);

            foreach (DataRow row in dtProductos.Rows)
            {
                var venta = historial.FirstOrDefault(v => v.VentaId == row["ventaId"].ToString());
                if (venta == null) continue;

                venta.Productos.Add(new ProductoVenta
                {
                    ProductoId = row["productoId"].ToString(),
                    Nombre = row["nombre"].ToString(),
                    Unidad = Convert.ToInt32(row["unidad"]),
                    Costo = Convert.ToDecimal(row["costo"]),
                    Cantidad = Convert.ToInt32(row["cantidad"])
                    // El importe se calcula automáticamente en la clase base
                });
            }

            return historial;
        }
    }
}