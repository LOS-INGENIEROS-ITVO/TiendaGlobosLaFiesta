using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TiendaGlobosLaFiesta.Data;
using TiendaGlobosLaFiesta.Inventario;
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
            try
            {
                List<ProductoVenta> lista = ConexionBD.ObtenerProductos();
                productos = new ObservableCollection<ProductoInventario>(
                    lista.Select(p => new ProductoInventario
                    {
                        ProductoId = p.ProductoId,
                        Nombre = p.Nombre,
                        Unidad = int.Parse(p.Unidad), // mantener string si así está en la BD
                        Stock = p.Stock,
                        Costo = p.Costo
                    })
                );

                foreach (var p in productos) p.PropertyChanged += Item_PropertyChanged;
                dgProductos.ItemsSource = productos;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar productos: " + ex.Message);
            }
        }

        private void CargarGlobos()
        {
            try
            {
                List<GloboVenta> lista = ConexionBD.ObtenerGlobos();
                globos = new ObservableCollection<GloboInventario>(
                    lista.Select(g => new GloboInventario
                    {
                        GloboId = g.GloboId,
                        Material = g.Material,
                        Unidad = g.Unidad,
                        Color = g.Color,
                        Stock = g.Stock,
                        Costo = g.Costo,
                        Tamanios = g.Tamano,   // ahora coincide con el alias de BD
                        Formas = g.Forma,
                        Tematicas = g.Tematica
                    })
                );

                foreach (var g in globos) g.PropertyChanged += Item_PropertyChanged;
                dgGlobos.ItemsSource = globos;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar globos: " + ex.Message);
            }
        }


        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ActualizarResumen();
        }

        private void ActualizarResumen()
        {
            txtTotalProductos.Text = productos?.Sum(p => p.Stock).ToString() ?? "0";
            txtTotalGlobos.Text = globos?.Sum(g => g.Stock).ToString() ?? "0";
            txtValorInventario.Text = ((productos?.Sum(p => p.Stock * p.Costo) ?? 0) +
                                       (globos?.Sum(g => g.Stock * g.Costo) ?? 0))
                                      .ToString("0.00");
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
