using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            CargarOpciones();

            if (producto == null)
            {
                esEdicion = false;
                Producto = new Producto();
            }
            else
            {
                esEdicion = true;
                Producto = producto;
                CargarDatos();
            }

            txtProductoId.Text = Producto.ProductoId ?? "";
            txtNombre.Focus();
        }

        #region Carga de opciones y datos
        private void CargarOpciones()
        {
            var dtProv = DbHelper.ExecuteQuery("SELECT proveedorId, razonSocial FROM Proveedor WHERE Activo = 1 ORDER BY razonSocial");
            cmbProveedor.ItemsSource = dtProv.AsEnumerable()
                .Select(r => new { Id = r["proveedorId"].ToString(), Nombre = r["razonSocial"].ToString() })
                .ToList();
            cmbProveedor.DisplayMemberPath = "Nombre";
            cmbProveedor.SelectedValuePath = "Id";

            var dtCat = DbHelper.ExecuteQuery("SELECT categoriaId, nombre FROM Categoria ORDER BY nombre");
            cmbCategoria.ItemsSource = dtCat.AsEnumerable()
                .Select(r => new { Id = Convert.ToInt32(r["categoriaId"]), Nombre = r["nombre"].ToString() })
                .ToList();
            cmbCategoria.DisplayMemberPath = "Nombre";
            cmbCategoria.SelectedValuePath = "Id";

            cmbProveedor.SelectedIndex = -1;
            cmbCategoria.SelectedIndex = -1;
        }

        private void CargarDatos()
        {
            txtProductoId.Text = Producto.ProductoId;
            txtNombre.Text = Producto.Nombre;
            txtUnidad.Text = Producto.Unidad;
            txtCosto.Text = Producto.Costo.ToString(CultureInfo.InvariantCulture);
            txtStock.Text = Producto.Stock.ToString();
            cmbProveedor.SelectedValue = Producto.ProveedorId;
            cmbCategoria.SelectedValue = Producto.CategoriaId;
        }
        #endregion

        #region Validaciones de campos
        private void TxtNumero_PreviewTextInput(object sender, TextCompositionEventArgs e)
            => e.Handled = !Regex.IsMatch(e.Text, @"^[0-9]+$");

        private void TxtDecimal_PreviewTextInput(object sender, TextCompositionEventArgs e)
            => e.Handled = !Regex.IsMatch(e.Text, @"^[0-9]*(?:\.[0-9]*)?$");

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
            lblMensaje.Visibility = Visibility.Collapsed;
        }

        private bool ValidarCampos()
        {
            bool esValido = true;

            if (string.IsNullOrWhiteSpace(txtNombre.Text) || txtNombre.Text.StartsWith("Ej."))
            {
                MarcarError(txtNombre, "Ingrese el nombre del producto.");
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

        #region Guardar producto
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
                {
                    repo.ActualizarProducto(Producto);
                }
                else
                {
                    // Verificar duplicado por nombre antes de insertar
                    if (repo.ObtenerProductos(false)
                            .Any(p => p.Nombre.Equals(Producto.Nombre, StringComparison.OrdinalIgnoreCase)))
                    {
                        MarcarError(txtNombre, "Ya existe un producto con este nombre.");
                        return;
                    }

                    // Insertar con lógica de generación de ID segura
                    repo.AgregarProducto(Producto);
                    txtProductoId.Text = Producto.ProductoId; // ID generado se refleja en pantalla
                }

                lblMensaje.Text = "✅ Producto guardado exitosamente";
                lblMensaje.Visibility = Visibility.Visible;

                Task.Delay(3000).ContinueWith(_ =>
                    Dispatcher.Invoke(() => lblMensaje.Visibility = Visibility.Collapsed));

                if (agregarOtro)
                    LimpiarParaAgregarOtro();
                else
                    DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error al guardar el producto:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Console.WriteLine($"[ERROR] {ex}");
            }
        }

        private void LimpiarParaAgregarOtro()
        {
            esEdicion = false;
            Producto = new Producto();
            txtProductoId.Text = "";
            txtNombre.Text = "Ej. Globo rojo 12''";
            txtUnidad.Clear();
            txtCosto.Clear();
            txtStock.Clear();
            cmbProveedor.SelectedIndex = -1;
            cmbCategoria.SelectedIndex = -1;
            txtNombre.Focus();
        }

        private void AsignarCamposAObjeto()
        {
            Producto.Nombre = txtNombre.Text.Trim();
            Producto.Unidad = txtUnidad.Text.Trim();
            Producto.Costo = decimal.Parse(txtCosto.Text, CultureInfo.InvariantCulture);
            Producto.Stock = int.Parse(txtStock.Text);
            Producto.ProveedorId = cmbProveedor.SelectedValue?.ToString();
            Producto.CategoriaId = cmbCategoria.SelectedValue != null ? Convert.ToInt32(cmbCategoria.SelectedValue) : (int?)null;
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

        #region Placeholder
        private void ClearPlaceholder(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb && tb.Text.StartsWith("Ej."))
                tb.Clear();
        }

        private void RestorePlaceholder(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb && string.IsNullOrWhiteSpace(tb.Text))
                tb.Text = "Ej. Globo rojo 12''";
        }
        #endregion
    }
}