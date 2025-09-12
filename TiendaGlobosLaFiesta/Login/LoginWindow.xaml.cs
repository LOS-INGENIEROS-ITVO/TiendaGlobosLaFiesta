using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using TiendaGlobosLaFiesta.ViewModels;

namespace TiendaGlobosLaFiesta
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();

            if (DataContext is LoginViewModel viewModel)
            {
                viewModel.OnLoginSuccess += (rolDelUsuario) =>
                {
                    if (rolDelUsuario == "Gerente")
                    {
                        var menuGerente = new MenuGerenteWindow(rolDelUsuario);
                        menuGerente.Show();
                    }
                    else
                    {
                        var menuEmpleado = new EmpleadoWindow(rolDelUsuario);
                        menuEmpleado.Show();
                    }
                    this.Close();
                };
                txtPassword.GotFocus += PasswordBox_FocusChanged;
                txtPassword.LostFocus += PasswordBox_FocusChanged;
                txtPassword.KeyDown += PasswordBox_KeyDown_CapsLock;

                txtPasswordVisible.GotFocus += PasswordBox_FocusChanged;
                txtPasswordVisible.LostFocus += PasswordBox_FocusChanged;
                txtPasswordVisible.KeyDown += PasswordBox_KeyDown_CapsLock;
            }
        }

        // --- MANEJADORES DE EVENTOS DE LA VISTA ---

        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // Flujo con la tecla Enter
        private void TxtUsername_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                txtPassword.Focus();
            }
        }

        private void TxtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (btnIngresar.Command.CanExecute(txtPassword))
                {
                    // Al presionar Enter, se actualiza el Password del PasswordBox antes de ejecutar el comando
                    txtPassword.Password = chkShowPassword.IsChecked == true ? txtPasswordVisible.Text : txtPassword.Password;
                    btnIngresar.Command.Execute(txtPassword);
                }
            }
        }

        // Funcionalidad para Mostrar/Ocultar contraseña
        private void chkShowPassword_Handler(object sender, RoutedEventArgs e)
        {
            if (chkShowPassword.IsChecked == true)
            {
                txtPasswordVisible.Text = txtPassword.Password;
                txtPasswordVisible.Visibility = Visibility.Visible;
                txtPassword.Visibility = Visibility.Collapsed;
            }
            else
            {
                txtPassword.Password = txtPasswordVisible.Text;
                txtPassword.Visibility = Visibility.Visible;
                txtPasswordVisible.Visibility = Visibility.Collapsed;
            }
        }

        // Funcionalidad para Recuperar Contraseña
        private void RecuperarContrasena_Click(object sender, RoutedEventArgs e)
        {
            // Se crea y muestra la nueva ventana de recuperación.
            var ventanaRecuperacion = new RecuperarContrasenaWindow();
            ventanaRecuperacion.ShowDialog(); // ShowDialog la muestra de forma modal
        }

        private void ActualizarAvisoCapsLock()
        {
            if (Console.CapsLock)
            {
                avisoCapsLock.Visibility = Visibility.Visible;
            }
            else
            {
                avisoCapsLock.Visibility = Visibility.Collapsed;
            }
        }

        private void PasswordBox_FocusChanged(object sender, RoutedEventArgs e)
        {
            var passwordBox = sender as FrameworkElement; // Funciona para TextBox y PasswordBox
            if (passwordBox.IsKeyboardFocusWithin)
            {
                // Si el usuario entra al campo, comprueba el estado
                ActualizarAvisoCapsLock();
            }
            else
            {
                // Si el usuario sale del campo, siempre oculta el aviso
                avisoCapsLock.Visibility = Visibility.Collapsed;
            }
        }

        private void PasswordBox_KeyDown_CapsLock(object sender, KeyEventArgs e)
        {
            // Comprueba el estado de Bloq Mayús después de que la tecla ha sido presionada
            Dispatcher.BeginInvoke(new Action(() => ActualizarAvisoCapsLock()), DispatcherPriority.Input);
        }
    }
}