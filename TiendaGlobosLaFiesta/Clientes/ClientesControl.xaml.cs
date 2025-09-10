using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using TiendaGlobosLaFiesta.Clientes;
using TiendaGlobosLaFiesta.Data;

namespace TiendaGlobosLaFiesta
{
    public partial class ClientesControl : UserControl
    {
        private ObservableCollection<Cliente> clientes;

        public ClientesControl()
        {
            InitializeComponent();
            CargarClientes();
        }

        private void CargarClientes(string filtro = "")
        {
            clientes = new ObservableCollection<Cliente>(ConexionBD.ObtenerClientes(filtro));
            dgClientes.ItemsSource = clientes;
        }

        private void BtnBuscarCliente_Click(object sender, RoutedEventArgs e)
        {
            CargarClientes(txtBuscarCliente.Text.Trim());
        }

        private void BtnAgregarCliente_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new ClienteForm();
            ventana.ShowDialog();
            CargarClientes();
        }

        private void BtnEditarCliente_Click(object sender, RoutedEventArgs e)
        {
            if (dgClientes.SelectedItem is Cliente seleccionado)
            {
                var ventana = new ClienteForm(seleccionado);
                ventana.ShowDialog();
                CargarClientes();
            }
            else
            {
                MessageBox.Show("Seleccione un cliente para editar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnEliminarCliente_Click(object sender, RoutedEventArgs e)
        {
            if (dgClientes.SelectedItem is Cliente seleccionado)
            {
                var result = MessageBox.Show($"¿Eliminar cliente {seleccionado.Nombre}?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    ConexionBD.EliminarCliente(seleccionado.ClienteId);
                    CargarClientes();
                }
            }
            else
            {
                MessageBox.Show("Seleccione un cliente para eliminar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnActualizarClientes_Click(object sender, RoutedEventArgs e)
        {
            CargarClientes();
        }
    }
}