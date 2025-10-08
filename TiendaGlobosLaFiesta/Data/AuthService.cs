using System;
using System.Data;
using System.Data.SqlClient;
using TiendaGlobosLaFiesta.Models.Utilities;

namespace TiendaGlobosLaFiesta.Data
{
    public static class AuthService
    {
        // ============================
        // MÉTODO DE LOGIN
        // ============================
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

                var parametros = new[] { new SqlParameter("@username", username) };
                DataTable dt = DbHelper.ExecuteQuery(query, parametros);

                if (dt.Rows.Count == 0)
                {
                    mensaje = "Usuario no encontrado o inactivo.";
                    return false;
                }

                DataRow row = dt.Rows[0];
                string passwordHashFromDB = row["passwordHash"]?.ToString().Trim() ?? "";

                if (!Services.PasswordService.VerifyPassword(password, passwordHashFromDB))
                {
                    mensaje = "Contraseña incorrecta.";
                    return false;
                }

                // Guardar datos en SesionActual
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

        // ============================
        // MÉTODO PARA RESTABLECER CONTRASEÑA
        // ============================
        public static bool RestablecerContrasena(string username, string telefono, string nuevaContrasena, out string mensaje)
        {
            mensaje = string.Empty;

            try
            {
                // Verificar si el usuario existe y obtener su teléfono
                string queryVerificacion = @"
                    SELECT e.telefono 
                    FROM Usuarios u
                    INNER JOIN Empleado e ON u.empleadoId = e.empleadoId
                    WHERE u.username = @username";

                var paramVerificacion = new[] { new SqlParameter("@username", username) };
                object telefonoResult = DbHelper.ExecuteScalar(queryVerificacion, paramVerificacion);

                if (telefonoResult == null || telefonoResult == DBNull.Value)
                {
                    mensaje = "El usuario no fue encontrado.";
                    return false;
                }

                string telefonoBD = telefonoResult.ToString().Trim();
                if (telefonoBD != telefono.Trim())
                {
                    mensaje = "El número de teléfono no coincide con el registrado.";
                    return false;
                }

                // Generar hash de la nueva contraseña
                string nuevoHash = Services.PasswordService.HashPassword(nuevaContrasena);

                // Actualizar contraseña en la base de datos
                string queryUpdate = "UPDATE Usuarios SET passwordHash = @hash WHERE username = @username";
                var paramUpdate = new[]
                {
                    new SqlParameter("@hash", nuevoHash),
                    new SqlParameter("@username", username)
                };

                int filasAfectadas = DbHelper.ExecuteNonQuery(queryUpdate, paramUpdate);

                if (filasAfectadas > 0)
                {
                    mensaje = "Contraseña actualizada exitosamente.";
                    return true;
                }
                else
                {
                    mensaje = "No se pudo actualizar la contraseña.";
                    return false;
                }
            }
            catch (Exception ex)
            {
                mensaje = $"Error en la base de datos: {ex.Message}";
                return false;
            }
        }
    }
}