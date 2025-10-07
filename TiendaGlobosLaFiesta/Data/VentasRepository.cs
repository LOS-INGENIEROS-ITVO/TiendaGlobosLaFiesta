using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using TiendaGlobosLaFiesta.Modelos;
using TiendaGlobosLaFiesta.Models;

namespace TiendaGlobosLaFiesta.Data
{
    public class VentasRepository
    {
        #region INSERTAR VENTAS (MAESTRO Y DETALLE)

        public void InsertarVentaMaestro(Venta venta, SqlConnection conn, SqlTransaction tran)
        {
            if (venta == null)
                throw new ArgumentNullException(nameof(venta));

            const string query = @"
                INSERT INTO Venta (ventaId, clienteId, empleadoId, fechaVenta, importeTotal, Estatus)
                VALUES (@ventaId, @clienteId, @empleadoId, @fecha, @total, @estatus)";

            using var cmd = new SqlCommand(query, conn, tran);
            cmd.Parameters.Add("@ventaId", SqlDbType.VarChar).Value = venta.VentaId;
            cmd.Parameters.Add("@clienteId", SqlDbType.VarChar).Value = venta.ClienteId;
            cmd.Parameters.Add("@empleadoId", SqlDbType.Int).Value = venta.EmpleadoId;
            cmd.Parameters.Add("@fecha", SqlDbType.DateTime).Value = venta.FechaVenta;
            cmd.Parameters.Add("@total", SqlDbType.Decimal).Value = venta.ImporteTotal;
            cmd.Parameters.Add("@estatus", SqlDbType.VarChar).Value = venta.Estatus ?? "Completada";
            cmd.ExecuteNonQuery();
        }

        public void InsertarDetalleProducto(string ventaId, ProductoVenta producto, SqlConnection conn, SqlTransaction tran)
        {
            if (producto == null)
                throw new ArgumentNullException(nameof(producto));

            const string query = @"
                INSERT INTO Detalle_Venta_Producto (ventaId, productoId, cantidad, costo, importe)
                VALUES (@ventaId, @productoId, @cantidad, @costo, @importe)";

            using var cmd = new SqlCommand(query, conn, tran);
            cmd.Parameters.Add("@ventaId", SqlDbType.VarChar).Value = ventaId;
            cmd.Parameters.Add("@productoId", SqlDbType.VarChar).Value = producto.ProductoId;
            cmd.Parameters.Add("@cantidad", SqlDbType.Int).Value = producto.Cantidad;
            cmd.Parameters.Add("@costo", SqlDbType.Decimal).Value = producto.Costo;
            cmd.Parameters.Add("@importe", SqlDbType.Decimal).Value = producto.Importe;
            cmd.ExecuteNonQuery();

            ActualizarStockProducto(producto.ProductoId, producto.Cantidad, ventaId, conn, tran);
        }

        public void InsertarDetalleGlobo(string ventaId, GloboVenta globo, SqlConnection conn, SqlTransaction tran)
        {
            if (globo == null)
                throw new ArgumentNullException(nameof(globo));

            const string query = @"
                INSERT INTO Detalle_Venta_Globo (ventaId, globoId, cantidad, costo, importe)
                VALUES (@ventaId, @globoId, @cantidad, @costo, @importe)";

            using var cmd = new SqlCommand(query, conn, tran);
            cmd.Parameters.Add("@ventaId", SqlDbType.VarChar).Value = ventaId;
            cmd.Parameters.Add("@globoId", SqlDbType.VarChar).Value = globo.GloboId;
            cmd.Parameters.Add("@cantidad", SqlDbType.Int).Value = globo.Cantidad;
            cmd.Parameters.Add("@costo", SqlDbType.Decimal).Value = globo.Costo;
            cmd.Parameters.Add("@importe", SqlDbType.Decimal).Value = globo.Importe;
            cmd.ExecuteNonQuery();

            ActualizarStockGlobo(globo.GloboId, globo.Cantidad, ventaId, conn, tran);
        }

