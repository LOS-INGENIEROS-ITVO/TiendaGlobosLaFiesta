using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Data;
using System.Collections.Generic;
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
            RefrescarDatos();
        }

        /// <summary>
        /// Refresca clientes, productos, globos y historial
        /// </summary>
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
                    Nombre = $"{r["primerNombre"]} {r["segundoNombre"]} {r["apellidoP"]} {r["apellidoM"]}".Trim()
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
                    v.ventaId, 
                    c.primerNombre + ' ' + ISNULL(c.segundoNombre,'') + ' ' + c.apellidoP + ' ' + c.apellidoM AS Cliente,
                    e.primerNombre + ' ' + ISNULL(e.segundoNombre,'') + ' ' + e.apellidoP + ' ' + e.apellidoM AS Empleado,
                    v.fechaVenta AS Fecha,
                    v.importeTotal AS Total
                FROM Venta v
                INNER JOIN Cliente c ON v.clienteId = c.clienteId
                INNER JOIN Empleado e ON v.empleadoId = e.empleadoId
                ORDER BY v.fechaVenta DESC";

            var dt = ConexionBD.EjecutarConsulta(query);
            dgHistorial.ItemsSource = dt.DefaultView;
        }

        private void BtnFiltrarHistorial_Click(object sender, RoutedEventArgs e)
        {
            string clienteId = (cmbFiltroCliente.SelectedItem as Cliente)?.ClienteId;
            DateTime? fechaDesde = dpFechaDesde.SelectedDate;
            DateTime? fechaHasta = dpFechaHasta.SelectedDate;

            var parametros = new List<System.Data.SqlClient.SqlParameter>();
            string query = @"
                SELECT 
                    v.ventaId, 
                    c.primerNombre + ' ' + ISNULL(c.segundoNombre,'') + ' ' + c.apellidoP + ' ' + c.apellidoM AS Cliente,
                    e.primerNombre + ' ' + ISNULL(e.segundoNombre,'') + ' ' + e.apellidoP + ' ' + e.apellidoM AS Empleado,
                    v.fechaVenta AS Fecha,
                    v.importeTotal AS Total
                FROM Venta v
                INNER JOIN Cliente c ON v.clienteId = c.clienteId
                INNER JOIN Empleado e ON v.empleadoId = e.empleadoId
                WHERE 1=1";

            if (!string.IsNullOrEmpty(clienteId))
            {
                query += " AND v.clienteId = @clienteId";
                parametros.Add(ConexionBD.Param("@clienteId", clienteId));
            }

            if (fechaDesde.HasValue)
            {
                query += " AND v.fechaVenta >= @fechaDesde";
                parametros.Add(ConexionBD.Param("@fechaDesde", fechaDesde.Value));
            }

            if (fechaHasta.HasValue)
            {
                query += " AND v.fechaVenta <= @fechaHasta";
                parametros.Add(ConexionBD.Param("@fechaHasta", fechaHasta.Value));
            }

            query += " ORDER BY v.fechaVenta DESC";

            dgHistorial.ItemsSource = ConexionBD.EjecutarConsulta(query, parametros.ToArray()).DefaultView;
        }

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e) => ActualizarResumen();

        private void ActualizarResumen()
        {
            txtTotalProductos.Text = productos.Sum(p => p.Cantidad).ToString();
            txtTotalGlobos.Text = globos.Sum(g => g.Cantidad).ToString();
            decimal total = productos.Sum(p => p.Cantidad * p.Costo) + globos.Sum(g => g.Cantidad * g.Costo);
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

            string ventaId = "V" + DateTime.Now.ToString("yyyyMMddHHmmss");
            decimal total = productos.Sum(p => p.Cantidad * p.Costo) + globos.Sum(g => g.Cantidad * g.Costo);

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
                                ConexionBD.Param("@importe", p.Cantidad * p.Costo)
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
                                ConexionBD.Param("@importe", g.Cantidad * g.Costo)
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

                    // Reiniciar cantidades y refrescar
                    foreach (var p in productos) p.Cantidad = 0;
                    foreach (var g in globos) g.Cantidad = 0;
                    RefrescarDatos();
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