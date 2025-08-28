using System;
using System.Windows;
using TiendaGlobosLaFiesta.Data;
using TiendaGlobosLaFiesta.Models;

namespace TiendaGlobosLaFiesta
{
    public partial class AgregarModificarGloboWindow : Window
    {
        private GloboInventario globo;
        private bool esNuevo;

        public AgregarModificarGloboWindow(GloboInventario globo = null)
        {
            InitializeComponent();

            if (globo == null)
            {
                this.globo = new GloboInventario();
                esNuevo = true;
            }
            else
            {
                this.globo = globo;
                cbMaterial.Text = globo.Material;
                txtColor.Text = globo.Color;
                txtUnidad.Text = globo.Unidad;
                txtStock.Text = globo.Stock.ToString();
                txtCosto.Text = globo.Costo.ToString();
                esNuevo = false;
            }
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                globo.Material = cbMaterial.Text;
                globo.Color = txtColor.Text.Trim();
                globo.Unidad = txtUnidad.Text.Trim();
                globo.Stock = int.Parse(txtStock.Text);
                globo.Costo = decimal.Parse(txtCosto.Text);

                if (esNuevo)
                {
                    string nuevoId = Guid.NewGuid().ToString();
                    globo.GloboId = nuevoId;
                    ConexionBD.EjecutarNonQuery(
                        "INSERT INTO Globo (globoId, material, color, unidad, stock, costo) VALUES (@id,@material,@color,@unidad,@stock,@costo)",
                        new[] {
                            ConexionBD.Param("@id", globo.GloboId),
                            ConexionBD.Param("@material", globo.Material),
                            ConexionBD.Param("@color", globo.Color),
                            ConexionBD.Param("@unidad", globo.Unidad),
                            ConexionBD.Param("@stock", globo.Stock),
                            ConexionBD.Param("@costo", globo.Costo)
                        });
                }
                else
                {
                    ConexionBD.EjecutarNonQuery(
                        "UPDATE Globo SET material=@material, color=@color, unidad=@unidad, stock=@stock, costo=@costo WHERE globoId=@id",
                        new[] {
                            ConexionBD.Param("@material", globo.Material),
                            ConexionBD.Param("@color", globo.Color),
                            ConexionBD.Param("@unidad", globo.Unidad),
                            ConexionBD.Param("@stock", globo.Stock),
                            ConexionBD.Param("@costo", globo.Costo),
                            ConexionBD.Param("@id", globo.GloboId)
                        });
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
