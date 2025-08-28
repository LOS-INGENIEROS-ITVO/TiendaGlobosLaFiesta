using System;
using System.Windows;
using TiendaGlobosLaFiesta.Data;
using TiendaGlobosLaFiesta.Models;

namespace TiendaGlobosLaFiesta
{
    public partial class AgregarModificarProductoWindow : Window
    {
        private ProductoInventario producto;
        private bool esNuevo;

        public AgregarModificarProductoWindow(ProductoInventario producto = null)
        {
            InitializeComponent();

            if (producto == null)
            {
                this.producto = new ProductoInventario();
                esNuevo = true;
            }
            else
            {
                this.producto = producto;
                txtNombre.Text = producto.Nombre;
                txtUnidad.Text = producto.Unidad.ToString();
                txtStock.Text = producto.Stock.ToString();
                txtCosto.Text = producto.Costo.ToString();
                esNuevo = false;
            }
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                producto.Nombre = txtNombre.Text.Trim();
                producto.Unidad = int.Parse(txtUnidad.Text);
                producto.Stock = int.Parse(txtStock.Text);
                producto.Costo = decimal.Parse(txtCosto.Text);

                if (esNuevo)
                {
                    string nuevoId = Guid.NewGuid().ToString();
                    producto.ProductoId = nuevoId;
                    ConexionBD.EjecutarNonQuery(
                        "INSERT INTO Producto (productoId, nombre, unidad, stock, costo) VALUES (@id,@nombre,@unidad,@stock,@costo)",
                        new[] {
                            ConexionBD.Param("@id", producto.ProductoId),
                            ConexionBD.Param("@nombre", producto.Nombre),
                            ConexionBD.Param("@unidad", producto.Unidad),
                            ConexionBD.Param("@stock", producto.Stock),
                            ConexionBD.Param("@costo", producto.Costo)
                        });
                }
                else
                {
                    ConexionBD.EjecutarNonQuery(
                        "UPDATE Producto SET nombre=@nombre, unidad=@unidad, stock=@stock, costo=@costo WHERE productoId=@id",
                        new[] {
                            ConexionBD.Param("@nombre", producto.Nombre),
                            ConexionBD.Param("@unidad", producto.Unidad),
                            ConexionBD.Param("@stock", producto.Stock),
                            ConexionBD.Param("@costo", producto.Costo),
                            ConexionBD.Param("@id", producto.ProductoId)
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
