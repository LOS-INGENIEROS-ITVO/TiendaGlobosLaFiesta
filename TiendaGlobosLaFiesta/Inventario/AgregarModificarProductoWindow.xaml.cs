using System;
using System.Windows;
using TiendaGlobosLaFiesta.Data;
using TiendaGlobosLaFiesta.Inventario;

namespace TiendaGlobosLaFiesta
{
    public partial class AgregarModificarProductoWindow : Window
    {
        private ProductoInventario _producto;
        private bool _esNuevo;

        public AgregarModificarProductoWindow(ProductoInventario producto = null)
        {
            InitializeComponent();

            if (producto == null)
            {
                _producto = new ProductoInventario();
                _esNuevo = true;
            }
            else
            {
                _producto = producto;
                _esNuevo = false;
                // Rellenar los TextBox con la info del producto existente
                txtNombre.Text = _producto.Nombre;
                txtUnidad.Text = _producto.Unidad.ToString();
                txtStock.Text = _producto.Stock.ToString();
                txtCosto.Text = _producto.Costo.ToString("0.00");
            }
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validaciones
                if (string.IsNullOrWhiteSpace(txtNombre.Text))
                {
                    MessageBox.Show("El nombre no puede estar vacío.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtNombre.Focus();
                    return;
                }

                if (!int.TryParse(txtUnidad.Text, out int unidad) || unidad <= 0)
                {
                    MessageBox.Show("La unidad debe ser un número mayor a 0.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtUnidad.Focus();
                    return;
                }

                if (!int.TryParse(txtStock.Text, out int stock) || stock < 0)
                {
                    MessageBox.Show("El stock debe ser un número válido (>=0).", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtStock.Focus();
                    return;
                }

                if (!decimal.TryParse(txtCosto.Text, out decimal costo) || costo <= 0)
                {
                    MessageBox.Show("El costo debe ser un número mayor a 0.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtCosto.Focus();
                    return;
                }

                // Actualizar el objeto
                _producto.Nombre = txtNombre.Text.Trim();
                _producto.Unidad = unidad;
                _producto.Stock = stock;
                _producto.Costo = costo;

                if (_esNuevo)
                {
                    ConexionBD.EjecutarNonQuery(
                        "INSERT INTO Producto (productoId, nombre, unidad, stock, costo) VALUES (@id, @nombre, @unidad, @stock, @costo)",
                        new[]
                        {
                            ConexionBD.Param("@id", _producto.ProductoId),
                            ConexionBD.Param("@nombre", _producto.Nombre),
                            ConexionBD.Param("@unidad", _producto.Unidad),
                            ConexionBD.Param("@stock", _producto.Stock),
                            ConexionBD.Param("@costo", _producto.Costo)
                        });
                }
                else
                {
                    ConexionBD.EjecutarNonQuery(
                        "UPDATE Producto SET nombre=@nombre, unidad=@unidad, stock=@stock, costo=@costo WHERE productoId=@id",
                        new[]
                        {
                            ConexionBD.Param("@id", _producto.ProductoId),
                            ConexionBD.Param("@nombre", _producto.Nombre),
                            ConexionBD.Param("@unidad", _producto.Unidad),
                            ConexionBD.Param("@stock", _producto.Stock),
                            ConexionBD.Param("@costo", _producto.Costo)
                        });
                }

                MessageBox.Show("Producto guardado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        // Navegación con Enter
        private void TxtNombre_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter) txtUnidad.Focus();
        }

        private void TxtUnidad_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter) txtStock.Focus();
        }

        private void TxtStock_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter) txtCosto.Focus();
        }

        private void TxtCosto_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter) BtnGuardar_Click(null, null);
        }
    }
}