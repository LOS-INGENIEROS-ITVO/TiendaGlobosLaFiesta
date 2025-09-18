using System;
using System.Data;
using System.Data.SqlClient;
using TiendaGlobosLaFiesta.Modelos;
using TiendaGlobosLaFiesta.Services;

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

                var parametros = new[] { new SqlParameter("@username", username) };
                DataTable dt = DbHelper.ExecuteQuery(query, parametros);

                if (dt.Rows.Count == 0)
                {
                    mensaje = "Usuario no encontrado o inactivo.";
                    return false;
                }

                DataRow row = dt.Rows[0];
                string passwordHashFromDB = row["passwordHash"].ToString();

                try
                {
                    string hashGeneradoAhora = Services.PasswordService.HashPassword("admin");
                    bool controlTest = Services.PasswordService.VerifyPassword("admin", hashGeneradoAhora);
                    bool dbTest = Services.PasswordService.VerifyPassword(password, passwordHashFromDB);
                    bool dbTrimmedTest = Services.PasswordService.VerifyPassword(password, passwordHashFromDB.Trim());

 
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Error en la validación: {ex.Message}");
                }

                // Lógica de Verificación Real
                if (!Services.PasswordService.VerifyPassword(password, passwordHashFromDB.Trim()))
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


        public static bool RestablecerContrasena(string username, long telefono, string nuevaContrasena, out string mensaje)
        {
            try
            {
                // Primero, verificamos que el usuario y el teléfono coincidan.
                string queryVerificacion = @"
            SELECT e.telefono 
            FROM Usuarios u
            INNER JOIN Empleado e ON u.empleadoId = e.empleadoId
            WHERE u.username = @username";

                var paramVerificacion = new[] { new SqlParameter("@username", username) };
                object telefonoResult = DbHelper.ExecuteScalar(queryVerificacion, paramVerificacion);

                if (telefonoResult == null || telefonoResult == DBNull.Value)
                {
                    mensaje = "El nombre de usuario no fue encontrado.";
                    return false;
                }

                if (Convert.ToInt64(telefonoResult) != telefono)
                {
                    mensaje = "El número de teléfono no coincide con el registrado.";
                    return false;
                }

                // Si la verificación es exitosa, procedemos a actualizar la contraseña.
                string nuevoHash = Services.PasswordService.HashPassword(nuevaContrasena);

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