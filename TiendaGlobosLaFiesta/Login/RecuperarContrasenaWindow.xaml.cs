using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using TiendaGlobosLaFiesta.Data;

namespace TiendaGlobosLaFiesta
{
    public partial class RecuperarContrasenaWindow : Window
    {
        public RecuperarContrasenaWindow()
        {
            InitializeComponent();
            txtUsername.Focus(); // foco inicial
        }

        private async void BtnConfirmar_Click(object sender, RoutedEventArgs e)
        {
            // limpias el mensaje antes
            lblMensaje.Text = "";
            lblMensaje.Foreground = Brushes.Crimson;

            // Validaciones de la UI
            if (string.IsNullOrWhiteSpace(txtUsername.Text) ||
                string.IsNullOrWhiteSpace(txtTelefono.Text) ||
                string.IsNullOrWhiteSpace(txtNuevaContrasena.Password) ||
                string.IsNullOrWhiteSpace(txtConfirmarContrasena.Password))
            {
                lblMensaje.Text = "Todos los campos son obligatorios.";
                return;
            }

            if (!long.TryParse(txtTelefono.Text, out long telefono))
            {
                lblMensaje.Text = "El número de teléfono debe contener solo números.";
                return;
            }

            if (txtNuevaContrasena.Password != txtConfirmarContrasena.Password)
            {
                lblMensaje.Text = "Las nuevas contraseñas no coinciden.";
                return;
            }

            // Deshabilitar botón y ejecutar en hilo de fondo
            btnConfirmar.IsEnabled = false;
            lblMensaje.Text = "Procesando, espere...";

            try
            {
                // Inicializa 'mensaje' antes de usarlo con out
                string mensaje = "";
                bool exito = await Task.Run(() =>
                    AuthService.RestablecerContrasena(txtUsername.Text.Trim(), telefono, txtNuevaContrasena.Password, out mensaje)
                );

                if (exito)
                {
                    lblMensaje.Foreground = Brushes.Green;
                    lblMensaje.Text = string.IsNullOrWhiteSpace(mensaje) ? "Contraseña actualizada correctamente." : mensaje;
                    await Task.Delay(1200);
                    this.Close();
                }
                else
                {
                    lblMensaje.Foreground = Brushes.Crimson;
                    lblMensaje.Text = string.IsNullOrWhiteSpace(mensaje) ? "No se pudo actualizar la contraseña." : mensaje;
                }
            }
            catch (Exception ex)
            {
                lblMensaje.Foreground = Brushes.Crimson;
                lblMensaje.Text = "Error inesperado: " + ex.Message;
            }
            finally
            {
                btnConfirmar.IsEnabled = true;
            }
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // --- MÉTODOS PARA EL FLUJO CON LA TECLA ENTER ---
        private void TxtUsername_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) txtTelefono.Focus();
        }

        private void TxtTelefono_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) txtNuevaContrasena.Focus();
        }

        private void TxtNuevaContrasena_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) txtConfirmarContrasena.Focus();
        }

        private void TxtConfirmarContrasena_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) BtnConfirmar_Click(sender, new RoutedEventArgs());
        }
    }
}