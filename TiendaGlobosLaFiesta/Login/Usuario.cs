using System;

namespace TiendaGlobosLaFiesta.Models
{
    public class Usuario
    {
        public int UsuarioId { get; set; }
        public int EmpleadoId { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}
