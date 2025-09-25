using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using TiendaGlobosLaFiesta.Models;

namespace TiendaGlobosLaFiesta.Data
{
    public class GloboRepository
    {
        public bool AgregarGlobo(Globo globo)
        {
            using var conn = DbHelper.ObtenerConexion();
            using var tran = conn.BeginTransaction();
            try
            {
                // 🔹 CAMBIO: Se añaden las nuevas columnas
                string queryGlobo = @"INSERT INTO Globo (globoId, material, unidad, color, costo, stock, proveedorId, Activo)
                                      VALUES (@id, @material, @unidad, @color, @costo, @stock, @proveedorId, 1)";
                using (var cmd = new SqlCommand(queryGlobo, conn, tran))
                {
                    cmd.Parameters.AddWithValue("@id", globo.GloboId);
                    cmd.Parameters.AddWithValue("@material", globo.Material);
                    cmd.Parameters.AddWithValue("@unidad", globo.Unidad);
                    cmd.Parameters.AddWithValue("@color", globo.Color);
                    cmd.Parameters.AddWithValue("@costo", globo.Costo);
                    cmd.Parameters.AddWithValue("@stock", globo.Stock);
                    cmd.Parameters.AddWithValue("@proveedorId", (object)globo.ProveedorId ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }

                InsertarCaracteristicas(globo.GloboId, "Globo_Tamanio", "tamanio", globo.Tamanos, conn, tran);
                InsertarCaracteristicas(globo.GloboId, "Globo_Forma", "forma", globo.Formas, conn, tran);
                InsertarCaracteristicas(globo.GloboId, "Tematica", "nombre", globo.Tematicas, conn, tran, true);

                tran.Commit();
                return true;
            }
            catch { tran.Rollback(); return false; }
        }

        public bool ActualizarGlobo(Globo globo)
        {
            using var conn = DbHelper.ObtenerConexion();
            using var tran = conn.BeginTransaction();
            try
            {
                // 🔹 CAMBIO: Se añade proveedorId a la actualización
                string queryGlobo = @"UPDATE Globo SET material=@material, unidad=@unidad, color=@color, 
                                      costo=@costo, stock=@stock, proveedorId=@proveedorId 
                                      WHERE globoId=@id";
                using (var cmd = new SqlCommand(queryGlobo, conn, tran))
                {
                    cmd.Parameters.AddWithValue("@id", globo.GloboId);
                    cmd.Parameters.AddWithValue("@material", globo.Material);
                    cmd.Parameters.AddWithValue("@unidad", globo.Unidad);
                    cmd.Parameters.AddWithValue("@color", globo.Color);
                    cmd.Parameters.AddWithValue("@costo", globo.Costo);
                    cmd.Parameters.AddWithValue("@stock", globo.Stock);
                    cmd.Parameters.AddWithValue("@proveedorId", (object)globo.ProveedorId ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }

                BorrarCaracteristicas(globo.GloboId, "Globo_Tamanio", conn, tran);
                InsertarCaracteristicas(globo.GloboId, "Globo_Tamanio", "tamanio", globo.Tamanos, conn, tran);
                BorrarCaracteristicas(globo.GloboId, "Globo_Forma", conn, tran);
                InsertarCaracteristicas(globo.GloboId, "Globo_Forma", "forma", globo.Formas, conn, tran);
                BorrarCaracteristicas(globo.GloboId, "Tematica", conn, tran);
                InsertarCaracteristicas(globo.GloboId, "Tematica", "nombre", globo.Tematicas, conn, tran, true);

                tran.Commit();
                return true;
            }
            catch { tran.Rollback(); return false; }
        }

        public bool EliminarGlobo(string globoId)
        {
            string query = "UPDATE Globo SET Activo = 0 WHERE globoId=@id";
            var parametros = new[] { new SqlParameter("@id", globoId) };
            return DbHelper.ExecuteNonQuery(query, parametros) > 0;
        }

        public List<Globo> ObtenerGlobos()
        {
            // 🔹 CAMBIO: Se añade "WHERE g.Activo = 1"
            string query = @"
                SELECT g.globoId, g.material, g.unidad, g.color, g.costo, g.stock, g.proveedorId, g.Activo,
                       ISNULL(Tam.Tamano, '') AS Tamano,
                       ISNULL(Form.Forma, '') AS Forma,
                       ISNULL(Temp.Tematica, '') AS Tematica
                FROM Globo g
                LEFT JOIN (SELECT globoId, STRING_AGG(tamanio, ', ') AS Tamano FROM Globo_Tamanio GROUP BY globoId) Tam ON g.globoId = Tam.globoId
                LEFT JOIN (SELECT globoId, STRING_AGG(forma, ', ') AS Forma FROM Globo_Forma GROUP BY globoId) Form ON g.globoId = Form.globoId
                LEFT JOIN (SELECT globoId, STRING_AGG(nombre, ', ') AS Tematica FROM Tematica GROUP BY globoId) Temp ON g.globoId = Temp.globoId
                WHERE g.Activo = 1
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
                    ProveedorId = row["proveedorId"] != DBNull.Value ? row["proveedorId"].ToString() : null,
                    Activo = Convert.ToBoolean(row["Activo"]),
                    Tamanos = row["Tamano"].ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList(),
                    Formas = row["Forma"].ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList(),
                    Tematicas = row["Tematica"].ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList(),
                });
            }
            return lista;
        }


