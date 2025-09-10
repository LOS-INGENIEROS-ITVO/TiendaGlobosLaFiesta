using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using TiendaGlobosLaFiesta.Models;

namespace TiendaGlobosLaFiesta.Data
{
    public class GloboRepository
    {
        public bool AgregarGlobo(Globo globo)
        {
            string query = @"INSERT INTO Globo (globoId, material, unidad, color, costo, stock)
                             VALUES (@id, @material, @unidad, @color, @costo, @stock)";

            var parametros = new[]
            {
                new SqlParameter("@id", globo.GloboId),
                new SqlParameter("@material", globo.Material),
                new SqlParameter("@unidad", globo.Unidad),
                new SqlParameter("@color", globo.Color),
                new SqlParameter("@costo", globo.Costo),
                new SqlParameter("@stock", globo.Stock)
            };

            return DbHelper.ExecuteNonQuery(query, parametros) > 0;
        }

        public bool ActualizarGlobo(Globo globo)
        {
            string query = @"UPDATE Globo 
                             SET material=@material, unidad=@unidad, color=@color, costo=@costo, stock=@stock
                             WHERE globoId=@id";

            var parametros = new[]
            {
                new SqlParameter("@id", globo.GloboId),
                new SqlParameter("@material", globo.Material),
                new SqlParameter("@unidad", globo.Unidad),
                new SqlParameter("@color", globo.Color),
                new SqlParameter("@costo", globo.Costo),
                new SqlParameter("@stock", globo.Stock)
            };

            return DbHelper.ExecuteNonQuery(query, parametros) > 0;
        }

        public bool EliminarGlobo(string globoId)
        {
            string query = "DELETE FROM Globo WHERE globoId=@id";
            var parametros = new[] { new SqlParameter("@id", globoId) };
            return DbHelper.ExecuteNonQuery(query, parametros) > 0;
        }

        public List<Globo> ObtenerGlobos()
        {
            string query = @"
                SELECT g.globoId, g.material, g.unidad, g.color, g.costo, g.stock,
                       ISNULL(Tam.Tamano, '') AS Tamano,
                       ISNULL(Form.Forma, '') AS Forma,
                       ISNULL(Temp.Tematica, '') AS Tematica
                FROM Globo g
                LEFT JOIN (
                    SELECT globoId, STRING_AGG(tamanio, ', ') AS Tamano
                    FROM Globo_Tamanio
                    GROUP BY globoId
                ) Tam ON g.globoId = Tam.globoId
                LEFT JOIN (
                    SELECT globoId, STRING_AGG(forma, ', ') AS Forma
                    FROM Globo_Forma
                    GROUP BY globoId
                ) Form ON g.globoId = Form.globoId
                LEFT JOIN (
                    SELECT globoId, STRING_AGG(nombre, ', ') AS Tematica
                    FROM Tematica
                    GROUP BY globoId
                ) Temp ON g.globoId = Temp.globoId
                ORDER BY g.globoId";

            DataTable dt = DbHelper.ExecuteQuery(query);
            var lista = new List<Globo>();

            foreach (DataRow row in dt.Rows)
            {
                lista.Add(new Globo
                {
                    GloboId = row["globoId"].ToString(),
                    Material = row["material"].ToString(),
                    Unidad = row["unidad"].ToString(),
                    Color = row["color"].ToString(),
                    Costo = Convert.ToDecimal(row["costo"]),
                    Stock = Convert.ToInt32(row["stock"]),
                    Tamano = row["Tamano"].ToString(),
                    Forma = row["Forma"].ToString(),
                    Tematica = row["Tematica"].ToString()
                });
            }

            return lista;
        }


        public Globo ObtenerGloboMasVendido()
        {
            string query = @"
                SELECT TOP 1 g.globoId, g.color, g.material, SUM(dvg.cantidad) AS Cantidad
                FROM Detalle_Venta_Globo dvg
                INNER JOIN Globo g ON dvg.globoId = g.globoId
                INNER JOIN Venta v ON dvg.ventaId = v.ventaId
                WHERE CAST(v.fechaVenta AS DATE) = CAST(GETDATE() AS DATE)
                GROUP BY g.globoId, g.color, g.material
                ORDER BY SUM(dvg.cantidad) DESC";

            DataTable dt = DbHelper.ExecuteQuery(query);

            if (dt.Rows.Count > 0)
            {
                return new Globo
                {
                    GloboId = dt.Rows[0]["globoId"].ToString(),
                    Color = dt.Rows[0]["color"].ToString(),
                    Material = dt.Rows[0]["material"].ToString(),
                    Stock = 0, // aquí no viene stock real
                    VentasHoy = Convert.ToInt32(dt.Rows[0]["Cantidad"])
                };
            }

            return null;
        }

        public Globo? ObtenerGloboPorId(string globoId)
        {
            string query = @"SELECT globoId, material, unidad, color, costo, stock
                             FROM Globo WHERE globoId=@id";

            var parametros = new[] { new SqlParameter("@id", globoId) };
            DataTable dt = DbHelper.ExecuteQuery(query, parametros);

            if (dt.Rows.Count == 0) return null;

            DataRow row = dt.Rows[0];
            return new Globo
            {
                GloboId = row["globoId"].ToString(),
                Material = row["material"].ToString(),
                Unidad = row["unidad"].ToString(),
                Color = row["color"].ToString(),
                Costo = Convert.ToDecimal(row["costo"]),
                Stock = Convert.ToInt32(row["stock"]),
                VentasHoy = 0
            };
        }
    }
}