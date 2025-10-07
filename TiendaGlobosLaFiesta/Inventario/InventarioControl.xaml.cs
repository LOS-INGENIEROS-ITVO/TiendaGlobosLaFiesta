using System.Windows;
using System.Windows.Controls;
using TiendaGlobosLaFiesta.Models;
using TiendaGlobosLaFiesta.ViewModels;

namespace TiendaGlobosLaFiesta.Views
{
    public partial class InventarioControl : UserControl
    {
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
            var ventana = new ProductoEditWindow();
            if (ventana.ShowDialog() == true && ventana.Producto != null)
            {
                VM.AgregarProducto(ventana.Producto);
                VM.ProductosViewFiltered.Refresh();
            }
        }

        private void EditarProducto_Click(object sender, RoutedEventArgs e)
        {
            if (ProductosDataGrid.SelectedItem is Producto p)
            {
                var ventana = new ProductoEditWindow(p);
                if (ventana.ShowDialog() == true && ventana.Producto != null)
                {
                    VM.EditarProducto(ventana.Producto);
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
                    VM.EliminarProducto(p);
                    VM.ProductosViewFiltered.Refresh();
                }
            }
        }

        #endregion

        #region Globos

        private void AgregarGlobo_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new GloboEditWindow();
            if (ventana.ShowDialog() == true && ventana.Globo != null)
            {
                VM.AgregarGlobo(ventana.Globo);
                VM.GlobosViewFiltered.Refresh();
            }
        }

        private void EditarGlobo_Click(object sender, RoutedEventArgs e)
        {
            if (GlobosDataGrid.SelectedItem is Globo g)
            {
                var ventana = new GloboEditWindow(g);
                if (ventana.ShowDialog() == true && ventana.Globo != null)
                {
                    VM.EditarGlobo(ventana.Globo);
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
                    VM.EliminarGlobo(g);
                    VM.GlobosViewFiltered.Refresh();
                }
            }
        }

        #endregion
    }
}