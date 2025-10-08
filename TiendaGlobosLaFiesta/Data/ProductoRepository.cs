using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using TiendaGlobosLaFiesta.Models.Inventario;
using TiendaGlobosLaFiesta.Models.Ventas;

namespace TiendaGlobosLaFiesta.Data
{
    public class ProductoRepository
    {
        #region Obtener Productos

        public List<Producto> ObtenerProductos(bool soloActivos = true)
        {
            var lista = new List<Producto>();
            string query = @"
                SELECT p.productoId, p.nombre, p.unidad, p.costo, p.stock, p.proveedorId, p.categoriaId, p.Activo,
                       ISNULL(pr.razonSocial, 'Sin proveedor') AS ProveedorNombre
                FROM Producto p
                LEFT JOIN Proveedor pr ON p.proveedorId = pr.proveedorId";

            if (soloActivos)
                query += " WHERE p.Activo = 1";

            DataTable dt = DbHelper.ExecuteQuery(query);
            foreach (DataRow row in dt.Rows)
                lista.Add(MapearProducto(row));

            return lista;
        }

        public List<Producto> ObtenerProductosEnStock()
        {
            return ObtenerProductos().Where(p => p.Stock > 0 && p.Activo).ToList();
        }

        public Producto? ObtenerProductoPorId(string productoId)
        {
            if (string.IsNullOrWhiteSpace(productoId))
                throw new ArgumentException("El ID del producto no puede ser vacío.", nameof(productoId));

            string query = @"
                SELECT p.productoId, p.nombre, p.unidad, p.costo, p.stock, p.proveedorId, p.categoriaId, p.Activo,
                       ISNULL(pr.razonSocial, 'Sin proveedor') AS ProveedorNombre
                FROM Producto p
                LEFT JOIN Proveedor pr ON p.proveedorId = pr.proveedorId
                WHERE p.productoId = @id";

            var parametros = new[] { new SqlParameter("@id", productoId) };
            DataTable dt = DbHelper.ExecuteQuery(query, parametros);
            return dt.Rows.Count == 0 ? null : MapearProducto(dt.Rows[0]);
        }

        public List<Producto> BuscarProductos(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return ObtenerProductos();

            string query = @"
                SELECT p.productoId, p.nombre, p.unidad, p.costo, p.stock, p.proveedorId, p.categoriaId, p.Activo,
                       ISNULL(pr.razonSocial, 'Sin proveedor') AS ProveedorNombre
                FROM Producto p
                LEFT JOIN Proveedor pr ON p.proveedorId = pr.proveedorId
                WHERE p.nombre LIKE @nombre";

            var parametros = new[] { new SqlParameter("@nombre", "%" + nombre + "%") };
            DataTable dt = DbHelper.ExecuteQuery(query, parametros);
            return dt.AsEnumerable().Select(MapearProducto).ToList();
        }

        public List<Producto> ObtenerProductosCriticos(int stockCritico = 10)
        {
            return ObtenerProductos().Where(p => p.Stock <= stockCritico && p.Activo).ToList();
        }

        private Producto MapearProducto(DataRow row)
        {
            return new Producto
            {
                ProductoId = row["productoId"]?.ToString() ?? string.Empty,
                Nombre = row["nombre"]?.ToString() ?? string.Empty,
                Unidad = row["unidad"] != DBNull.Value ? row["unidad"].ToString() : "pieza",
                Costo = row["costo"] != DBNull.Value ? Convert.ToDecimal(row["costo"]) : 0,
                Stock = row["stock"] != DBNull.Value ? Convert.ToInt32(row["stock"]) : 0,
                ProveedorId = row["proveedorId"] != DBNull.Value ? row["proveedorId"].ToString() : null,
                ProveedorNombre = row["ProveedorNombre"]?.ToString() ?? "Sin proveedor",
                CategoriaId = row["categoriaId"] != DBNull.Value ? Convert.ToInt32(row["categoriaId"]) : (int?)null,
                Activo = row["Activo"] != DBNull.Value && Convert.ToBoolean(row["Activo"]),
                VentasHoy = 0
            };
        }

        public ProductoVenta MapearParaVenta(Producto producto, int cantidad = 1)
        {
            return new ProductoVenta
            {
                ProductoId = producto.ProductoId,
                NombreProducto = producto.Nombre,
                Cantidad = cantidad,
                Costo = producto.Costo,
                Stock = producto.Stock
            };
        }


        #endregion

        #region CRUD Productos

