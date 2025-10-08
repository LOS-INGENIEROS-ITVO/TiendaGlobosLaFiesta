using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using TiendaGlobosLaFiesta.Models.Clientes;
using TiendaGlobosLaFiesta.Models.Utilities;
using TiendaGlobosLaFiesta.Models.Ventas;
using TiendaGlobosLaFiesta.Models.Inventario;

namespace TiendaGlobosLaFiesta.Data
{
    public class VentasRepository
    {
        #region CLIENTES

        public List<Cliente> ObtenerClientes()
        {
            var clientes = new List<Cliente>();
            const string query = @"
                SELECT clienteId, primerNombre, segundoNombre, apellidoP, apellidoM 
                FROM Cliente 
                WHERE Activo = 1 
                ORDER BY primerNombre";

            DataTable dt = DbHelper.ExecuteQuery(query);

            foreach (DataRow row in dt.Rows)
            {
                clientes.Add(new Cliente
                {
                    ClienteId = row["clienteId"].ToString(),
                    PrimerNombre = row["primerNombre"]?.ToString(),
                    SegundoNombre = row["segundoNombre"]?.ToString(),
                    ApellidoP = row["apellidoP"]?.ToString(),
                    ApellidoM = row["apellidoM"]?.ToString()
                });
            }

            return clientes;
        }

        #endregion

        #region INSERTAR VENTA MAESTRO

        public void InsertarVentaMaestro(Venta venta, SqlConnection conn, SqlTransaction tran)
        {
            const string queryVenta = @"
                INSERT INTO Venta (ventaId, clienteId, empleadoId, fechaVenta, importeTotal, Estatus)
                VALUES (@ventaId, @clienteId, @empleadoId, @fecha, @total, @estatus)";

            using var cmdVenta = new SqlCommand(queryVenta, conn, tran);
            cmdVenta.Parameters.AddWithValue("@ventaId", venta.VentaId);
            cmdVenta.Parameters.AddWithValue("@clienteId", venta.ClienteId);
            cmdVenta.Parameters.AddWithValue("@empleadoId", SesionActual.EmpleadoId ?? throw new Exception("No hay sesión activa de empleado."));
            cmdVenta.Parameters.AddWithValue("@fecha", DateTime.Now);
            cmdVenta.Parameters.AddWithValue("@total", venta.Productos.Sum(p => p.Importe) + venta.Globos.Sum(g => g.Importe));
            cmdVenta.Parameters.AddWithValue("@estatus", "Completada");
            cmdVenta.ExecuteNonQuery();
        }

        #endregion

        #region DETALLES VENTA

        public void InsertarDetalleProducto(string ventaId, ProductoVenta producto, SqlConnection conn, SqlTransaction tran)
        {
            const string query = @"
                INSERT INTO Detalle_Venta_Producto (ventaId, productoId, cantidad, costo, importe)
                VALUES (@ventaId, @productoId, @cantidad, @costo, @importe)";

            using var cmd = new SqlCommand(query, conn, tran);
            cmd.Parameters.AddWithValue("@ventaId", ventaId);
            cmd.Parameters.AddWithValue("@productoId", producto.ProductoId);
            cmd.Parameters.AddWithValue("@cantidad", producto.Cantidad);
            cmd.Parameters.AddWithValue("@costo", producto.Costo);
            cmd.Parameters.AddWithValue("@importe", producto.Importe);
            cmd.ExecuteNonQuery();

            ActualizarStockProducto(producto.ProductoId, producto.Cantidad, ventaId, conn, tran);
        }

