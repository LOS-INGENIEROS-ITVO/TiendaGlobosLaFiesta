using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using TiendaGlobosLaFiesta.Models;

namespace TiendaGlobosLaFiesta.Data
{
    public static class ConexionBD
    {
        private const string Servidor = @"LALOVG25\SQLEXPRESS";
        private const string BaseDatos = "Globeriadb";
        private const bool UsarWindowsAuth = true;

        private static readonly string ConnectionString = UsarWindowsAuth
            ? $"Server={Servidor};Database={BaseDatos};Trusted_Connection=True;"
            : throw new NotSupportedException("Solo está configurado Windows Authentication.");

        // ===========================
        // CONEXIÓN
        // ===========================
        public static SqlConnection ObtenerConexion()
        {
            try
            {
                var conn = new SqlConnection(ConnectionString);
                conn.Open();
                return conn;
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo conectar a la base de datos: " + ex.Message,
                                "Error de conexión", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

        // ===========================
        // MÉTODOS AUXILIARES
        // ===========================
        public static DataTable EjecutarConsulta(string sql, SqlParameter[] parametros = null)
        {
            using SqlConnection conn = ObtenerConexion();
            using SqlCommand cmd = new SqlCommand(sql, conn);
            if (parametros != null) cmd.Parameters.AddRange(parametros);
            using SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        public static int EjecutarNonQuery(string sql, SqlParameter[] parametros = null)
        {
            using SqlConnection conn = ObtenerConexion();
            using SqlCommand cmd = new SqlCommand(sql, conn);
            if (parametros != null) cmd.Parameters.AddRange(parametros);
            return cmd.ExecuteNonQuery();
        }

        public static SqlParameter Param(string nombre, object valor)
        {
            return new SqlParameter(nombre, valor ?? DBNull.Value);
        }

        // ===========================
        // CLIENTES
        // ===========================
        public static List<Cliente> ObtenerClientes(string filtro = "")
        {
            List<Cliente> lista = new List<Cliente>();
            try
            {
                string sql = "SELECT clienteId, primerNombre, segundoNombre, apellidoP, apellidoM, telefono FROM Cliente";
                SqlParameter[] parametros = null;

                if (!string.IsNullOrEmpty(filtro))
                {
                    sql += " WHERE primerNombre LIKE @filtro OR segundoNombre LIKE @filtro OR apellidoP LIKE @filtro OR apellidoM LIKE @filtro";
                    parametros = new SqlParameter[] { Param("@filtro", "%" + filtro + "%") };
                }

                DataTable dt = EjecutarConsulta(sql, parametros);

                foreach (DataRow row in dt.Rows)
                {
                    lista.Add(new Cliente
                    {
                        ClienteId = row["clienteId"].ToString(),
                        PrimerNombre = row["primerNombre"].ToString(),
                        SegundoNombre = row["segundoNombre"].ToString(),
                        ApellidoP = row["apellidoP"].ToString(),
                        ApellidoM = row["apellidoM"].ToString(),
                        Telefono = row["telefono"] != DBNull.Value ? Convert.ToInt64(row["telefono"]) : (long?)null
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al obtener clientes: " + ex.Message);
            }

            return lista;
        }

        public static bool AgregarCliente(Cliente cliente)
        {
            try
            {
                string sql = @"INSERT INTO Cliente (clienteId, primerNombre, segundoNombre, apellidoP, apellidoM, telefono)
                               VALUES (@ClienteId, @PrimerNombre, @SegundoNombre, @ApellidoP, @ApellidoM, @Telefono)";
                SqlParameter[] parametros = {
                    Param("@ClienteId", cliente.ClienteId),
                    Param("@PrimerNombre", cliente.PrimerNombre),
                    Param("@SegundoNombre", cliente.SegundoNombre),
                    Param("@ApellidoP", cliente.ApellidoP),
                    Param("@ApellidoM", cliente.ApellidoM),
                    Param("@Telefono", cliente.Telefono.HasValue ? (object)cliente.Telefono.Value : DBNull.Value)
                };
                return EjecutarNonQuery(sql, parametros) > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al agregar cliente: " + ex.Message);
                return false;
            }
        }

        public static bool ActualizarCliente(Cliente cliente)
        {
            try
            {
                string sql = @"UPDATE Cliente SET primerNombre=@PrimerNombre, segundoNombre=@SegundoNombre, apellidoP=@ApellidoP, apellidoM=@ApellidoM, telefono=@Telefono
                               WHERE clienteId=@ClienteId";
                SqlParameter[] parametros = {
                    Param("@ClienteId", cliente.ClienteId),
                    Param("@PrimerNombre", cliente.PrimerNombre),
                    Param("@SegundoNombre", cliente.SegundoNombre),
                    Param("@ApellidoP", cliente.ApellidoP),
                    Param("@ApellidoM", cliente.ApellidoM),
                    Param("@Telefono", cliente.Telefono.HasValue ? (object)cliente.Telefono.Value : DBNull.Value)
                };
                return EjecutarNonQuery(sql, parametros) > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al actualizar cliente: " + ex.Message);
                return false;
            }
        }

        public static bool EliminarCliente(string clienteId)
        {
            try
            {
                string sql = "DELETE FROM Cliente WHERE clienteId=@ClienteId";
                SqlParameter[] parametros = { Param("@ClienteId", clienteId) };
                return EjecutarNonQuery(sql, parametros) > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al eliminar cliente: " + ex.Message);
                return false;
            }
        }

        // ===========================
        // PRODUCTOS
        // ===========================
        public static DataTable ObtenerProductos()
        {
            string query = "SELECT productoId, nombre, unidad, stock, costo FROM Producto";
            return EjecutarConsulta(query);
        }

        public static bool ActualizarStockProducto(string productoId, int nuevoStock)
        {
            string query = "UPDATE Producto SET Stock = @Stock WHERE productoId = @Id";
            SqlParameter[] parametros = {
                Param("@Stock", nuevoStock),
                Param("@Id", productoId)
            };
            return EjecutarNonQuery(query, parametros) > 0;
        }

        // ===========================
        // GLOBOS
        // ===========================
        public static DataTable ObtenerGlobos()
        {
            string query = @"
        SELECT g.globoId, g.material, g.unidad, g.color, g.stock, g.costo,
               ISNULL(TamaniosList.Tamanios, '') AS Tamanios,
               ISNULL(FormasList.Formas, '') AS Formas,
               ISNULL(TematicasList.Tematicas, '') AS Tematicas
        FROM Globo g
        LEFT JOIN (
            SELECT globoId, STRING_AGG(tamanio, ', ') AS Tamanios
            FROM Globo_Tamanio
            GROUP BY globoId
        ) AS TamaniosList ON g.globoId = TamaniosList.globoId
        LEFT JOIN (
            SELECT globoId, STRING_AGG(forma, ', ') AS Formas
            FROM Globo_Forma
            GROUP BY globoId
        ) AS FormasList ON g.globoId = FormasList.globoId
        LEFT JOIN (
            SELECT globoId, STRING_AGG(nombre, ', ') AS Tematicas
            FROM Tematica
            GROUP BY globoId
        ) AS TematicasList ON g.globoId = TematicasList.globoId
        ORDER BY g.globoId;
    ";
            return EjecutarConsulta(query);
        }

        public static bool ActualizarStockGlobo(string globoId, int nuevoStock)
        {
            string query = "UPDATE Globo SET Stock = @Stock WHERE globoId = @Id";
            SqlParameter[] parametros = {
                Param("@Stock", nuevoStock),
                Param("@Id", globoId)
            };
            return EjecutarNonQuery(query, parametros) > 0;
        }
    }
}