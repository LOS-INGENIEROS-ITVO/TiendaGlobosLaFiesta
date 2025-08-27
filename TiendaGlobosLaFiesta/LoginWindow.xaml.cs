using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TiendaGlobosLaFiesta
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        // ENTER en usuario -> mueve al password
        private void TxtUsername_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                txtPassword.Focus();
        }

        // ENTER en password -> ejecuta login
        private void TxtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                BtnLogin_Click(this, new RoutedEventArgs());
        }

        // Mostrar contraseña
        private void chkShowPassword_Checked(object sender, RoutedEventArgs e)
        {
            txtPasswordVisible.Text = txtPassword.Password;
            txtPasswordVisible.Visibility = Visibility.Visible;
            txtPassword.Visibility = Visibility.Collapsed;
            txtPasswordVisible.TextChanged += TxtPasswordVisible_TextChanged;
        }

        // Ocultar contraseña
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


        // BOTÓN LOGIN
        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text;
            string password = chkShowPassword.IsChecked == true ? txtPasswordVisible.Text : txtPassword.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Por favor, ingresa usuario y contraseña.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Cadena de conexión a tu SQL Server Express
            string connectionString = @"Data Source=LALOVG25\SQLEXPRESS;Initial Catalog=Globeriadb;Integrated Security=True;";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    string query = @"
                        SELECT u.usuarioId, e.puestoId 
                        FROM Usuarios u
                        JOIN Empleado e ON u.empleadoId = e.empleadoId
                        WHERE u.username=@username AND u.passwordHash=@password AND u.activo=1";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@password", password); // En producción usa hash

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string rol = reader["puestoId"]?.ToString() ?? "";

                            // Abrir ventana según rol
                            if (rol == "Gerente")
                            {
                                MainWindow mainWindow = new MainWindow(rol);
                                mainWindow.Show();
                            }
                            else if (rol == "Empleado")
                            {
                                EmpleadoWindow empleadoWindow = new EmpleadoWindow(rol);
                                empleadoWindow.Show();
                            }

                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("Usuario o contraseña incorrectos.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
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
}