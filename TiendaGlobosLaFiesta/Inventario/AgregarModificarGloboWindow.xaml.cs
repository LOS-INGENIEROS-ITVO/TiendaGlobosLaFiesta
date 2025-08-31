using System;
using System.Windows;
using TiendaGlobosLaFiesta.Data;
using TiendaGlobosLaFiesta.Inventario;

namespace TiendaGlobosLaFiesta
{
    public partial class AgregarModificarGloboWindow : Window
    {
        private GloboInventario globo;

        public AgregarModificarGloboWindow(GloboInventario g = null)
        {
            InitializeComponent();
            globo = g;
            if (globo != null)
            {
                txtMaterial.Text = globo.Material;
                txtUnidad.Text = globo.Unidad;
                txtColor.Text = globo.Color;
                txtStock.Text = globo.Stock.ToString();
                txtCosto.Text = globo.Costo.ToString();
            }
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtMaterial.Text))
                    throw new Exception("El material es obligatorio.");

                int stock = int.Parse(txtStock.Text);
                decimal costo = decimal.Parse(txtCosto.Text);

                if (globo == null)
                {
                    // Agregar
                    ConexionBD.EjecutarNonQuery(
                        "INSERT INTO Globo (globoId, material, unidad, color, stock, costo) VALUES (UUID(), @Material, @Unidad, @Color, @Stock, @Costo)",
                        new[] {
                            ConexionBD.Param("@Material", txtMaterial.Text),
                            ConexionBD.Param("@Unidad", txtUnidad.Text),
                            ConexionBD.Param("@Color", txtColor.Text),
                            ConexionBD.Param("@Stock", stock),
                            ConexionBD.Param("@Costo", costo)
                        });
                }
                else
                {
                    // Modificar
                    ConexionBD.EjecutarNonQuery(
                        "UPDATE Globo SET material=@Material, unidad=@Unidad, color=@Color, stock=@Stock, costo=@Costo WHERE globoId=@Id",
                        new[] {
                            ConexionBD.Param("@Material", txtMaterial.Text),
                            ConexionBD.Param("@Unidad", txtUnidad.Text),
                            ConexionBD.Param("@Color", txtColor.Text),
                            ConexionBD.Param("@Stock", stock),
                            ConexionBD.Param("@Costo", costo),
                            ConexionBD.Param("@Id", globo.GloboId)
                        });
                }

                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}