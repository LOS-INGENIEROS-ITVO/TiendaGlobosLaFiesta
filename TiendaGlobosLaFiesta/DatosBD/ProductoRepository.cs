using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using TiendaGlobosLaFiesta.Models;

namespace TiendaGlobosLaFiesta.Data
{
    public class ProductoRepository
    {
        // Crear producto
        public bool AgregarProducto(Producto producto)
        {
            string query = @"INSERT INTO Producto (productoId, nombre, descripcion, precio, stock)
                             VALUES (@id, @nombre, @descripcion, @precio, @stock)";

            var parametros = new[]
            {
                new SqlParameter("@id", producto.ProductoId),
                new SqlParameter("@nombre", producto.Nombre),
                new SqlParameter("@descripcion", (object?)producto.Descripcion ?? DBNull.Value),
                new SqlParameter("@precio", producto.Precio),
                new SqlParameter("@stock", producto.Stock)
            };

            return DbHelper.ExecuteNonQuery(query, parametros) > 0;
        }

        // Actualizar producto
        public bool ActualizarProducto(Producto producto)
        {
            string query = @"UPDATE Producto 
                             SET nombre=@nombre, descripcion=@descripcion, precio=@precio, stock=@stock
                             WHERE productoId=@id";

            var parametros = new[]
            {
                new SqlParameter("@id", producto.ProductoId),
                new SqlParameter("@nombre", producto.Nombre),
                new SqlParameter("@descripcion", (object?)producto.Descripcion ?? DBNull.Value),
                new SqlParameter("@precio", producto.Precio),
                new SqlParameter("@stock", producto.Stock)
            };

            return DbHelper.ExecuteNonQuery(query, parametros) > 0;
        }

        // Eliminar producto
        public bool EliminarProducto(string productoId)
        {
            string query = "DELETE FROM Producto WHERE productoId=@id";
            var parametros = new[] { new SqlParameter("@id", productoId) };

            return DbHelper.ExecuteNonQuery(query, parametros) > 0;
        }

        // Obtener todos los productos
        public List<Producto> ObtenerProductos()
        {
            string query = "SELECT productoId, nombre, descripcion, precio, stock FROM Producto";
            DataTable dt = DbHelper.ExecuteQuery(query);

            var lista = new List<Producto>();
            foreach (DataRow row in dt.Rows)
            {
                lista.Add(new Producto
                {
                    ProductoId = row["productoId"].ToString(),
                    Nombre = row["nombre"].ToString(),
                    Descripcion = row["descripcion"] == DBNull.Value ? null : row["descripcion"].ToString(),
                    Precio = Convert.ToDecimal(row["precio"]),
                    Stock = Convert.ToInt32(row["stock"])
                });
            }
            return lista;
        }

        // Buscar producto por ID
        public Producto? ObtenerProductoPorId(string productoId)
        {
            string query = @"SELECT productoId, nombre, descripcion, precio, stock
                             FROM Producto WHERE productoId=@id";

            var parametros = new[] { new SqlParameter("@id", productoId) };
            DataTable dt = DbHelper.ExecuteQuery(query, parametros);

            if (dt.Rows.Count == 0) return null;

            DataRow row = dt.Rows[0];
            return new Producto
            {
                ProductoId = row["productoId"].ToString(),
                Nombre = row["nombre"].ToString(),
                Descripcion = row["descripcion"] == DBNull.Value ? null : row["descripcion"].ToString(),
                Precio = Convert.ToDecimal(row["precio"]),
                Stock = Convert.ToInt32(row["stock"])
            };
        }
    }
}