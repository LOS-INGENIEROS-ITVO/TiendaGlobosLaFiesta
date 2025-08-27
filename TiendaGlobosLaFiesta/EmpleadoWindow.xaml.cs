using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace TiendaGlobosLaFiesta
{
    public partial class EmpleadoWindow : Window
    {
        private string RolUsuario;

        public EmpleadoWindow(string rol)
        {
            InitializeComponent();
            RolUsuario = rol;
            CargarImagenPorDefecto();
        }

        // Cargar imagen de fondo predeterminada
        private void CargarImagenPorDefecto()
        {
            BitmapImage imagen = new BitmapImage();
            imagen.BeginInit();
            imagen.UriSource = new System.Uri("pack://application:,,,/TiendaGlobosLaFiesta;component/Assets/Fondo3.png");
            imagen.EndInit();
            imgFondo.Source = imagen;
        }

        // Cargar módulo de Ventas en el panel dinámico
        private void btnVentas_Click(object sender, RoutedEventArgs e)
        {
            VentasControl ventas = new VentasControl();
            PanelDinamico.Children.Clear();
            PanelDinamico.Children.Add(ventas);
        }

        // Regresar a login
        private void btnRegresar_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow login = new LoginWindow();
            login.Show();
            this.Close();
        }
    }
}