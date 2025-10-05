using System.Windows;
using System.Windows.Controls;
using TiendaGlobosLaFiesta.Models;
using TiendaGlobosLaFiesta.ViewModels;

namespace TiendaGlobosLaFiesta.Views
{
    public partial class InventarioControl : UserControl
    {
        public InventarioViewModel VM { get; set; }

        public InventarioControl()
        {
            InitializeComponent();

            VM = new InventarioViewModel(); // Instanciamos el ViewModel aquí
            this.DataContext = VM;
        }

        #region PRODUCTOS
        private void AgregarProducto_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new ProductoEditWindow(new Producto());
            if (ventana.ShowDialog() == true)
            {
                VM.ProductosView.Add(ventana.Producto);
                VM.ProductosViewFiltered.Refresh();
            }
        }

        private void EditarProducto_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is Producto p)
            {
                var ventana = new ProductoEditWindow(p);
                ventana.ShowDialog();
                VM.ProductosViewFiltered.Refresh();
            }
        }

        private void EliminarProducto_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is Producto p)
            {
                if (MessageBox.Show($"¿Eliminar producto {p.Nombre}?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    VM.ProductosView.Remove(p);
                    VM.ProductosViewFiltered.Refresh();
                }
            }
        }
        #endregion

        #region GLOBOS
        private void AgregarGlobo_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new GloboEditWindow(new Globo());
            if (ventana.ShowDialog() == true)
            {
                VM.GlobosView.Add(ventana.Globo);
                VM.GlobosViewFiltered.Refresh();
            }
        }

        private void EditarGlobo_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is Globo g)
            {
                var ventana = new GloboEditWindow(g);
                ventana.ShowDialog();
                VM.GlobosViewFiltered.Refresh();
            }
        }

        private void EliminarGlobo_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is Globo g)
            {
                if (MessageBox.Show($"¿Eliminar globo {g.Nombre}?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    VM.GlobosView.Remove(g);
                    VM.GlobosViewFiltered.Refresh();
                }
            }
        }
        #endregion

        #region STOCK CRÍTICO
        private void EditarStock_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is StockCriticoItem item)
            {
                if (item.Tipo == "Producto")
                {
                    var ventana = new ProductoEditWindow(item.Producto);
                    ventana.ShowDialog();
                }
                else
                {
                    var ventana = new GloboEditWindow(item.Globo);
                    ventana.ShowDialog();
                }
            }
        }

        private void AjustarStock_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is StockCriticoItem item)
            {
                var ventana = new AjustarStockWindow(item.Nombre);
                if (ventana.ShowDialog() == true)
                {
                    if (item.Tipo == "Producto")
                        item.Producto.Stock = ventana.NuevaCantidad;
                    else
                        item.Globo.Stock = ventana.NuevaCantidad;

                    VM.StockCriticoView.Refresh();
                    VM.ProductosViewFiltered.Refresh();
                    VM.GlobosViewFiltered.Refresh();
                }
            }
        }
        #endregion
    }
}