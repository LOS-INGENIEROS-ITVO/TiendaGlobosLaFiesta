using System;
using System.Data;
using System.Data.SqlClient;
using TiendaGlobosLaFiesta.Data;
using TiendaGlobosLaFiesta.Modelos;

namespace TiendaGlobosLaFiesta
{
    public static class AuthService
    {
        public static bool Login(string usuario, string contrasena, out string rol, out string mensaje)
        {
            rol = null;
            mensaje = null;

            try
            {
                string query = @"
                    SELECT u.usuarioId, 
                           u.empleadoId, 
                           u.passwordHash, 
                           e.puestoId,
                           e.primerNombre, 
                           e.segundoNombre, 
                           e.apellidoP, 
                           e.apellidoM
                    FROM Usuarios u
                    JOIN Empleado e ON u.empleadoId = e.empleadoId
                    WHERE u.username = @usuario AND u.activo = 1
                ";

                var parametros = new SqlParameter[] { new SqlParameter("@usuario", usuario) };
                DataTable dt = DbHelper.ExecuteQuery(query, parametros);

                if (dt.Rows.Count == 0)
                {
                    mensaje = "Usuario no encontrado o inactivo.";
                    return false;
                }

                string hashAlmacenado = dt.Rows[0]["passwordHash"].ToString().Trim();
                if (!HashHelper.VerificarHash(contrasena, hashAlmacenado))
                {
                    mensaje = "Contraseña incorrecta.";
                    return false;
                }

                rol = dt.Rows[0]["puestoId"].ToString();
                SesionActual.UsuarioId = Convert.ToInt32(dt.Rows[0]["usuarioId"]);
                SesionActual.EmpleadoId = Convert.ToInt32(dt.Rows[0]["empleadoId"]);
                SesionActual.Username = usuario;
                SesionActual.Rol = rol;

                string primerNombre = dt.Rows[0]["primerNombre"].ToString();
                string segundoNombre = dt.Rows[0]["segundoNombre"].ToString();
                string apellidoP = dt.Rows[0]["apellidoP"].ToString();
                string apellidoM = dt.Rows[0]["apellidoM"].ToString();
                SesionActual.NombreEmpleadoCompleto = $"{primerNombre} {segundoNombre} {apellidoP} {apellidoM}".Trim();

                return true;
            }
            catch (Exception ex)
            {
                mensaje = "Error de conexión: " + ex.Message;
                return false;
            }
        }
    }
}