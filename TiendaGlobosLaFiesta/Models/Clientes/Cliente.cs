using System.ComponentModel;

namespace TiendaGlobosLaFiesta.Models.Clientes
{
    public class Cliente : INotifyPropertyChanged
    {
        private string clienteId;
        private string primerNombre;
        private string segundoNombre;
        private string apellidoP;
        private string apellidoM;
        private long? telefono;
        public bool Activo { get; set; } = true;

        public string ClienteId
        {
            get => clienteId;
            set { clienteId = value; OnPropertyChanged(nameof(ClienteId)); }
        }

        public string PrimerNombre
        {
            get => primerNombre;
            set { primerNombre = value; OnPropertyChanged(nameof(PrimerNombre)); OnPropertyChanged(nameof(Nombre)); OnPropertyChanged(nameof(NombreCompleto)); }
        }

        public string SegundoNombre
        {
            get => segundoNombre;
            set { segundoNombre = value; OnPropertyChanged(nameof(SegundoNombre)); OnPropertyChanged(nameof(Nombre)); OnPropertyChanged(nameof(NombreCompleto)); }
        }

        public string ApellidoP
        {
            get => apellidoP;
            set { apellidoP = value; OnPropertyChanged(nameof(ApellidoP)); OnPropertyChanged(nameof(Nombre)); OnPropertyChanged(nameof(NombreCompleto)); }
        }

        public string ApellidoM
        {
            get => apellidoM;
            set { apellidoM = value; OnPropertyChanged(nameof(ApellidoM)); OnPropertyChanged(nameof(Nombre)); OnPropertyChanged(nameof(NombreCompleto)); }
        }

        public long? Telefono
        {
            get => telefono;
            set { telefono = value; OnPropertyChanged(nameof(Telefono)); OnPropertyChanged(nameof(TelefonoTexto)); }
        }

        // Nombre simplificado
        public string Nombre => $"{PrimerNombre} {SegundoNombre} {ApellidoP} {ApellidoM}".Replace("  ", " ").Trim();

        // Compatibilidad con código anterior
        public string NombreCompleto => Nombre;

        public string TelefonoTexto => Telefono.HasValue ? Telefono.Value.ToString() : "-";

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}