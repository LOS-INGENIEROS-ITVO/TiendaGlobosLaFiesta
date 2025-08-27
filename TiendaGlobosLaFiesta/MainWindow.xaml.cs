using System.Windows;

namespace TiendaGlobosLaFiesta
{
    public partial class MainWindow : Window
    {
        private string RolUsuario;

        public MainWindow()
        {
            InitializeComponent();
        }

        public MainWindow(string rol) : this()
        {
            RolUsuario = rol;
            lblWelcome.Text = $"Bienvenido - Rol: {rol}";
            ConfigurarPermisos(rol);
        }

        private void ConfigurarPermisos(string rol)
        {
            if (rol == "Empleado")
            {
                btnInventario.IsEnabled = false;
                btnPedidos.IsEnabled = false;
                btnReportes.IsEnabled = false;
            }
            else if (rol == "Gerente")
            {
                btnInventario.IsEnabled = true;
                btnPedidos.IsEnabled = true;
                btnReportes.IsEnabled = true;
            }
        }

        private void btnVentas_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Abrir módulo de Ventas...");
        }

        private void btnInventario_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Abrir módulo de Inventario...");
        }

        private void btnClientes_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Abrir módulo de Clientes...");
        }

        private void btnPedidos_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Abrir módulo de Pedidos...");
        }

        private void btnReportes_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Abrir módulo de Reportes...");
        }
    }
}