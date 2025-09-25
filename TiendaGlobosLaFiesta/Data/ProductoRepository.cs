using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using TiendaGlobosLaFiesta.Models;

namespace TiendaGlobosLaFiesta.Data
{
    public class ProductoRepository
    {
        public List<Producto> ObtenerProductos()
        {
            // 🔹 CAMBIO: Se añade "WHERE Activo = 1" y se obtienen las nuevas columnas
            string query = @"
                SELECT productoId, nombre, unidad, costo, stock, proveedorId, categoriaId, Activo 
                FROM Producto 
                WHERE Activo = 1";

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
                    ProveedorId = row["proveedorId"] != DBNull.Value ? row["proveedorId"].ToString() : null,
                    CategoriaId = row["categoriaId"] != DBNull.Value ? Convert.ToInt32(row["categoriaId"]) : (int?)null,
                    Activo = Convert.ToBoolean(row["Activo"])
                });
            }
            return lista;
        }

        public bool AgregarProducto(Producto producto)
        {
            // 🔹 CAMBIO: La consulta INSERT ahora incluye las nuevas columnas
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
            // 🔹 CAMBIO: La consulta UPDATE ahora incluye las nuevas columnas
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
            // 🔹 CAMBIO: Ahora es un UPDATE (borrado lógico) en lugar de un DELETE
            string query = "UPDATE Producto SET Activo = 0 WHERE productoId=@id";
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