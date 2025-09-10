using System;
using System.Windows;
using System.Windows.Input;
using TiendaGlobosLaFiesta.Clientes;
using TiendaGlobosLaFiesta.Data;

namespace TiendaGlobosLaFiesta
{
    public partial class ClienteForm : Window
    {
        private Cliente cliente;

        public ClienteForm(Cliente cliente = null)
        {
            InitializeComponent();

            if (cliente != null)
            {
                this.cliente = cliente;
                LlenarDatos();
                txtClienteId.IsEnabled = false; // No permitir editar ID existente
            }
            else
            {
                this.cliente = new Cliente();
            }

            // Asociar KeyDown a todos los TextBox
            txtClienteId.KeyDown += TextBox_KeyDown;
            txtPrimerNombre.KeyDown += TextBox_KeyDown;
            txtSegundoNombre.KeyDown += TextBox_KeyDown;
            txtApellidoP.KeyDown += TextBox_KeyDown;
            txtApellidoM.KeyDown += TextBox_KeyDown;
            txtTelefono.KeyDown += TextBox_KeyDown;
        }

        private void LlenarDatos()
        {
            txtClienteId.Text = cliente.ClienteId;
            txtPrimerNombre.Text = cliente.PrimerNombre;
            txtSegundoNombre.Text = cliente.SegundoNombre;
            txtApellidoP.Text = cliente.ApellidoP;
            txtApellidoM.Text = cliente.ApellidoM;
            txtTelefono.Text = cliente.Telefono?.ToString() ?? "";
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            GuardarCliente();
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true; // Evitar sonido por Enter

                // Navegación secuencial entre TextBox
                if (sender == txtClienteId)
                    txtPrimerNombre.Focus();
                else if (sender == txtPrimerNombre)
                    txtSegundoNombre.Focus();
                else if (sender == txtSegundoNombre)
                    txtApellidoP.Focus();
                else if (sender == txtApellidoP)
                    txtApellidoM.Focus();
                else if (sender == txtApellidoM)
                    txtTelefono.Focus();
                else if (sender == txtTelefono)
                    GuardarCliente(); // Al final, guardar automáticamente
            }
        }

        private void GuardarCliente()
        {
            // Validaciones obligatorias
            if (string.IsNullOrWhiteSpace(txtClienteId.Text) ||
                string.IsNullOrWhiteSpace(txtPrimerNombre.Text) ||
                string.IsNullOrWhiteSpace(txtApellidoP.Text))
            {
                MessageBox.Show("ID, Primer Nombre y Apellido Paterno son obligatorios.",
                                "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Asignar valores al modelo
            cliente.ClienteId = txtClienteId.Text.Trim();
            cliente.PrimerNombre = txtPrimerNombre.Text.Trim();
            cliente.SegundoNombre = txtSegundoNombre.Text.Trim();
            cliente.ApellidoP = txtApellidoP.Text.Trim();
            cliente.ApellidoM = txtApellidoM.Text.Trim();
            cliente.Telefono = long.TryParse(txtTelefono.Text.Trim(), out long tel) ? tel : (long?)null;

            // Guardar o actualizar según corresponda
            bool exito = txtClienteId.IsEnabled
                ? ConexionBD.AgregarCliente(cliente)
                : ConexionBD.ActualizarCliente(cliente);

            if (exito)
            {
                MessageBox.Show("Cliente guardado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
        }
    }
}
