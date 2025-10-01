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

        public static SqlConnection ObtenerConexion()
        {
            var conn = new SqlConnection(GetConnectionString());
            conn.Open();
            return conn;
        }

        public static DataTable ExecuteQuery(string query, SqlParameter[] parameters = null)
        {
            using var conn = ObtenerConexion();
            using var cmd = new SqlCommand(query, conn);
            if (parameters != null)
            {
                cmd.Parameters.AddRange(parameters);
            }
            using var adapter = new SqlDataAdapter(cmd);
            var dt = new DataTable();
            adapter.Fill(dt);
            return dt;
        }

        public static int ExecuteNonQuery(string query, SqlParameter[] parameters = null)
        {
            using var conn = ObtenerConexion();
            using var cmd = new SqlCommand(query, conn);
            if (parameters != null)
            {
                cmd.Parameters.AddRange(parameters);
            }
            return cmd.ExecuteNonQuery();
        }

        public static object ExecuteScalar(string query, SqlParameter[] parameters = null)
        {
            using var conn = ObtenerConexion();
            using var cmd = new SqlCommand(query, conn);
            if (parameters != null)
            {
                cmd.Parameters.AddRange(parameters);
            }
            return cmd.ExecuteScalar();
        }
    }
}