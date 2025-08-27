using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace TiendaGlobosLaFiesta
{
    public partial class MainWindow : Window
    {
        private string RolUsuario;

        public MainWindow(string rol)
        {
            InitializeComponent();
            RolUsuario = rol;
            ConfigurarPermisos(rol);
            CargarImagenPorDefecto();
        }

        private void ConfigurarPermisos(string rol)
        {
            if (rol == "Empleado")
            {
                btnInventario.IsEnabled = false;
                btnPedidos.IsEnabled = false;
                btnReportes.IsEnabled = false;
            }
        }

        private void CargarImagenPorDefecto()
        {
            BitmapImage imagen = new BitmapImage();
            imagen.BeginInit();
            imagen.UriSource = new System.Uri("pack://application:,,,/TiendaGlobosLaFiesta;component/Assets/FondoGerente.png"); // Cambia según tu imagen
            imagen.EndInit();
            imgFondo.Source = imagen;
        }

        private void MostrarModulo(UserControl control)
        {
            MainContent.Content = control;
            imgFondo.Visibility = Visibility.Collapsed; // Oculta la imagen de fondo al mostrar un módulo
        }

        private void BtnVentas_Click(object sender, RoutedEventArgs e)
        {
            MostrarModulo(new VentasControl());
        }

        private void BtnInventario_Click(object sender, RoutedEventArgs e)
        {
            MostrarModulo(new InventarioControl());
        }

        private void BtnClientes_Click(object sender, RoutedEventArgs e)
        {
            MostrarModulo(new ClientesControl());
        }

        private void BtnPedidos_Click(object sender, RoutedEventArgs e)
        {
            MostrarModulo(new PedidosControl());
        }

        private void BtnReportes_Click(object sender, RoutedEventArgs e)
        {
            MostrarModulo(new ReportesControl());
        }

        private void BtnCerrarSesion_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow login = new LoginWindow();
            login.Show();
            this.Close();
        }
    }
}