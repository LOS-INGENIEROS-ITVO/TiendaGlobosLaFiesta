using System;
using System.Windows;
using System.Windows.Controls;
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
                    if (rolDelUsuario == "Gerente") new MenuGerenteWindow(rolDelUsuario).Show();
                    else new EmpleadoWindow(rolDelUsuario).Show();
                    this.Close();
                };
            }

            // Suscripción a eventos para el aviso de Bloq Mayús
            txtPassword.GotFocus += PasswordBox_FocusChanged;
            txtPassword.LostFocus += PasswordBox_FocusChanged;
            txtPassword.KeyDown += PasswordBox_KeyDown_CapsLock;
            txtPasswordVisible.GotFocus += PasswordBox_FocusChanged;
            txtPasswordVisible.LostFocus += PasswordBox_FocusChanged;
            txtPasswordVisible.KeyDown += PasswordBox_KeyDown_CapsLock;
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e) { Application.Current.Shutdown(); }

        private void TxtUsername_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) txtPassword.Focus();
        }

        private void TxtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) BtnIngresar_Click(sender, new RoutedEventArgs());
        }

        private void BtnIngresar_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel vm)
            {
                string password = chkShowPassword.IsChecked == true ? txtPasswordVisible.Text : txtPassword.Password;
                if (vm.LoginCommand.CanExecute(password))
                {
                    vm.LoginCommand.Execute(password);
                }
            }
        }

        private void chkShowPassword_Handler(object sender, RoutedEventArgs e)
        {
            var password = chkShowPassword.IsChecked == true ? txtPassword.Password : txtPasswordVisible.Text;
            txtPassword.Password = password;
            txtPasswordVisible.Text = password;

            txtPassword.Visibility = chkShowPassword.IsChecked == true ? Visibility.Collapsed : Visibility.Visible;
            txtPasswordVisible.Visibility = chkShowPassword.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        }

        private void RecuperarContrasena_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new RecuperarContrasenaWindow();
            ventana.ShowDialog();
        }

        private void ActualizarAvisoCapsLock() { avisoCapsLock.Visibility = Console.CapsLock ? Visibility.Visible : Visibility.Collapsed; }

        private void PasswordBox_FocusChanged(object sender, RoutedEventArgs e)
        {
            var control = sender as FrameworkElement;
            if (control.IsKeyboardFocusWithin) ActualizarAvisoCapsLock();
            else avisoCapsLock.Visibility = Visibility.Collapsed;
        }

        private void PasswordBox_KeyDown_CapsLock(object sender, KeyEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(ActualizarAvisoCapsLock), System.Windows.Threading.DispatcherPriority.Input);
        }
    }
}