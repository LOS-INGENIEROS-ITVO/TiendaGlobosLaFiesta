using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using TiendaGlobosLaFiesta.Models.Inventario;
using TiendaGlobosLaFiesta.Models.Ventas;

namespace TiendaGlobosLaFiesta.Data
{
    public class GloboRepository
    {
        #region Obtener Globos

        public List<GloboVenta> ObtenerGlobosParaVenta()
        {
            string query = @"
                SELECT g.globoId, g.material, g.unidad, g.color, g.costo, g.stock,
                       ISNULL(Tam.Tamanos, '') AS Tamano,
                       ISNULL(Form.Formas, '') AS Forma,
                       ISNULL(Temp.Tematicas, '') AS Tematica
                FROM Globo g
                LEFT JOIN (SELECT globoId, STRING_AGG(tamanio, ', ') AS Tamanos FROM Globo_Tamanio GROUP BY globoId) Tam ON g.globoId = Tam.globoId
                LEFT JOIN (SELECT globoId, STRING_AGG(forma, ', ') AS Formas FROM Globo_Forma GROUP BY globoId) Form ON g.globoId = Form.globoId
                LEFT JOIN (SELECT globoId, STRING_AGG(nombre, ', ') AS Tematicas FROM Tematica GROUP BY globoId) Temp ON g.globoId = Temp.globoId
                WHERE g.Activo = 1 AND g.stock > 0
                ORDER BY g.globoId";

            DataTable dt = DbHelper.ExecuteQuery(query);
            return dt.AsEnumerable().Select(MapearGloboVenta).ToList();
        }

        public GloboVenta? ObtenerGloboVentaPorId(string globoId)
        {
            if (string.IsNullOrWhiteSpace(globoId))
                throw new ArgumentException("El ID del globo no puede ser vacío.", nameof(globoId));

            string query = @"
                SELECT g.globoId, g.material, g.unidad, g.color, g.costo, g.stock,
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

            return dt.Rows.Count == 0 ? null : MapearGloboVenta(dt.Rows[0]);
        }

        private GloboVenta MapearGloboVenta(DataRow row)
        {
            return new GloboVenta
            {
                GloboId = row["globoId"]?.ToString() ?? string.Empty,
                Material = row["material"]?.ToString() ?? string.Empty,
                Color = row["color"]?.ToString() ?? string.Empty,
                Tamano = row["Tamano"]?.ToString() ?? string.Empty,
                Forma = row["Forma"]?.ToString() ?? string.Empty,
                Tematica = row["Tematica"]?.ToString() ?? string.Empty,
                Unidad = row["unidad"]?.ToString() ?? "pieza",
                Stock = row["stock"] != DBNull.Value ? Convert.ToInt32(row["stock"]) : 0,
                Costo = row["costo"] != DBNull.Value ? Convert.ToDecimal(row["costo"]) : 0,
                Cantidad = 0
            };
        }




        public GloboVenta MapearParaVenta(Globo globo, int cantidad = 1)
        {
            return new GloboVenta
            {
                GloboId = globo.GloboId,
                Material = globo.Material,
                Color = globo.Color,
                Tamano = "",    // si no aplica, o mapear según tu lógica
                Forma = "",
                Tematica = "",
                Unidad = globo.Unidad,
                Stock = globo.Stock,
                Costo = globo.Costo,
                Cantidad = cantidad
            };
        }


        public List<Globo> ObtenerGlobos(bool soloActivos = true)
        {
            var lista = new List<Globo>();
            string query = @"
        SELECT g.globoId, g.material, g.unidad, g.color, g.costo, g.stock
        FROM Globo g
        WHERE (@soloActivos = 0 OR g.Activo = 1)";

            var parametros = new[] { new SqlParameter("@soloActivos", soloActivos ? 1 : 0) };
            DataTable dt = DbHelper.ExecuteQuery(query, parametros);

            foreach (DataRow row in dt.Rows)
                lista.Add(new Globo
                {
                    GloboId = row["globoId"].ToString(),
                    Material = row["material"].ToString(),
                    Color = row["color"].ToString(),
                    Unidad = row["unidad"].ToString(),
                    Stock = Convert.ToInt32(row["stock"]),
                    Costo = Convert.ToDecimal(row["costo"])
                });

            return lista;
        }

