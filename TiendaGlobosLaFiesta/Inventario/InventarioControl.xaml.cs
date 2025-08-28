using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TiendaGlobosLaFiesta.Data;
using TiendaGlobosLaFiesta.Models;

namespace TiendaGlobosLaFiesta
{
    public partial class InventarioControl : UserControl
    {
        private ObservableCollection<ProductoInventario> productos;
        private ObservableCollection<GloboInventario> globos;

        public InventarioControl()
        {
            InitializeComponent();
            CargarProductos();
            CargarGlobos();
            ActualizarResumen();
        }

        private void CargarProductos()
        {
            var dt = ConexionBD.EjecutarConsulta("SELECT productoId, nombre, unidad, stock, costo FROM Producto");
            productos = dt.Rows.Cast<DataRow>().Select(r =>
            {
                var p = new ProductoInventario
                {
                    ProductoId = r["productoId"].ToString(),
                    Nombre = r["nombre"].ToString(),
                    Unidad = Convert.ToInt32(r["unidad"]),
                    Stock = Convert.ToInt32(r["stock"]),
                    Costo = Convert.ToDecimal(r["costo"])
                };
                p.PropertyChanged += Item_PropertyChanged;
                return p;
            }).ToObservableCollection();

            dgProductos.ItemsSource = productos;
        }

        private void CargarGlobos()
        {
            var dt = ConexionBD.EjecutarConsulta("SELECT globoId, material, color, unidad, stock, costo FROM Globo");
            globos = dt.Rows.Cast<DataRow>().Select(r =>
            {
                var g = new GloboInventario
                {
                    GloboId = r["globoId"].ToString(),
                    Material = r["material"].ToString(),
                    Color = r["color"].ToString(),
                    Unidad = r["unidad"].ToString(),
                    Stock = Convert.ToInt32(r["stock"]),
                    Costo = Convert.ToDecimal(r["costo"])
                };
                g.PropertyChanged += Item_PropertyChanged;
                return g;
            }).ToObservableCollection();

            dgGlobos.ItemsSource = globos;
        }

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ActualizarResumen();
        }

        private void ActualizarResumen()
        {
            txtTotalProductos.Text = productos.Sum(p => p.Stock).ToString();
            txtTotalGlobos.Text = globos.Sum(g => g.Stock).ToString();
            txtValorInventario.Text = (productos.Sum(p => p.Stock * p.Costo) + globos.Sum(g => g.Stock * g.Costo)).ToString("0.00");
        }

        #region Botones Productos
        private void BtnAgregarProducto_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new AgregarModificarProductoWindow();
            if (ventana.ShowDialog() == true)
            {
                CargarProductos();
                ActualizarResumen();
            }
        }

        private void BtnModificarProducto_Click(object sender, RoutedEventArgs e)
        {
            if (dgProductos.SelectedItem is ProductoInventario seleccionado)
            {
                var ventana = new AgregarModificarProductoWindow(seleccionado);
                if (ventana.ShowDialog() == true)
                {
                    CargarProductos();
                    ActualizarResumen();
                }
            }
            else MessageBox.Show("Selecciona un producto para modificar.");
        }

        private void BtnEliminarProducto_Click(object sender, RoutedEventArgs e)
        {
            if (dgProductos.SelectedItem is ProductoInventario seleccionado)
            {
                if (MessageBox.Show($"¿Eliminar producto {seleccionado.Nombre}?", "Confirmar", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    ConexionBD.EjecutarNonQuery("DELETE FROM Producto WHERE productoId=@id",
                        new[] { ConexionBD.Param("@id", seleccionado.ProductoId) });
                    CargarProductos();
                    ActualizarResumen();
                }
            }
            else MessageBox.Show("Selecciona un producto para eliminar.");
        }
        #endregion

        #region Botones Globos
        private void BtnAgregarGlobo_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new AgregarModificarGloboWindow();
            if (ventana.ShowDialog() == true)
            {
                CargarGlobos();
                ActualizarResumen();
            }
        }

        private void BtnModificarGlobo_Click(object sender, RoutedEventArgs e)
        {
            if (dgGlobos.SelectedItem is GloboInventario seleccionado)
            {
                var ventana = new AgregarModificarGloboWindow(seleccionado);
                if (ventana.ShowDialog() == true)
                {
                    CargarGlobos();
                    ActualizarResumen();
                }
            }
            else MessageBox.Show("Selecciona un globo para modificar.");
        }

        private void BtnEliminarGlobo_Click(object sender, RoutedEventArgs e)
        {
            if (dgGlobos.SelectedItem is GloboInventario seleccionado)
            {
                if (MessageBox.Show($"¿Eliminar globo {seleccionado.GloboId}?", "Confirmar", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    ConexionBD.EjecutarNonQuery("DELETE FROM Globo WHERE globoId=@id",
                        new[] { ConexionBD.Param("@id", seleccionado.GloboId) });
                    CargarGlobos();
                    ActualizarResumen();
                }
            }
            else MessageBox.Show("Selecciona un globo para eliminar.");
        }
        #endregion
    }
}