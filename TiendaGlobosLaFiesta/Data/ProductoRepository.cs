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

        // Obtener todos los productos activos
        public List<Producto> ObtenerProductos()
        {
            var lista = new List<Producto>();
            string query = @"SELECT productoId, nombre, unidad, stock, costo, proveedorId, categoriaId, Activo 
                             FROM Producto
                             WHERE Activo = 1";

            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(new Producto
                        {
                            ProductoId = reader["productoId"].ToString(),
                            Nombre = reader["nombre"].ToString(),
                            Unidad = reader["unidad"].ToString(),
                            Stock = Convert.ToInt32(reader["stock"]),
                            Costo = Convert.ToDecimal(reader["costo"]),
                            ProveedorId = reader["proveedorId"]?.ToString(),
                            CategoriaId = reader["categoriaId"] != DBNull.Value ? Convert.ToInt32(reader["categoriaId"]) : (int?)null,
                            Activo = Convert.ToBoolean(reader["Activo"])
                        });
                    }
                }
            }
            return lista;
        }

        // Obtener un producto por su ID
        public Producto ObtenerProductoPorId(string productoId)
        {
            Producto producto = null;
            string query = @"SELECT productoId, nombre, unidad, stock, costo, proveedorId, categoriaId, Activo
                             FROM Producto
                             WHERE productoId = @productoId";

            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@productoId", productoId);
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        producto = new Producto
                        {
                            ProductoId = reader["productoId"].ToString(),
                            Nombre = reader["nombre"].ToString(),
                            Unidad = reader["unidad"].ToString(),
                            Stock = Convert.ToInt32(reader["stock"]),
                            Costo = Convert.ToDecimal(reader["costo"]),
                            ProveedorId = reader["proveedorId"]?.ToString(),
                            CategoriaId = reader["categoriaId"] != DBNull.Value ? Convert.ToInt32(reader["categoriaId"]) : (int?)null,
                            Activo = Convert.ToBoolean(reader["Activo"])
                        };
                    }
                }
            }
            return producto;
        }

        // Insertar un nuevo producto
        public bool InsertarProducto(Producto producto)
        {
            string query = @"INSERT INTO Producto (productoId, nombre, unidad, stock, costo, proveedorId, categoriaId, Activo)
                             VALUES (@productoId, @nombre, @unidad, @stock, @costo, @proveedorId, @categoriaId, @Activo)";
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@productoId", producto.ProductoId);
                cmd.Parameters.AddWithValue("@nombre", producto.Nombre);
                cmd.Parameters.AddWithValue("@unidad", producto.Unidad);
                cmd.Parameters.AddWithValue("@stock", producto.Stock);
                cmd.Parameters.AddWithValue("@costo", producto.Costo);
                cmd.Parameters.AddWithValue("@proveedorId", (object)producto.ProveedorId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@categoriaId", (object)producto.CategoriaId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Activo", producto.Activo);

                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // Actualizar un producto existente
        public bool ActualizarProducto(Producto producto)
        {
            string query = @"UPDATE Producto
                             SET nombre=@nombre, unidad=@unidad, stock=@stock, costo=@costo, 
                                 proveedorId=@proveedorId, categoriaId=@categoriaId, Activo=@Activo
                             WHERE productoId=@productoId";
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@productoId", producto.ProductoId);
                cmd.Parameters.AddWithValue("@nombre", producto.Nombre);
                cmd.Parameters.AddWithValue("@unidad", producto.Unidad);
                cmd.Parameters.AddWithValue("@stock", producto.Stock);
                cmd.Parameters.AddWithValue("@costo", producto.Costo);
                cmd.Parameters.AddWithValue("@proveedorId", (object)producto.ProveedorId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@categoriaId", (object)producto.CategoriaId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Activo", producto.Activo);

                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // Eliminar (desactivar) un producto
        public bool EliminarProducto(string productoId)
        {
            string query = @"UPDATE Producto SET Activo = 0 WHERE productoId = @productoId";
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@productoId", productoId);
                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }
    }
}