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
            if (!int.TryParse(txtNuevaCantidad.Text, out int nuevaCantidad) || nuevaCantidad < 0) { /*...*/ return; }
            if (cmbMotivo.SelectedItem == null) { /*...*/ return; }

            NuevaCantidad = nuevaCantidad;
            Motivo = (cmbMotivo.SelectedItem as ComboBoxItem).Content.ToString();
            this.DialogResult = true;
        }
    }
}