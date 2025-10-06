namespace TiendaGlobosLaFiesta.Models
{
    public class Usuario
    {
        public int UsuarioId { get; set; }
        public int EmpleadoId { get; set; }      // Relación con empleado
        public string NombreUsuario { get; set; } // Mapea 'username' en la BD
        public string ContrasenaHash { get; set; } // Mapea 'passwordHash' en la BD
        public bool Activo { get; set; }          // Mapea 'activo' en la BD
        public DateTime FechaCreacion { get; set; } // Mapea 'fechaCreacion' en la BD

        // Propiedad opcional para permisos o roles
        public string? Rol { get; set; }
    }
}
