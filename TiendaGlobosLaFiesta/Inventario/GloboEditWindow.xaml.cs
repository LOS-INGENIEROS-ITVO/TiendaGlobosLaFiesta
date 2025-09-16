using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TiendaGlobosLaFiesta.Models;

namespace TiendaGlobosLaFiesta.Views
{
    public partial class GloboEditWindow : Window
    {
        public Globo Globo { get; private set; }

        private ObservableCollection<string> _tamanos;
        private ObservableCollection<string> _formas;
        private ObservableCollection<string> _tematicas;

        public GloboEditWindow(Globo globo)
        {
            InitializeComponent();
            Globo = globo;
            CargarDatos();
        }

        private void CargarDatos()
        {
            txtGloboId.Text = Globo.GloboId;
            cmbMaterial.Text = Globo.Material;
            txtColor.Text = Globo.Color;
            txtCosto.Text = Globo.Costo.ToString();
            txtStock.Text = Globo.Stock.ToString();

            _tamanos = new ObservableCollection<string>(Globo.Tamanos);
            _formas = new ObservableCollection<string>(Globo.Formas);
            _tematicas = new ObservableCollection<string>(Globo.Tematicas);

            lstTamanos.ItemsSource = _tamanos;
            lstFormas.ItemsSource = _formas;
            lstTematicas.ItemsSource = _tematicas;
        }

        private void Guardar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(cmbMaterial.Text) || !decimal.TryParse(txtCosto.Text, out _) || !int.TryParse(txtStock.Text, out _))
            {
                MessageBox.Show("Rellene Material, Costo y Stock con valores válidos.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Globo.Material = cmbMaterial.Text;
            Globo.Color = txtColor.Text;
            Globo.Costo = decimal.Parse(txtCosto.Text);
            Globo.Stock = int.Parse(txtStock.Text);

            Globo.Tamanos = _tamanos.ToList();
            Globo.Formas = _formas.ToList();
            Globo.Tematicas = _tematicas.ToList();

            this.DialogResult = true;
        }

        #region Manejadores de Listas
        private void Agregar_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;

            ObservableCollection<string> coleccion = null;
            TextBox textBox = null;

            switch (button.Name)
            {
                case "btnAgregarTamano": coleccion = _tamanos; textBox = txtNuevoTamano; break;
                case "btnAgregarForma": coleccion = _formas; textBox = txtNuevaForma; break;
                case "btnAgregarTematica": coleccion = _tematicas; textBox = txtNuevaTematica; break;
            }

            if (coleccion != null && textBox != null && !string.IsNullOrWhiteSpace(textBox.Text))
            {
                coleccion.Add(textBox.Text.Trim());
                textBox.Clear();
            }
        }

        private void Quitar_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;

            ObservableCollection<string> coleccion = null;
            ListBox listBox = null;

            switch (button.Name)
            {
                case "btnQuitarTamano": coleccion = _tamanos; listBox = lstTamanos; break;
                case "btnQuitarForma": coleccion = _formas; listBox = lstFormas; break;
                case "btnQuitarTematica": coleccion = _tematicas; listBox = lstTematicas; break;
            }

            if (coleccion != null && listBox != null && listBox.SelectedItem != null)
            {
                coleccion.Remove(listBox.SelectedItem as string);
            }
        }
        #endregion
    }
}