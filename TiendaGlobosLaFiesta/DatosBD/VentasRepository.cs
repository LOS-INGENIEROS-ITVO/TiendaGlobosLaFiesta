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

        public void InsertarVentaMaestro(Venta venta, SqlConnection conn, SqlTransaction tran)
        {
            string queryVenta = @"INSERT INTO Venta (ventaId, clienteId, empleadoId, fechaVenta, importeTotal, Estatus)
                                  VALUES (@ventaId, @clienteId, @empleadoId, @fecha, @total, @estatus)";
            using var cmd = new SqlCommand(queryVenta, conn, tran);
            cmd.Parameters.AddWithValue("@ventaId", venta.VentaId);
            cmd.Parameters.AddWithValue("@clienteId", venta.ClienteId);
            cmd.Parameters.AddWithValue("@empleadoId", venta.EmpleadoId);
            cmd.Parameters.AddWithValue("@fecha", venta.FechaVenta);
            cmd.Parameters.AddWithValue("@total", venta.ImporteTotal);
            cmd.Parameters.AddWithValue("@estatus", venta.Estatus ?? "Completada"); // Usa el estatus del objeto o el default
            cmd.ExecuteNonQuery();
        }


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


        public List<VentaHistorial> ObtenerHistorialVentas()
        {
            string queryVentas = @"
            SELECT v.ventaId, v.clienteId, v.empleadoId, v.fechaVenta, v.importeTotal, v.Estatus,
                   c.primerNombre AS primerNombreCliente, c.segundoNombre AS segundoNombreCliente,
                   c.apellidoP AS apellidoPCliente, c.apellidoM AS apellidoMCliente,
                   e.primerNombre AS primerNombreEmpleado, e.segundoNombre AS segundoNombreEmpleado,
                   e.apellidoP AS apellidoPEmpleado, e.apellidoM AS apellidoMEmpleado
            FROM Venta v
            JOIN Cliente c ON v.clienteId = c.clienteId
            JOIN Empleado e ON v.empleadoId = e.empleadoId
            WHERE c.Activo = 1 AND e.Activo = 1  -- Solo muestra ventas de clientes y empleados activos
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
                    Estatus = row["Estatus"].ToString(),
                    Productos = new ObservableCollection<ProductoVenta>(),
                    Globos = new ObservableCollection<GloboVenta>()
                });
            }

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
                });
            }

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