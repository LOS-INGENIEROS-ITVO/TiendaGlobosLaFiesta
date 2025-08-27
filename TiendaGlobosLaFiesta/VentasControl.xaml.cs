using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TiendaGlobosLaFiesta.Models;
using TiendaGlobosLaFiesta.Models.TiendaGlobosLaFiesta.Models.TiendaGlobosLaFiesta.Models;

namespace TiendaGlobosLaFiesta
{
    public partial class VentasControl : UserControl
    {
        private BindingList<ProductoVenta> productos;
        private BindingList<GloboVenta> globos;
        private BindingList<Cliente> clientes;

        private string connStr = @"Data Source=LALOVG25\SQLEXPRESS;Initial Catalog=Globeriadb;Integrated Security=True;";

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
            clientes = new BindingList<Cliente>();
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT clienteId, primerNombre, apellidoP FROM Cliente", conn);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        clientes.Add(new Cliente
                        {
                            clienteId = reader["clienteId"].ToString(),
                            primerNombre = reader["primerNombre"].ToString(),
                            apellidoP = reader["apellidoP"].ToString()
                        });
                    }
                }
            }
            cmbClientes.ItemsSource = clientes;
            cmbClientes.DisplayMemberPath = "primerNombre";
            cmbClientes.SelectedValuePath = "clienteId";
        }

        private void CargarProductos()
        {
            productos = new BindingList<ProductoVenta>
            {
                new ProductoVenta { productoId="P001", nombre="Pegamento", stock=50, cantidad=0, costo=15.5 },
                new ProductoVenta { productoId="P002", nombre="Cinta", stock=30, cantidad=0, costo=10.0 }
            };
            foreach (var p in productos) p.PropertyChanged += Item_PropertyChanged;
            dgProductos.ItemsSource = productos;
        }

        private void CargarGlobos()
        {
            globos = new BindingList<GloboVenta>
            {
                new GloboVenta { globoId="G001", material="Latex", color="Rojo", cantidad=0, costo=5.0 },
                new GloboVenta { globoId="G002", material="Metalicos", color="Azul", cantidad=0, costo=8.0 }
            };
            foreach (var g in globos) g.PropertyChanged += Item_PropertyChanged;
            dgGlobos.ItemsSource = globos;
        }

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e) => ActualizarResumen();

        private void ActualizarResumen()
        {
            txtTotalProductos.Text = productos.Sum(p => p.cantidad).ToString();
            txtTotalGlobos.Text = globos.Sum(g => g.cantidad).ToString();
            double total = productos.Sum(p => p.cantidad * p.costo) + globos.Sum(g => g.cantidad * g.costo);
            txtImporteTotal.Text = total.ToString("0.00");
        }

        private void BtnRegistrarVenta_Click(object sender, RoutedEventArgs e)
        {
            if (!(cmbClientes.SelectedItem is Cliente cliente))
            {
                MessageBox.Show("Selecciona un cliente.");
                return;
            }

            if (productos.Sum(p => p.cantidad) == 0 && globos.Sum(g => g.cantidad) == 0)
            {
                MessageBox.Show("Agrega productos o globos a la venta.");
                return;
            }

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                try
                {
                    string ventaId = "V" + DateTime.Now.ToString("yyyyMMddHHmmss");

                    SqlCommand cmdVenta = new SqlCommand(
                        "INSERT INTO Venta (ventaId, empleadoId, clienteId, fechaVenta, importeTotal) VALUES (@ventaId,@empleadoId,@clienteId,@fecha,@total)", conn, tran);
                    cmdVenta.Parameters.AddWithValue("@ventaId", ventaId);
                    cmdVenta.Parameters.AddWithValue("@empleadoId", 1); // Temporal
                    cmdVenta.Parameters.AddWithValue("@clienteId", cliente.clienteId);
                    cmdVenta.Parameters.AddWithValue("@fecha", DateTime.Now);
                    double total = productos.Sum(p => p.cantidad * p.costo) + globos.Sum(g => g.cantidad * g.costo);
                    cmdVenta.Parameters.AddWithValue("@total", total);
                    cmdVenta.ExecuteNonQuery();

                    foreach (var p in productos.Where(x => x.cantidad > 0))
                    {
                        SqlCommand cmdDet = new SqlCommand(
                            "INSERT INTO Detalle_Venta_Producto (ventaId, productoId, cantidad, costo, importe) VALUES (@ventaId,@prodId,@cant,@costo,@importe)", conn, tran);
                        cmdDet.Parameters.AddWithValue("@ventaId", ventaId);
                        cmdDet.Parameters.AddWithValue("@prodId", p.productoId);
                        cmdDet.Parameters.AddWithValue("@cant", p.cantidad);
                        cmdDet.Parameters.AddWithValue("@costo", p.costo);
                        cmdDet.Parameters.AddWithValue("@importe", p.cantidad * p.costo);
                        cmdDet.ExecuteNonQuery();
                    }

                    foreach (var g in globos.Where(x => x.cantidad > 0))
                    {
                        SqlCommand cmdDet = new SqlCommand(
                            "INSERT INTO Detalle_Venta_Globo (ventaId, globoId, cantidad, costo, importe) VALUES (@ventaId,@globoId,@cant,@costo,@importe)", conn, tran);
                        cmdDet.Parameters.AddWithValue("@ventaId", ventaId);
                        cmdDet.Parameters.AddWithValue("@globoId", g.globoId);
                        cmdDet.Parameters.AddWithValue("@cant", g.cantidad);
                        cmdDet.Parameters.AddWithValue("@costo", g.costo);
                        cmdDet.Parameters.AddWithValue("@importe", g.cantidad * g.costo);
                        cmdDet.ExecuteNonQuery();
                    }

                    tran.Commit();
                    MessageBox.Show("Venta registrada correctamente.");
                    foreach (var p in productos) p.cantidad = 0;
                    foreach (var g in globos) g.cantidad = 0;
                    ActualizarResumen();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    MessageBox.Show("Error al registrar la venta: " + ex.Message);
                }
            }
        }

        private void BtnAumentarCantidad_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                if (btn.Tag is ProductoVenta p && p.cantidad < p.stock) p.cantidad++;
                else if (btn.Tag is GloboVenta g) g.cantidad++;
            }
        }

        private void BtnDisminuirCantidad_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                if (btn.Tag is ProductoVenta p && p.cantidad > 0) p.cantidad--;
                else if (btn.Tag is GloboVenta g && g.cantidad > 0) g.cantidad--;
            }
        }
    }
}