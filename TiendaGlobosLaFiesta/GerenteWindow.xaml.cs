using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using TiendaGlobosLaFiesta.Data;

namespace TiendaGlobosLaFiesta
{
    public partial class MenuGerenteWindow : Window
    {
        private string RolUsuario;
        private Dictionary<string, UserControl> Modulos = new Dictionary<string, UserControl>();

        public MenuGerenteWindow(string rol)
        {
            InitializeComponent();
            RolUsuario = rol;

            txtBienvenida.Text = $"Bienvenido, {SesionActual.Username}";

            ConfigurarPermisos(rol);
            CargarImagenPorDefecto();
        }

        private void ConfigurarPermisos(string rol)
        {
            if (rol == "Empleado")
            {
                DeshabilitarBoton(btnInventario);
                DeshabilitarBoton(btnPedidos);
                DeshabilitarBoton(btnReportes);
            }
        }

        private void DeshabilitarBoton(Button boton)
        {
            boton.Style = (Style)FindResource("DisabledMenuButtonStyle");
            boton.IsEnabled = false;
        }

        private void CargarImagenPorDefecto()
        {
            BitmapImage imagen = new BitmapImage();
            imagen.BeginInit();
            imagen.UriSource = new System.Uri("pack://application:,,,/TiendaGlobosLaFiesta;component/Recursos/FondoGerente.png");
            imagen.EndInit();
            imgFondo.Source = imagen;
        }

        private void MostrarModulo(UserControl control)
        {
            MainContent.Content = control;
            imgFondo.Visibility = Visibility.Collapsed;
        }

        // BOTONES DEL MENÚ
        private void BtnVentas_Click(object sender, RoutedEventArgs e)
        {
            if (!Modulos.ContainsKey("Ventas"))
                Modulos["Ventas"] = new VentasControl();
            MostrarModulo(Modulos["Ventas"]);
        }

        private void BtnInventario_Click(object sender, RoutedEventArgs e)
        {
            if (!Modulos.ContainsKey("Inventario"))
                Modulos["Inventario"] = new InventarioControl();
            MostrarModulo(Modulos["Inventario"]);
        }

        private void BtnClientes_Click(object sender, RoutedEventArgs e)
        {
            if (!Modulos.ContainsKey("Clientes"))
                Modulos["Clientes"] = new ClientesControl();
            MostrarModulo(Modulos["Clientes"]);
        }

        private void BtnPedidos_Click(object sender, RoutedEventArgs e)
        {
            if (!Modulos.ContainsKey("Pedidos"))
                Modulos["Pedidos"] = new PedidosControl();
            MostrarModulo(Modulos["Pedidos"]);
        }

        private void BtnReportes_Click(object sender, RoutedEventArgs e)
        {
            if (!Modulos.ContainsKey("Reportes"))
                Modulos["Reportes"] = new ReportesControl();
            MostrarModulo(Modulos["Reportes"]);
        }

        private void BtnCerrarSesion_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow login = new LoginWindow();
            login.Show();
            this.Close();
        }
    }
}