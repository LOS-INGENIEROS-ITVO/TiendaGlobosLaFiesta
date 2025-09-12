using System.Windows;
using System.Windows.Input;
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
            // Aquí iría la lógica para abrir la nueva ventana de recuperación.
            // Por ejemplo:
            // var ventanaRecuperacion = new RecuperarContrasenaWindow();
            // ventanaRecuperacion.ShowDialog();

            MessageBox.Show("Funcionalidad de 'Recuperar Contraseña' por implementar.", "En Desarrollo", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}