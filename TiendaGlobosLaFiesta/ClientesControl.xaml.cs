using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using TiendaGlobosLaFiesta.Models;

namespace TiendaGlobosLaFiesta
{
    public partial class ClientesControl : UserControl
    {
        private BindingList<Cliente> clientes;
        private string connStr = @"Data Source=LALOVG25\SQLEXPRESS;Initial Catalog=Globeriadb;Integrated Security=True;";

        public ClientesControl()
        {
            InitializeComponent();
            CargarClientes();
        }

        private void CargarClientes()
        {
            try
            {
                clientes = new BindingList<Cliente>();

                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    string query = @"SELECT clienteId, primerNombre, segundoNombre, apellidoP, apellidoM, telefono 
                                     FROM Cliente";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            clientes.Add(new Cliente
                            {
                                clienteId = reader["clienteId"].ToString(),
                                primerNombre = reader["primerNombre"]?.ToString() ?? "",
                                segundoNombre = reader["segundoNombre"] != DBNull.Value ? reader["segundoNombre"].ToString() : "",
                                apellidoP = reader["apellidoP"]?.ToString() ?? "",
                                apellidoM = reader["apellidoM"] != DBNull.Value ? reader["apellidoM"].ToString() : "",
                                telefono = reader["telefono"] != DBNull.Value ? reader["telefono"].ToString() : ""
                            });
                        }
                    }
                }

                dgClientes.ItemsSource = clientes;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar clientes: " + ex.Message);
            }
        }

        private void BtnAgregarCliente_Click(object sender, RoutedEventArgs e)
        {
            Cliente nuevo = new Cliente
            {
                clienteId = Guid.NewGuid().ToString(),
                primerNombre = "",
                segundoNombre = "",
                apellidoP = "",
                apellidoM = "",
                telefono = ""
            };
            clientes.Add(nuevo);
            dgClientes.SelectedItem = nuevo;
            dgClientes.ScrollIntoView(nuevo);
        }

        private void BtnGuardarCambios_Click(object sender, RoutedEventArgs e)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                try
                {
                    foreach (var c in clientes)
                    {
                        if (string.IsNullOrWhiteSpace(c.primerNombre) || string.IsNullOrWhiteSpace(c.apellidoP))
                        {
                            MessageBox.Show("El nombre y apellido paterno son obligatorios.");
                            continue;
                        }

                        SqlCommand cmdCheck = new SqlCommand("SELECT COUNT(*) FROM Cliente WHERE clienteId=@id", conn, tran);
                        cmdCheck.Parameters.AddWithValue("@id", c.clienteId);
                        int count = (int)cmdCheck.ExecuteScalar();

                        if (count == 0)
                        {
                            SqlCommand cmdInsert = new SqlCommand(
                                "INSERT INTO Cliente (clienteId, primerNombre, segundoNombre, apellidoP, apellidoM, telefono) " +
                                "VALUES (@id,@nombre,@segundo,@apP,@apM,@tel)", conn, tran);

                            cmdInsert.Parameters.AddWithValue("@id", c.clienteId);
                            cmdInsert.Parameters.AddWithValue("@nombre", c.primerNombre);
                            cmdInsert.Parameters.AddWithValue("@segundo", string.IsNullOrEmpty(c.segundoNombre) ? (object)DBNull.Value : c.segundoNombre);
                            cmdInsert.Parameters.AddWithValue("@apP", c.apellidoP);
                            cmdInsert.Parameters.AddWithValue("@apM", string.IsNullOrEmpty(c.apellidoM) ? (object)DBNull.Value : c.apellidoM);
                            cmdInsert.Parameters.AddWithValue("@tel", string.IsNullOrEmpty(c.telefono) ? (object)DBNull.Value : c.telefono);

                            cmdInsert.ExecuteNonQuery();
                        }
                        else
                        {
                            SqlCommand cmdUpdate = new SqlCommand(
                                "UPDATE Cliente SET primerNombre=@nombre, segundoNombre=@segundo, apellidoP=@apP, apellidoM=@apM, telefono=@tel " +
                                "WHERE clienteId=@id", conn, tran);

                            cmdUpdate.Parameters.AddWithValue("@id", c.clienteId);
                            cmdUpdate.Parameters.AddWithValue("@nombre", c.primerNombre);
                            cmdUpdate.Parameters.AddWithValue("@segundo", string.IsNullOrEmpty(c.segundoNombre) ? (object)DBNull.Value : c.segundoNombre);
                            cmdUpdate.Parameters.AddWithValue("@apP", c.apellidoP);
                            cmdUpdate.Parameters.AddWithValue("@apM", string.IsNullOrEmpty(c.apellidoM) ? (object)DBNull.Value : c.apellidoM);
                            cmdUpdate.Parameters.AddWithValue("@tel", string.IsNullOrEmpty(c.telefono) ? (object)DBNull.Value : c.telefono);

                            cmdUpdate.ExecuteNonQuery();
                        }
                    }

                    tran.Commit();
                    MessageBox.Show("Cambios guardados correctamente.");
                    CargarClientes();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    MessageBox.Show("Error al guardar los cambios: " + ex.Message);
                }
            }
        }

        private void BtnEliminarCliente_Click(object sender, RoutedEventArgs e)
        {
            if (dgClientes.SelectedItem is Cliente cliente)
            {
                if (MessageBox.Show($"¿Deseas eliminar al cliente {cliente.primerNombre} {cliente.apellidoP}?",
                    "Confirmar", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    using (SqlConnection conn = new SqlConnection(connStr))
                    {
                        conn.Open();
                        SqlCommand cmd = new SqlCommand("DELETE FROM Cliente WHERE clienteId=@id", conn);
                        cmd.Parameters.AddWithValue("@id", cliente.clienteId);
                        cmd.ExecuteNonQuery();

                        clientes.Remove(cliente);
                        MessageBox.Show("Cliente eliminado correctamente.");
                    }
                }
            }
            else
            {
                MessageBox.Show("Selecciona un cliente para eliminar.");
            }
        }
    }
}