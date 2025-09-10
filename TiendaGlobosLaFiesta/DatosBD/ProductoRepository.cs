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
                    Stock = Convert.ToInt32(row["stock"])
                });
            }
            return lista;
        }

        public class ProductoMasVendido
        {
            public string Nombre { get; set; }
            public int Cantidad { get; set; }
        }

        public ProductoMasVendido ObtenerProductoMasVendido(string periodo)
        {
            string where = periodo switch
            {
                "DIA" => "CAST(v.fecha AS DATE) = CAST(GETDATE() AS DATE)",
                "SEMANA" => "v.fecha >= DATEADD(DAY,-6,CAST(GETDATE() AS DATE))",
                "MES" => "MONTH(v.fecha) = MONTH(GETDATE()) AND YEAR(v.fecha) = YEAR(GETDATE())",
                _ => "1=0"
            };

            string query = $@"
            SELECT TOP 1 p.nombre, SUM(vd.cantidad) AS Cantidad
            FROM VentaDetalle vd
            JOIN Venta v ON vd.ventaId = v.ventaId
            JOIN Producto p ON vd.productoId = p.productoId
            WHERE {where}
            GROUP BY p.nombre
            ORDER BY SUM(vd.cantidad) DESC";

            DataTable dt = DbHelper.ExecuteQuery(query);
            if (dt.Rows.Count == 0) return null;

            return new ProductoMasVendido
            {
                Nombre = dt.Rows[0]["nombre"].ToString(),
                Cantidad = Convert.ToInt32(dt.Rows[0]["Cantidad"])
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
                Stock = Convert.ToInt32(row["stock"])
            };
        }
    }
}