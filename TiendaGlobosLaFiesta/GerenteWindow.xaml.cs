using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TiendaGlobosLaFiesta.Data;

namespace TiendaGlobosLaFiesta
{
    public partial class MenuGerenteWindow : Window
    {
        private string RolUsuario;

        public MenuGerenteWindow(string rol)
        {
            InitializeComponent();

            RolUsuario = rol;

            // Mostrar nombre del usuario que inició sesión
            txtBienvenida.Text = $"Bienvenido, {SesionActual.Username}";

            ConfigurarPermisos(rol);
            CargarImagenPorDefecto();
        }

        /// <summary>
        /// Configura la disponibilidad y apariencia de botones según el rol del usuario
        /// </summary>
        /// <param name="rol">Rol del usuario (Empleado/Gerente)</param>
        private void ConfigurarPermisos(string rol)
        {
            // Si es empleado, desactivar ciertos botones y cambiar apariencia
            if (rol == "Empleado")
            {
                DeshabilitarBoton(btnInventario);
                DeshabilitarBoton(btnPedidos);
                DeshabilitarBoton(btnReportes);
            }
        }

        private void DeshabilitarBoton(Button boton)
        {
            boton.IsEnabled = false;
            boton.Foreground = Brushes.Gray;
            boton.Background = Brushes.LightGray;
            boton.Cursor = System.Windows.Input.Cursors.Arrow;
        }

        /// <summary>
        /// Carga la imagen de fondo por defecto para gerente
        /// </summary>
        private void CargarImagenPorDefecto()
        {
            BitmapImage imagen = new BitmapImage();
            imagen.BeginInit();
            imagen.UriSource = new System.Uri("pack://application:,,,/TiendaGlobosLaFiesta;component/Assets/FondoGerente.png");
            imagen.EndInit();
            imgFondo.Source = imagen;
        }

        /// <summary>
        /// Muestra un UserControl en el área principal y oculta la imagen de fondo
        /// </summary>
        /// <param name="control">UserControl a mostrar</param>
        private void MostrarModulo(UserControl control)
        {
            MainContent.Content = control;
            imgFondo.Visibility = Visibility.Collapsed;
        }

        // BOTONES DEL MENÚ
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

        /// <summary>
        /// Cierra sesión y vuelve a la ventana de login
        /// </summary>
        private void BtnCerrarSesion_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow login = new LoginWindow();
            login.Show();
            this.Close();
        }
    }
}