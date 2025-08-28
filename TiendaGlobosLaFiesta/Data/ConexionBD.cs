using System;
using System.Data;
using System.Data.SqlClient;

namespace TiendaGlobosLaFiesta.Data
{
    public static partial class ConexionBD
    {
        private static readonly string connectionString =
            @"Data Source=LALOVG25\SQLEXPRESS;Initial Catalog=Globeriadb;Integrated Security=True;";

        // Obtener conexión abierta
        public static SqlConnection ObtenerConexion()
        {
            var conexion = new SqlConnection(connectionString);
            conexion.Open();
            return conexion;
        }

        // Crear parámetro
        public static SqlParameter Param(string nombre, object valor)
        {
            return new SqlParameter(nombre, valor ?? DBNull.Value);
        }

        // Ejecutar non query normal
        public static int EjecutarNonQuery(string query, SqlParameter[] parameters = null)
        {
            using (var conexion = new SqlConnection(connectionString))
            using (var comando = new SqlCommand(query, conexion))
            {
                if (parameters != null)
                    comando.Parameters.AddRange(parameters);

                conexion.Open();
                return comando.ExecuteNonQuery();
            }
        }

        // Sobrecarga para transacciones
        public static int EjecutarNonQuery(string query, SqlParameter[] parameters, SqlConnection conn, SqlTransaction tran)
        {
            using (var comando = new SqlCommand(query, conn, tran))
            {
                if (parameters != null)
                    comando.Parameters.AddRange(parameters);

                return comando.ExecuteNonQuery();
            }
        }

        // Ejecutar consulta
        public static DataTable EjecutarConsulta(string query, SqlParameter[] parameters = null)
        {
            using (var conexion = new SqlConnection(connectionString))
            using (var comando = new SqlCommand(query, conexion))
            {
                if (parameters != null)
                    comando.Parameters.AddRange(parameters);

                using (var adaptador = new SqlDataAdapter(comando))
                {
                    var tabla = new DataTable();
                    adaptador.Fill(tabla);
                    return tabla;
                }
            }
        }
    }
}