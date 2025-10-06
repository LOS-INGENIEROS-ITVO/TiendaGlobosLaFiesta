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
            string query = "SELECT productoId, nombre, unidad, costo, stock, proveedorId, categoriaId, Activo FROM Producto";
            if (soloActivos) query += " WHERE Activo = 1";

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
            string query = @"SELECT productoId, nombre, unidad, costo, stock, proveedorId, categoriaId, Activo
                             FROM Producto WHERE productoId=@id";
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
                ProductoId = row["productoId"].ToString(),
                Nombre = row["nombre"].ToString(),
                Unidad = Convert.ToInt32(row["unidad"]),
                Costo = Convert.ToDecimal(row["costo"]),
                Stock = Convert.ToInt32(row["stock"]),
                ProveedorId = row["proveedorId"] != DBNull.Value ? row["proveedorId"].ToString() : null,
                CategoriaId = row["categoriaId"] != DBNull.Value ? Convert.ToInt32(row["categoriaId"]) : (int?)null,
                Activo = Convert.ToBoolean(row["Activo"]),
                VentasHoy = 0
            };
        }

        #endregion

        #region CRUD Productos

        public bool AgregarProducto(Producto producto)
        {
            string query = @"
                INSERT INTO Producto (productoId, nombre, unidad, costo, stock, proveedorId, categoriaId, Activo)
                VALUES (@id, @nombre, @unidad, @costo, @stock, @proveedorId, @categoriaId, 1)";

            var parametros = new[]
            {
                new SqlParameter("@id", producto.ProductoId),
                new SqlParameter("@nombre", producto.Nombre),
                new SqlParameter("@unidad", producto.Unidad),
                new SqlParameter("@costo", producto.Costo),
                new SqlParameter("@stock", producto.Stock),
                new SqlParameter("@proveedorId", (object)producto.ProveedorId ?? DBNull.Value),
                new SqlParameter("@categoriaId", (object)producto.CategoriaId ?? DBNull.Value)
            };

            try
            {
                using SqlConnection conn = DbHelper.ObtenerConexion();
                return DbHelper.ExecuteNonQuery(query, parametros, conn) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al agregar producto: " + ex.Message, ex);
            }
        }

        public bool ActualizarProducto(Producto producto)
        {
            string query = @"
                UPDATE Producto 
                SET nombre=@nombre, unidad=@unidad, costo=@costo, stock=@stock, 
                    proveedorId=@proveedorId, categoriaId=@categoriaId
                WHERE productoId=@id";

            var parametros = new[]
            {
                new SqlParameter("@id", producto.ProductoId),
                new SqlParameter("@nombre", producto.Nombre),
                new SqlParameter("@unidad", producto.Unidad),
                new SqlParameter("@costo", producto.Costo),
                new SqlParameter("@stock", producto.Stock),
                new SqlParameter("@proveedorId", (object)producto.ProveedorId ?? DBNull.Value),
                new SqlParameter("@categoriaId", (object)producto.CategoriaId ?? DBNull.Value)
            };

            try
            {
                using SqlConnection conn = DbHelper.ObtenerConexion();
                return DbHelper.ExecuteNonQuery(query, parametros, conn) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al actualizar producto {producto.ProductoId}: {ex.Message}", ex);
            }
        }

        public bool EliminarProducto(string productoId)
        {
            string query = "UPDATE Producto SET Activo = 0 WHERE productoId=@id";
            var parametros = new[] { new SqlParameter("@id", productoId) };

            try
            {
                using SqlConnection conn = DbHelper.ObtenerConexion();
                return DbHelper.ExecuteNonQuery(query, parametros, conn) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al desactivar producto {productoId}: {ex.Message}", ex);
            }
        }

        #endregion

        #region Stock y Ajustes

        public void ActualizarStock(string productoId, int cantidad, int? empleadoId, SqlConnection conn, SqlTransaction tran)
        {
            // Obtener stock actual
            int stockActual = 0;
            using (var cmdSelect = new SqlCommand("SELECT stock FROM Producto WHERE productoId=@productoId", conn, tran))
            {
                cmdSelect.Parameters.AddWithValue("@productoId", productoId);
                stockActual = (int)cmdSelect.ExecuteScalar();
            }

            int stockNuevo = stockActual - cantidad;

            // Actualizar stock
            using (var cmdUpdate = new SqlCommand("UPDATE Producto SET stock=@nuevoStock WHERE productoId=@productoId", conn, tran))
            {
                cmdUpdate.Parameters.AddWithValue("@nuevoStock", stockNuevo);
                cmdUpdate.Parameters.AddWithValue("@productoId", productoId);
                cmdUpdate.ExecuteNonQuery();
            }

            // Insertar auditoría (opcional)
            using var cmdHist = new SqlCommand(@"
        INSERT INTO HistorialAjusteStock (ProductoId, CantidadAnterior, CantidadNueva, EmpleadoId)
        VALUES (@productoId, @anterior, @nuevo, @empleado)", conn, tran);
            cmdHist.Parameters.AddWithValue("@productoId", productoId);
            cmdHist.Parameters.AddWithValue("@anterior", stockActual);
            cmdHist.Parameters.AddWithValue("@nuevo", stockNuevo);
            cmdHist.Parameters.AddWithValue("@empleado", (object)empleadoId ?? DBNull.Value);
            cmdHist.ExecuteNonQuery();
        }

        public void RegistrarAjusteStock(string productoId, int cantidadAnterior, int cantidadNueva, string motivo)
        {
            string query = @"
                INSERT INTO HistorialAjusteStock (ProductoId, CantidadAnterior, CantidadNueva, Motivo, EmpleadoId)
                VALUES (@ProductoId, @CantAnterior, @CantNueva, @Motivo, @EmpleadoId)";

            var parametros = new[]
            {
                new SqlParameter("@ProductoId", productoId),
                new SqlParameter("@CantAnterior", cantidadAnterior),
                new SqlParameter("@CantNueva", cantidadNueva),
                new SqlParameter("@Motivo", motivo),
                new SqlParameter("@EmpleadoId", SesionActual.EmpleadoId)
            };

            try
            {
                using SqlConnection conn = DbHelper.ObtenerConexion();
                DbHelper.ExecuteNonQuery(query, parametros, conn);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al registrar ajuste de stock del producto {productoId}: {ex.Message}", ex);
            }
        }

        #endregion
    }
}