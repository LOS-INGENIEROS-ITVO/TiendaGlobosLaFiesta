using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TiendaGlobosLaFiesta.Models.Inventario;

namespace TiendaGlobosLaFiesta.Models.Ventas
{
    // Clase base para items de venta
    public abstract class ItemVenta : INotifyPropertyChanged
    {
        public abstract string Id { get; }
        public abstract string Nombre { get; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}
