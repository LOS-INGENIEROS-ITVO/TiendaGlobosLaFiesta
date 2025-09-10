using System;
using System.Data;
using System.Data.SqlClient;
using TiendaGlobosLaFiesta.Modelos;

namespace TiendaGlobosLaFiesta.Data
{
    public static class AuthService
    {
        public static bool Login(string username, string password, out string rol, out string mensaje)
        {
            rol = string.Empty;
            mensaje = string.Empty;

            try
            {
                string query = @"
                    SELECT u.usuarioId, u.username, u.empleadoId, u.activo,
                           e.puestoId AS Rol,
                           e.primerNombre + ' ' + ISNULL(e.segundoNombre,'') + ' ' + e.apellidoP + ' ' + e.apellidoM AS NombreCompleto,
                           u.passwordHash
                    FROM Usuarios u
                    INNER JOIN Empleado e ON u.empleadoId = e.empleadoId
                    WHERE u.username = @username AND u.activo = 1";

                var parametros = new[]
                {
                    new SqlParameter("@username", username)
                };

                DataTable dt = DbHelper.ExecuteQuery(query, parametros);

                if (dt.Rows.Count == 0)
                {
                    mensaje = "Usuario no encontrado o inactivo.";
                    return false;
                }

                DataRow row = dt.Rows[0];

                // Validar contraseña (SHA256 hash en tu DB)
                string passwordHash = row["passwordHash"].ToString();
                if (!VerifyPasswordHash(password, passwordHash))
                {
                    mensaje = "Contraseña incorrecta.";
                    return false;
                }

                // Guardar en SesionActual
                SesionActual.UsuarioId = Convert.ToInt32(row["usuarioId"]);
                SesionActual.EmpleadoId = Convert.ToInt32(row["empleadoId"]);
                SesionActual.Username = row["username"].ToString();
                SesionActual.Rol = row["Rol"].ToString();
                SesionActual.NombreEmpleadoCompleto = row["NombreCompleto"].ToString();

                rol = SesionActual.Rol;
                return true;
            }
            catch (Exception ex)
            {
                mensaje = $"Error en login: {ex.Message}";
                return false;
            }
        }

        // Método para comparar la contraseña ingresada con el hash SHA256 almacenado
        private static bool VerifyPasswordHash(string password, string storedHash)
        {
            using var sha = System.Security.Cryptography.SHA256.Create();
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(password);
            byte[] hash = sha.ComputeHash(bytes);
            string hashString = BitConverter.ToString(hash).Replace("-", "").ToLower();
            return hashString == storedHash.ToLower();
        }
    }
}