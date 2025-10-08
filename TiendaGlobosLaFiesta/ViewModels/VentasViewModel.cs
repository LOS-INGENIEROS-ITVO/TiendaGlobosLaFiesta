using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using TiendaGlobosLaFiesta.Managers;
using TiendaGlobosLaFiesta.Models.Utilities;
using TiendaGlobosLaFiesta.Models.Ventas;

namespace TiendaGlobosLaFiesta.ViewModels
{
    public class VentasViewModel : INotifyPropertyChanged
    {
        private readonly ModuloManager _moduloManager;

        public ObservableCollection<ProductoVenta> Productos { get; set; }
        public ObservableCollection<GloboVenta> Globos { get; set; }
        public ObservableCollection<ItemVenta> Carrito { get; set; }

        public ICommand AgregarProductoCommand { get; set; }
        public ICommand AgregarGloboCommand { get; set; }
        public ICommand RegistrarVentaCommand { get; set; }

        public VentasViewModel(ModuloManager moduloManager)
        {
            _moduloManager = moduloManager;

            // Cargar inventario desde BD
            Productos = new ObservableCollection<ProductoVenta>(_moduloManager.ObtenerProductos());
            Globos = new ObservableCollection<GloboVenta>(_moduloManager.ObtenerGlobos());
            Carrito = new ObservableCollection<ItemVenta>();

            // Comandos
            AgregarProductoCommand = new RelayCommand<object>(p => AgregarProducto((ProductoVenta)p));
            AgregarGloboCommand = new RelayCommand<object>(g => AgregarGlobo((GloboVenta)g));
            RegistrarVentaCommand = new RelayCommand(_ => RegistrarVenta()); 
        }

        private void AgregarProducto(ProductoVenta producto)
        {
            if (producto == null) return;

            // Validar stock
            if (producto.Cantidad + 1 > producto.Stock) return;

            producto.Cantidad += 1;
            if (!Carrito.Contains(producto))
                Carrito.Add(producto);

            OnPropertyChanged(nameof(Carrito));
        }

        private void AgregarGlobo(GloboVenta globo)
        {
            if (globo == null) return;

            // Validar stock
            if (globo.Cantidad + 1 > globo.Stock) return;

            globo.Cantidad += 1;
            if (!Carrito.Contains(globo))
                Carrito.Add(globo);

            OnPropertyChanged(nameof(Carrito));
        }

        private void RegistrarVenta()
        {
            if (Carrito.Count == 0) return;

            if (!SesionActual.EmpleadoId.HasValue) return; // No hay empleado logueado
            int empleadoId = SesionActual.EmpleadoId.Value;

            string ventaId = Guid.NewGuid().ToString();
            string clienteId = "C0001"; // TODO: reemplazar con selección dinámica de cliente

            // Separar productos y globos del carrito
            var productos = new List<ProductoVenta>();
            var globos = new List<GloboVenta>();

            foreach (var item in Carrito)
            {
                if (item is ProductoVenta p) productos.Add(p);
                else if (item is GloboVenta g) globos.Add(g);
            }

            // Registrar venta usando ModuloManager
            bool exito = _moduloManager.RegistrarVenta(ventaId, empleadoId, clienteId, productos, globos);

            if (exito)
            {
                Carrito.Clear();
                Productos.Clear();
                Globos.Clear();

                // Recargar inventario desde BD
                foreach (var p in _moduloManager.ObtenerProductos()) Productos.Add(p);
                foreach (var g in _moduloManager.ObtenerGlobos()) Globos.Add(g);

                OnPropertyChanged(nameof(Carrito));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}