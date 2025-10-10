using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using TiendaGlobosLaFiesta.Models.Ventas;

namespace TiendaGlobosLaFiesta.Data
{
    public class DetalleVentaRepository
    {
        // Obtiene los productos de una venta
        public List<ProductoVenta> ObtenerDetalleProducto(string ventaId)
        {
            string query = @"
                SELECT dvp.productoId, p.nombre, dvp.cantidad, dvp.costo
                FROM Detalle_Venta_Producto dvp
                JOIN Producto p ON dvp.productoId = p.productoId
                WHERE dvp.ventaId=@ventaId";

            var parametros = new[] { new SqlParameter("@ventaId", ventaId) };
            DataTable dt = DbHelper.ExecuteQuery(query, parametros);

            var lista = new List<ProductoVenta>();
            foreach (DataRow row in dt.Rows)
            {
                lista.Add(new ProductoVenta
                {
                    ProductoId = row["productoId"].ToString(),
                    Nombre = row["nombre"].ToString(),
                    Cantidad = Convert.ToInt32(row["cantidad"]),
                    Costo = Convert.ToDecimal(row["costo"])
                    // Importe se calcula automáticamente
                });
            }

            return lista;
        }

        // Obtiene los globos de una venta
        public List<GloboVenta> ObtenerDetalleGlobo(string ventaId)
        {
            string query = @"
                SELECT dvg.globoId, g.material, g.color, dvg.cantidad, dvg.costo
                FROM Detalle_Venta_Globo dvg
                JOIN Globo g ON dvg.globoId = g.globoId
                WHERE dvg.ventaId=@ventaId";

            var parametros = new[] { new SqlParameter("@ventaId", ventaId) };
            DataTable dt = DbHelper.ExecuteQuery(query, parametros);

            var lista = new List<GloboVenta>();
            foreach (DataRow row in dt.Rows)
            {
                lista.Add(new GloboVenta
                {
                    GloboId = row["globoId"].ToString(),
                    Material = row["material"].ToString(),
                    Color = row["color"].ToString(),
                    Cantidad = Convert.ToInt32(row["cantidad"]),
                    Costo = Convert.ToDecimal(row["costo"])
                    // Importe se calcula automáticamente
                });
            }

            return lista;
        }
    }
}