using System.Windows;
using TiendaGlobosLaFiesta.Models;

namespace TiendaGlobosLaFiesta.Inventario
{
    public partial class ProductoEditWindow : Window
    {
        public Producto Producto { get; private set; }

        public ProductoEditWindow(Producto producto)
        {
            InitializeComponent();
            Producto = producto;
            this.DataContext = Producto;
            this.Title = string.IsNullOrEmpty(producto.Nombre) ? "Agregar Producto Nuevo" : "Editar Producto";
        }

        private void Guardar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Producto.Nombre) || Producto.Costo < 0 || Producto.Stock < 0)
            {
                MessageBox.Show("Por favor, rellene todos los campos con valores válidos.", "Error de Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            this.DialogResult = true;
        }
    }
}