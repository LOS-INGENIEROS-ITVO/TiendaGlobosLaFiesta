using System;
using System.Data;
using System.Data.SqlClient;
using TiendaGlobosLaFiesta.Models.Usuarios;

namespace TiendaGlobosLaFiesta.Data
{
    public class UsuarioRepository
    {
        public Usuario? ObtenerUsuarioPorNombre(string username)
        {
            string query = @"
                SELECT usuarioId, empleadoId, username, passwordHash, activo, fechaCreacion
                FROM Usuarios
                WHERE username=@username";

            var parametros = new[] { new SqlParameter("@username", username) };
            DataTable dt = DbHelper.ExecuteQuery(query, parametros);

            if (dt.Rows.Count == 0) return null;

            var row = dt.Rows[0];
            return new Usuario
            {
                UsuarioId = Convert.ToInt32(row["usuarioId"]),
                EmpleadoId = Convert.ToInt32(row["empleadoId"]),
                NombreUsuario = row["username"].ToString(),
                ContrasenaHash = row["passwordHash"].ToString(),
                Activo = Convert.ToBoolean(row["activo"]),
                FechaCreacion = Convert.ToDateTime(row["fechaCreacion"])
            };
        }

        public List<Usuario> ObtenerTodosUsuarios()
        {
            string query = @"
                SELECT usuarioId, empleadoId, username, passwordHash, activo, fechaCreacion
                FROM Usuarios";

            DataTable dt = DbHelper.ExecuteQuery(query, null);
            var lista = new List<Usuario>();

            foreach (DataRow row in dt.Rows)
            {
                lista.Add(new Usuario
                {
                    UsuarioId = Convert.ToInt32(row["usuarioId"]),
                    EmpleadoId = Convert.ToInt32(row["empleadoId"]),
                    NombreUsuario = row["username"].ToString(),
                    ContrasenaHash = row["passwordHash"].ToString(),
                    Activo = Convert.ToBoolean(row["activo"]),
                    FechaCreacion = Convert.ToDateTime(row["fechaCreacion"])
                });
            }

            return lista;
        }
    }
}