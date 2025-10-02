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
            string query = "SELECT productoId, nombre, unidad, costo, stock, proveedorId, categoriaId, Activo FROM Producto";
            if (soloActivos) query += " WHERE Activo = 1";

            DataTable dt = DbHelper.ExecuteQuery(query);
            var lista = new List<Producto>();

            foreach (DataRow row in dt.Rows)
            {
                lista.Add(MapearProducto(row));
            }
            return lista;
        }

        public Producto? ObtenerProductoPorId(string productoId)
        {
            string query = @"SELECT productoId, nombre, unidad, costo, stock, proveedorId, categoriaId, Activo
                             FROM Producto WHERE productoId=@id";
            var parametros = new[] { new SqlParameter("@id", productoId) };
            DataTable dt = DbHelper.ExecuteQuery(query, parametros);

            if (dt.Rows.Count == 0) return null;
            return MapearProducto(dt.Rows[0]);
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

            return DbHelper.ExecuteNonQuery(query, parametros) > 0;
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

            return DbHelper.ExecuteNonQuery(query, parametros) > 0;
        }

        public bool EliminarProducto(string productoId)
        {
            string query = "UPDATE Producto SET Activo = 0 WHERE productoId=@id";
            var parametros = new[] { new SqlParameter("@id", productoId) };
            return DbHelper.ExecuteNonQuery(query, parametros) > 0;
        }

        #endregion

        #region Stock y Ajustes

        public void ActualizarStock(string productoId, int cantidad, SqlConnection conn, SqlTransaction tran)
        {
            string query = "UPDATE Producto SET stock = stock - @cantidad WHERE productoId = @productoId";
            using var cmd = new SqlCommand(query, conn, tran);
            cmd.Parameters.AddWithValue("@cantidad", cantidad);
            cmd.Parameters.AddWithValue("@productoId", productoId);
            cmd.ExecuteNonQuery();
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
            DbHelper.ExecuteNonQuery(query, parametros);
        }

        #endregion
    }
}
