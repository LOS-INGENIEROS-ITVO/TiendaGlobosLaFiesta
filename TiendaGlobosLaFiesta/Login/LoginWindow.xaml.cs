using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TiendaGlobosLaFiesta.Data;

namespace TiendaGlobosLaFiesta
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void TxtUsername_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                txtPassword.Focus();
        }

        private void TxtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                BtnLogin_Click(this, new RoutedEventArgs());
        }

        private void chkShowPassword_Checked(object sender, RoutedEventArgs e)
        {
            txtPasswordVisible.Text = txtPassword.Password;
            txtPasswordVisible.Visibility = Visibility.Visible;
            txtPassword.Visibility = Visibility.Collapsed;
            txtPasswordVisible.TextChanged += TxtPasswordVisible_TextChanged;
        }

        private void chkShowPassword_Unchecked(object sender, RoutedEventArgs e)
        {
            txtPassword.Password = txtPasswordVisible.Text;
            txtPassword.Visibility = Visibility.Visible;
            txtPasswordVisible.Visibility = Visibility.Collapsed;
            txtPasswordVisible.TextChanged -= TxtPasswordVisible_TextChanged;
        }

        private void TxtPasswordVisible_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtPassword.Password = txtPasswordVisible.Text;
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text;
            string password = chkShowPassword.IsChecked == true ? txtPasswordVisible.Text : txtPassword.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Por favor, ingresa usuario y contraseña.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (SqlConnection conn = ConexionBD.ObtenerConexion()) // ✅ Usamos ConexionBD
                {
                    string query = @"
                        SELECT u.usuarioId, u.empleadoId, e.puestoId 
                        FROM Usuarios u
                        JOIN Empleado e ON u.empleadoId = e.empleadoId
                        WHERE u.username=@username AND u.passwordHash=@password AND u.activo=1";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@password", password); // ⚠ En producción, usar hash seguro

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            SesionActual.UsuarioId = Convert.ToInt32(reader["usuarioId"]);
                            SesionActual.EmpleadoId = Convert.ToInt32(reader["empleadoId"]);
                            SesionActual.Rol = reader["puestoId"]?.ToString() ?? "";
                            SesionActual.Username = username;

                            MenuGerenteWindow GerenteWindow = new MenuGerenteWindow(SesionActual.Rol);
                            GerenteWindow.Show();
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("Usuario o contraseña incorrectos.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Error de conexión a la base de datos: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}