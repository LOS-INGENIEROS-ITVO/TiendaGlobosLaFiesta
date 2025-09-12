using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using TiendaGlobosLaFiesta.Models;

namespace TiendaGlobosLaFiesta.Data
{
    public class ProductoRepository
    {
        public bool AgregarProducto(Producto producto)
        {
            string query = @"INSERT INTO Producto (productoId, nombre, unidad, costo, stock)
                             VALUES (@id, @nombre, @unidad, @costo, @stock)";

            var parametros = new[]
            {
                new SqlParameter("@id", producto.ProductoId),
                new SqlParameter("@nombre", producto.Nombre),
                new SqlParameter("@unidad", producto.Unidad),
                new SqlParameter("@costo", producto.Costo),
                new SqlParameter("@stock", producto.Stock)
            };

            return DbHelper.ExecuteNonQuery(query, parametros) > 0;
        }

        public bool ActualizarProducto(Producto producto)
        {
            string query = @"UPDATE Producto 
                             SET nombre=@nombre, unidad=@unidad, costo=@costo, stock=@stock
                             WHERE productoId=@id";

            var parametros = new[]
            {
                new SqlParameter("@id", producto.ProductoId),
                new SqlParameter("@nombre", producto.Nombre),
                new SqlParameter("@unidad", producto.Unidad),
                new SqlParameter("@costo", producto.Costo),
                new SqlParameter("@stock", producto.Stock)
            };

            return DbHelper.ExecuteNonQuery(query, parametros) > 0;
        }

        public bool EliminarProducto(string productoId)
        {
            string query = "DELETE FROM Producto WHERE productoId=@id";
            var parametros = new[] { new SqlParameter("@id", productoId) };
            return DbHelper.ExecuteNonQuery(query, parametros) > 0;
        }


        public void ActualizarStock(string productoId, int cantidadVendida, SqlConnection conn, SqlTransaction tran)
        {
            string query = "UPDATE Producto SET stock = stock - @cantidad WHERE productoId = @productoId";
            using var cmd = new SqlCommand(query, conn, tran);
            cmd.Parameters.AddWithValue("@cantidad", cantidadVendida);
            cmd.Parameters.AddWithValue("@productoId", productoId);
            cmd.ExecuteNonQuery();
        }


        public List<Producto> ObtenerProductos()
        {
            string query = "SELECT productoId, nombre, unidad, costo, stock FROM Producto";
            DataTable dt = DbHelper.ExecuteQuery(query);

            var lista = new List<Producto>();
            foreach (DataRow row in dt.Rows)
            {
                lista.Add(new Producto
                {
                    ProductoId = row["productoId"].ToString(),
                    Nombre = row["nombre"].ToString(),
                    Unidad = Convert.ToInt32(row["unidad"]),
                    Costo = Convert.ToDecimal(row["costo"]),
                    Stock = Convert.ToInt32(row["stock"]),
                    VentasHoy = 0 // por defecto
                });
            }
            return lista;
        }

        public Producto? ObtenerProductoPorId(string productoId)
        {
            string query = @"SELECT productoId, nombre, unidad, costo, stock
                             FROM Producto WHERE productoId=@id";

            var parametros = new[] { new SqlParameter("@id", productoId) };
            DataTable dt = DbHelper.ExecuteQuery(query, parametros);

            if (dt.Rows.Count == 0) return null;

            DataRow row = dt.Rows[0];
            return new Producto
            {
                ProductoId = row["productoId"].ToString(),
                Nombre = row["nombre"].ToString(),
                Unidad = Convert.ToInt32(row["unidad"]),
                Costo = Convert.ToDecimal(row["costo"]),
                Stock = Convert.ToInt32(row["stock"]),
                VentasHoy = 0
            };
        }
    }
}