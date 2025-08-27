using System.Windows;

namespace TiendaGlobosLaFiesta
{
    public partial class EmpleadoWindow : Window
    {
        private string RolUsuario;

        public EmpleadoWindow(string rol)
        {
            InitializeComponent();
            RolUsuario = rol;
            lblRol.Text = $"Bienvenido - Rol: {rol}";
        }

        private void btnVentas_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Abrir módulo de Ventas para empleado...");
        }
    }
}