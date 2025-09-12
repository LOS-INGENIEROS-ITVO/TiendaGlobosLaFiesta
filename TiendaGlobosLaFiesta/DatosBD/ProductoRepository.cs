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


        // ... (Dentro de la clase ProductoRepository, junto a los métodos existentes) ...

        /// <summary>
        /// Actualiza el stock de un producto restando la cantidad vendida.
        /// Debe ser llamado dentro de una transacción existente.
        /// </summary>
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

        public Producto? ObtenerProductoMasVendido(string periodo)
        {
            string filtroFecha = periodo switch
            {
                "DIA" => "CAST(v.fechaVenta AS DATE) = CAST(GETDATE() AS DATE)",
                "SEMANA" => "v.fechaVenta >= DATEADD(DAY, -7, CAST(GETDATE() AS DATE))",
                "MES" => "MONTH(v.fechaVenta) = MONTH(GETDATE()) AND YEAR(v.fechaVenta) = YEAR(GETDATE())",
                _ => "1=1" // sin filtro
            };

            string query = $@"
        SELECT TOP 1 p.productoId, p.nombre, SUM(dvp.cantidad) AS Cantidad
        FROM Detalle_Venta_Producto dvp
        INNER JOIN Producto p ON dvp.productoId = p.productoId
        INNER JOIN Venta v ON dvp.ventaId = v.ventaId
        WHERE {filtroFecha}
        GROUP BY p.productoId, p.nombre
        ORDER BY SUM(dvp.cantidad) DESC";

            var dt = DbHelper.ExecuteQuery(query);

            if (dt.Rows.Count == 0) return null;

            return new Producto
            {
                ProductoId = dt.Rows[0]["productoId"].ToString(),
                Nombre = dt.Rows[0]["nombre"].ToString(),
                VentasHoy = Convert.ToInt32(dt.Rows[0]["Cantidad"]),
                Stock = 0
            };
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