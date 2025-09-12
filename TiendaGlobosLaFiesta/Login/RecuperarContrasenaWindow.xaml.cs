using System;
using System.Windows;
using System.Windows.Input; // Necesario para KeyEventArgs
using TiendaGlobosLaFiesta.Data;

namespace TiendaGlobosLaFiesta
{
    public partial class RecuperarContrasenaWindow : Window
    {
        public RecuperarContrasenaWindow()
        {
            InitializeComponent();
            txtUsername.Focus(); // Pone el foco en el primer campo al abrir
        }

        private void BtnConfirmar_Click(object sender, RoutedEventArgs e)
        {
            // Validaciones de la UI
            if (string.IsNullOrWhiteSpace(txtUsername.Text) ||
                string.IsNullOrWhiteSpace(txtTelefono.Text) ||
                string.IsNullOrWhiteSpace(txtNuevaContrasena.Password) ||
                string.IsNullOrWhiteSpace(txtConfirmarContrasena.Password))
            {
                MessageBox.Show("Todos los campos son obligatorios.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!long.TryParse(txtTelefono.Text, out long telefono))
            {
                MessageBox.Show("El número de teléfono debe contener solo números.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (txtNuevaContrasena.Password != txtConfirmarContrasena.Password)
            {
                MessageBox.Show("Las nuevas contraseñas no coinciden.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Llamada al servicio de autenticación
            if (AuthService.RestablecerContrasena(txtUsername.Text, telefono, txtNuevaContrasena.Password, out string mensaje))
            {
                MessageBox.Show(mensaje, "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close(); // Cierra la ventana si el cambio fue exitoso
            }
            else
            {
                MessageBox.Show(mensaje, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // --- MÉTODOS PARA EL FLUJO CON LA TECLA ENTER ---

        private void TxtUsername_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                txtTelefono.Focus();
            }
        }

        private void TxtTelefono_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                txtNuevaContrasena.Focus();
            }
        }

        private void TxtNuevaContrasena_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                txtConfirmarContrasena.Focus();
            }
        }

        private void TxtConfirmarContrasena_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BtnConfirmar_Click(sender, new RoutedEventArgs());
            }
        }
    }
}