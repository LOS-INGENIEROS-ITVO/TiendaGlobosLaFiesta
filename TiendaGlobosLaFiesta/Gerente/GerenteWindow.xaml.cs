using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using TiendaGlobosLaFiesta.Managers;
using TiendaGlobosLaFiesta.Models.Utilities;
using TiendaGlobosLaFiesta.Views;

namespace TiendaGlobosLaFiesta
{
    public partial class MenuGerenteWindow : Window
    {
        private readonly string RolUsuario;
        private readonly Dictionary<string, UserControl> Modulos = new();

        public MenuGerenteWindow(string rol)
        {
            InitializeComponent();
            RolUsuario = rol;

            txtBienvenida.Text = $"Bienvenido, {SesionActual.NombreEmpleadoCompleto}";

            ConfigurarPermisos(rol);
            CargarDashboard();
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

        private void MostrarModulo(UserControl control)
        {
            MainContent.Content = control;
            imgFondo.Visibility = Visibility.Collapsed;
        }

        // BOTONES DEL MENÚ
        private void BtnDashboard_Click(object sender, RoutedEventArgs e)
        {
            AbrirModulo("Dashboard", () => new DashboardGerenteControl());
        }

        private void BtnVentas_Click(object sender, RoutedEventArgs e)
        {
            AbrirModulo("Ventas", () =>
            {
                var ventasControl = new VentasControl();
                ModuloManager.Instancia.RegistrarModulo("Ventas", ventasControl);

                // Suscribirse solo si Dashboard existe
                if (Modulos.TryGetValue("Dashboard", out var dashboardObj) &&
                    dashboardObj is DashboardGerenteControl dashboardControl)
                {
                    ModuloManager.Instancia.VentaRegistrada -= dashboardControl.RefrescarKPIs;
                    ModuloManager.Instancia.VentaRegistrada += dashboardControl.RefrescarKPIs;
                }

                return ventasControl;
            });
        }

        private void BtnInventario_Click(object sender, RoutedEventArgs e)
        {
            AbrirModulo("Inventario", () => new InventarioControl());
        }

        private void BtnClientes_Click(object sender, RoutedEventArgs e)
        {
            AbrirModulo("Clientes", () => new ClientesControl());
        }

        private void BtnPedidos_Click(object sender, RoutedEventArgs e)
        {
            AbrirModulo("Pedidos", () => new PedidosControl());
        }

        private void BtnReportes_Click(object sender, RoutedEventArgs e)
        {
            AbrirModulo("Reportes", () => new ReportesControl());
        }

        private void BtnCerrarSesion_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow login = new LoginWindow();
            login.Show();
            this.Close();
        }

        private void CargarDashboard()
        {
            AbrirModulo("Dashboard", () => new DashboardGerenteControl());
        }

        // Método genérico para abrir módulos
        private void AbrirModulo(string nombre, System.Func<UserControl> crearModulo)
        {
            if (!Modulos.ContainsKey(nombre))
            {
                var modulo = crearModulo();
                Modulos[nombre] = modulo;
            }

            MostrarModulo(Modulos[nombre]);
        }
    }
}