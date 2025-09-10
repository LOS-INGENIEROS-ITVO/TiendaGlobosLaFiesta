namespace TiendaGlobosLaFiesta.Models
{
    public class Usuario
    {
        public int UsuarioId { get; set; }
        public string NombreUsuario { get; set; }
        public string ContrasenaHash { get; set; }
        public string Rol { get; set; }
    }
}
