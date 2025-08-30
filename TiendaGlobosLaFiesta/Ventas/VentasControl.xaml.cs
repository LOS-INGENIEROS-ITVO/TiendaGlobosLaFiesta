using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using TiendaGlobosLaFiesta.Models;
using TiendaGlobosLaFiesta.Data;
using TiendaGlobosLaFiesta.Ventas;
using System.Data;

namespace TiendaGlobosLaFiesta
{
    public partial class VentasControl : UserControl
    {
        private ObservableCollection<ProductoVenta> productos;
        private ObservableCollection<GloboVenta> globos;
        private ObservableCollection<Cliente> clientes;
        private ObservableCollection<VentaHistorial> historialVentas;

        private VentaDAO ventaDAO = new VentaDAO();

        public VentasControl()
        {
            InitializeComponent();
            RefrescarDatos();
        }

        private void RefrescarDatos()
        {
            CargarClientes();
            CargarProductos();
            CargarGlobos();
            CargarHistorialVentas();
            ActualizarResumen();
        }

        private void CargarClientes()
        {
            clientes = ConexionBD.EjecutarConsulta(
                "SELECT clienteId, primerNombre, segundoNombre, apellidoP, apellidoM FROM Cliente")
                .AsEnumerable()
                .Select(r => new Cliente
                {
                    ClienteId = r["clienteId"].ToString(),
                    PrimerNombre = r["primerNombre"].ToString(),
                    SegundoNombre = r["segundoNombre"].ToString(),
                    ApellidoP = r["apellidoP"].ToString(),
                    ApellidoM = r["apellidoM"].ToString()
                    // Telefono si lo necesitas
                    // Telefono = int.TryParse(r["telefono"].ToString(), out int t) ? t : (int?)null
                }).ToObservableCollection();


            cmbClientes.ItemsSource = clientes;
            cmbClientes.DisplayMemberPath = "Nombre";
            cmbClientes.SelectedValuePath = "ClienteId";

            cmbFiltroCliente.ItemsSource = clientes;
            cmbFiltroCliente.DisplayMemberPath = "Nombre";
            cmbFiltroCliente.SelectedValuePath = "ClienteId";
        }

        private void CargarProductos()
        {
            productos = ConexionBD.EjecutarConsulta(
                "SELECT productoId, nombre, unidad, stock, costo FROM Producto")
                .AsEnumerable()
                .Select(r =>
                {
                    var p = new ProductoVenta
                    {
                        ProductoId = r["productoId"].ToString(),
                        Nombre = r["nombre"].ToString(),
                        Unidad = r["unidad"].ToString(),
                        Stock = Convert.ToInt32(r["stock"]),
                        Cantidad = 0,
                        Costo = Convert.ToDecimal(r["costo"])
                    };
                    p.PropertyChanged += Item_PropertyChanged;
                    return p;
                }).ToObservableCollection();

            dgProductos.ItemsSource = productos;
        }

        private void CargarGlobos()
        {
            string query = @"
                SELECT 
                    g.globoId, g.material, g.color, g.unidad, g.stock, g.costo,
                    ISNULL(STUFF((SELECT ', ' + gt.tamanio 
                                  FROM Globo_Tamanio gt 
                                  WHERE gt.globoId = g.globoId 
                                  FOR XML PATH('')), 1, 2, ''), '') AS Tamano,
                    ISNULL(STUFF((SELECT ', ' + gf.forma 
                                  FROM Globo_Forma gf 
                                  WHERE gf.globoId = g.globoId 
                                  FOR XML PATH('')), 1, 2, ''), '') AS Forma,
                    ISNULL(STUFF((SELECT ', ' + t.nombre 
                                  FROM Tematica t 
                                  WHERE t.globoId = g.globoId 
                                  FOR XML PATH('')), 1, 2, ''), '') AS Tematica
                FROM Globo g";

            globos = ConexionBD.EjecutarConsulta(query)
                .AsEnumerable()
                .Select(r =>
                {
                    var g = new GloboVenta
                    {
                        GloboId = r["globoId"].ToString(),
                        Material = r["material"].ToString(),
                        Color = r["color"].ToString(),
                        Unidad = r["unidad"].ToString(),
                        Stock = Convert.ToInt32(r["stock"]),
                        Costo = Convert.ToDecimal(r["costo"]),
                        Tamano = r["Tamano"].ToString(),
                        Forma = r["Forma"].ToString(),
                        Tematica = r["Tematica"].ToString(),
                        Cantidad = 0
                    };
                    g.PropertyChanged += Item_PropertyChanged;
                    return g;
                }).ToObservableCollection();

            dgGlobos.ItemsSource = globos;
        }

