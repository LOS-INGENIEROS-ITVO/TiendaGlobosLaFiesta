namespace TiendaGlobosLaFiesta.Login
{
    public class Empleado
    {
        public int EmpleadoId { get; set; }
        public string PrimerNombre { get; set; }
        public string SegundoNombre { get; set; }
        public string ApellidoP { get; set; }
        public string ApellidoM { get; set; }
        public string Telefono { get; set; }
        public string PuestoId { get; set; } // FK a Puesto
        public int ClaveDireccion { get; set; } // FK a Direccion
    }
}
