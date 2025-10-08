using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using TiendaGlobosLaFiesta.Models;
using TiendaGlobosLaFiesta.Models.Inventario;

namespace TiendaGlobosLaFiesta.Data
{
    public class ProductoRepository
    {
        private readonly string _connectionString;

        public ProductoRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        // ===========================
        // Crear
        // ===========================
        public bool AgregarProducto(Producto producto)
        {
            using var conn = new SqlConnection(_connectionString);
            var query = @"INSERT INTO Producto (productoId, nombre, unidad, stock, costo, proveedorId, categoriaId, Activo)
                          VALUES (@Id, @Nombre, @Unidad, @Stock, @Costo, @ProveedorId, @CategoriaId, @Activo)";
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Id", producto.ProductoId);
            cmd.Parameters.AddWithValue("@Nombre", producto.Nombre);
            cmd.Parameters.AddWithValue("@Unidad", producto.Unidad);
            cmd.Parameters.AddWithValue("@Stock", producto.Stock);
            cmd.Parameters.AddWithValue("@Costo", producto.Costo);
            cmd.Parameters.AddWithValue("@ProveedorId", (object)producto.ProveedorId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CategoriaId", (object)producto.CategoriaId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Activo", producto.Activo);

            conn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }

        // ===========================
        // Leer todos
        // ===========================
        public List<Producto> ObtenerProductos(bool soloActivos = true)
        {
            var lista = new List<Producto>();
            using var conn = new SqlConnection(_connectionString);
            var query = "SELECT productoId, nombre, unidad, stock, costo, proveedorId, categoriaId, Activo FROM Producto";
            if (soloActivos) query += " WHERE Activo = 1";

            using var cmd = new SqlCommand(query, conn);
            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(new Producto
                {
                    ProductoId = reader.GetString(0),
                    Nombre = reader.GetString(1),
                    Unidad = reader.GetString(2),
                    Stock = reader.GetInt32(3),
                    Costo = reader.GetDecimal(4),
                    ProveedorId = reader.IsDBNull(5) ? null : reader.GetString(5),
                    CategoriaId = reader.IsDBNull(6) ? null : reader.GetInt32(6),
                    Activo = reader.GetBoolean(7)
                });
            }
            return lista;
        }

        // ===========================
        // Leer por ID
        // ===========================
        public Producto ObtenerProductoPorId(string productoId)
        {
            using var conn = new SqlConnection(_connectionString);
            var query = @"SELECT productoId, nombre, unidad, stock, costo, proveedorId, categoriaId, Activo
                          FROM Producto
                          WHERE productoId = @Id";
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Id", productoId);
            conn.Open();
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new Producto
                {
                    ProductoId = reader.GetString(0),
                    Nombre = reader.GetString(1),
                    Unidad = reader.GetString(2),
                    Stock = reader.GetInt32(3),
                    Costo = reader.GetDecimal(4),
                    ProveedorId = reader.IsDBNull(5) ? null : reader.GetString(5),
                    CategoriaId = reader.IsDBNull(6) ? null : reader.GetInt32(6),
                    Activo = reader.GetBoolean(7)
                };
            }
            return null;
        }

        // ===========================
        // Actualizar
        // ===========================
        public bool ActualizarProducto(Producto producto)
        {
            using var conn = new SqlConnection(_connectionString);
            var query = @"UPDATE Producto
                          SET nombre = @Nombre,
                              unidad = @Unidad,
                              stock = @Stock,
                              costo = @Costo,
                              proveedorId = @ProveedorId,
                              categoriaId = @CategoriaId,
                              Activo = @Activo
                          WHERE productoId = @Id";
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Id", producto.ProductoId);
            cmd.Parameters.AddWithValue("@Nombre", producto.Nombre);
            cmd.Parameters.AddWithValue("@Unidad", producto.Unidad);
            cmd.Parameters.AddWithValue("@Stock", producto.Stock);
            cmd.Parameters.AddWithValue("@Costo", producto.Costo);
            cmd.Parameters.AddWithValue("@ProveedorId", (object)producto.ProveedorId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CategoriaId", (object)producto.CategoriaId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Activo", producto.Activo);

            conn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }

        // ===========================
        // Eliminar
        // ===========================
        public bool EliminarProducto(string productoId)
        {
            using var conn = new SqlConnection(_connectionString);
            var query = "DELETE FROM Producto WHERE productoId = @Id";
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Id", productoId);
            conn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }
    }
}