        private void CargarHistorialVentas()
        {
            string query = @"
                SELECT 
                    v.ventaId AS VentaId, 
                    c.primerNombre + ' ' + ISNULL(c.segundoNombre,'') + ' ' + c.apellidoP + ' ' + c.apellidoM AS Cliente,
                    e.primerNombre + ' ' + ISNULL(e.segundoNombre,'') + ' ' + e.apellidoP + ' ' + e.apellidoM AS Empleado,
                    v.fechaVenta AS Fecha,
                    v.importeTotal AS Total
                FROM Venta v
                INNER JOIN Cliente c ON v.clienteId = c.clienteId
                INNER JOIN Empleado e ON v.empleadoId = e.empleadoId
                ORDER BY v.fechaVenta DESC";

            historialVentas = ConexionBD.EjecutarConsulta(query)
                .AsEnumerable()
                .Select(r => new VentaHistorial
                {
                    VentaId = r["VentaId"].ToString(),
                    Cliente = r["Cliente"].ToString(),
                    Empleado = r["Empleado"].ToString(),
                    Fecha = Convert.ToDateTime(r["Fecha"]),
                    Total = Convert.ToDecimal(r["Total"])
                }).ToObservableCollection();

            dgHistorial.ItemsSource = historialVentas;
        }

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e) => ActualizarResumen();

        private void ActualizarResumen()
        {
            txtTotalProductos.Text = productos.Sum(p => p.Cantidad).ToString();
            txtTotalGlobos.Text = globos.Sum(g => g.Cantidad).ToString();
            decimal total = productos.Sum(p => p.Importe) + globos.Sum(g => g.Importe);
            txtImporteTotal.Text = total.ToString("0.00");
        }

        private void BtnAumentarCantidad_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                if (btn.Tag is ProductoVenta p && p.Cantidad < p.Stock) p.Cantidad++;
                else if (btn.Tag is GloboVenta g && g.Cantidad < g.Stock) g.Cantidad++;
            }
        }

        private void BtnDisminuirCantidad_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                if (btn.Tag is ProductoVenta p && p.Cantidad > 0) p.Cantidad--;
                else if (btn.Tag is GloboVenta g && g.Cantidad > 0) g.Cantidad--;
            }
        }


        private void BtnFiltrarHistorial_Click(object sender, RoutedEventArgs e)
        {
            string clienteId = (cmbFiltroCliente.SelectedItem as Cliente)?.ClienteId;
            DateTime? fechaDesde = dpFechaDesde.SelectedDate;
            DateTime? fechaHasta = dpFechaHasta.SelectedDate;

            historialVentas = ventaDAO.ObtenerHistorialFiltrado(clienteId, fechaDesde, fechaHasta);
            dgHistorial.ItemsSource = historialVentas;
        }


        private void BtnRegistrarVenta_Click(object sender, RoutedEventArgs e)
        {
            if (!(cmbClientes.SelectedItem is Cliente cliente))
            {
                MessageBox.Show("Selecciona un cliente.");
                return;
            }

            if (productos.Sum(p => p.Cantidad) == 0 && globos.Sum(g => g.Cantidad) == 0)
            {
                MessageBox.Show("Agrega productos o globos a la venta.");
                return;
            }

            // Validación de stock
            foreach (var p in productos.Where(x => x.Cantidad > 0))
            {
                if (p.Cantidad > p.Stock)
                {
                    MessageBox.Show($"No hay suficiente stock de {p.Nombre}.");
                    return;
                }
            }

            foreach (var g in globos.Where(x => x.Cantidad > 0))
            {
                if (g.Cantidad > g.Stock)
                {
                    MessageBox.Show($"No hay suficiente stock de {g.Material} {g.Color}.");
                    return;
                }
            }

            // Crear objeto Venta
            Venta venta = new Venta
            {
                VentaId = "V" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                EmpleadoId = SesionActual.EmpleadoId,
                ClienteId = cliente.ClienteId,
                FechaVenta = DateTime.Now,
                ImporteTotal = productos.Sum(p => p.Importe) + globos.Sum(g => g.Importe),
                Productos = new ObservableCollection<ProductoVenta>(productos.Where(p => p.Cantidad > 0)),
                Globos = new ObservableCollection<GloboVenta>(globos.Where(g => g.Cantidad > 0))
            };

            // Registrar con DAO
            if (ventaDAO.RegistrarVenta(venta))
            {
                MessageBox.Show("Venta registrada correctamente.");
                foreach (var p in productos) p.Cantidad = 0;
                foreach (var g in globos) g.Cantidad = 0;
                RefrescarDatos();
            }
            else
            {
                MessageBox.Show("Error al registrar la venta.");
            }
        }
    }
}