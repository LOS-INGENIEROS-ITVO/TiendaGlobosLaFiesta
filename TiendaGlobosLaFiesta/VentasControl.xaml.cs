using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace TiendaGlobosLaFiesta
{
    public partial class VentasControl : UserControl
    {
        private BindingList<ProductoVenta> productos;
        private BindingList<GloboVenta> globos;

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
            string connStr = @"Data Source=LALOVG25\SQLEXPRESS;Initial Catalog=Globeriadb;Integrated Security=True;";
            List<Cliente> clientes = new List<Cliente>();
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT clienteId, primerNombre, apellidoP FROM Cliente", conn);
                SqlDataReader reader = cmd.ExecuteReader();
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
            cmbClientes.ItemsSource = clientes;
        }

        private void CargarProductos()
        {
            productos = new BindingList<ProductoVenta>
            {
                new ProductoVenta { productoId="P001", nombre="Pegamento", stock=50, cantidad=0, costo=15.5 },
                new ProductoVenta { productoId="P002", nombre="Cinta", stock=30, cantidad=0, costo=10.0 }
            };
            foreach (var p in productos)
                p.PropertyChanged += Item_PropertyChanged;
            dgProductos.ItemsSource = productos;
        }

        private void CargarGlobos()
        {
            globos = new BindingList<GloboVenta>
            {
                new GloboVenta { globoId="G001", material="Latex", color="Rojo", cantidad=0, costo=5.0 },
                new GloboVenta { globoId="G002", material="Metalicos", color="Azul", cantidad=0, costo=8.0 }
            };
            foreach (var g in globos)
                g.PropertyChanged += Item_PropertyChanged;
            dgGlobos.ItemsSource = globos;
        }

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ActualizarResumen();
        }

        private void ActualizarResumen()
        {
            txtTotalProductos.Text = productos.Sum(p => p.cantidad).ToString();
            txtTotalGlobos.Text = globos.Sum(g => g.cantidad).ToString();
            double total = productos.Sum(p => p.cantidad * p.costo) + globos.Sum(g => g.cantidad * g.costo);
            txtImporteTotal.Text = total.ToString("0.00");
        }

        private void BtnRegistrarVenta_Click(object sender, RoutedEventArgs e)
        {
            if (cmbClientes.SelectedItem is not Cliente cliente)
            {
                MessageBox.Show("Selecciona un cliente.");
                return;
            }

            if (productos.Sum(p => p.cantidad) == 0 && globos.Sum(g => g.cantidad) == 0)
            {
                MessageBox.Show("Agrega productos o globos a la venta.");
                return;
            }

            string connStr = @"Data Source=LALOVG25\SQLEXPRESS;Initial Catalog=Globeriadb;Integrated Security=True;";
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                try
                {
                    string ventaId = "V" + DateTime.Now.ToString("yyyyMMddHHmmss");

                    // Insertar venta
                    SqlCommand cmdVenta = new SqlCommand(
                        "INSERT INTO Venta (ventaId, empleadoId, clienteId, fechaVenta, importeTotal) VALUES (@ventaId,@empleadoId,@clienteId,@fecha,@total)", conn, tran);
                    cmdVenta.Parameters.AddWithValue("@ventaId", ventaId);
                    cmdVenta.Parameters.AddWithValue("@empleadoId", 1); // Temporal
                    cmdVenta.Parameters.AddWithValue("@clienteId", cliente.clienteId);
                    cmdVenta.Parameters.AddWithValue("@fecha", DateTime.Now);
                    double total = productos.Sum(p => p.cantidad * p.costo) + globos.Sum(g => g.cantidad * g.costo);
                    cmdVenta.Parameters.AddWithValue("@total", total);
                    cmdVenta.ExecuteNonQuery();

                    // Detalles productos
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

                    // Detalles globos
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
                    // Reiniciar cantidades
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
    }

    public class Cliente
    {
        public string clienteId { get; set; }
        public string primerNombre { get; set; }
        public string apellidoP { get; set; }
    }

    public class ProductoVenta : INotifyPropertyChanged
    {
        public string productoId { get; set; }
        public string nombre { get; set; }
        public int stock { get; set; }
        private int _cantidad;
        public int cantidad
        {
            get => _cantidad;
            set { _cantidad = value; OnPropertyChanged(nameof(cantidad)); OnPropertyChanged(nameof(Importe)); }
        }
        public double costo { get; set; }
        public double Importe => cantidad * costo;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class GloboVenta : INotifyPropertyChanged
    {
        public string globoId { get; set; }
        public string material { get; set; }
        public string color { get; set; }
        private int _cantidad;
        public int cantidad
        {
            get => _cantidad;
            set { _cantidad = value; OnPropertyChanged(nameof(cantidad)); OnPropertyChanged(nameof(Importe)); }
        }
        public double costo { get; set; }
        public double Importe => cantidad * costo;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}