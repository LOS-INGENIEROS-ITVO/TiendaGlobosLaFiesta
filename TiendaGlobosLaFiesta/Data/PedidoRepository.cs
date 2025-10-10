using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using TiendaGlobosLaFiesta.Models.Pedidos;

namespace TiendaGlobosLaFiesta.Data
{
    public class PedidoRepository
    {
        public List<Pedido> ObtenerPedidos(string estatus = null)
        {
            string query = "SELECT pedidoId, proveedorId, fechaPedido, estatus, total FROM Pedido";
            if (!string.IsNullOrWhiteSpace(estatus))
                query += " WHERE estatus=@estatus";

            var parametros = estatus != null ? new[] { new SqlParameter("@estatus", estatus) } : null;
            DataTable dt = DbHelper.ExecuteQuery(query, parametros);

            var lista = new List<Pedido>();
            foreach (DataRow row in dt.Rows)
            {
                lista.Add(new Pedido
                {
                    PedidoId = Convert.ToInt32(row["pedidoId"]),
                    ProveedorId = row["proveedorId"].ToString(),
                    FechaPedido = Convert.ToDateTime(row["fechaPedido"]),
                    Estatus = row["estatus"].ToString(),
                    Total = Convert.ToDecimal(row["total"])
                });
            }
            return lista;
        }
    }
}