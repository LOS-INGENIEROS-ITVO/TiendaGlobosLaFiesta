using System;
using System.Data;
using System.Data.SqlClient;

namespace TiendaGlobosLaFiesta.Data
{
    public static class DbHelper
    {
        private const string Servidor = @"LALOVG25\SQLEXPRESS";
        private const string BaseDatos = "Globeriadb";
        private const bool UsarWindowsAuth = true;

        private static string ObtenerCadenaConexion()
        {
            return UsarWindowsAuth
                ? $"Server={Servidor};Database={BaseDatos};Integrated Security=True;"
                : $"Server={Servidor};Database={BaseDatos};User Id=tuUsuario;Password=tuPassword;";
        }

        internal static SqlConnection ObtenerConexion()
        {
            var conn = new SqlConnection(ObtenerCadenaConexion());
            conn.Open();
            return conn;
        }

        public static DataTable ExecuteQuery(string query, params SqlParameter[] parametros)
        {
            using var conn = ObtenerConexion();
            using var cmd = new SqlCommand(query, conn);

            if (parametros != null)
                cmd.Parameters.AddRange(parametros);

            var dt = new DataTable();
            using var da = new SqlDataAdapter(cmd);
            da.Fill(dt);
            return dt;
        }

        public static int ExecuteNonQuery(string query, params SqlParameter[] parametros)
        {
            using var conn = ObtenerConexion();
            using var cmd = new SqlCommand(query, conn);

            if (parametros != null)
                cmd.Parameters.AddRange(parametros);

            return cmd.ExecuteNonQuery();
        }

        public static object ExecuteScalar(string query, params SqlParameter[] parametros)
        {
            using var conn = ObtenerConexion();
            using var cmd = new SqlCommand(query, conn);

            if (parametros != null)
                cmd.Parameters.AddRange(parametros);

            return cmd.ExecuteScalar();
        }
    }
}