using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using TiendaGlobosLaFiesta.Models;

namespace TiendaGlobosLaFiesta.Data
{
    public class ProductoRepository
    {
        #region Obtener Productos

        public List<Producto> ObtenerProductos(bool soloActivos = true)
        {
            var lista = new List<Producto>();
            string query = @"SELECT p.productoId, p.nombre, p.unidad, p.costo, p.stock, p.proveedorId, p.categoriaId, p.Activo,
                                    ISNULL(pr.razonSocial, 'Sin proveedor') AS ProveedorNombre
                             FROM Producto p
                             LEFT JOIN Proveedor pr ON p.proveedorId = pr.proveedorId";

            if (soloActivos)
                query += " WHERE p.Activo = 1";

            try
            {
                using SqlConnection conn = DbHelper.ObtenerConexion();
                DataTable dt = DbHelper.ExecuteQuery(query, null, conn);

                foreach (DataRow row in dt.Rows)
                    lista.Add(MapearProducto(row));
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener productos: " + ex.Message, ex);
            }

            return lista;
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

            try
            {
                using SqlConnection conn = DbHelper.ObtenerConexion();
                DataTable dt = DbHelper.ExecuteQuery(query, parametros, conn);
                if (dt.Rows.Count == 0) return null;

                return MapearProducto(dt.Rows[0]);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener el producto {productoId}: {ex.Message}", ex);
            }
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

        #endregion

        #region CRUD Productos

        public bool AgregarProducto(Producto producto)
        {
            if (producto == null) throw new ArgumentNullException(nameof(producto));

            // Si no tiene ID, generamos uno único
            if (string.IsNullOrWhiteSpace(producto.ProductoId))
                producto.ProductoId = GenerarIdUnico();

            // Validar duplicado de ID
            if (ObtenerProductoPorId(producto.ProductoId) != null)
                throw new Exception($"Ya existe un producto con el ID {producto.ProductoId}. Genera uno diferente.");

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

            try
            {
                using SqlConnection conn = DbHelper.ObtenerConexion();
                return DbHelper.ExecuteNonQuery(query, parametros, conn) > 0;
            }
            catch (SqlException ex) when (ex.Number == 2627)
            {
                throw new Exception("Ya existe un producto con ese ID en la base de datos.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al agregar producto: " + ex.Message, ex);
            }
        }

        private string GenerarIdUnico()
        {
            string id;
            int intentos = 0;
            do
            {
                id = $"PRD{DateTime.Now:yyMMddHHmmss}{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}";
                intentos++;
                if (intentos > 5) throw new Exception("No se pudo generar un ID único para el producto.");
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

            try
            {
                using SqlConnection conn = DbHelper.ObtenerConexion();
                return DbHelper.ExecuteNonQuery(query, parametros, conn) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al desactivar el producto {productoId}: {ex.Message}", ex);
            }
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