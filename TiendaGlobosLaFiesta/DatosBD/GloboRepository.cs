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
            string query = "SELECT globoId, material, unidad, color, costo, stock FROM Globo";
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
                    Stock = Convert.ToInt32(row["stock"])
                });
            }
            return lista;
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
                Stock = Convert.ToInt32(row["stock"])
            };
        }
    }
}