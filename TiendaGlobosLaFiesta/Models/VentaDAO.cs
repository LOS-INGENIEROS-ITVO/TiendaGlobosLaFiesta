using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using TiendaGlobosLaFiesta.Data;
using TiendaGlobosLaFiesta.Models;

namespace TiendaGlobosLaFiesta.Ventas
{
    public class VentaDAO
    {


        public ObservableCollection<VentaHistorial> ObtenerHistorialFiltrado(string clienteId, DateTime? fechaDesde, DateTime? fechaHasta)
        {
            var parametros = new List<System.Data.SqlClient.SqlParameter>();
            string query = @"
        SELECT 
            v.ventaId AS VentaId, 
            c.primerNombre + ' ' + ISNULL(c.segundoNombre,'') + ' ' + c.apellidoP + ' ' + c.apellidoM AS Cliente,
            e.primerNombre + ' ' + ISNULL(e.segundoNombre,'') + ' ' + e.apellidoP + ' ' + e.apellidoM AS Empleado,
            v.fechaVenta AS Fecha,
            v.importeTotal AS Total
        FROM Venta v
        INNER JOIN Cliente c ON v.clienteId = c.clienteId
        INNER JOIN Empleado e ON v.empleadoId = e.empleadoId
        WHERE 1=1";

            if (!string.IsNullOrEmpty(clienteId))
            {
                query += " AND v.clienteId = @clienteId";
                parametros.Add(ConexionBD.Param("@clienteId", clienteId));
            }

            if (fechaDesde.HasValue)
            {
                query += " AND v.fechaVenta >= @fechaDesde";
                parametros.Add(ConexionBD.Param("@fechaDesde", fechaDesde.Value));
            }

            if (fechaHasta.HasValue)
            {
                query += " AND v.fechaVenta <= @fechaHasta";
                parametros.Add(ConexionBD.Param("@fechaHasta", fechaHasta.Value));
            }

            query += " ORDER BY v.fechaVenta DESC";

            return ConexionBD.EjecutarConsulta(query, parametros.ToArray())
                .AsEnumerable()
                .Select(r => new VentaHistorial
                {
                    VentaId = r["VentaId"].ToString(),
                    Cliente = r["Cliente"].ToString(),
                    Empleado = r["Empleado"].ToString(),
                    Fecha = Convert.ToDateTime(r["Fecha"]),
                    Total = Convert.ToDecimal(r["Total"])
                }).ToObservableCollection();
        }


        public bool RegistrarVenta(Venta venta)
        {
            using (SqlConnection conn = ConexionBD.ObtenerConexion())
            {
                SqlTransaction transaction = conn.BeginTransaction();





                try
                {
                    // Insertar Venta
                    string sqlVenta = "INSERT INTO Venta (ventaId, empleadoId, clienteId, fechaVenta, importeTotal) " +
                                      "VALUES (@ventaId, @empleadoId, @clienteId, @fechaVenta, @importeTotal)";
                    SqlCommand cmdVenta = new SqlCommand(sqlVenta, conn, transaction);
                    cmdVenta.Parameters.AddWithValue("@ventaId", venta.VentaId);
                    cmdVenta.Parameters.AddWithValue("@empleadoId", venta.EmpleadoId);
                    cmdVenta.Parameters.AddWithValue("@clienteId", venta.ClienteId);
                    cmdVenta.Parameters.AddWithValue("@fechaVenta", venta.FechaVenta);
                    cmdVenta.Parameters.AddWithValue("@importeTotal", venta.ImporteTotal);
                    cmdVenta.ExecuteNonQuery();

                    // Detalles Productos
                    foreach (var detalle in venta.Productos)
                    {
                        string sqlDetalle = "INSERT INTO Detalle_Venta_Producto (ventaId, productoId, cantidad, costo, importe) " +
                                            "VALUES (@ventaId, @productoId, @cantidad, @costo, @importe)";
                        SqlCommand cmdDetalle = new SqlCommand(sqlDetalle, conn, transaction);
                        cmdDetalle.Parameters.AddWithValue("@ventaId", venta.VentaId);
                        cmdDetalle.Parameters.AddWithValue("@productoId", detalle.ProductoId);
                        cmdDetalle.Parameters.AddWithValue("@cantidad", detalle.Cantidad);
                        cmdDetalle.Parameters.AddWithValue("@costo", detalle.Costo);
                        cmdDetalle.Parameters.AddWithValue("@importe", detalle.Importe);
                        cmdDetalle.ExecuteNonQuery();

                        string sqlStock = "UPDATE Producto SET stock = stock - @cantidad WHERE productoId = @productoId";
                        SqlCommand cmdStock = new SqlCommand(sqlStock, conn, transaction);
                        cmdStock.Parameters.AddWithValue("@cantidad", detalle.Cantidad);
                        cmdStock.Parameters.AddWithValue("@productoId", detalle.ProductoId);
                        cmdStock.ExecuteNonQuery();
                    }

                    // Detalles Globos
                    foreach (var detalle in venta.Globos)
                    {
                        string sqlDetalleG = "INSERT INTO Detalle_Venta_Globo (ventaId, globoId, cantidad, costo, importe) " +
                                             "VALUES (@ventaId, @globoId, @cantidad, @costo, @importe)";
                        SqlCommand cmdDetalleG = new SqlCommand(sqlDetalleG, conn, transaction);
                        cmdDetalleG.Parameters.AddWithValue("@ventaId", venta.VentaId);
                        cmdDetalleG.Parameters.AddWithValue("@globoId", detalle.GloboId);
                        cmdDetalleG.Parameters.AddWithValue("@cantidad", detalle.Cantidad);
                        cmdDetalleG.Parameters.AddWithValue("@costo", detalle.Costo);
                        cmdDetalleG.Parameters.AddWithValue("@importe", detalle.Importe);
                        cmdDetalleG.ExecuteNonQuery();

                        string sqlStockG = "UPDATE Globo SET stock = stock - @cantidad WHERE globoId = @globoId";
                        SqlCommand cmdStockG = new SqlCommand(sqlStockG, conn, transaction);
                        cmdStockG.Parameters.AddWithValue("@cantidad", detalle.Cantidad);
                        cmdStockG.Parameters.AddWithValue("@globoId", detalle.GloboId);
                        cmdStockG.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Console.WriteLine("Error en registrar venta: " + ex.Message);
                    return false;
                }
            }
        }
    }
}