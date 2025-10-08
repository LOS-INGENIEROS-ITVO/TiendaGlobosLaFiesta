using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using TiendaGlobosLaFiesta.Models;
using TiendaGlobosLaFiesta.Models.Inventario;

namespace TiendaGlobosLaFiesta.Data
{
    public class GloboRepository
    {
        private readonly string _connectionString;

        public GloboRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        // ===========================
        // Crear
        // ===========================
        public bool AgregarGlobo(Globo globo)
        {
            using var conn = new SqlConnection(_connectionString);
            var query = @"INSERT INTO Globo (globoId, material, unidad, color, stock, costo, proveedorId, Activo)
                          VALUES (@Id, @Material, @Unidad, @Color, @Stock, @Costo, @ProveedorId, @Activo)";
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Id", globo.GloboId);
            cmd.Parameters.AddWithValue("@Material", globo.Material);
            cmd.Parameters.AddWithValue("@Unidad", globo.Unidad);
            cmd.Parameters.AddWithValue("@Color", globo.Color);
            cmd.Parameters.AddWithValue("@Stock", globo.Stock);
            cmd.Parameters.AddWithValue("@Costo", globo.Costo);
            cmd.Parameters.AddWithValue("@ProveedorId", (object)globo.ProveedorId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Activo", globo.Activo);

            conn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }

        // ===========================
        // Leer todos
        // ===========================
        public List<Globo> ObtenerGlobos(bool soloActivos = true)
        {
            var lista = new List<Globo>();
            using var conn = new SqlConnection(_connectionString);
            var query = "SELECT globoId, material, unidad, color, stock, costo, proveedorId, Activo FROM Globo";
            if (soloActivos) query += " WHERE Activo = 1";

            using var cmd = new SqlCommand(query, conn);
            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(new Globo
                {
                    GloboId = reader.GetString(0),
                    Material = reader.GetString(1),
                    Unidad = reader.GetString(2),
                    Color = reader.GetString(3),
                    Stock = reader.GetInt32(4),
                    Costo = reader.GetDecimal(5),
                    ProveedorId = reader.IsDBNull(6) ? null : reader.GetString(6),
                    Activo = reader.GetBoolean(7)
                });
            }
            return lista;
        }

        // ===========================
        // Leer por ID
        // ===========================
        public Globo ObtenerGloboPorId(string globoId)
        {
            using var conn = new SqlConnection(_connectionString);
            var query = @"SELECT globoId, material, unidad, color, stock, costo, proveedorId, Activo
                          FROM Globo
                          WHERE globoId = @Id";
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Id", globoId);
            conn.Open();
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new Globo
                {
                    GloboId = reader.GetString(0),
                    Material = reader.GetString(1),
                    Unidad = reader.GetString(2),
                    Color = reader.GetString(3),
                    Stock = reader.GetInt32(4),
                    Costo = reader.GetDecimal(5),
                    ProveedorId = reader.IsDBNull(6) ? null : reader.GetString(6),
                    Activo = reader.GetBoolean(7)
                };
            }
            return null;
        }

        // ===========================
        // Actualizar
        // ===========================
        public bool ActualizarGlobo(Globo globo)
        {
            using var conn = new SqlConnection(_connectionString);
            var query = @"UPDATE Globo
                          SET material = @Material,
                              unidad = @Unidad,
                              color = @Color,
                              stock = @Stock,
                              costo = @Costo,
                              proveedorId = @ProveedorId,
                              Activo = @Activo
                          WHERE globoId = @Id";
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Id", globo.GloboId);
            cmd.Parameters.AddWithValue("@Material", globo.Material);
            cmd.Parameters.AddWithValue("@Unidad", globo.Unidad);
            cmd.Parameters.AddWithValue("@Color", globo.Color);
            cmd.Parameters.AddWithValue("@Stock", globo.Stock);
            cmd.Parameters.AddWithValue("@Costo", globo.Costo);
            cmd.Parameters.AddWithValue("@ProveedorId", (object)globo.ProveedorId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Activo", globo.Activo);

            conn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }

        // ===========================
        // Eliminar
        // ===========================
        public bool EliminarGlobo(string globoId)
        {
            using var conn = new SqlConnection(_connectionString);
            var query = "DELETE FROM Globo WHERE globoId = @Id";
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Id", globoId);
            conn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }
    }
}