        #endregion

        #region ACTUALIZACIÓN DE STOCK Y AUDITORÍA

        private void ActualizarStockProducto(string productoId, int cantidadVendida, string ventaId, SqlConnection conn, SqlTransaction tran)
        {
            int stockActual;

            using (var cmdSel = new SqlCommand("SELECT stock FROM Producto WHERE productoId = @id", conn, tran))
            {
                cmdSel.Parameters.Add("@id", SqlDbType.VarChar).Value = productoId;
                var result = cmdSel.ExecuteScalar();
                if (result == null)
                    throw new Exception($"El producto con ID {productoId} no existe.");
                stockActual = Convert.ToInt32(result);
            }

            int nuevoStock = Math.Max(stockActual - cantidadVendida, 0);

            using (var cmdUpd = new SqlCommand("UPDATE Producto SET stock = @nuevo WHERE productoId = @id", conn, tran))
            {
                cmdUpd.Parameters.Add("@nuevo", SqlDbType.Int).Value = nuevoStock;
                cmdUpd.Parameters.Add("@id", SqlDbType.VarChar).Value = productoId;
                cmdUpd.ExecuteNonQuery();
            }

            using var cmdHist = new SqlCommand(@"
                INSERT INTO HistorialAjusteStock (ProductoId, CantidadAnterior, CantidadNueva, Motivo, EmpleadoId)
                VALUES (@ProductoId, @Anterior, @Nuevo, @Motivo, @EmpleadoId)", conn, tran);
            cmdHist.Parameters.Add("@ProductoId", SqlDbType.VarChar).Value = productoId;
            cmdHist.Parameters.Add("@Anterior", SqlDbType.Int).Value = stockActual;
            cmdHist.Parameters.Add("@Nuevo", SqlDbType.Int).Value = nuevoStock;
            cmdHist.Parameters.Add("@Motivo", SqlDbType.VarChar).Value = $"Venta {ventaId}";
            cmdHist.Parameters.Add("@EmpleadoId", SqlDbType.Int).Value = SesionActual.EmpleadoId ?? (object)DBNull.Value;
            cmdHist.ExecuteNonQuery();
        }

        private void ActualizarStockGlobo(string globoId, int cantidadVendida, string ventaId, SqlConnection conn, SqlTransaction tran)
        {
            int stockActual;

            using (var cmdSel = new SqlCommand("SELECT stock FROM Globo WHERE globoId = @id", conn, tran))
            {
                cmdSel.Parameters.Add("@id", SqlDbType.VarChar).Value = globoId;
                var result = cmdSel.ExecuteScalar();
                if (result == null)
                    throw new Exception($"El globo con ID {globoId} no existe.");
                stockActual = Convert.ToInt32(result);
            }

            int nuevoStock = Math.Max(stockActual - cantidadVendida, 0);

            using (var cmdUpd = new SqlCommand("UPDATE Globo SET stock = @nuevo WHERE globoId = @id", conn, tran))
            {
                cmdUpd.Parameters.Add("@nuevo", SqlDbType.Int).Value = nuevoStock;
                cmdUpd.Parameters.Add("@id", SqlDbType.VarChar).Value = globoId;
                cmdUpd.ExecuteNonQuery();
            }

            using var cmdHist = new SqlCommand(@"
                INSERT INTO HistorialAjusteStockGlobo (GloboId, CantidadAnterior, CantidadNueva, Motivo, EmpleadoId)
                VALUES (@GloboId, @Anterior, @Nuevo, @Motivo, @EmpleadoId)", conn, tran);
            cmdHist.Parameters.Add("@GloboId", SqlDbType.VarChar).Value = globoId;
            cmdHist.Parameters.Add("@Anterior", SqlDbType.Int).Value = stockActual;
            cmdHist.Parameters.Add("@Nuevo", SqlDbType.Int).Value = nuevoStock;
            cmdHist.Parameters.Add("@Motivo", SqlDbType.VarChar).Value = $"Venta {ventaId}";
            cmdHist.Parameters.Add("@EmpleadoId", SqlDbType.Int).Value = SesionActual.EmpleadoId ?? (object)DBNull.Value;
            cmdHist.ExecuteNonQuery();
        }

        #endregion

        #region OBTENER HISTORIAL DE VENTAS

        public List<VentaHistorial> ObtenerHistorialVentas()
        {
            var historial = new List<VentaHistorial>();

            const string queryVentas = @"
                SELECT v.ventaId, v.clienteId, v.empleadoId, v.fechaVenta, v.importeTotal, v.Estatus,
                       c.primerNombre AS primerNombreCliente, c.segundoNombre AS segundoNombreCliente,
                       c.apellidoP AS apellidoPCliente, c.apellidoM AS apellidoMCliente,
                       e.primerNombre AS primerNombreEmpleado, e.segundoNombre AS segundoNombreEmpleado,
                       e.apellidoP AS apellidoPEmpleado, e.apellidoM AS apellidoMEmpleado
                FROM Venta v
                JOIN Cliente c ON v.clienteId = c.clienteId
                JOIN Empleado e ON v.empleadoId = e.empleadoId
                WHERE c.Activo = 1 AND e.Activo = 1
                ORDER BY v.fechaVenta DESC";

            DataTable dtVentas = DbHelper.ExecuteQuery(queryVentas);

            foreach (DataRow row in dtVentas.Rows)
            {
                historial.Add(new VentaHistorial
                {
                    VentaId = row["ventaId"].ToString(),
                    ClienteId = row["clienteId"].ToString(),
                    ClienteNombre = $"{row["primerNombreCliente"] ?? ""} {row["segundoNombreCliente"] ?? ""} {row["apellidoPCliente"] ?? ""} {row["apellidoMCliente"] ?? ""}".Replace("  ", " ").Trim(),
                    NombreEmpleado = $"{row["primerNombreEmpleado"] ?? ""} {row["segundoNombreEmpleado"] ?? ""} {row["apellidoPEmpleado"] ?? ""} {row["apellidoMEmpleado"] ?? ""}".Replace("  ", " ").Trim(),
                    FechaVenta = Convert.ToDateTime(row["fechaVenta"]),
                    Total = Convert.ToDecimal(row["importeTotal"]),
                    Estatus = row["Estatus"]?.ToString() ?? "Completada",
                    Productos = new ObservableCollection<ProductoVenta>(),
                    Globos = new ObservableCollection<GloboVenta>()
                });
            }

            // Productos
            const string queryProductos = @"
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
                    Unidad = row["unidad"] != DBNull.Value ? row["unidad"].ToString() : "1",
                    Costo = row["costo"] != DBNull.Value ? Convert.ToDecimal(row["costo"]) : 0,
                    Cantidad = row["cantidad"] != DBNull.Value ? Convert.ToInt32(row["cantidad"]) : 0
                });
            }

            // Globos
            const string queryGlobos = @"
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

                // Evitar duplicados en caso de multiples JOIN
                if (venta.Globos.Any(g => g.GloboId == row["globoId"].ToString())) continue;

                venta.Globos.Add(new GloboVenta
                {
                    GloboId = row["globoId"].ToString(),
                    Material = row["material"]?.ToString(),
                    Color = row["color"]?.ToString(),
                    Unidad = row["unidad"]?.ToString(),
                    Costo = row["costo"] != DBNull.Value ? Convert.ToDecimal(row["costo"]) : 0,
                    Tamano = row["tamanio"]?.ToString(),
                    Forma = row["forma"]?.ToString(),
                    Tematica = row["tematica"]?.ToString(),
                    Cantidad = row["cantidad"] != DBNull.Value ? Convert.ToInt32(row["cantidad"]) : 0
                });
            }

            return historial;
        }

        #endregion
    }
}
