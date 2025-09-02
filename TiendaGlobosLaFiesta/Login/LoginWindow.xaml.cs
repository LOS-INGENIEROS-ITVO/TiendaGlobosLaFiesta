using System;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TiendaGlobosLaFiesta.Data;

namespace TiendaGlobosLaFiesta
{
    public partial class LoginWindow : Window
    {
        private int intentosFallidos = 0;
        private const int MAX_INTENTOS = 5;
        private const int BLOQUEO_SEGUNDOS = 30;

        public LoginWindow()
        {
            InitializeComponent();
            txtPasswordVisible.TextChanged += TxtPasswordVisible_TextChanged;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtUsername.Focus();

            if (!string.IsNullOrEmpty(Properties.Settings.Default.UsuarioGuardado))
            {
                txtUsername.Text = Properties.Settings.Default.UsuarioGuardado;
                txtPassword.Focus();
                chkRemember.IsChecked = true;
            }
        }

        private void TxtUsername_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                txtPassword.Focus();
        }

        private void TxtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                BtnLogin_Click(sender, new RoutedEventArgs());
        }

        private void chkShowPassword_Checked(object sender, RoutedEventArgs e)
        {
            txtPasswordVisible.Text = txtPassword.Password;
            txtPasswordVisible.Visibility = Visibility.Visible;
            txtPassword.Visibility = Visibility.Collapsed;
        }

        private void chkShowPassword_Unchecked(object sender, RoutedEventArgs e)
        {
            txtPassword.Password = txtPasswordVisible.Text;
            txtPassword.Visibility = Visibility.Visible;
            txtPasswordVisible.Visibility = Visibility.Collapsed;
        }

        private void TxtPasswordVisible_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtPassword.Password = txtPasswordVisible.Text;
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = chkShowPassword.IsChecked == true ? txtPasswordVisible.Text : txtPassword.Password;

            txtMensaje.Text = "";
            progressLogin.Visibility = Visibility.Visible;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                txtMensaje.Text = "Por favor, ingresa usuario y contraseña.";
                progressLogin.Visibility = Visibility.Collapsed;
                return;
            }

            if (intentosFallidos >= MAX_INTENTOS)
            {
                txtMensaje.Text = $"Has excedido {MAX_INTENTOS} intentos. Intenta de nuevo en {BLOQUEO_SEGUNDOS} segundos.";
                await Task.Delay(BLOQUEO_SEGUNDOS * 1000);
                intentosFallidos = 0;
                txtMensaje.Text = "";
                progressLogin.Visibility = Visibility.Collapsed;
                return;
            }

            bool loginExitoso = await Task.Run(() => AuthService.ValidarLogin(username, password, out string rol));
            progressLogin.Visibility = Visibility.Collapsed;

            if (loginExitoso)
            {
                if (chkRemember.IsChecked == true)
                    Properties.Settings.Default.UsuarioGuardado = username;
                else
                    Properties.Settings.Default.UsuarioGuardado = string.Empty;

                Properties.Settings.Default.Save();

                if (SesionActual.Rol == "Gerente")
                    new MenuGerenteWindow(SesionActual.Rol).Show();
                else
                    new EmpleadoWindow(SesionActual.Rol).Show();

                this.Close();
            }
            else
            {
                intentosFallidos++;
                txtMensaje.Text = $"Usuario o contraseña incorrectos. Intento {intentosFallidos}/{MAX_INTENTOS}";
            }
        }
    }
}
