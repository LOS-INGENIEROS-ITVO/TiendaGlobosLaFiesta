using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TiendaGlobosLaFiesta.Data;
using TiendaGlobosLaFiesta.Models;

namespace TiendaGlobosLaFiesta.Views
{
    public partial class ProductoEditWindow : Window
    {
        public Producto Producto { get; private set; }
        private bool esEdicion;
        private readonly ProductoRepository repo = new();

        public ProductoEditWindow(Producto producto = null)
        {
            InitializeComponent();
            CargarOpciones();

            if (producto == null)
            {
                esEdicion = false;
                Producto = new Producto { ProductoId = GenerarId() };
            }
            else
            {
                esEdicion = true;
                Producto = producto;
                CargarDatos();
            }

            txtProductoId.Text = Producto.ProductoId;
            txtNombre.Focus();
        }

        private void CargarOpciones()
        {
            // Proveedores
            var dtProv = DbHelper.ExecuteQuery("SELECT proveedorId, razonSocial FROM Proveedor WHERE Activo = 1 ORDER BY razonSocial");
            cmbProveedor.ItemsSource = dtProv.AsEnumerable()
                .Select(r => new { Id = r["proveedorId"], Nombre = r["razonSocial"] }).ToList();
            cmbProveedor.DisplayMemberPath = "Nombre";
            cmbProveedor.SelectedValuePath = "Id";

            // Categorías
            var dtCat = DbHelper.ExecuteQuery("SELECT categoriaId, nombre FROM Categoria ORDER BY nombre");
            cmbCategoria.ItemsSource = dtCat.AsEnumerable()
                .Select(r => new { Id = r["categoriaId"], Nombre = r["nombre"] }).ToList();
            cmbCategoria.DisplayMemberPath = "Nombre";
            cmbCategoria.SelectedValuePath = "Id";

            cmbProveedor.SelectedIndex = -1;
            cmbCategoria.SelectedIndex = -1;
        }

        private void CargarDatos()
        {
            txtNombre.Text = Producto.Nombre;
            txtUnidad.Text = Producto.Unidad.ToString();
            txtCosto.Text = Producto.Costo.ToString(CultureInfo.InvariantCulture);
            txtStock.Text = Producto.Stock.ToString();
            cmbProveedor.SelectedValue = Producto.ProveedorId;
            cmbCategoria.SelectedValue = Producto.CategoriaId;
        }

        private string GenerarId() => $"PRD{DateTime.Now:yyMMddHHmmss}";

        #region Validaciones en tiempo real

        private void TxtNumero_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^[0-9]+$");
        }

        private void TxtDecimal_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^[0-9]*(?:\.[0-9]*)?$");
        }

        private void MarcarError(Control control, string mensaje)
        {
            control.BorderBrush = Brushes.Red;
            control.ToolTip = mensaje;
        }

        private void LimpiarErrores()
        {
            foreach (var ctrl in new Control[] { txtNombre, txtUnidad, txtCosto, txtStock, cmbProveedor, cmbCategoria })
            {
                ctrl.ClearValue(Border.BorderBrushProperty);
                ctrl.ClearValue(ToolTipProperty);
            }
        }

        #endregion

        #region Guardar

        private void Guardar_Click(object sender, RoutedEventArgs e) => Guardar(false);

        private void GuardarYAgregar_Click(object sender, RoutedEventArgs e) => Guardar(true);

        private void Guardar(bool agregarOtro)
        {
            LimpiarErrores();

            if (!ValidarCampos()) return;
            AsignarCamposAObjeto();

            try
            {
                if (esEdicion)
                    repo.ActualizarProducto(Producto);
                else
                {
                    if (repo.ObtenerProductos(false).Any(p => p.Nombre.Equals(txtNombre.Text.Trim(), StringComparison.OrdinalIgnoreCase)))
                    {
                        MarcarError(txtNombre, "Ya existe un producto con este nombre.");
                        return;
                    }
                    repo.AgregarProducto(Producto);
                }

                lblMensaje.Text = "✅ Producto guardado exitosamente";
                lblMensaje.Visibility = Visibility.Visible;

                if (agregarOtro)
                {
                    esEdicion = false;
                    Producto = new Producto { ProductoId = GenerarId() };
                    txtProductoId.Text = Producto.ProductoId;
                    txtNombre.Clear();
                    txtUnidad.Clear();
                    txtCosto.Clear();
                    txtStock.Clear();
                    cmbProveedor.SelectedIndex = -1;
                    cmbCategoria.SelectedIndex = -1;
                    txtNombre.Focus();
                }
                else
                {
                    DialogResult = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error al guardar el producto:\n{ex.Message}",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AsignarCamposAObjeto()
        {
            Producto.Nombre = txtNombre.Text.Trim();


            if (int.TryParse(txtUnidad.Text, out int unidad) && unidad > 0)
            {
                Producto.Unidad = unidad.ToString(); // Convertimos a string
            }
            else
            {
                MarcarError(txtUnidad, "Unidad inválida.");
            }

            Producto.Costo = decimal.Parse(txtCosto.Text, CultureInfo.InvariantCulture);
            Producto.Stock = int.Parse(txtStock.Text);
            Producto.ProveedorId = cmbProveedor.SelectedValue?.ToString();
            Producto.CategoriaId = cmbCategoria.SelectedValue != null ? Convert.ToInt32(cmbCategoria.SelectedValue) : (int?)null;
        }

        private bool ValidarCampos()
        {
            bool esValido = true;

            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MarcarError(txtNombre, "Ingrese el nombre.");
                esValido = false;
            }

            if (!int.TryParse(txtUnidad.Text, out int unidad) || unidad <= 0)
            {
                MarcarError(txtUnidad, "Unidad inválida.");
                esValido = false;
            }

            if (!decimal.TryParse(txtCosto.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal costo) || costo < 0)
            {
                MarcarError(txtCosto, "Costo inválido.");
                esValido = false;
            }

            if (!int.TryParse(txtStock.Text, out int stock) || stock < 0)
            {
                MarcarError(txtStock, "Stock inválido.");
                esValido = false;
            }

            if (cmbProveedor.SelectedItem == null)
            {
                MarcarError(cmbProveedor, "Seleccione un proveedor.");
                esValido = false;
            }

            if (cmbCategoria.SelectedItem == null)
            {
                MarcarError(cmbCategoria, "Seleccione una categoría.");
                esValido = false;
            }

            return esValido;
        }

        #endregion

        #region Cancelar

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtNombre.Text) ||
                !string.IsNullOrWhiteSpace(txtUnidad.Text) ||
                !string.IsNullOrWhiteSpace(txtCosto.Text) ||
                !string.IsNullOrWhiteSpace(txtStock.Text) ||
                cmbProveedor.SelectedItem != null ||
                cmbCategoria.SelectedItem != null)
            {
                if (MessageBox.Show("Los cambios no guardados se perderán. ¿Desea continuar?",
                    "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                    return;
            }
            DialogResult = false;
        }

        #endregion

        private void Window_Loaded(object sender, RoutedEventArgs e) => txtNombre.Focus();

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Cancelar_Click(null, null);
            else if (e.Key == Key.Enter)
                Guardar_Click(null, null);
        }
    }
}