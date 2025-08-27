using System;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace TiendaGlobosLaFiesta
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void chkShowPassword_Checked(object sender, RoutedEventArgs e)
        {
            txtPassword.Visibility = Visibility.Hidden;
            var txtShow = new TextBox
            {
                Text = txtPassword.Password,
                Width = txtPassword.Width,
                Height = txtPassword.Height,
                Margin = txtPassword.Margin,
                VerticalAlignment = txtPassword.VerticalAlignment,
                Name = "txtShowPassword"
            };
            txtShow.TextChanged += (s, ev) => { txtPassword.Password = txtShow.Text; };
            (this.Content as Grid).Children.Add(txtShow);
        }

        private void chkShowPassword_Unchecked(object sender, RoutedEventArgs e)
        {
            txtPassword.Visibility = Visibility.Visible;
            var grid = this.Content as Grid;
            var txtShow = grid.Children.OfType<TextBox>().FirstOrDefault(t => t.Name == "txtShowPassword");
            if (txtShow != null) grid.Children.Remove(txtShow);
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text;
            string password = txtPassword.Password;

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
                    cmd.Parameters.AddWithValue("@password", password);

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
                catch (Exception ex)
                {
                    MessageBox.Show("Error de conexión: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}