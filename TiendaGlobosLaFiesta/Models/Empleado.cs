using System.ComponentModel;

namespace TiendaGlobosLaFiesta.Models
{
    public class Empleado : INotifyPropertyChanged
    {
        public int EmpleadoId { get; set; }
        public string PuestoId { get; set; }
        public int ClaveDireccion { get; set; }
        public string PrimerNombre { get; set; }
        public string SegundoNombre { get; set; }
        public string ApellidoP { get; set; }
        public string ApellidoM { get; set; }
        public long Telefono { get; set; }
        public bool Activo { get; set; } = true;

        public string NombreCompleto => $"{PrimerNombre} {SegundoNombre} {ApellidoP} {ApellidoM}".Trim();

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}