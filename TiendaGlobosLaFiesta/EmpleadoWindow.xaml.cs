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

        private void CargarImagenPorDefecto()
        {
            BitmapImage imagen = new BitmapImage();
            imagen.BeginInit();
            imagen.UriSource = new System.Uri("pack://application:,,,/TiendaGlobosLaFiesta;component/Assets/Fondo3.png");
            imagen.EndInit();
            imgFondo.Source = imagen;
        }

        private void btnVentas_Click(object sender, RoutedEventArgs e)
        {
            // Aquí puedes cargar un UserControl en PanelDinamico si quieres, por ahora mensaje
            MessageBox.Show("Abrir módulo de Ventas para empleado...");
        }

        private void btnRegresar_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow login = new LoginWindow();
            login.Show();
            this.Close();
        }
    }
}