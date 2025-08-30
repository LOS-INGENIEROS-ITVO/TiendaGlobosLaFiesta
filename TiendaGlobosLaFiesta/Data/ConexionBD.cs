using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;

namespace TiendaGlobosLaFiesta.Data
{
    public static class ConexionBD
    {
        // ========================
        // CONFIGURACIÓN DEL SERVIDOR
        // ========================
        private const string Servidor = @"LALOVG25\SQLEXPRESS"; // tu instancia
        private const string BaseDatos = "Globeriadb";
        private const bool UsarWindowsAuth = true; // Windows Authentication

        // ========================
        // CADENA DE CONEXIÓN
        // ========================
        private static readonly string ConnectionString = UsarWindowsAuth
            ? $"Server={Servidor};Database={BaseDatos};Trusted_Connection=True;"
            : throw new NotSupportedException("Solo está configurado Windows Authentication.");

        // ========================
        // OBTENER CONEXIÓN
        // ========================
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

        // ========================
        // EJECUTAR CONSULTA SIN PARÁMETROS
        // ========================
        public static DataTable EjecutarConsulta(string sql)
        {
            using SqlConnection conn = ObtenerConexion();
            using SqlCommand cmd = new SqlCommand(sql, conn);
            using SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        // ========================
        // EJECUTAR CONSULTA CON PARÁMETROS
        // ========================
        public static DataTable EjecutarConsulta(string sql, SqlParameter[] parametros)
        {
            using SqlConnection conn = ObtenerConexion();
            using SqlCommand cmd = new SqlCommand(sql, conn);
            if (parametros != null)
                cmd.Parameters.AddRange(parametros);
            using SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        // ========================
        // EJECUTAR INSERT/UPDATE/DELETE
        // ========================
        public static int EjecutarNonQuery(string sql, SqlParameter[] parametros = null)
        {
            using SqlConnection conn = ObtenerConexion();
            using SqlCommand cmd = new SqlCommand(sql, conn);
            if (parametros != null)
                cmd.Parameters.AddRange(parametros);
            return cmd.ExecuteNonQuery();
        }

        public static int EjecutarNonQuery(string sql, SqlParameter[] parametros, SqlConnection conn, SqlTransaction tran)
        {
            using SqlCommand cmd = new SqlCommand(sql, conn, tran);
            if (parametros != null)
                cmd.Parameters.AddRange(parametros);
            return cmd.ExecuteNonQuery();
        }

        // ========================
        // CREAR PARÁMETRO RÁPIDO
        // ========================
        public static SqlParameter Param(string nombre, object valor)
        {
            return new SqlParameter(nombre, valor ?? DBNull.Value);
        }
    }
}
