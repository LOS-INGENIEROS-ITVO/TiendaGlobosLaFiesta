using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using TiendaGlobosLaFiesta.Modelos;
using TiendaGlobosLaFiesta.Models;

namespace TiendaGlobosLaFiesta.Data
{
    public class ProductoRepository
    {
        #region Obtener Productos

        public List<Producto> ObtenerProductos(bool soloActivos = true)
        {
            var lista = new List<Producto>();
            string query = @"SELECT productoId, nombre, unidad, costo, stock, proveedorId, categoriaId, Activo FROM Producto";

            if (soloActivos)
                query += " WHERE Activo = 1";

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
                SELECT productoId, nombre, unidad, costo, stock, proveedorId, categoriaId, Activo
                FROM Producto
                WHERE productoId = @id";

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
                Unidad = row["unidad"] != DBNull.Value ? row["unidad"].ToString() : "1",
                Costo = row["costo"] != DBNull.Value ? Convert.ToDecimal(row["costo"]) : 0,
                Stock = row["stock"] != DBNull.Value ? Convert.ToInt32(row["stock"]) : 0,
                ProveedorId = row["proveedorId"] != DBNull.Value ? row["proveedorId"].ToString() : null,
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
                throw new Exception("Ya existe un producto con ese ID. Verifica el código antes de registrar.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al agregar producto: " + ex.Message, ex);
            }
        }

        public bool ActualizarProducto(Producto producto)
        {
            if (producto == null) throw new ArgumentNullException(nameof(producto));

            string query = @"
                UPDATE Producto
                SET nombre = @nombre,
                    unidad = @unidad,
                    costo = @costo,
                    stock = @stock,
                    proveedorId = @proveedorId,
                    categoriaId = @categoriaId,
                    Activo = @activo
                WHERE productoId = @id";

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
            catch (Exception ex)
            {
                throw new Exception($"Error al actualizar el producto {producto.ProductoId}: {ex.Message}", ex);
            }
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
    }
}