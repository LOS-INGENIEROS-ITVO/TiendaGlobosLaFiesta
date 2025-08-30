using System.Data;
using TiendaGlobosLaFiesta.Data;
using TiendaGlobosLaFiesta;

public static class AuthService
{
    public static bool ValidarLogin(string usuario, string contrasena, out string rol)
    {
        rol = null;

        string query = @"SELECT usuarioId, empleadoId, passwordHash 
                         FROM Usuarios 
                         WHERE username = @usuario AND activo = 1";

        var parametros = new[] { ConexionBD.Param("@usuario", usuario) };
        DataTable dt = ConexionBD.EjecutarConsulta(query, parametros);

        if (dt.Rows.Count == 0)
            return false;

        string hashAlmacenado = dt.Rows[0]["passwordHash"].ToString().Trim();
        int empleadoId = Convert.ToInt32(dt.Rows[0]["empleadoId"]);
        SesionActual.UsuarioId = Convert.ToInt32(dt.Rows[0]["usuarioId"]);

        if (!HashHelper.VerificarHash(contrasena, hashAlmacenado))
            return false;

        SesionActual.EmpleadoId = empleadoId;
        SesionActual.Username = usuario;

        string rolQuery = @"SELECT puestoId FROM Empleado WHERE empleadoId = @empleadoId";
        var rolParam = new[] { ConexionBD.Param("@empleadoId", empleadoId) };
        DataTable rolDt = ConexionBD.EjecutarConsulta(rolQuery, rolParam);
        if (rolDt.Rows.Count > 0)
            rol = rolDt.Rows[0]["puestoId"].ToString();

        SesionActual.Rol = rol;
        return true;
    }
}