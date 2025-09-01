using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using TiendaGlobosLaFiesta.Models;

namespace TiendaGlobosLaFiesta.Data
{
    public static class ConexionBD
    {
        private const string Servidor = @"LALOVG25\SQLEXPRESS";
        private const string BaseDatos = "Globeriadb";
        private const bool UsarWindowsAuth = true;

        private static readonly string ConnectionString = UsarWindowsAuth
            ? $"Server={Servidor};Database={BaseDatos};Trusted_Connection=True;"
            : throw new NotSupportedException("Solo está configurado Windows Authentication.");

        // -------------------------
        // Conexión y ejecución
        // -------------------------
        public static SqlConnection ObtenerConexion()
        {
            try
            {
                var conn = new SqlConnection(ConnectionString);
                conn.Open();
                return conn;
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo conectar a la base de datos: " + ex.Message,
                                "Error de conexión", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

        public static DataTable EjecutarConsulta(string sql, SqlParameter[] parametros = null)
        {
            using SqlConnection conn = ObtenerConexion();
            using SqlCommand cmd = new SqlCommand(sql, conn);
            if (parametros != null) cmd.Parameters.AddRange(parametros);
            using SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        public static int EjecutarNonQuery(string sql, SqlParameter[] parametros = null)
        {
            using SqlConnection conn = ObtenerConexion();
            using SqlCommand cmd = new SqlCommand(sql, conn);
            if (parametros != null) cmd.Parameters.AddRange(parametros);
            return cmd.ExecuteNonQuery();
        }

        public static SqlParameter Param(string nombre, object valor)
        {
            return new SqlParameter(nombre, valor ?? DBNull.Value);
        }

        // ===========================
        // CLIENTES
        // ===========================
        public static List<Cliente> ObtenerClientes(string filtro = "")
        {
            List<Cliente> lista = new List<Cliente>();
            try
            {
                string sql = "SELECT clienteId, primerNombre, segundoNombre, apellidoP, apellidoM, telefono FROM Cliente";
                SqlParameter[] parametros = null;

                if (!string.IsNullOrEmpty(filtro))
                {
                    sql += " WHERE primerNombre LIKE @filtro OR segundoNombre LIKE @filtro OR apellidoP LIKE @filtro OR apellidoM LIKE @filtro";
                    parametros = new SqlParameter[] { Param("@filtro", "%" + filtro + "%") };
                }

                DataTable dt = EjecutarConsulta(sql, parametros);

                foreach (DataRow row in dt.Rows)
                {
                    lista.Add(new Cliente
                    {
                        ClienteId = row["clienteId"].ToString(),
                        PrimerNombre = row["primerNombre"].ToString(),
                        SegundoNombre = row["segundoNombre"].ToString(),
                        ApellidoP = row["apellidoP"].ToString(),
                        ApellidoM = row["apellidoM"].ToString(),
                        Telefono = row["telefono"] != DBNull.Value ? Convert.ToInt64(row["telefono"]) : (long?)null
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al obtener clientes: " + ex.Message);
            }

            return lista;
        }

        public static bool AgregarCliente(Cliente cliente)
        {
            try
            {
                string sql = @"INSERT INTO Cliente (clienteId, primerNombre, segundoNombre, apellidoP, apellidoM, telefono)
                               VALUES (@ClienteId, @PrimerNombre, @SegundoNombre, @ApellidoP, @ApellidoM, @Telefono)";
                SqlParameter[] parametros = {
                    Param("@ClienteId", cliente.ClienteId),
                    Param("@PrimerNombre", cliente.PrimerNombre),
                    Param("@SegundoNombre", cliente.SegundoNombre),
                    Param("@ApellidoP", cliente.ApellidoP),
                    Param("@ApellidoM", cliente.ApellidoM),
                    Param("@Telefono", cliente.Telefono.HasValue ? (object)cliente.Telefono.Value : DBNull.Value)
                };
                return EjecutarNonQuery(sql, parametros) > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al agregar cliente: " + ex.Message);
                return false;
            }
        }

        public static bool ActualizarCliente(Cliente cliente)
        {
            try
            {
                string sql = @"UPDATE Cliente SET primerNombre=@PrimerNombre, segundoNombre=@SegundoNombre, apellidoP=@ApellidoP, apellidoM=@ApellidoM, telefono=@Telefono
                               WHERE clienteId=@ClienteId";
                SqlParameter[] parametros = {
                    Param("@ClienteId", cliente.ClienteId),
                    Param("@PrimerNombre", cliente.PrimerNombre),
                    Param("@SegundoNombre", cliente.SegundoNombre),
                    Param("@ApellidoP", cliente.ApellidoP),
                    Param("@ApellidoM", cliente.ApellidoM),
                    Param("@Telefono", cliente.Telefono.HasValue ? (object)cliente.Telefono.Value : DBNull.Value)
                };
                return EjecutarNonQuery(sql, parametros) > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al actualizar cliente: " + ex.Message);
                return false;
            }
        }

        public static bool EliminarCliente(string clienteId)
        {
            try
            {
                string sql = "DELETE FROM Cliente WHERE clienteId=@ClienteId";
                SqlParameter[] parametros = { Param("@ClienteId", clienteId) };
                return EjecutarNonQuery(sql, parametros) > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al eliminar cliente: " + ex.Message);
                return false;
            }
        }

        // -------------------------
        // Productos
        // -------------------------
        public static List<ProductoVenta> ObtenerProductos()
        {
            List<ProductoVenta> lista = new List<ProductoVenta>();
            string sql = "SELECT productoId, nombre, unidad, stock, costo FROM Producto";
            DataTable dt = EjecutarConsulta(sql);
            foreach (DataRow row in dt.Rows)
            {
                lista.Add(new ProductoVenta
                {
                    ProductoId = row["productoId"].ToString(),
                    Nombre = row["nombre"].ToString(),
                    Unidad = row["unidad"].ToString(),
                    Stock = Convert.ToInt32(row["stock"]),
                    Costo = Convert.ToDecimal(row["costo"])
                });
            }
            return lista;
        }

        // -------------------------
        // Globos
        // -------------------------
        public static List<GloboVenta> ObtenerGlobos()
        {
            List<GloboVenta> lista = new List<GloboVenta>();
            string sql = @"
                SELECT g.globoId, g.material, g.color, g.unidad, g.stock, g.costo,
                       ISNULL(t.Tamanios, '') AS Tamano,
                       ISNULL(f.Formas, '') AS Forma,
                       ISNULL(tm.Tematicas, '') AS Tematica
                FROM Globo g
                LEFT JOIN (
                    SELECT globoId, STRING_AGG(tamanio, ', ') AS Tamanios FROM Globo_Tamanio GROUP BY globoId
                ) t ON g.globoId = t.globoId
                LEFT JOIN (
                    SELECT globoId, STRING_AGG(forma, ', ') AS Formas FROM Globo_Forma GROUP BY globoId
                ) f ON g.globoId = f.globoId
                LEFT JOIN (
                    SELECT globoId, STRING_AGG(nombre, ', ') AS Tematicas FROM Tematica GROUP BY globoId
                ) tm ON g.globoId = tm.globoId
                ORDER BY g.globoId";

            DataTable dt = EjecutarConsulta(sql);
            foreach (DataRow row in dt.Rows)
            {
                lista.Add(new GloboVenta
                {
                    GloboId = row["globoId"].ToString(),
                    Material = row["material"].ToString(),
                    Color = row["color"].ToString(),
                    Unidad = row["unidad"].ToString(),
                    Stock = Convert.ToInt32(row["stock"]),
                    Costo = Convert.ToDecimal(row["costo"]),
                    Tamano = row["Tamano"].ToString(),
                    Forma = row["Forma"].ToString(),
                    Tematica = row["Tematica"].ToString()
                });
            }
            return lista;
        }

        // -------------------------
        // Registrar venta
        // -------------------------
        public static string ObtenerSiguienteVentaId()
        {
            string sql = "SELECT TOP 1 ventaId FROM Venta ORDER BY ventaId DESC";
            DataTable dt = EjecutarConsulta(sql);

            if (dt.Rows.Count == 0)
                return "VEN0001";

            string ultimoId = dt.Rows[0]["ventaId"].ToString();
            int numero = int.Parse(ultimoId.Substring(3));
            numero++;
            return "VEN" + numero.ToString("D4");
        }

        public static bool RegistrarVenta(Venta venta)
        {
            using SqlConnection conn = ObtenerConexion();
            using SqlTransaction tran = conn.BeginTransaction();
            try
            {
                // Insertar Venta
                string sqlVenta = @"
                    INSERT INTO Venta (ventaId, empleadoId, clienteId, fechaVenta, importeTotal)
                    VALUES (@VentaId, @EmpleadoId, @ClienteId, @FechaVenta, @ImporteTotal)";
                SqlCommand cmdVenta = new SqlCommand(sqlVenta, conn, tran);
                cmdVenta.Parameters.AddRange(new[]
                {
                    Param("@VentaId", venta.VentaId),
                    Param("@EmpleadoId", venta.EmpleadoId),
                    Param("@ClienteId", venta.ClienteId),
                    Param("@FechaVenta", venta.FechaVenta),
                    Param("@ImporteTotal", venta.ImporteTotal)
                });
                cmdVenta.ExecuteNonQuery();

                // Detalle Productos
                foreach (var p in venta.Productos)
                {
                    if (p.Cantidad > p.Stock)
                        throw new Exception($"No hay suficiente stock para el producto {p.Nombre}.");

                    string sqlDetP = @"
                        INSERT INTO Detalle_Venta_Producto (ventaId, productoId, cantidad, costo, importe)
                        VALUES (@VentaId, @ProductoId, @Cantidad, @Costo, @Importe)";
                    SqlCommand cmdDetP = new SqlCommand(sqlDetP, conn, tran);
                    cmdDetP.Parameters.AddRange(new[]
                    {
                        Param("@VentaId", venta.VentaId),
                        Param("@ProductoId", p.ProductoId),
                        Param("@Cantidad", p.Cantidad),
                        Param("@Costo", p.Costo),
                        Param("@Importe", p.Importe)
                    });
                    cmdDetP.ExecuteNonQuery();

                    string sqlUpdStock = "UPDATE Producto SET stock = stock - @Cantidad WHERE productoId = @Id";
                    SqlCommand cmdStock = new SqlCommand(sqlUpdStock, conn, tran);
                    cmdStock.Parameters.AddRange(new[]
                    {
                        Param("@Cantidad", p.Cantidad),
                        Param("@Id", p.ProductoId)
                    });
                    cmdStock.ExecuteNonQuery();
                }

                // Detalle Globos
                foreach (var g in venta.Globos)
                {
                    if (g.Cantidad > g.Stock)
                        throw new Exception($"No hay suficiente stock para el globo {g.Material} - {g.Color}.");

                    string sqlDetG = @"
                        INSERT INTO Detalle_Venta_Globo (ventaId, globoId, cantidad, costo, importe)
                        VALUES (@VentaId, @GloboId, @Cantidad, @Costo, @Importe)";
                    SqlCommand cmdDetG = new SqlCommand(sqlDetG, conn, tran);
                    cmdDetG.Parameters.AddRange(new[]
                    {
                        Param("@VentaId", venta.VentaId),
                        Param("@GloboId", g.GloboId),
                        Param("@Cantidad", g.Cantidad),
                        Param("@Costo", g.Costo),
                        Param("@Importe", g.Importe)
                    });
                    cmdDetG.ExecuteNonQuery();

                    string sqlUpdStockG = "UPDATE Globo SET stock = stock - @Cantidad WHERE globoId = @Id";
                    SqlCommand cmdStockG = new SqlCommand(sqlUpdStockG, conn, tran);
                    cmdStockG.Parameters.AddRange(new[]
                    {
                        Param("@Cantidad", g.Cantidad),
                        Param("@Id", g.GloboId)
                    });
                    cmdStockG.ExecuteNonQuery();
                }

                tran.Commit();
                return true;
            }
            catch (Exception ex)
            {
                tran.Rollback();
                MessageBox.Show("Error al registrar venta: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        // -------------------------
        // Historial de ventas
        // -------------------------

        public static List<VentaHistorial> ObtenerHistorialVentas()
        {
            string sql = @"
            SELECT v.ventaId, c.clienteId,
                   c.primerNombre + ' ' + c.segundoNombre + ' ' + c.apellidoP + ' ' + c.apellidoM AS ClienteNombre,
                   e.primerNombre + ' ' + e.segundoNombre + ' ' + e.apellidoP + ' ' + e.apellidoM AS Empleado,
                   v.fechaVenta, v.importeTotal AS Total,
                   dp.productoId, p.nombre AS ProductoNombre, p.unidad AS ProductoUnidad, dp.cantidad AS ProductoCantidad, dp.costo AS ProductoCosto,
                   dg.globoId, g.material, g.color, g.unidad AS GloboUnidad, dg.cantidad AS GloboCantidad, dg.costo AS GloboCosto,
                   t.Tamanios, f.Formas, tm.Tematicas
            FROM Venta v
            INNER JOIN Cliente c ON v.clienteId = c.clienteId
            INNER JOIN Empleado e ON v.empleadoId = e.empleadoId
            LEFT JOIN Detalle_Venta_Producto dp ON v.ventaId = dp.ventaId
            LEFT JOIN Producto p ON dp.productoId = p.productoId
            LEFT JOIN Detalle_Venta_Globo dg ON v.ventaId = dg.ventaId
            LEFT JOIN Globo g ON dg.globoId = g.globoId
            LEFT JOIN (SELECT globoId, STRING_AGG(tamanio, ', ') AS Tamanios FROM Globo_Tamanio GROUP BY globoId) t ON g.globoId = t.globoId
            LEFT JOIN (SELECT globoId, STRING_AGG(forma, ', ') AS Formas FROM Globo_Forma GROUP BY globoId) f ON g.globoId = f.globoId
            LEFT JOIN (SELECT globoId, STRING_AGG(nombre, ', ') AS Tematicas FROM Tematica GROUP BY globoId) tm ON g.globoId = tm.globoId
            ORDER BY v.fechaVenta DESC";

            DataTable dt = EjecutarConsulta(sql);

            var ventas = dt.AsEnumerable()
                           .GroupBy(r => r["ventaId"].ToString())
                           .Select(g =>
                           {
                               var first = g.First();
                               var ventaHist = new VentaHistorial
                               {
                                   VentaId = first["ventaId"].ToString(),
                                   ClienteId = first["clienteId"].ToString(),
                                   ClienteNombre = first["ClienteNombre"].ToString(),
                                   Empleado = first["Empleado"].ToString(),
                                   FechaVenta = Convert.ToDateTime(first["fechaVenta"]),
                                   Total = Convert.ToDecimal(first["Total"])
                               };

                               // Productos
                               foreach (var row in g.Where(r => r["productoId"] != DBNull.Value))
                               {
                                   ventaHist.Productos.Add(new ProductoVenta
                                   {
                                       ProductoId = row["productoId"].ToString(),
                                       Nombre = row["ProductoNombre"].ToString(),
                                       Unidad = row["ProductoUnidad"].ToString(),
                                       Cantidad = Convert.ToInt32(row["ProductoCantidad"]),
                                       Costo = Convert.ToDecimal(row["ProductoCosto"])
                                   });
                               }

                               // Globos
                               foreach (var row in g.Where(r => r["globoId"] != DBNull.Value))
                               {
                                   ventaHist.Globos.Add(new GloboVenta
                                   {
                                       GloboId = row["globoId"].ToString(),
                                       Material = row["material"].ToString(),
                                       Color = row["color"].ToString(),
                                       Unidad = row["GloboUnidad"].ToString(),
                                       Cantidad = Convert.ToInt32(row["GloboCantidad"]),
                                       Costo = Convert.ToDecimal(row["GloboCosto"]),
                                       Tamano = row["Tamanios"].ToString(),
                                       Forma = row["Formas"].ToString(),
                                       Tematica = row["Tematicas"].ToString()
                                   });
                               }

                               return ventaHist;
                           }).ToList();

            return ventas;
        }
    }
}