using System;
using System.ComponentModel;

namespace TiendaGlobosLaFiesta.Inventario
{
    public class ProductoInventario : INotifyPropertyChanged
    {
        private string productoId;
        private string nombre;
        private int unidad;
        private int stock;
        private decimal costo;

        public ProductoInventario()
        {
            ProductoId = Guid.NewGuid().ToString("N").Substring(0, 20);
        }

        public string ProductoId
        {
            get => productoId;
            set { productoId = value; OnPropertyChanged(nameof(ProductoId)); }
        }

        public string Nombre
        {
            get => nombre;
            set { nombre = value; OnPropertyChanged(nameof(Nombre)); }
        }

        public int Unidad
        {
            get => unidad;
            set { unidad = value; OnPropertyChanged(nameof(Unidad)); }
        }

        public int Stock
        {
            get => stock;
            set { stock = value; OnPropertyChanged(nameof(Stock)); }
        }

        public decimal Costo
        {
            get => costo;
            set { costo = value; OnPropertyChanged(nameof(Costo)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