        #endregion

        #region CRUD Globos

        public bool AgregarGlobo(GloboVenta globo)
        {
            if (globo == null) throw new ArgumentNullException(nameof(globo));

            using var conn = DbHelper.ObtenerConexion();
            using var tran = conn.BeginTransaction();
            try
            {
                string queryGlobo = @"
                    INSERT INTO Globo (globoId, material, unidad, color, costo, stock, proveedorId, Activo)
                    VALUES (@id, @material, @unidad, @color, @costo, @stock, NULL, 1)";

                var parametros = new[]
                {
                    new SqlParameter("@id", globo.GloboId),
                    new SqlParameter("@material", globo.Material),
                    new SqlParameter("@unidad", globo.Unidad),
                    new SqlParameter("@color", globo.Color),
                    new SqlParameter("@costo", globo.Costo),
                    new SqlParameter("@stock", globo.Stock)
                };

                DbHelper.ExecuteNonQuery(queryGlobo, parametros, conn, tran);

                InsertarCaracteristicas(globo.GloboId, "Globo_Tamanio", "tamanio", globo.Tamano.Split(',').Select(x => x.Trim()).ToList(), conn, tran);
                InsertarCaracteristicas(globo.GloboId, "Globo_Forma", "forma", globo.Forma.Split(',').Select(x => x.Trim()).ToList(), conn, tran);
                InsertarCaracteristicas(globo.GloboId, "Tematica", "nombre", globo.Tematica.Split(',').Select(x => x.Trim()).ToList(), conn, tran, true);

                tran.Commit();
                return true;
            }
            catch
            {
                tran.Rollback();
                return false;
            }
        }

        public bool ActualizarGlobo(GloboVenta globo)
        {
            using var conn = DbHelper.ObtenerConexion();
            string query = @"
                UPDATE Globo
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

            return DbHelper.ExecuteNonQuery(query, parametros, conn) > 0;
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

        public bool AjustarStockConHistorial(string globoId, int nuevaCantidad, int empleadoId, string motivo)
        {
            using var conn = DbHelper.ObtenerConexion();
            using var tran = conn.BeginTransaction();
            try
            {
                var globo = ObtenerGloboVentaPorId(globoId);
                if (globo == null) throw new Exception("Globo no encontrado");

                int stockAnterior = globo.Stock;
                globo.Stock = nuevaCantidad;

                ActualizarGlobo(globo);

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
                tran.Commit();
                return true;
            }
            catch
            {
                tran.Rollback();
                return false;
            }
        }

        #endregion

        #region Métodos auxiliares

        private void InsertarCaracteristicas(string globoId, string tabla, string columna, List<string> items, SqlConnection conn, SqlTransaction tran, bool esTematica = false)
        {
            if (items == null || items.Count == 0) return;

            string deleteQuery = $"DELETE FROM {tabla} WHERE globoId=@globoId";
            using (var cmd = new SqlCommand(deleteQuery, conn, tran))
            {
                cmd.Parameters.AddWithValue("@globoId", globoId);
                cmd.ExecuteNonQuery();
            }

            foreach (var item in items.Distinct())
            {
                string insertQuery = esTematica
                    ? $"INSERT INTO Tematica (globoId, nombre) VALUES (@globoId, @item)"
                    : $"INSERT INTO {tabla} (globoId, {columna}) VALUES (@globoId, @item)";

                using var cmd = new SqlCommand(insertQuery, conn, tran);
                cmd.Parameters.AddWithValue("@globoId", globoId);
                cmd.Parameters.AddWithValue("@item", item);
                cmd.ExecuteNonQuery();
            }
        }

        #endregion
    }
}