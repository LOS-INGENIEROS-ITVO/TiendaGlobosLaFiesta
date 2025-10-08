using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using TiendaGlobosLaFiesta.Models.Clientes;
using TiendaGlobosLaFiesta.Models.Ventas;
using TiendaGlobosLaFiesta.Models.Inventario;

namespace TiendaGlobosLaFiesta.Data
{
    public class VentasRepository
    {
        private readonly string _connectionString;
        private readonly StockManagerRepository _stockManager;

        public VentasRepository(string connectionString, StockManagerRepository stockManager)
        {
            _connectionString = connectionString;
            _stockManager = stockManager;
        }

        public bool RegistrarVenta(Venta venta)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            using var tran = conn.BeginTransaction();

            try
            {
                // Insertar venta
                using (var cmdVenta = new SqlCommand(
                    @"INSERT INTO Venta(ventaId, empleadoId, clienteId, fechaVenta, importeTotal, estatus)
                      VALUES(@Id, @Empleado, @Cliente, @Fecha, @ImporteTotal, @Estatus)", conn, tran))
                {
                    cmdVenta.Parameters.AddWithValue("@Id", venta.VentaId);
                    cmdVenta.Parameters.AddWithValue("@Empleado", venta.EmpleadoId);
                    cmdVenta.Parameters.AddWithValue("@Cliente", venta.ClienteId);
                    cmdVenta.Parameters.AddWithValue("@Fecha", venta.FechaVenta);
                    cmdVenta.Parameters.AddWithValue("@ImporteTotal", venta.ImporteTotal);
                    cmdVenta.Parameters.AddWithValue("@Estatus", venta.Estatus ?? "Completada");
                    cmdVenta.ExecuteNonQuery();
                }

                // Detalle productos
                foreach (var p in venta.Productos)
                {
                    using var cmdDet = new SqlCommand(
                        @"INSERT INTO Detalle_Venta_Producto (ventaId, productoId, cantidad, costo, importe)
                          VALUES(@VentaId, @ProductoId, @Cantidad, @Costo, @Importe)", conn, tran);
                    cmdDet.Parameters.AddWithValue("@VentaId", venta.VentaId);
                    cmdDet.Parameters.AddWithValue("@ProductoId", p.Id);
                    cmdDet.Parameters.AddWithValue("@Cantidad", p.Cantidad);
                    cmdDet.Parameters.AddWithValue("@Costo", p.Costo);
                    cmdDet.Parameters.AddWithValue("@Importe", p.Importe);
                    cmdDet.ExecuteNonQuery();

                    // Ajustar stock con transacción
                    _stockManager.AjustarStockProducto(p.Id, -p.Cantidad, conn, tran, venta.EmpleadoId, "Venta");
                }

                // Detalle globos
                foreach (var g in venta.Globos)
                {
                    using var cmdDet = new SqlCommand(
                        @"INSERT INTO Detalle_Venta_Globo (ventaId, globoId, cantidad, costo, importe)
                          VALUES(@VentaId, @GloboId, @Cantidad, @Costo, @Importe)", conn, tran);
                    cmdDet.Parameters.AddWithValue("@VentaId", venta.VentaId);
                    cmdDet.Parameters.AddWithValue("@GloboId", g.Id);
                    cmdDet.Parameters.AddWithValue("@Cantidad", g.Cantidad);
                    cmdDet.Parameters.AddWithValue("@Costo", g.Costo);
                    cmdDet.Parameters.AddWithValue("@Importe", g.Importe);
                    cmdDet.ExecuteNonQuery();

                    // Ajustar stock con transacción
                    _stockManager.AjustarStockGlobo(g.Id, -g.Cantidad, conn, tran, venta.EmpleadoId, "Venta");
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

        public List<Cliente> ObtenerClientes()
        {
            var lista = new List<Cliente>();
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var query = "SELECT clienteId, primerNombre, segundoNombre, apellidoP, apellidoM, telefono FROM Cliente WHERE Activo = 1";
            using var cmd = new SqlCommand(query, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                lista.Add(new Cliente
                {
                    ClienteId = reader.GetString(0),
                    PrimerNombre = reader.GetString(1),
                    SegundoNombre = reader.IsDBNull(2) ? "" : reader.GetString(2),
                    ApellidoP = reader.IsDBNull(3) ? "" : reader.GetString(3),
                    ApellidoM = reader.IsDBNull(4) ? "" : reader.GetString(4),
                    Telefono = reader.IsDBNull(5) ? null : (long?)reader.GetInt64(5),
                    Activo = true
                });
            }

            return lista;
        }

        public List<VentaHistorial> ObtenerHistorialVentas()
        {
            var lista = new List<VentaHistorial>();
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var query = @"
                SELECT v.ventaId, v.clienteId, c.primerNombre, c.segundoNombre, c.apellidoP, c.apellidoM,
                       e.nombre, v.fechaVenta, v.estatus, v.importeTotal
                FROM Venta v
                LEFT JOIN Cliente c ON v.clienteId = c.clienteId
                LEFT JOIN Empleado e ON v.empleadoId = e.empleadoId";

            using var cmd = new SqlCommand(query, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                var venta = new Venta
                {
                    VentaId = reader.GetString(0),
                    ClienteId = reader.GetString(1),
                    FechaVenta = reader.GetDateTime(7),
                    Estatus = reader.GetString(8)
                };

                var nombreCliente = $"{reader.GetString(2)} {reader.GetString(3)} {reader.GetString(4)} {reader.GetString(5)}".Trim();
                lista.Add(new VentaHistorial(
                    venta,
                    reader.GetString(1),
                    nombreCliente,
                    reader.IsDBNull(6) ? "" : reader.GetString(6)
                ));
            }

            return lista;
        }
    }
}