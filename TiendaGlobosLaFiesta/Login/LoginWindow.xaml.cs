using System;
using System.Data.SqlClient;
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

        public LoginWindow()
        {
            InitializeComponent();

            // Registrar evento solo una vez
            txtPasswordVisible.TextChanged += TxtPasswordVisible_TextChanged;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtUsername.Focus();

            // Cargar usuario guardado si existe
            if (!string.IsNullOrEmpty(Properties.Settings.Default.UsuarioGuardado))
            {
                txtUsername.Text = Properties.Settings.Default.UsuarioGuardado;
                txtPassword.Focus();
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
                txtMensaje.Text = "Has excedido el número máximo de intentos. Intenta más tarde.";
                progressLogin.Visibility = Visibility.Collapsed;
                return;
            }

            // Ejecutar login en un hilo separado
            bool loginExitoso = await Task.Run(() => ValidarLogin(username, password));

            progressLogin.Visibility = Visibility.Collapsed;

            if (loginExitoso)
            {
                // Guardar usuario para la próxima vez
                Properties.Settings.Default.UsuarioGuardado = username;
                Properties.Settings.Default.Save();

                MenuGerenteWindow gerenteWindow = new MenuGerenteWindow(SesionActual.Rol);
                gerenteWindow.Show();
                this.Close();
            }
            else
            {
                intentosFallidos++;
                txtMensaje.Text = $"Usuario o contraseña incorrectos. Intento {intentosFallidos}/{MAX_INTENTOS}";
            }
        }

        private bool ValidarLogin(string username, string password)
        {
            try
            {
                using (SqlConnection conn = ConexionBD.ObtenerConexion())
                {
                    string query = @"
                        SELECT u.usuarioId, u.empleadoId, e.puestoId 
                        FROM Usuarios u
                        JOIN Empleado e ON u.empleadoId = e.empleadoId
                        WHERE u.username=@username AND u.passwordHash=@password AND u.activo=1";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        cmd.Parameters.AddWithValue("@password", password);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                SesionActual.UsuarioId = Convert.ToInt32(reader["usuarioId"]);
                                SesionActual.EmpleadoId = Convert.ToInt32(reader["empleadoId"]);
                                SesionActual.Rol = reader["puestoId"]?.ToString() ?? "";
                                SesionActual.Username = username;
                                return true;
                            }
                        }
                    }
                }
            }
            catch
            {
                Dispatcher.Invoke(() => txtMensaje.Text = "Error de conexión a la base de datos.");
            }

            return false;
        }
    }
}