        public Globo? ObtenerGloboPorId(string globoId)
        {
            string query = @"
                SELECT g.globoId, g.material, g.unidad, g.color, g.costo, g.stock, g.proveedorId, g.Activo,
                       ISNULL(Tam.Tamano, '') AS Tamano,
                       ISNULL(Form.Forma, '') AS Forma,
                       ISNULL(Temp.Tematica, '') AS Tematica
                FROM Globo g
                LEFT JOIN (SELECT globoId, STRING_AGG(tamanio, ', ') AS Tamano FROM Globo_Tamanio GROUP BY globoId) Tam ON g.globoId = Tam.globoId
                LEFT JOIN (SELECT globoId, STRING_AGG(forma, ', ') AS Forma FROM Globo_Forma GROUP BY globoId) Form ON g.globoId = Form.globoId
                LEFT JOIN (SELECT globoId, STRING_AGG(nombre, ', ') AS Tematica FROM Tematica GROUP BY globoId) Temp ON g.globoId = Temp.globoId
                WHERE g.globoId = @id";

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
                ProveedorId = row["proveedorId"] != DBNull.Value ? row["proveedorId"].ToString() : null,
                Activo = Convert.ToBoolean(row["Activo"]),
                Tamanos = row["Tamano"].ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList(),
                Formas = row["Forma"].ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList(),
                Tematicas = row["Tematica"].ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList()
            };
        }


        private void BorrarCaracteristicas(string globoId, string tabla, SqlConnection conn, SqlTransaction tran)
        {
            string query = $"DELETE FROM {tabla} WHERE globoId = @globoId";
            using var cmd = new SqlCommand(query, conn, tran);
            cmd.Parameters.AddWithValue("@globoId", globoId);
            cmd.ExecuteNonQuery();
        }

        private void InsertarCaracteristicas(string globoId, string tabla, string columna, IEnumerable<string> valores, SqlConnection conn, SqlTransaction tran, bool tieneClavePK = false)
        {
            foreach (var valor in valores.Where(v => !string.IsNullOrWhiteSpace(v)))
            {
                string query;
                using var cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.Transaction = tran;

                if (tieneClavePK)
                {
                    string pkColumn = "claveTematica";
                    string pkValue = $"{valor.Substring(0, Math.Min(3, valor.Length)).ToUpper()}{DateTime.Now.Ticks % 1000000}";
                    query = $"INSERT INTO {tabla} ({pkColumn}, globoId, {columna}) VALUES (@pkValue, @globoId, @valor)";
                    cmd.Parameters.AddWithValue("@pkValue", pkValue);
                }
                else
                {
                    query = $"INSERT INTO {tabla} (globoId, {columna}) VALUES (@globoId, @valor)";
                }

                cmd.CommandText = query;
                cmd.Parameters.AddWithValue("@globoId", globoId);
                cmd.Parameters.AddWithValue("@valor", valor.Trim());
                cmd.ExecuteNonQuery();
            }
        }

        public void ActualizarStock(string globoId, int cantidadVendida, SqlConnection conn, SqlTransaction tran)
        {
            string query = "UPDATE Globo SET stock = stock - @cantidad WHERE globoId = @globoId";
            using var cmd = new SqlCommand(query, conn, tran);
            cmd.Parameters.AddWithValue("@cantidad", cantidadVendida);
            cmd.Parameters.AddWithValue("@globoId", globoId);
            cmd.ExecuteNonQuery();
        }
    }
}