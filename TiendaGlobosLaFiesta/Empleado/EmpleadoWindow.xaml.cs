using System.Windows;
using System.Windows.Media.Imaging;
using TiendaGlobosLaFiesta.Views;


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
            try
            {
                // Crea el BitmapImage de manera segura
                BitmapImage imagen = new BitmapImage();
                imagen.BeginInit();

                // Ruta relativa al recurso dentro del proyecto
                imagen.UriSource = new Uri("pack://application:,,,/TiendaGlobosLaFiesta;component/Recursos/Fondo3.png", UriKind.Absolute);

                imagen.CacheOption = BitmapCacheOption.OnLoad; // Mejora el rendimiento
                imagen.EndInit();
                imagen.Freeze(); // Para que sea seguro usar desde otros hilos

                imgFondo.Source = imagen;
            }
            catch (Exception ex)
            {
                // Opcional: log o mensaje de error sin romper la app
                Console.WriteLine("No se pudo cargar la imagen de fondo: " + ex.Message);

            }
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