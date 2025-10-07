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
        #region Obtener Globos

        public List<Globo> ObtenerGlobos(bool soloActivos = true)
        {
            string query = @"
                SELECT g.globoId, g.material, g.unidad, g.color, g.costo, g.stock, g.proveedorId, g.Activo,
                       ISNULL(Tam.Tamanos, '') AS Tamano,
                       ISNULL(Form.Formas, '') AS Forma,
                       ISNULL(Temp.Tematicas, '') AS Tematica
                FROM Globo g
                LEFT JOIN (SELECT globoId, STRING_AGG(tamanio, ', ') AS Tamanos FROM Globo_Tamanio GROUP BY globoId) Tam ON g.globoId = Tam.globoId
                LEFT JOIN (SELECT globoId, STRING_AGG(forma, ', ') AS Formas FROM Globo_Forma GROUP BY globoId) Form ON g.globoId = Form.globoId
                LEFT JOIN (SELECT globoId, STRING_AGG(nombre, ', ') AS Tematicas FROM Tematica GROUP BY globoId) Temp ON g.globoId = Temp.globoId";

            if (soloActivos) query += " WHERE g.Activo = 1";
            query += " ORDER BY g.globoId";

            DataTable dt = DbHelper.ExecuteQuery(query);
            return dt.AsEnumerable().Select(MapearGlobo).ToList();
        }

        public Globo? ObtenerGloboPorId(string globoId)
        {
            if (string.IsNullOrWhiteSpace(globoId))
                throw new ArgumentException("El ID del globo no puede ser vacío.", nameof(globoId));

            string query = @"
                SELECT g.globoId, g.material, g.unidad, g.color, g.costo, g.stock, g.proveedorId, g.Activo,
                       ISNULL(Tam.Tamanos, '') AS Tamano,
                       ISNULL(Form.Formas, '') AS Forma,
                       ISNULL(Temp.Tematicas, '') AS Tematica
                FROM Globo g
                LEFT JOIN (SELECT globoId, STRING_AGG(tamanio, ', ') AS Tamanos FROM Globo_Tamanio GROUP BY globoId) Tam ON g.globoId = Tam.globoId
                LEFT JOIN (SELECT globoId, STRING_AGG(forma, ', ') AS Formas FROM Globo_Forma GROUP BY globoId) Form ON g.globoId = Form.globoId
                LEFT JOIN (SELECT globoId, STRING_AGG(nombre, ', ') AS Tematicas FROM Tematica GROUP BY globoId) Temp ON g.globoId = Temp.globoId
                WHERE g.globoId = @id";

            var parametros = new[] { new SqlParameter("@id", globoId) };
            DataTable dt = DbHelper.ExecuteQuery(query, parametros);
            return dt.Rows.Count == 0 ? null : MapearGlobo(dt.Rows[0]);
        }

        private Globo MapearGlobo(DataRow row)
        {
            return new Globo
            {
                GloboId = row["globoId"]?.ToString() ?? string.Empty,
                Material = row["material"]?.ToString() ?? string.Empty,
                Unidad = row["unidad"]?.ToString() ?? string.Empty,
                Color = row["color"]?.ToString() ?? string.Empty,
                Costo = row["costo"] != DBNull.Value ? Convert.ToDecimal(row["costo"]) : 0,
                Stock = row["stock"] != DBNull.Value ? Convert.ToInt32(row["stock"]) : 0,
                ProveedorId = row["proveedorId"] != DBNull.Value ? row["proveedorId"].ToString() : null,
                Activo = row["Activo"] != DBNull.Value && Convert.ToBoolean(row["Activo"]),
                Tamanos = row["Tamano"].ToString().Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).Distinct().ToList(),
                Formas = row["Forma"].ToString().Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).Distinct().ToList(),
                Tematicas = row["Tematica"].ToString().Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).Distinct().ToList(),
                VentasHoy = 0
            };
        }

        #endregion

        #region CRUD Globos

        public bool AgregarGlobo(Globo globo)
        {
            if (globo == null) throw new ArgumentNullException(nameof(globo));

            using var conn = DbHelper.ObtenerConexion();
            using var tran = conn.BeginTransaction();
            try
            {
                string queryGlobo = @"
                    INSERT INTO Globo (globoId, material, unidad, color, costo, stock, proveedorId, Activo)
                    VALUES (@id, @material, @unidad, @color, @costo, @stock, @proveedorId, 1)";

                var parametros = new[]
                {
                    new SqlParameter("@id", globo.GloboId),
                    new SqlParameter("@material", globo.Material),
                    new SqlParameter("@unidad", globo.Unidad),
                    new SqlParameter("@color", globo.Color),
                    new SqlParameter("@costo", globo.Costo),
                    new SqlParameter("@stock", globo.Stock),
                    new SqlParameter("@proveedorId", (object)globo.ProveedorId ?? DBNull.Value)
                };

                DbHelper.ExecuteNonQuery(queryGlobo, parametros, conn, tran);

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

        public bool ActualizarGlobo(Globo globo)
        {
            using var conn = DbHelper.ObtenerConexion();
            string query = @"
                UPDATE Globo
                SET material=@material, unidad=@unidad, color=@color, costo=@costo, stock=@stock, proveedorId=@proveedorId, Activo=@activo
                WHERE globoId=@id";

            var parametros = new[]
            {
                new SqlParameter("@id", globo.GloboId),
                new SqlParameter("@material", globo.Material),
                new SqlParameter("@unidad", globo.Unidad),
                new SqlParameter("@color", globo.Color),
                new SqlParameter("@costo", globo.Costo),
                new SqlParameter("@stock", globo.Stock),
                new SqlParameter("@proveedorId", (object)globo.ProveedorId ?? DBNull.Value),
                new SqlParameter("@activo", globo.Activo)
            };

            return DbHelper.ExecuteNonQuery(query, parametros, conn) > 0;
        }

        // 🟢 Sobrecarga con conexión y transacción
        public bool ActualizarGlobo(Globo globo, SqlConnection conn, SqlTransaction tran)
        {
            string query = @"
                UPDATE Globo
                SET material=@material, unidad=@unidad, color=@color, costo=@costo, stock=@stock, proveedorId=@proveedorId, Activo=@activo
                WHERE globoId=@id";

            var parametros = new[]
            {
                new SqlParameter("@id", globo.GloboId),
                new SqlParameter("@material", globo.Material),
                new SqlParameter("@unidad", globo.Unidad),
                new SqlParameter("@color", globo.Color),
                new SqlParameter("@costo", globo.Costo),
                new SqlParameter("@stock", globo.Stock),
                new SqlParameter("@proveedorId", (object)globo.ProveedorId ?? DBNull.Value),
                new SqlParameter("@activo", globo.Activo)
            };

            DbHelper.ExecuteNonQuery(query, parametros, conn, tran);
            return true;
        }

        public bool EliminarGlobo(string globoId)
        {
            if (string.IsNullOrWhiteSpace(globoId))
                throw new ArgumentException("El ID del globo no puede ser vacío.", nameof(globoId));

            string query = "UPDATE Globo SET Activo = 0 WHERE globoId=@id";
            var parametros = new[] { new SqlParameter("@id", globoId) };
            using var conn = DbHelper.ObtenerConexion();
            return DbHelper.ExecuteNonQuery(query, parametros, conn) > 0;
        }

        #endregion

        #region Ajuste Stock con Historial

        public bool AjustarStockConHistorial(string globoId, int nuevaCantidad, int empleadoId, string motivo, SqlConnection conn, SqlTransaction tran)
        {
            var globo = ObtenerGloboPorId(globoId);
            if (globo == null) throw new Exception("Globo no encontrado");

            int stockAnterior = globo.Stock;
            globo.Stock = nuevaCantidad;

            // Usa la sobrecarga con conexión y transacción
            ActualizarGlobo(globo, conn, tran);

            string queryHist = @"
                INSERT INTO HistorialAjusteStockGlobo (GloboId, CantidadAnterior, CantidadNueva, Motivo, EmpleadoId)
                VALUES (@GloboId, @CantidadAnterior, @CantidadNueva, @Motivo, @EmpleadoId)";

            var parametros = new[]
            {
                new SqlParameter("@GloboId", globoId),
                new SqlParameter("@CantidadAnterior", stockAnterior),
                new SqlParameter("@CantidadNueva", nuevaCantidad),
                new SqlParameter("@Motivo", motivo),
                new SqlParameter("@EmpleadoId", empleadoId)
            };

            DbHelper.ExecuteNonQuery(queryHist, parametros, conn, tran);
            return true;
        }

        #endregion

        #region Métodos auxiliares

        private void BorrarCaracteristicas(string globoId, string tabla, SqlConnection conn, SqlTransaction tran)
        {
            string query = $"DELETE FROM {tabla} WHERE globoId=@globoId";
            using var cmd = new SqlCommand(query, conn, tran);
            cmd.Parameters.AddWithValue("@globoId", globoId);
            cmd.ExecuteNonQuery();
        }

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

        private void ReemplazarCaracteristicas(string globoId, string tabla, string columna, IEnumerable<string> valores, SqlConnection conn, SqlTransaction tran, bool usarGuidComoPK = false)
        {
            BorrarCaracteristicas(globoId, tabla, conn, tran);
            InsertarCaracteristicasUnicas(globoId, tabla, columna, valores, conn, tran, usarGuidComoPK);
        }

        #endregion
    }
}