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
        // Obtiene todos los globos o solo los activos
        public List<Globo> ObtenerGlobos(bool soloActivos = true)
        {
            string query = @"
                SELECT g.globoId, g.material, g.unidad, g.color, g.costo, g.stock, g.proveedorId, g.Activo,
                       ISNULL(Tam.Tamanos, '') AS Tamano,
                       ISNULL(Form.Formas, '') AS Forma,
                       ISNULL(Temp.Tematicas, '') AS Tematica
                FROM Globo g
                LEFT JOIN (
                    SELECT globoId, STRING_AGG(tamanio, ', ') AS Tamanos
                    FROM Globo_Tamanio GROUP BY globoId
                ) Tam ON g.globoId = Tam.globoId
                LEFT JOIN (
                    SELECT globoId, STRING_AGG(forma, ', ') AS Formas
                    FROM Globo_Forma GROUP BY globoId
                ) Form ON g.globoId = Form.globoId
                LEFT JOIN (
                    SELECT globoId, STRING_AGG(nombre, ', ') AS Tematicas
                    FROM Tematica GROUP BY globoId
                ) Temp ON g.globoId = Temp.globoId";

            if (soloActivos) query += " WHERE g.Activo = 1";
            query += " ORDER BY g.globoId";

            DataTable dt = DbHelper.ExecuteQuery(query);
            return dt.AsEnumerable().Select(MapearGlobo).ToList();
        }

        // Obtiene un globo por su ID
        public Globo? ObtenerGloboPorId(string globoId)
        {
            string query = @"
                SELECT g.globoId, g.material, g.unidad, g.color, g.costo, g.stock, g.proveedorId, g.Activo,
                       ISNULL(Tam.Tamanos, '') AS Tamano,
                       ISNULL(Form.Formas, '') AS Forma,
                       ISNULL(Temp.Tematicas, '') AS Tematica
                FROM Globo g
                LEFT JOIN (
                    SELECT globoId, STRING_AGG(tamanio, ', ') AS Tamanos
                    FROM Globo_Tamanio GROUP BY globoId
                ) Tam ON g.globoId = Tam.globoId
                LEFT JOIN (
                    SELECT globoId, STRING_AGG(forma, ', ') AS Formas
                    FROM Globo_Forma GROUP BY globoId
                ) Form ON g.globoId = Form.globoId
                LEFT JOIN (
                    SELECT globoId, STRING_AGG(nombre, ', ') AS Tematicas
                    FROM Tematica GROUP BY globoId
                ) Temp ON g.globoId = Temp.globoId
                WHERE g.globoId = @id";

            var parametros = new[] { new SqlParameter("@id", globoId) };
            DataTable dt = DbHelper.ExecuteQuery(query, parametros);
            return dt.Rows.Count == 0 ? null : MapearGlobo(dt.Rows[0]);
        }

        // Mapea un DataRow a un objeto Globo
        private Globo MapearGlobo(DataRow row) => new()
        {
            GloboId = row["globoId"].ToString(),
            Material = row["material"].ToString(),
            Unidad = row["unidad"].ToString(),
            Color = row["color"].ToString(),
            Costo = Convert.ToDecimal(row["costo"]),
            Stock = Convert.ToInt32(row["stock"]),
            ProveedorId = row["proveedorId"] != DBNull.Value ? row["proveedorId"].ToString() : null,
            Activo = Convert.ToBoolean(row["Activo"]),
            Tamanos = row["Tamano"].ToString()
                         .Split(',', StringSplitOptions.RemoveEmptyEntries)
                         .Select(x => x.Trim())
                         .Distinct()
                         .ToList(),
            Formas = row["Forma"].ToString()
                         .Split(',', StringSplitOptions.RemoveEmptyEntries)
                         .Select(x => x.Trim())
                         .Distinct()
                         .ToList(),
            Tematicas = row["Tematica"].ToString()
                         .Split(',', StringSplitOptions.RemoveEmptyEntries)
                         .Select(x => x.Trim())
                         .Distinct()
                         .ToList(),
            VentasHoy = 0
        };

        // Agrega un nuevo globo con características
        public bool AgregarGlobo(Globo globo)
        {
            using var conn = DbHelper.ObtenerConexion();
            using var tran = conn.BeginTransaction();
            try
            {
                string queryGlobo = @"
                    INSERT INTO Globo (globoId, material, unidad, color, costo, stock, proveedorId, Activo)
                    VALUES (@id, @material, @unidad, @color, @costo, @stock, @proveedorId, 1)";

                using var cmd = new SqlCommand(queryGlobo, conn, tran);
                cmd.Parameters.AddWithValue("@id", globo.GloboId);
                cmd.Parameters.AddWithValue("@material", globo.Material);
                cmd.Parameters.AddWithValue("@unidad", globo.Unidad);
                cmd.Parameters.AddWithValue("@color", globo.Color);
                cmd.Parameters.AddWithValue("@costo", globo.Costo);
                cmd.Parameters.AddWithValue("@stock", globo.Stock);
                cmd.Parameters.AddWithValue("@proveedorId", (object)globo.ProveedorId ?? DBNull.Value);
                cmd.ExecuteNonQuery();

                // Insertar características únicas
                InsertarCaracteristicasUnicas(globo.GloboId, "Globo_Tamanio", "tamanio", globo.Tamanos, conn, tran);
                InsertarCaracteristicasUnicas(globo.GloboId, "Globo_Forma", "forma", globo.Formas, conn, tran);
                InsertarCaracteristicasUnicas(globo.GloboId, "Tematica", "nombre", globo.Tematicas, conn, tran, true);

                tran.Commit();
                return true;
            }
            catch
            {
                tran.Rollback();
                return false;
            }
        }

        // Actualiza un globo existente y sus características
        public bool ActualizarGlobo(Globo globo)
        {
            using var conn = DbHelper.ObtenerConexion();
            using var tran = conn.BeginTransaction();
            try
            {
                string queryGlobo = @"
                    UPDATE Globo
                    SET material=@material, unidad=@unidad, color=@color, costo=@costo, stock=@stock, proveedorId=@proveedorId
                    WHERE globoId=@id";

                using var cmd = new SqlCommand(queryGlobo, conn, tran);
                cmd.Parameters.AddWithValue("@id", globo.GloboId);
                cmd.Parameters.AddWithValue("@material", globo.Material);
                cmd.Parameters.AddWithValue("@unidad", globo.Unidad);
                cmd.Parameters.AddWithValue("@color", globo.Color);
                cmd.Parameters.AddWithValue("@costo", globo.Costo);
                cmd.Parameters.AddWithValue("@stock", globo.Stock);
                cmd.Parameters.AddWithValue("@proveedorId", (object)globo.ProveedorId ?? DBNull.Value);
                cmd.ExecuteNonQuery();

                // Borrar e insertar características únicas
                ReemplazarCaracteristicas(globo.GloboId, "Globo_Tamanio", "tamanio", globo.Tamanos, conn, tran);
                ReemplazarCaracteristicas(globo.GloboId, "Globo_Forma", "forma", globo.Formas, conn, tran);
                ReemplazarCaracteristicas(globo.GloboId, "Tematica", "nombre", globo.Tematicas, conn, tran, true);

                tran.Commit();
                return true;
            }
            catch
            {
                tran.Rollback();
                return false;
            }
        }

        // Elimina un globo (soft delete)
        public bool EliminarGlobo(string globoId)
        {
            string query = "UPDATE Globo SET Activo = 0 WHERE globoId=@id";
            var parametros = new[] { new SqlParameter("@id", globoId) };
            return DbHelper.ExecuteNonQuery(query, parametros) > 0;
        }

        // Actualiza stock de un globo con auditoría
        public void ActualizarStock(string globoId, int cantidad, int? empleadoId, SqlConnection conn, SqlTransaction tran)
        {
            // Obtener stock actual
            int stockActual = 0;
            using (var cmdSelect = new SqlCommand("SELECT stock FROM Globo WHERE globoId=@globoId", conn, tran))
            {
                cmdSelect.Parameters.AddWithValue("@globoId", globoId);
                stockActual = (int)cmdSelect.ExecuteScalar();
            }

            int stockNuevo = stockActual - cantidad;

            using var cmdUpdate = new SqlCommand("UPDATE Globo SET stock=@nuevoStock WHERE globoId=@globoId", conn, tran);
            cmdUpdate.Parameters.AddWithValue("@nuevoStock", stockNuevo);
            cmdUpdate.Parameters.AddWithValue("@globoId", globoId);
            cmdUpdate.ExecuteNonQuery();

            // Insertar auditoría
            using var cmdHist = new SqlCommand(@"
                INSERT INTO HistorialAjusteStockGlobo (GloboId, CantidadAnterior, CantidadNueva, EmpleadoId)
                VALUES (@globoId, @anterior, @nuevo, @empleado)", conn, tran);
            cmdHist.Parameters.AddWithValue("@globoId", globoId);
            cmdHist.Parameters.AddWithValue("@anterior", stockActual);
            cmdHist.Parameters.AddWithValue("@nuevo", stockNuevo);
            cmdHist.Parameters.AddWithValue("@empleado", (object)empleadoId ?? DBNull.Value);
            cmdHist.ExecuteNonQuery();
        }

        // Borra características de una tabla
        private void BorrarCaracteristicas(string globoId, string tabla, SqlConnection conn, SqlTransaction tran)
        {
            string query = $"DELETE FROM {tabla} WHERE globoId=@globoId";
            using var cmd = new SqlCommand(query, conn, tran);
            cmd.Parameters.AddWithValue("@globoId", globoId);
            cmd.ExecuteNonQuery();
        }

        // Inserta características únicas evitando duplicados
        private void InsertarCaracteristicasUnicas(string globoId, string tabla, string columna, IEnumerable<string> valores, SqlConnection conn, SqlTransaction tran, bool usarGuidComoPK = false)
        {
            foreach (var valor in valores.Where(v => !string.IsNullOrWhiteSpace(v)).Distinct())
            {
                using var cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.Transaction = tran;

                if (usarGuidComoPK)
                {
                    string pkValue = Guid.NewGuid().ToString("N");
                    cmd.CommandText = $"INSERT INTO {tabla} (claveTematica, globoId, {columna}) VALUES (@pk, @globoId, @valor)";
                    cmd.Parameters.AddWithValue("@pk", pkValue);
                }
                else
                {
                    cmd.CommandText = $"INSERT INTO {tabla} (globoId, {columna}) VALUES (@globoId, @valor)";
                }

                cmd.Parameters.AddWithValue("@globoId", globoId);
                cmd.Parameters.AddWithValue("@valor", valor.Trim());
                cmd.ExecuteNonQuery();
            }
        }

        // Reemplaza características: borra y vuelve a insertar
        private void ReemplazarCaracteristicas(string globoId, string tabla, string columna, IEnumerable<string> valores, SqlConnection conn, SqlTransaction tran, bool usarGuidComoPK = false)
        {
            BorrarCaracteristicas(globoId, tabla, conn, tran);
            InsertarCaracteristicasUnicas(globoId, tabla, columna, valores, conn, tran, usarGuidComoPK);
        }
    }
}