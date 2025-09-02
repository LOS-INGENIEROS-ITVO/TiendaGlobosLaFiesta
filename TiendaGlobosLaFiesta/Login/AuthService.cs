using System;
using System.Data;
using TiendaGlobosLaFiesta;
using TiendaGlobosLaFiesta.Data;

public static class AuthService
{
    public static bool ValidarLogin(string usuario, string contrasena, out string rol)
    {
        rol = null;

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

        var parametros = new[] { ConexionBD.Param("@usuario", usuario) };
        DataTable dt = ConexionBD.EjecutarConsulta(query, parametros);

        if (dt.Rows.Count == 0)
            return false;

        string hashAlmacenado = dt.Rows[0]["passwordHash"].ToString().Trim();
        int empleadoId = Convert.ToInt32(dt.Rows[0]["empleadoId"]);
        SesionActual.UsuarioId = Convert.ToInt32(dt.Rows[0]["usuarioId"]);

        if (!HashHelper.VerificarHash(contrasena, hashAlmacenado))
            return false;

        rol = dt.Rows[0]["puestoId"].ToString();
        SesionActual.EmpleadoId = empleadoId;
        SesionActual.Username = usuario;
        SesionActual.Rol = rol;

        // Construimos el nombre completo
        string primerNombre = dt.Rows[0]["primerNombre"].ToString();
        string segundoNombre = dt.Rows[0]["segundoNombre"].ToString();
        string apellidoP = dt.Rows[0]["apellidoP"].ToString();
        string apellidoM = dt.Rows[0]["apellidoM"].ToString();

        SesionActual.NombreEmpleadoCompleto = $"{primerNombre} {segundoNombre} {apellidoP} {apellidoM}".Trim();

        return true;
    }
}