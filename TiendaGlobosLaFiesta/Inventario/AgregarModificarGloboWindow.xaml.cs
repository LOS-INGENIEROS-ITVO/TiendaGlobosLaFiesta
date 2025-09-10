using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using TiendaGlobosLaFiesta.Clientes;
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

        private const string LogFile = "Logs/ConexionBD.log";

        #region Logging
        private static void LogError(string mensaje, Exception ex = null)
        {
            try
            {
                Directory.CreateDirectory("Logs");
                File.AppendAllText(LogFile, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {mensaje} {ex?.Message}\n{ex?.StackTrace}\n");
            }
            catch { }
        }
        #endregion

        #region Conexión y ejecución
        private static SqlConnection ObtenerConexion()
        {
            var conn = new SqlConnection(ConnectionString);
            conn.Open();
            return conn;
        }

        public static DataTable EjecutarConsulta(string sql, SqlParameter[] parametros = null)
        {
            try
            {
                using var conn = ObtenerConexion();
                using var cmd = new SqlCommand(sql, conn);
                if (parametros != null) cmd.Parameters.AddRange(parametros);
                using var da = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
            catch (Exception ex)
            {
                LogError($"Error en EjecutarConsulta: {sql}", ex);
                return new DataTable();
            }
        }

        public static int EjecutarNonQuery(string sql, SqlParameter[] parametros = null)
        {
            try
            {
                using var conn = ObtenerConexion();
                using var cmd = new SqlCommand(sql, conn);
                if (parametros != null) cmd.Parameters.AddRange(parametros);
                return cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                LogError($"Error en EjecutarNonQuery: {sql}", ex);
                return 0;
            }
        }

        public static T EjecutarScalar<T>(string sql, SqlParameter[] parametros = null)
        {
            try
            {
                using var conn = ObtenerConexion();
                using var cmd = new SqlCommand(sql, conn);
                if (parametros != null) cmd.Parameters.AddRange(parametros);
                object result = cmd.ExecuteScalar();
                return result != DBNull.Value && result != null ? (T)Convert.ChangeType(result, typeof(T)) : default;
            }
            catch (Exception ex)
            {
                LogError($"Error en EjecutarScalar: {sql}", ex);
                return default;
            }
        }

        public static SqlParameter Param(string nombre, object valor) => new SqlParameter(nombre, valor ?? DBNull.Value);
        #endregion

        #region Clientes
        public static List<Cliente> ObtenerClientes(string filtro = "")
        {
            try
            {
                string sql = "SELECT clienteId, primerNombre, segundoNombre, apellidoP, apellidoM, telefono FROM Cliente";
                SqlParameter[] parametros = null;
                if (!string.IsNullOrEmpty(filtro))
                {
                    sql += " WHERE primerNombre LIKE @filtro OR segundoNombre LIKE @filtro OR apellidoP LIKE @filtro OR apellidoM LIKE @filtro";
                    parametros = new[] { Param("@filtro", "%" + filtro + "%") };
                }

                var dt = EjecutarConsulta(sql, parametros);
                return dt.AsEnumerable().Select(row => new Cliente
                {
                    ClienteId = row["clienteId"].ToString(),
                    PrimerNombre = row["primerNombre"].ToString(),
                    SegundoNombre = row["segundoNombre"].ToString(),
                    ApellidoP = row["apellidoP"].ToString(),
                    ApellidoM = row["apellidoM"].ToString(),
                    Telefono = row["telefono"] != DBNull.Value ? Convert.ToInt64(row["telefono"]) : (long?)null
                }).ToList();
            }
            catch (Exception ex)
            {
                LogError("Error en ObtenerClientes.", ex);
                return new List<Cliente>();
            }
        }

        public static bool AgregarCliente(Cliente cliente)
        {
            try
            {
                string sql = @"INSERT INTO Cliente (clienteId, primerNombre, segundoNombre, apellidoP, apellidoM, telefono)
                               VALUES (@ClienteId,@PrimerNombre,@SegundoNombre,@ApellidoP,@ApellidoM,@Telefono)";
                var parametros = new[]
                {
                    Param("@ClienteId", cliente.ClienteId),
                    Param("@PrimerNombre", cliente.PrimerNombre),
                    Param("@SegundoNombre", cliente.SegundoNombre),
                    Param("@ApellidoP", cliente.ApellidoP),
                    Param("@ApellidoM", cliente.ApellidoM),
                    Param("@Telefono", cliente.Telefono ?? (object)DBNull.Value)
                };
                return EjecutarNonQuery(sql, parametros) > 0;
            }
            catch (Exception ex)
            {
                LogError("Error en AgregarCliente.", ex);
                return false;
            }
        }

        public static bool ActualizarCliente(Cliente cliente)
        {
            try
            {
                string sql = @"UPDATE Cliente SET primerNombre=@PrimerNombre, segundoNombre=@SegundoNombre,
                               apellidoP=@ApellidoP, apellidoM=@ApellidoM, telefono=@Telefono WHERE clienteId=@ClienteId";
                var parametros = new[]
                {
                    Param("@ClienteId", cliente.ClienteId),
                    Param("@PrimerNombre", cliente.PrimerNombre),
                    Param("@SegundoNombre", cliente.SegundoNombre),
                    Param("@ApellidoP", cliente.ApellidoP),
                    Param("@ApellidoM", cliente.ApellidoM),
                    Param("@Telefono", cliente.Telefono ?? (object)DBNull.Value)
                };
                return EjecutarNonQuery(sql, parametros) > 0;
            }
            catch (Exception ex)
            {
                LogError("Error en ActualizarCliente.", ex);
                return false;
            }
        }

        public static bool EliminarCliente(string clienteId)
        {
            try
            {
                string sql = "DELETE FROM Cliente WHERE clienteId=@ClienteId";
                return EjecutarNonQuery(sql, new[] { Param("@ClienteId", clienteId) }) > 0;
            }
            catch (Exception ex)
            {
                LogError("Error en EliminarCliente.", ex);
                return false;
            }
        }
        #endregion

        #region Productos
        public static List<ProductoVenta> ObtenerProductos()
        {
            try
            {
                string sql = "SELECT productoId, nombre, unidad, stock, costo FROM Producto";
                var dt = EjecutarConsulta(sql);
                return dt.AsEnumerable().Select(row => new ProductoVenta
                {
                    ProductoId = row["productoId"].ToString(),
                    Nombre = row["nombre"].ToString(),
                    Unidad = row["unidad"].ToString(),
                    Stock = Convert.ToInt32(row["stock"]),
                    Costo = Convert.ToDecimal(row["costo"])
                }).ToList();
            }
            catch (Exception ex)
            {
                LogError("Error en ObtenerProductos.", ex);
                return new List<ProductoVenta>();
            }
        }
        #endregion

        #region Globos
        public static List<GloboVenta> ObtenerGlobos()
        {
            try
            {
                string sql = @"
                SELECT g.globoId, g.material, g.color, g.unidad, g.stock, g.costo,
                       ISNULL(t.Tamanios,'') AS Tamano,
                       ISNULL(f.Formas,'') AS Forma,
                       ISNULL(tm.Tematicas,'') AS Tematica
                FROM Globo g
                LEFT JOIN (SELECT globoId, STRING_AGG(tamanio, ', ') AS Tamanios FROM Globo_Tamanio GROUP BY globoId) t ON g.globoId=t.globoId
                LEFT JOIN (SELECT globoId, STRING_AGG(forma, ', ') AS Formas FROM Globo_Forma GROUP BY globoId) f ON g.globoId=f.globoId
                LEFT JOIN (SELECT globoId, STRING_AGG(nombre, ', ') AS Tematicas FROM Tematica GROUP BY globoId) tm ON g.globoId=tm.globoId
                ORDER BY g.globoId";
                var dt = EjecutarConsulta(sql);
                return dt.AsEnumerable().Select(row => new GloboVenta
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
                }).ToList();
            }
            catch (Exception ex)
            {
                LogError("Error en ObtenerGlobos.", ex);
                return new List<GloboVenta>();
            }
        }
        #endregion

        #region Ventas
        public static string ObtenerSiguienteVentaId()
        {
            try
            {
                string sql = "SELECT TOP 1 ventaId FROM Venta ORDER BY ventaId DESC";
                var ultimoId = EjecutarScalar<string>(sql);
                if (string.IsNullOrEmpty(ultimoId)) return "VEN0001";
                int numero = int.Parse(ultimoId.Substring(3)) + 1;
                return "VEN" + numero.ToString("D4");
            }
            catch (Exception ex)
            {
                LogError("Error en ObtenerSiguienteVentaId.", ex);
                return "VEN0001";
            }
        }

        public static bool RegistrarVenta(Venta venta)
        {
            try
            {
                using var conn = ObtenerConexion();
                using var tran = conn.BeginTransaction();

                // Insert Venta
                string sqlVenta = @"INSERT INTO Venta (ventaId, empleadoId, clienteId, fechaVenta, importeTotal)
                                    VALUES (@VentaId,@EmpleadoId,@ClienteId,@FechaVenta,@ImporteTotal)";
                using (var cmd = new SqlCommand(sqlVenta, conn, tran))
                {
                    cmd.Parameters.AddRange(new[]
                    {
                        Param("@VentaId", venta.VentaId),
                        Param("@EmpleadoId", venta.EmpleadoId),
                        Param("@ClienteId", venta.ClienteId),
                        Param("@FechaVenta", venta.FechaVenta),
                        Param("@ImporteTotal", venta.ImporteTotal)
                    });
                    cmd.ExecuteNonQuery();
                }

                // Productos
                foreach (var p in venta.Productos)
                {
                    if (p.Cantidad > p.Stock) throw new Exception($"Stock insuficiente para {p.Nombre}.");
                    string sqlDetP = @"INSERT INTO Detalle_Venta_Producto (ventaId, productoId, cantidad, costo, importe)
                                       VALUES (@VentaId,@ProductoId,@Cantidad,@Costo,@Importe);
                                       UPDATE Producto SET stock = stock - @Cantidad WHERE productoId=@ProductoId";
                    using var cmd = new SqlCommand(sqlDetP, conn, tran);
                    cmd.Parameters.AddRange(new[]
                    {
                        Param("@VentaId", venta.VentaId),
                        Param("@ProductoId", p.ProductoId),
                        Param("@Cantidad", p.Cantidad),
                        Param("@Costo", p.Costo),
                        Param("@Importe", p.Importe)
                    });
                    cmd.ExecuteNonQuery();
                }

                // Globos
                foreach (var g in venta.Globos)
                {
                    if (g.Cantidad > g.Stock) throw new Exception($"Stock insuficiente para {g.Material} - {g.Color}.");
                    string sqlDetG = @"INSERT INTO Detalle_Venta_Globo (ventaId, globoId, cantidad, costo, importe)
                                       VALUES (@VentaId,@GloboId,@Cantidad,@Costo,@Importe);
                                       UPDATE Globo SET stock = stock - @Cantidad WHERE globoId=@GloboId";
                    using var cmd = new SqlCommand(sqlDetG, conn, tran);
                    cmd.Parameters.AddRange(new[]
                    {
                        Param("@VentaId", venta.VentaId),
                        Param("@GloboId", g.GloboId),
                        Param("@Cantidad", g.Cantidad),
                        Param("@Costo", g.Costo),
                        Param("@Importe", g.Importe)
                    });
                    cmd.ExecuteNonQuery();
                }

                tran.Commit();
                return true;
            }
            catch (Exception ex)
            {
                LogError("Error en RegistrarVenta.", ex);
                return false;
            }
        }
        #endregion

        #region KPIs Ventas
        public static decimal ObtenerVentasHoy() => EjecutarScalar<decimal>(@"SELECT ISNULL(SUM(importeTotal),0) FROM Venta WHERE CAST(fechaVenta AS DATE)=CAST(GETDATE() AS DATE)");
        public static decimal ObtenerVentasAyer() => EjecutarScalar<decimal>(@"SELECT ISNULL(SUM(importeTotal),0) FROM Venta WHERE CAST(fechaVenta AS DATE)=CAST(DATEADD(DAY,-1,GETDATE()) AS DATE)");
        public static decimal ObtenerVentasMes() => EjecutarScalar<decimal>(@"SELECT ISNULL(SUM(importeTotal),0) FROM Venta WHERE YEAR(fechaVenta)=YEAR(GETDATE()) AND MONTH(fechaVenta)=MONTH(GETDATE())");
        public static decimal ObtenerVentasMesAnterior() => EjecutarScalar<decimal>(@"SELECT ISNULL(SUM(importeTotal),0) FROM Venta WHERE YEAR(fechaVenta)=YEAR(GETDATE()) AND MONTH(fechaVenta)=MONTH(DATEADD(MONTH,-1,GETDATE()))");
        public static decimal ObtenerTicketPromedioHoy() => EjecutarScalar<decimal>(@"SELECT ISNULL(AVG(importeTotal),0) FROM Venta WHERE CAST(fechaVenta AS DATE)=CAST(GETDATE() AS DATE)");

        public static Dictionary<DateTime, decimal> ObtenerVentasUltimos7DiasDetalle()
        {
            string sql = @"SELECT CAST(fechaVenta AS DATE) AS Fecha, ISNULL(SUM(importeTotal),0) AS Total
                           FROM Venta WHERE fechaVenta>=DATEADD(DAY,-6,GETDATE())
                           GROUP BY CAST(fechaVenta AS DATE) ORDER BY CAST(fechaVenta AS DATE)";
            var dt = EjecutarConsulta(sql);
            Dictionary<DateTime, decimal> ventas = dt.Rows.Cast<DataRow>().ToDictionary(r => Convert.ToDateTime(r["Fecha"]), r => Convert.ToDecimal(r["Total"]));
            for (int i = 6; i >= 0; i--)
            {
                DateTime dia = DateTime.Today.AddDays(-i);
                if (!ventas.ContainsKey(dia)) ventas.Add(dia, 0m);
            }
            return ventas.OrderBy(v => v.Key).ToDictionary(k => k.Key, v => v.Value);
        }

        public static decimal ObtenerVentasUltimos7DiasTotal() => ObtenerVentasUltimos7DiasDetalle().Values.Sum();
        public static decimal ObtenerVentas7DiasAnterior() => EjecutarScalar<decimal>(@"SELECT ISNULL(SUM(importeTotal),0) FROM Venta WHERE CAST(fechaVenta AS DATE)>=CAST(DATEADD(DAY,-13,GETDATE()) AS DATE) AND CAST(fechaVenta AS DATE)<=CAST(DATEADD(DAY,-7,GETDATE()) AS DATE)");
        #endregion

        #region KPIs Stock
        public class ProductoStockCritico { public string Nombre { get; set; } public int Stock { get; set; } }
        public static List<ProductoStockCritico> ObtenerProductosStockCritico(int limite = 5)
        {
            string sql = @"SELECT nombre, stock FROM Producto WHERE stock<=@Limite ORDER BY stock ASC";
            var dt = EjecutarConsulta(sql, new[] { Param("@Limite", limite) });
            return dt.Rows.Cast<DataRow>().Select(r => new ProductoStockCritico { Nombre = r["nombre"].ToString(), Stock = Convert.ToInt32(r["stock"]) }).ToList();
        }
        #endregion

        #region KPIs Clientes
        public class ClienteFrecuente { public string Nombre { get; set; } public string Apellido { get; set; } public string NombreCompleto() => $"{Nombre} {Apellido}"; }
        public static List<ClienteFrecuente> ObtenerClientesFrecuentesDetalle()
        {
            string sql = @"SELECT c.primerNombre AS Nombre, c.apellidoP AS Apellido
                           FROM Venta v INNER JOIN Cliente c ON v.clienteId=c.clienteId
                           WHERE CAST(v.fechaVenta AS DATE)>=DATEADD(DAY,-30,GETDATE())
                           GROUP BY c.primerNombre,c.apellidoP HAVING COUNT(v.clienteId)>=3";
            var dt = EjecutarConsulta(sql);
            return dt.Rows.Cast<DataRow>().Select(r => new ClienteFrecuente { Nombre = r["Nombre"].ToString(), Apellido = r["Apellido"].ToString() }).ToList();
        }

        public class TopClienteMes { public string ClienteId { get; set; } public string Nombre { get; set; } public decimal Total { get; set; } }
        public static TopClienteMes ObtenerTopClienteMes()
        {
            string sql = @"SELECT TOP 1 c.clienteId, LTRIM(RTRIM(c.primerNombre + ' ' + ISNULL(c.segundoNombre,'') + ' ' + c.apellidoP + ' ' + c.apellidoM)) AS Nombre, SUM(v.importeTotal) AS Total
                           FROM Venta v INNER JOIN Cliente c ON v.clienteId=c.clienteId
                           WHERE YEAR(v.fechaVenta)=YEAR(GETDATE()) AND MONTH(v.fechaVenta)=MONTH(GETDATE())
                           GROUP BY c.clienteId, c.primerNombre, c.segundoNombre, c.apellidoP, c.apellidoM
                           ORDER BY Total DESC";
            var dt = EjecutarConsulta(sql);
            if (dt.Rows.Count == 0) return null;
            var r = dt.Rows[0];
            return new TopClienteMes { ClienteId = r["clienteId"].ToString(), Nombre = r["Nombre"].ToString(), Total = r["Total"] != DBNull.Value ? Convert.ToDecimal(r["Total"]) : 0m };
        }

        public static ProductoVenta ObtenerProductoMasVendido(string periodo)
        {
            string filtroFecha = periodo switch
            {
                "DIA" => "CAST(fechaVenta AS DATE)=CAST(GETDATE() AS DATE)",
                "SEMANA" => "CAST(fechaVenta AS DATE)>=CAST(DATEADD(DAY,-((DATEPART(WEEKDAY,GETDATE())-1)),GETDATE()) AS DATE)",
                "MES" => "YEAR(fechaVenta)=YEAR(GETDATE()) AND MONTH(fechaVenta)=MONTH(GETDATE())",
                _ => "1=1"
            };
            string sql = $@"SELECT TOP 1 p.productoId, p.nombre, SUM(dv.cantidad) AS Cantidad
                            FROM Detalle_Venta_Producto dv INNER JOIN Producto p ON dv.productoId=p.productoId
                            INNER JOIN Venta v ON dv.ventaId=v.ventaId
                            WHERE {filtroFecha} GROUP BY p.productoId,p.nombre ORDER BY Cantidad DESC";
            var dt = EjecutarConsulta(sql);
            if (dt.Rows.Count == 0) return null;
            var r = dt.Rows[0];
            return new ProductoVenta { ProductoId = r["productoId"].ToString(), Nombre = r["nombre"].ToString(), Stock = Convert.ToInt32(r["Cantidad"]) };
        }
        #endregion
    }
}
