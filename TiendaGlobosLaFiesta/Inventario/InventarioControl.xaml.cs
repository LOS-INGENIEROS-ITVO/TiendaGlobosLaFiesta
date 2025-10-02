using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using TiendaGlobosLaFiesta.ViewModels;

namespace TiendaGlobosLaFiesta.Views
{
    public partial class InventarioControl : UserControl
    {
        public InventarioViewModel ViewModel { get; }

        public InventarioControl()
        {
            InitializeComponent(); // Esto ya funcionará

            ViewModel = new InventarioViewModel();
            this.DataContext = ViewModel;

            // Invoca la carga de datos al cargar el control
            this.Loaded += InventarioControl_Loaded;
        }

        private void InventarioControl_Loaded(object sender, RoutedEventArgs e)
        {
            var vm = this.DataContext;
            if (vm == null) return;

            var mi = vm.GetType().GetMethod("CargarDatos", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                  ?? vm.GetType().GetMethod("CargarDatosIniciales", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            mi?.Invoke(vm, null);
        }
    }
}