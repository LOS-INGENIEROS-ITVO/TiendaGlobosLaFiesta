using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using TiendaGlobosLaFiesta.Models;

namespace TiendaGlobosLaFiesta.Data
{
    public class GloboRepository
    {
        // Crear globo
        public bool AgregarGlobo(Globo globo)
        {
            string query = @"INSERT INTO Globo (globoId, nombre, color, tamanio, precio, stock)
                             VALUES (@id, @nombre, @color, @tamanio, @precio, @stock)";

            var parametros = new[]
            {
                new SqlParameter("@id", globo.GloboId),
                new SqlParameter("@nombre", globo.Nombre),
                new SqlParameter("@color", globo.Color),
                new SqlParameter("@tamanio", globo.Tamanio),
                new SqlParameter("@precio", globo.Precio),
                new SqlParameter("@stock", globo.Stock)
            };

            return DbHelper.ExecuteNonQuery(query, parametros) > 0;
        }

        // Actualizar globo
        public bool ActualizarGlobo(Globo globo)
        {
            string query = @"UPDATE Globo 
                             SET nombre=@nombre, color=@color, tamanio=@tamanio, 
                                 precio=@precio, stock=@stock
                             WHERE globoId=@id";

            var parametros = new[]
            {
                new SqlParameter("@id", globo.GloboId),
                new SqlParameter("@nombre", globo.Nombre),
                new SqlParameter("@color", globo.Color),
                new SqlParameter("@tamanio", globo.Tamanio),
                new SqlParameter("@precio", globo.Precio),
                new SqlParameter("@stock", globo.Stock)
            };

            return DbHelper.ExecuteNonQuery(query, parametros) > 0;
        }

        // Eliminar globo
        public bool EliminarGlobo(string globoId)
        {
            string query = "DELETE FROM Globo WHERE globoId=@id";
            var parametros = new[] { new SqlParameter("@id", globoId) };

            return DbHelper.ExecuteNonQuery(query, parametros) > 0;
        }

        // Obtener todos los globos
        public List<Globo> ObtenerGlobos()
        {
            string query = "SELECT globoId, nombre, color, tamanio, precio, stock FROM Globo";
            DataTable dt = DbHelper.ExecuteQuery(query);

            var lista = new List<Globo>();
            foreach (DataRow row in dt.Rows)
            {
                lista.Add(new Globo
                {
                    GloboId = row["globoId"].ToString(),
                    Nombre = row["nombre"].ToString(),
                    Color = row["color"].ToString(),
                    Tamanio = row["tamanio"].ToString(),
                    Precio = Convert.ToDecimal(row["precio"]),
                    Stock = Convert.ToInt32(row["stock"])
                });
            }
            return lista;
        }

        // Buscar globo por ID
        public Globo? ObtenerGloboPorId(string globoId)
        {
            string query = @"SELECT globoId, nombre, color, tamanio, precio, stock
                             FROM Globo WHERE globoId=@id";

            var parametros = new[] { new SqlParameter("@id", globoId) };
            DataTable dt = DbHelper.ExecuteQuery(query, parametros);

            if (dt.Rows.Count == 0) return null;

            DataRow row = dt.Rows[0];
            return new Globo
            {
                GloboId = row["globoId"].ToString(),
                Nombre = row["nombre"].ToString(),
                Color = row["color"].ToString(),
                Tamanio = row["tamanio"].ToString(),
                Precio = Convert.ToDecimal(row["precio"]),
                Stock = Convert.ToInt32(row["stock"])
            };
        }
    }
}