        public bool AgregarProducto(Producto producto)
        {
            if (producto == null) throw new ArgumentNullException(nameof(producto));

            if (ObtenerProductos(false)
                    .Any(p => p.Nombre.Equals(producto.Nombre, StringComparison.OrdinalIgnoreCase)))
                throw new Exception("Ya existe un producto con este nombre.");

            if (string.IsNullOrWhiteSpace(producto.ProductoId))
                producto.ProductoId = GenerarIdUnico();

            int intentos = 0;
            bool insertado = false;

            while (!insertado && intentos < 5)
            {
                try
                {
                    string query = @"
                        INSERT INTO Producto (productoId, nombre, unidad, costo, stock, proveedorId, categoriaId, Activo)
                        VALUES (@id, @nombre, @unidad, @costo, @stock, @proveedorId, @categoriaId, @activo)";

                    var parametros = new[]
                    {
                        new SqlParameter("@id", producto.ProductoId),
                        new SqlParameter("@nombre", producto.Nombre),
                        new SqlParameter("@unidad", producto.Unidad),
                        new SqlParameter("@costo", producto.Costo),
                        new SqlParameter("@stock", producto.Stock),
                        new SqlParameter("@proveedorId", (object)producto.ProveedorId ?? DBNull.Value),
                        new SqlParameter("@categoriaId", (object)producto.CategoriaId ?? DBNull.Value),
                        new SqlParameter("@activo", producto.Activo)
                    };

                    using SqlConnection conn = DbHelper.ObtenerConexion();
                    int rows = DbHelper.ExecuteNonQuery(query, parametros, conn);
                    insertado = rows > 0;
                }
                catch (SqlException ex) when (ex.Number == 2627)
                {
                    producto.ProductoId = GenerarIdUnico();
                    intentos++;
                }
            }

            if (!insertado)
                throw new Exception("No se pudo insertar el producto después de varios intentos. Último ID: " + producto.ProductoId);

            return true;
        }

        public string GenerarIdUnico()
        {
            string id;
            int intentos = 0;

            do
            {
                id = $"PRD{DateTime.Now:yyMMddHHmmss}{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}";
                intentos++;
                if (intentos > 20)
                    throw new Exception("No se pudo generar un ID único para el producto después de varios intentos.");

            } while (ObtenerProductoPorId(id) != null);

            return id;
        }

        public bool ActualizarProducto(Producto producto)
        {
            using var conn = DbHelper.ObtenerConexion();
            using var tran = conn.BeginTransaction();
            try
            {
                ActualizarProducto(producto, conn, tran);
                tran.Commit();
                return true;
            }
            catch
            {
                tran.Rollback();
                return false;
            }
        }

        public bool ActualizarProducto(Producto producto, SqlConnection conn, SqlTransaction tran)
        {
            string query = @"
                UPDATE Producto
                SET nombre=@nombre, unidad=@unidad, costo=@costo, stock=@stock,
                    proveedorId=@proveedorId, categoriaId=@categoriaId, Activo=@activo
                WHERE productoId=@id";

            var parametros = new[]
            {
                new SqlParameter("@id", producto.ProductoId),
                new SqlParameter("@nombre", producto.Nombre),
                new SqlParameter("@unidad", producto.Unidad),
                new SqlParameter("@costo", producto.Costo),
                new SqlParameter("@stock", producto.Stock),
                new SqlParameter("@proveedorId", (object)producto.ProveedorId ?? DBNull.Value),
                new SqlParameter("@categoriaId", (object)producto.CategoriaId ?? DBNull.Value),
                new SqlParameter("@activo", producto.Activo)
            };

            DbHelper.ExecuteNonQuery(query, parametros, conn, tran);
            return true;
        }

        public bool EliminarProducto(string productoId)
        {
            if (string.IsNullOrWhiteSpace(productoId))
                throw new ArgumentException("El ID del producto no puede ser vacío.", nameof(productoId));

            string query = "UPDATE Producto SET Activo = 0 WHERE productoId = @id";
            var parametros = new[] { new SqlParameter("@id", productoId) };

            using SqlConnection conn = DbHelper.ObtenerConexion();
            return DbHelper.ExecuteNonQuery(query, parametros, conn) > 0;
        }

        #endregion

        #region Ajuste Stock con Historial

        public bool AjustarStockConHistorial(string productoId, int nuevaCantidad, int empleadoId, string motivo, SqlConnection conn, SqlTransaction tran)
        {
            var producto = ObtenerProductoPorId(productoId);
            if (producto == null) throw new Exception("Producto no encontrado");

            int stockAnterior = producto.Stock;
            producto.Stock = nuevaCantidad;

            ActualizarProducto(producto, conn, tran);

            string queryHist = @"
                INSERT INTO HistorialAjusteStock (ProductoId, CantidadAnterior, CantidadNueva, Motivo, EmpleadoId)
                VALUES (@ProductoId, @CantidadAnterior, @CantidadNueva, @Motivo, @EmpleadoId)";

            var parametros = new[]
            {
                new SqlParameter("@ProductoId", productoId),
                new SqlParameter("@CantidadAnterior", stockAnterior),
                new SqlParameter("@CantidadNueva", nuevaCantidad),
                new SqlParameter("@Motivo", motivo),
                new SqlParameter("@EmpleadoId", empleadoId)
            };

            DbHelper.ExecuteNonQuery(queryHist, parametros, conn, tran);
            return true;
        }

        #endregion
    }
}