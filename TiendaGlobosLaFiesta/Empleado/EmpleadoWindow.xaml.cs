using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using TiendaGlobosLaFiesta.Models.Utilities;
using TiendaGlobosLaFiesta.Views;

namespace TiendaGlobosLaFiesta
{
    public partial class EmpleadoWindow : Window
    {
        // Usamos el mismo patrón de diccionario para cargar y reutilizar los módulos
        private Dictionary<string, UserControl> Modulos = new Dictionary<string, UserControl>();

        public EmpleadoWindow(string rol)
        {
            InitializeComponent();

            // Personalizamos la bienvenida con el nombre del empleado logueado
            txtBienvenida.Text = $"Bienvenido, {SesionActual.NombreEmpleadoCompleto}";

            // Cargamos el dashboard del empleado por defecto al iniciar
            CargarDashboard();
        }

        // Método reutilizado para mostrar el módulo seleccionado en el ContentControl
        private void MostrarModulo(UserControl control)
        {
            MainContent.Content = control;
        }

        // Carga inicial del Dashboard
        private void CargarDashboard()
        {
            // Asumimos que crearás un UserControl "DashboardEmpleadoControl"
            if (!Modulos.ContainsKey("Dashboard"))
                Modulos["Dashboard"] = new DashboardEmpleadoControl();
            MostrarModulo(Modulos["Dashboard"]);
        }

        // --- MANEJADORES DE CLIC PARA LOS BOTONES ---

        private void BtnDashboard_Click(object sender, RoutedEventArgs e)
        {
            if (!Modulos.ContainsKey("Dashboard"))
                Modulos["Dashboard"] = new DashboardEmpleadoControl();
            MostrarModulo(Modulos["Dashboard"]);
        }

        private void BtnVentas_Click(object sender, RoutedEventArgs e)
        {
            if (!Modulos.ContainsKey("Ventas"))
                Modulos["Ventas"] = new VentasControl();
            MostrarModulo(Modulos["Ventas"]);
        }

        private void BtnClientes_Click(object sender, RoutedEventArgs e)
        {
            if (!Modulos.ContainsKey("Clientes"))
                Modulos["Clientes"] = new ClientesControl();
            MostrarModulo(Modulos["Clientes"]);
        }

        private void BtnCerrarSesion_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow login = new LoginWindow();
            login.Show();
            this.Close();
        }
    }
}