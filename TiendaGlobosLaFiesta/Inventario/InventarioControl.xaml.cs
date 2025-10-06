using System.Windows;
using System.Windows.Controls;
using TiendaGlobosLaFiesta.Models;
using TiendaGlobosLaFiesta.Data;
using TiendaGlobosLaFiesta.Views;
using TiendaGlobosLaFiesta.ViewModels;

namespace TiendaGlobosLaFiesta.Views
{
    public partial class InventarioControl : UserControl
    {
        private readonly ProductoRepository _productoRepo = new();
        private readonly GloboRepository _globoRepo = new();
        public InventarioViewModel VM { get; private set; }

        public InventarioControl()
        {
            InitializeComponent();
            VM = new InventarioViewModel();
            DataContext = VM;
        }

        #region Productos

        private void AgregarProducto_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new ProductoEditWindow(); // Ventana para agregar
            if (ventana.ShowDialog() == true && ventana.Producto != null)
            {
                _productoRepo.AgregarProducto(ventana.Producto);
                VM.ProductosView.Add(ventana.Producto);
                VM.ProductosViewFiltered.Refresh();
            }
        }

        private void EditarProducto_Click(object sender, RoutedEventArgs e)
        {
            if (ProductosDataGrid.SelectedItem is Producto p)
            {
                var ventana = new ProductoEditWindow(p); // Ventana para editar
                if (ventana.ShowDialog() == true && ventana.Producto != null)
                {
                    _productoRepo.ActualizarProducto(ventana.Producto);
                    VM.ProductosViewFiltered.Refresh();
                }
            }
        }

        private void EliminarProducto_Click(object sender, RoutedEventArgs e)
        {
            if (ProductosDataGrid.SelectedItem is Producto p)
            {
                if (MessageBox.Show($"¿Eliminar el producto {p.Nombre}?", "Confirmar",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    _productoRepo.EliminarProducto(p.ProductoId);
                    VM.ProductosView.Remove(p);
                    VM.ProductosViewFiltered.Refresh();
                }
            }
        }

        #endregion

        #region Globos

        private void AgregarGlobo_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new GloboEditWindow(); // Ventana para agregar
            if (ventana.ShowDialog() == true && ventana.Globo != null)
            {
                _globoRepo.AgregarGlobo(ventana.Globo);
                VM.GlobosView.Add(ventana.Globo);
                VM.GlobosViewFiltered.Refresh();
            }
        }

        private void EditarGlobo_Click(object sender, RoutedEventArgs e)
        {
            if (GlobosDataGrid.SelectedItem is Globo g)
            {
                var ventana = new GloboEditWindow(g); // Ventana para editar
                if (ventana.ShowDialog() == true && ventana.Globo != null)
                {
                    _globoRepo.ActualizarGlobo(ventana.Globo);
                    VM.GlobosViewFiltered.Refresh();
                }
            }
        }

        private void EliminarGlobo_Click(object sender, RoutedEventArgs e)
        {
            if (GlobosDataGrid.SelectedItem is Globo g)
            {
                if (MessageBox.Show($"¿Eliminar el globo {g.Color}?", "Confirmar",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    _globoRepo.EliminarGlobo(g.GloboId);
                    VM.GlobosView.Remove(g);
                    VM.GlobosViewFiltered.Refresh();
                }
            }
        }

        #endregion
    }
}