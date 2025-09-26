using System.Windows;
using System.Windows.Controls;

namespace TiendaGlobosLaFiesta.Views
{
    public partial class AjustarStockWindow : Window
    {
        public int NuevaCantidad { get; private set; }
        public string Motivo { get; private set; }

        public AjustarStockWindow(string nombreProducto)
        {
            InitializeComponent();
            lblProducto.Text += nombreProducto;
        }

        private void Confirmar_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(txtNuevaCantidad.Text, out int nuevaCantidad) || nuevaCantidad < 0)
            {
                MessageBox.Show("Por favor, introduce una cantidad numérica válida.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (cmbMotivo.SelectedItem == null)
            {
                MessageBox.Show("Por favor, selecciona un motivo para el ajuste.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            NuevaCantidad = nuevaCantidad;
            Motivo = (cmbMotivo.SelectedItem as ComboBoxItem).Content.ToString();
            this.DialogResult = true;
        }
    }
}