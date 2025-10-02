using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace TiendaGlobosLaFiesta.Data
{
    public static class DbHelper
    {
        private static string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        }

        // Obtener conexión abierta
        public static SqlConnection ObtenerConexion()
        {
            var conn = new SqlConnection(GetConnectionString());
            conn.Open();
            return conn;
        }

        // Ejecutar consulta que devuelve DataTable
        public static DataTable ExecuteQuery(string query, SqlParameter[] parameters = null, SqlConnection conn = null, SqlTransaction tran = null)
        {
            bool cerrarConexion = false;
            if (conn == null)
            {
                conn = ObtenerConexion();
                cerrarConexion = true;
            }

            using var cmd = new SqlCommand(query, conn);
            if (tran != null) cmd.Transaction = tran;
            if (parameters != null) cmd.Parameters.AddRange(parameters);

            using var adapter = new SqlDataAdapter(cmd);
            var dt = new DataTable();
            adapter.Fill(dt);

            if (cerrarConexion) conn.Close();
            return dt;
        }

        // Ejecutar comandos que no devuelven resultados (INSERT, UPDATE, DELETE)
        public static int ExecuteNonQuery(string query, SqlParameter[] parameters = null, SqlConnection conn = null, SqlTransaction tran = null)
        {
            bool cerrarConexion = false;
            if (conn == null)
            {
                conn = ObtenerConexion();
                cerrarConexion = true;
            }

            using var cmd = new SqlCommand(query, conn);
            if (tran != null) cmd.Transaction = tran;
            if (parameters != null) cmd.Parameters.AddRange(parameters);

            int filas = cmd.ExecuteNonQuery();
            if (cerrarConexion) conn.Close();
            return filas;
        }

        // Ejecutar consultas que devuelven un valor escalar
        public static object ExecuteScalar(string query, SqlParameter[] parameters = null, SqlConnection conn = null, SqlTransaction tran = null)
        {
            bool cerrarConexion = false;
            if (conn == null)
            {
                conn = ObtenerConexion();
                cerrarConexion = true;
            }

            using var cmd = new SqlCommand(query, conn);
            if (tran != null) cmd.Transaction = tran;
            if (parameters != null) cmd.Parameters.AddRange(parameters);

            object result = cmd.ExecuteScalar();
            if (cerrarConexion) conn.Close();
            return result;
        }
    }
}