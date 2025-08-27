using System;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TiendaGlobosLaFiesta.Models;

namespace TiendaGlobosLaFiesta
{
    public partial class InventarioControl : UserControl
    {
        private BindingList<Producto> productos;
        private BindingList<Globo> globos;

        private string connStr = @"Data Source=LALOVG25\SQLEXPRESS;Initial Catalog=Globeriadb;Integrated Security=True;";

        public InventarioControl()
        {
            InitializeComponent();
            CargarProductos();
            CargarGlobos();
        }

        private void CargarProductos()
        {
            productos = new BindingList<Producto>();
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT productoId, nombre, stock, costo FROM Producto", conn);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        productos.Add(new Producto
                        {
                            productoId = reader["productoId"].ToString(),
                            nombre = reader["nombre"].ToString(),
                            stock = Convert.ToInt32(reader["stock"]),
                            costo = Convert.ToDouble(reader["costo"])
                        });
                    }
                }
            }
            dgProductos.ItemsSource = productos;
        }

        private void CargarGlobos()
        {
            globos = new BindingList<Globo>();
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT globoId, material, color, stock, costo FROM Globo", conn);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        globos.Add(new Globo
                        {
                            globoId = reader["globoId"].ToString(),
                            material = reader["material"].ToString(),
                            color = reader["color"].ToString(),
                            stock = Convert.ToInt32(reader["stock"]),
                            costo = Convert.ToDouble(reader["costo"])
                        });
                    }
                }
            }
            dgGlobos.ItemsSource = globos;
        }

        private void BtnActualizar_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("¿Deseas actualizar el inventario?", "Confirmar", MessageBoxButton.YesNo) == MessageBoxResult.No)
                return;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                try
                {
                    foreach (var p in productos)
                    {
                        SqlCommand cmd = new SqlCommand("UPDATE Producto SET stock=@stock WHERE productoId=@id", conn, tran);
                        cmd.Parameters.AddWithValue("@stock", p.stock);
                        cmd.Parameters.AddWithValue("@id", p.productoId);
                        cmd.ExecuteNonQuery();
                    }

                    foreach (var g in globos)
                    {
                        SqlCommand cmd = new SqlCommand("UPDATE Globo SET stock=@stock WHERE globoId=@id", conn, tran);
                        cmd.Parameters.AddWithValue("@stock", g.stock);
                        cmd.Parameters.AddWithValue("@id", g.globoId);
                        cmd.ExecuteNonQuery();
                    }

                    tran.Commit();
                    MessageBox.Show("Inventario actualizado correctamente.");
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    MessageBox.Show("Error al actualizar inventario: " + ex.Message);
                }
            }
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            CargarProductos();
            CargarGlobos();
        }

        // Validación para solo números
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !int.TryParse(e.Text, out _);
        }
    }
}