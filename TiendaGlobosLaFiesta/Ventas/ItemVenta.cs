using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TiendaGlobosLaFiesta.Models
{
    public abstract class ItemVenta : INotifyPropertyChanged
    {
        // 🔹 PROPIEDAD AÑADIDA: Un identificador genérico para productos o globos
        public string Id { get; set; }
        public string Unidad { get; set; }
        public string Nombre { get; set; }


        private int cantidad;
        public int Cantidad
        {
            get => cantidad;
            set
            {
                // 🔹 MEJORA: Se añaden validaciones para evitar errores
                if (value > Stock) value = Stock; // No permitir que la cantidad exceda el stock
                if (value < 0) value = 0;     // No permitir cantidades negativas

                if (cantidad != value)
                {
                    cantidad = value;
                    // 🔹 CORRECCIÓN: Notifica a la UI que tanto 'Cantidad' como 'Importe' han cambiado
                    OnPropertyChanged(nameof(Cantidad));
                    OnPropertyChanged(nameof(Importe));
                }
            }
        }

        public int Stock { get; set; }
        public decimal Costo { get; set; }
        public decimal Importe => Cantidad * Costo;

        public void Incrementar() => Cantidad = (Cantidad < Stock) ? Cantidad + 1 : Stock;
        public void Decrementar() => Cantidad = (Cantidad > 0) ? Cantidad - 1 : 0;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
      }
    }