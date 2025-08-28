using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Data;
using TiendaGlobosLaFiesta.Models;
using TiendaGlobosLaFiesta.Data;

namespace TiendaGlobosLaFiesta
{
    public partial class VentasControl : UserControl
    {
        private ObservableCollection<ProductoVenta> productos;
        private ObservableCollection<GloboVenta> globos;
        private ObservableCollection<Cliente> clientes;

        public VentasControl()
        {
            InitializeComponent();
            CargarClientes();
            CargarProductos();
            CargarGlobos();
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
                    Nombre = $"{r["primerNombre"]} {r["segundoNombre"]} {r["apellidoP"]} {r["apellidoM"]}".Trim()
                }).ToObservableCollection();

            cmbClientes.ItemsSource = clientes;
            cmbClientes.DisplayMemberPath = "Nombre";
            cmbClientes.SelectedValuePath = "ClienteId";
        }

        private void CargarProductos()
        {
            productos = ConexionBD.EjecutarConsulta(
                "SELECT productoId, nombre, unidad, stock, costo FROM Producto") // <--- Unidad incluida
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

            string ventaId = "V" + DateTime.Now.ToString("yyyyMMddHHmmss");
            decimal total = productos.Sum(p => p.Importe) + globos.Sum(g => g.Importe);

            using (var conn = ConexionBD.ObtenerConexion())
            {
                var tran = conn.BeginTransaction();
                try
                {
                    // Insertar Venta
                    ConexionBD.EjecutarNonQuery(
                        "INSERT INTO Venta (ventaId, empleadoId, clienteId, fechaVenta, importeTotal) VALUES (@ventaId,@empleadoId,@clienteId,@fecha,@total)",
                        new[]
                        {
                            ConexionBD.Param("@ventaId", ventaId),
                            ConexionBD.Param("@empleadoId", SesionActual.EmpleadoId),
                            ConexionBD.Param("@clienteId", cliente.ClienteId),
                            ConexionBD.Param("@fecha", DateTime.Now),
                            ConexionBD.Param("@total", total)
                        }, conn, tran);

                    // Detalles Productos
                    foreach (var p in productos.Where(x => x.Cantidad > 0))
                    {
                        ConexionBD.EjecutarNonQuery(
                            "INSERT INTO Detalle_Venta_Producto (ventaId, productoId, cantidad, costo, importe) VALUES (@ventaId,@prodId,@cant,@costo,@importe)",
                            new[]
                            {
                                ConexionBD.Param("@ventaId", ventaId),
                                ConexionBD.Param("@prodId", p.ProductoId),
                                ConexionBD.Param("@cant", p.Cantidad),
                                ConexionBD.Param("@costo", p.Costo),
                                ConexionBD.Param("@importe", p.Importe)
                            }, conn, tran);

                        ConexionBD.EjecutarNonQuery(
                            "UPDATE Producto SET stock = stock - @cant WHERE productoId = @prodId",
                            new[]
                            {
                                ConexionBD.Param("@cant", p.Cantidad),
                                ConexionBD.Param("@prodId", p.ProductoId)
                            }, conn, tran);
                    }

                    // Detalles Globos
                    foreach (var g in globos.Where(x => x.Cantidad > 0))
                    {
                        ConexionBD.EjecutarNonQuery(
                            "INSERT INTO Detalle_Venta_Globo (ventaId, globoId, cantidad, costo, importe) VALUES (@ventaId,@globoId,@cant,@costo,@importe)",
                            new[]
                            {
                                ConexionBD.Param("@ventaId", ventaId),
                                ConexionBD.Param("@globoId", g.GloboId),
                                ConexionBD.Param("@cant", g.Cantidad),
                                ConexionBD.Param("@costo", g.Costo),
                                ConexionBD.Param("@importe", g.Importe)
                            }, conn, tran);

                        ConexionBD.EjecutarNonQuery(
                            "UPDATE Globo SET stock = stock - @cant WHERE globoId = @globoId",
                            new[]
                            {
                                ConexionBD.Param("@cant", g.Cantidad),
                                ConexionBD.Param("@globoId", g.GloboId)
                            }, conn, tran);
                    }

                    tran.Commit();
                    MessageBox.Show("Venta registrada correctamente.");

                    foreach (var p in productos) p.Cantidad = 0;
                    foreach (var g in globos) g.Cantidad = 0;
                    ActualizarResumen();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    MessageBox.Show("Error al registrar la venta: " + ex.Message);
                }
            }
        }
    }

    public static class Extensiones
    {
        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> enumerable)
        {
            return new ObservableCollection<T>(enumerable);
        }
    }
}
