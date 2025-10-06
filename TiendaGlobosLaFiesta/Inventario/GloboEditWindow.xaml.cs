using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TiendaGlobosLaFiesta.Data;
using TiendaGlobosLaFiesta.Models;

namespace TiendaGlobosLaFiesta.Views
{
    public partial class GloboEditWindow : Window
    {
        public Globo Globo { get; private set; }
        private bool esEdicion;

        private readonly GloboRepository repo = new();

        public GloboEditWindow(Globo globo = null)
        {
            InitializeComponent();
            CargarOpciones();

            if (globo == null)
            {
                esEdicion = false;
                Globo = new Globo { GloboId = GenerarId() };
            }
            else
            {
                esEdicion = true;
                Globo = globo;
                CargarDatos();
            }

            txtGloboId.Text = Globo.GloboId;
            cmbMaterial.Focus();
        }

        private void CargarOpciones()
        {
            cmbMaterial.ItemsSource = new List<string> { "Metalicos", "Latex", "Burbuja" };

            // Tamaños, Formas, Temáticas existentes en BD
            var globos = repo.ObtenerGlobos(false);
            lstTamano.ItemsSource = globos.SelectMany(g => g.Tamanos).Distinct().OrderBy(x => x).ToList();
            lstForma.ItemsSource = globos.SelectMany(g => g.Formas).Distinct().OrderBy(x => x).ToList();
            lstTematica.ItemsSource = globos.SelectMany(g => g.Tematicas).Distinct().OrderBy(x => x).ToList();

            // Proveedores activos
            var dt = DbHelper.ExecuteQuery("SELECT proveedorId, razonSocial FROM Proveedor WHERE Activo = 1 ORDER BY razonSocial");
            cmbProveedor.ItemsSource = dt.AsEnumerable()
                                         .Select(r => new { Id = r["proveedorId"], Nombre = r["razonSocial"] })
                                         .ToList();
            cmbProveedor.DisplayMemberPath = "Nombre";
            cmbProveedor.SelectedValuePath = "Id";
        }

        private void CargarDatos()
        {
            cmbMaterial.SelectedItem = Globo.Material;
            txtColor.Text = Globo.Color;
            txtUnidad.Text = Globo.Unidad;
            txtCosto.Text = Globo.Costo.ToString(CultureInfo.InvariantCulture);
            txtStock.Text = Globo.Stock.ToString();

            // Validar que los items existan antes de seleccionar
            foreach (var item in Globo.Tamanos)
                if (lstTamano.Items.Contains(item))
                    lstTamano.SelectedItems.Add(item);

            foreach (var item in Globo.Formas)
                if (lstForma.Items.Contains(item))
                    lstForma.SelectedItems.Add(item);

            foreach (var item in Globo.Tematicas)
                if (lstTematica.Items.Contains(item))
                    lstTematica.SelectedItems.Add(item);

            cmbProveedor.SelectedValue = Globo.ProveedorId;
        }

        private string GenerarId() => $"GLB{DateTime.Now:yyMMddHHmmss}";

        private void Guardar_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidarCampos()) return;
            AsignarCamposAObjeto();

            try
            {
                if (esEdicion)
                    repo.ActualizarGlobo(Globo);
                else
                    repo.AgregarGlobo(Globo);

                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error al guardar el globo:\n{ex.Message}",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GuardarYAgregar_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidarCampos()) return;
            AsignarCamposAObjeto();

            try
            {
                if (esEdicion)
                    repo.ActualizarGlobo(Globo);
                else
                    repo.AgregarGlobo(Globo);

                MessageBox.Show("Globo guardado exitosamente.", "Confirmación", MessageBoxButton.OK, MessageBoxImage.Information);

                // Limpiar para agregar otro
                esEdicion = false;
                Globo = new Globo { GloboId = GenerarId() };
                txtGloboId.Text = Globo.GloboId;
                cmbMaterial.SelectedIndex = -1;
                txtColor.Clear();
                txtUnidad.Clear();
                txtCosto.Clear();
                txtStock.Clear();
                cmbProveedor.SelectedIndex = -1;
                lstTamano.UnselectAll();
                lstForma.UnselectAll();
                lstTematica.UnselectAll();
                cmbMaterial.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error al guardar el globo:\n{ex.Message}",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AsignarCamposAObjeto()
        {
            Globo.Material = cmbMaterial.SelectedItem?.ToString();
            Globo.Color = txtColor.Text.Trim();
            Globo.Unidad = txtUnidad.Text.Trim();
            Globo.ProveedorId = cmbProveedor.SelectedValue?.ToString();

            Globo.Costo = decimal.Parse(txtCosto.Text, CultureInfo.InvariantCulture);
            Globo.Stock = int.Parse(txtStock.Text);

            Globo.Tamanos = lstTamano.SelectedItems.Cast<string>().ToList();
            Globo.Formas = lstForma.SelectedItems.Cast<string>().ToList();
            Globo.Tematicas = lstTematica.SelectedItems.Cast<string>().ToList();
        }

        private bool ValidarCampos()
        {
            ResetearBordes();
            List<string> errores = new();

            if (cmbMaterial.SelectedItem == null) errores.Add("Seleccione un material.");
            if (string.IsNullOrWhiteSpace(txtColor.Text)) errores.Add("Ingrese un color.");
            if (lstTamano.SelectedItems.Count == 0) errores.Add("Seleccione al menos un tamaño.");
            if (lstForma.SelectedItems.Count == 0) errores.Add("Seleccione al menos una forma.");
            if (string.IsNullOrWhiteSpace(txtUnidad.Text)) errores.Add("Ingrese la unidad.");
            if (!decimal.TryParse(txtCosto.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal costo) || costo < 0)
                errores.Add("Ingrese un costo válido >= 0.");
            if (!int.TryParse(txtStock.Text, out int stock) || stock < 0)
                errores.Add("Ingrese un stock válido >= 0.");
            if (cmbProveedor.SelectedItem == null) errores.Add("Seleccione un proveedor.");

            if (errores.Count > 0)
            {
                MessageBox.Show(string.Join("\n", errores), "Errores de validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void ResetearBordes()
        {
            foreach (var c in new Control[] { cmbMaterial, txtColor, txtUnidad, txtCosto, txtStock, cmbProveedor, lstTamano, lstForma, lstTematica })
            {
                c.ClearValue(Border.BorderBrushProperty);
                c.ClearValue(ToolTipProperty);
            }
        }

        private void Cancelar_Click(object sender, RoutedEventArgs e) => DialogResult = false;

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                (sender as UIElement)?.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) => cmbMaterial.Focus();

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                DialogResult = false;
        }
    }
}