        public void InsertarDetalleGlobo(string ventaId, GloboVenta globo, SqlConnection conn, SqlTransaction tran)
        {
            const string query = @"
                INSERT INTO Detalle_Venta_Globo (ventaId, globoId, cantidad, costo, importe)
                VALUES (@ventaId, @globoId, @cantidad, @costo, @importe)";

            using var cmd = new SqlCommand(query, conn, tran);
            cmd.Parameters.AddWithValue("@ventaId", ventaId);
            cmd.Parameters.AddWithValue("@globoId", globo.GloboId);
            cmd.Parameters.AddWithValue("@cantidad", globo.Cantidad);
            cmd.Parameters.AddWithValue("@costo", globo.Costo);
            cmd.Parameters.AddWithValue("@importe", globo.Importe);
            cmd.ExecuteNonQuery();

            ActualizarStockGlobo(globo.GloboId, globo.Cantidad, ventaId, conn, tran);
        }

        #endregion

        #region ACTUALIZACIÓN DE STOCK

        private void ActualizarStockProducto(string productoId, int cantidadVendida, string ventaId, SqlConnection conn, SqlTransaction tran)
        {
            // Obtener stock actual
            int stockActual = Convert.ToInt32(new SqlCommand("SELECT stock FROM Producto WHERE productoId=@id", conn, tran)
            {
                Parameters = { new SqlParameter("@id", productoId) }
            }.ExecuteScalar() ?? 0);

            int nuevoStock = Math.Max(stockActual - cantidadVendida, 0);

            // Actualizar stock
            new SqlCommand("UPDATE Producto SET stock=@nuevo WHERE productoId=@id", conn, tran)
            {
                Parameters =
                {
                    new SqlParameter("@nuevo", nuevoStock),
                    new SqlParameter("@id", productoId)
                }
            }.ExecuteNonQuery();

            // Insertar historial de ajuste
            using var cmdHist = new SqlCommand(@"
                INSERT INTO HistorialAjusteStock (ProductoId, CantidadAnterior, CantidadNueva, Motivo, EmpleadoId)
                VALUES (@ProductoId, @Anterior, @Nuevo, @Motivo, @EmpleadoId)", conn, tran);

            cmdHist.Parameters.AddWithValue("@ProductoId", productoId);
            cmdHist.Parameters.AddWithValue("@Anterior", stockActual);
            cmdHist.Parameters.AddWithValue("@Nuevo", nuevoStock);
            cmdHist.Parameters.AddWithValue("@Motivo", $"Venta {ventaId}");
            cmdHist.Parameters.AddWithValue("@EmpleadoId", SesionActual.EmpleadoId ?? (object)DBNull.Value);
            cmdHist.ExecuteNonQuery();
        }

        private void ActualizarStockGlobo(string globoId, int cantidadVendida, string ventaId, SqlConnection conn, SqlTransaction tran)
        {
            int stockActual = Convert.ToInt32(new SqlCommand("SELECT stock FROM Globo WHERE globoId=@id", conn, tran)
            {
                Parameters = { new SqlParameter("@id", globoId) }
            }.ExecuteScalar() ?? 0);

            int nuevoStock = Math.Max(stockActual - cantidadVendida, 0);

            new SqlCommand("UPDATE Globo SET stock=@nuevo WHERE globoId=@id", conn, tran)
            {
                Parameters =
                {
                    new SqlParameter("@nuevo", nuevoStock),
                    new SqlParameter("@id", globoId)
                }
            }.ExecuteNonQuery();

            using var cmdHist = new SqlCommand(@"
                INSERT INTO HistorialAjusteStockGlobo (GloboId, CantidadAnterior, CantidadNueva, Motivo, EmpleadoId)
                VALUES (@GloboId, @Anterior, @Nuevo, @Motivo, @EmpleadoId)", conn, tran);

            cmdHist.Parameters.AddWithValue("@GloboId", globoId);
            cmdHist.Parameters.AddWithValue("@Anterior", stockActual);
            cmdHist.Parameters.AddWithValue("@Nuevo", nuevoStock);
            cmdHist.Parameters.AddWithValue("@Motivo", $"Venta {ventaId}");
            cmdHist.Parameters.AddWithValue("@EmpleadoId", SesionActual.EmpleadoId ?? (object)DBNull.Value);
            cmdHist.ExecuteNonQuery();
        }

        #endregion

        #region HISTORIAL VENTAS

        public List<VentaHistorial> ObtenerHistorialVentas()
        {
            var historial = new List<VentaHistorial>();
            const string query = @"
                SELECT v.ventaId, v.clienteId, v.empleadoId, v.fechaVenta, v.importeTotal, v.Estatus,
                       c.primerNombre, c.segundoNombre, c.apellidoP, c.apellidoM,
                       e.primerNombre AS emp1, e.segundoNombre AS emp2, e.apellidoP AS empP, e.apellidoM AS empM
                FROM Venta v
                JOIN Cliente c ON v.clienteId = c.clienteId
                JOIN Empleado e ON v.empleadoId = e.empleadoId
                ORDER BY v.fechaVenta DESC";

            DataTable dt = DbHelper.ExecuteQuery(query);
            foreach (DataRow row in dt.Rows)
            {
                var venta = new VentaHistorial
                {
                    VentaId = row["ventaId"].ToString(),
                    ClienteId = row["clienteId"].ToString(),
                    ClienteNombre = $"{row["primerNombre"]} {row["segundoNombre"]} {row["apellidoP"]} {row["apellidoM"]}".Replace("  ", " ").Trim(),
                    NombreEmpleado = $"{row["emp1"]} {row["emp2"]} {row["empP"]} {row["empM"]}".Replace("  ", " ").Trim(),
                    FechaVenta = Convert.ToDateTime(row["fechaVenta"]),
                    Total = Convert.ToDecimal(row["importeTotal"]),
                    Estatus = row["Estatus"].ToString(),
                    Productos = new ObservableCollection<ProductoVenta>(ObtenerDetalleProducto(row["ventaId"].ToString())),
                    Globos = new ObservableCollection<GloboVenta>(ObtenerDetalleGlobo(row["ventaId"].ToString()))
                };

                historial.Add(venta);
            }

            return historial;
        }

        #endregion

        #region DETALLES VENTA CONSULTA

        public List<ProductoVenta> ObtenerDetalleProducto(string ventaId)
        {
            const string query = @"
                SELECT dvp.productoId, p.nombre, dvp.cantidad, dvp.costo
                FROM Detalle_Venta_Producto dvp
                JOIN Producto p ON dvp.productoId = p.productoId
                WHERE dvp.ventaId=@ventaId";

            var parametros = new[] { new SqlParameter("@ventaId", ventaId) };
            DataTable dt = DbHelper.ExecuteQuery(query, parametros);

            return dt.AsEnumerable()
                     .Select(row => new ProductoVenta
                     {
                         ProductoId = row["productoId"].ToString(),
                         NombreProducto = row["nombre"]?.ToString(),
                         Cantidad = row.Field<int>("cantidad"),
                         Costo = row.Field<decimal>("costo")
                     })
                     .ToList();
        }

        public List<GloboVenta> ObtenerDetalleGlobo(string ventaId)
        {
            const string query = @"
                SELECT dvg.globoId, g.material, g.color, g.tamano, g.forma, g.tematica,
                       dvg.cantidad, dvg.costo
                FROM Detalle_Venta_Globo dvg
                JOIN Globo g ON dvg.globoId = g.globoId
                WHERE dvg.ventaId=@ventaId";

            var parametros = new[] { new SqlParameter("@ventaId", ventaId) };
            DataTable dt = DbHelper.ExecuteQuery(query, parametros);

            return dt.AsEnumerable()
                     .Select(MapearGlobo)
                     .ToList();
        }

        private GloboVenta MapearGlobo(DataRow row)
        {
            return new GloboVenta
            {
                GloboId = row["globoId"].ToString(),
                Material = row["material"]?.ToString(),
                Color = row["color"]?.ToString(),
                Tamano = row["tamano"]?.ToString(),
                Forma = row["forma"]?.ToString(),
                Tematica = row["tematica"]?.ToString(),
                Cantidad = row.Field<int>("cantidad"),
                Costo = row.Field<decimal>("costo")
            };
        }

        #endregion
    }
}