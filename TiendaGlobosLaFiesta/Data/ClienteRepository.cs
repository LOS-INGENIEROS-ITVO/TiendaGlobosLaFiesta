using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using TiendaGlobosLaFiesta.Models.Clientes;

namespace TiendaGlobosLaFiesta.Data
{
    public class ClienteRepository
    {
        // ========================
        // CRUD Básico
        // ========================

        public bool AgregarCliente(Cliente cliente, out string mensaje)
        {
            mensaje = string.Empty;
            try
            {
                if (ClienteExiste(cliente.ClienteId))
                {
                    mensaje = $"El cliente con ID '{cliente.ClienteId}' ya existe.";
                    return false;
                }

                string query = @"INSERT INTO Cliente (clienteId, primerNombre, segundoNombre, apellidoP, apellidoM, telefono)
                                 VALUES (@id, @nombre1, @nombre2, @apellidoP, @apellidoM, @telefono)";

                var parametros = new[]
                {
                    new SqlParameter("@id", cliente.ClienteId),
                    new SqlParameter("@nombre1", cliente.PrimerNombre),
                    new SqlParameter("@nombre2", (object?)cliente.SegundoNombre ?? DBNull.Value),
                    new SqlParameter("@apellidoP", cliente.ApellidoP),
                    new SqlParameter("@apellidoM", cliente.ApellidoM),
                    new SqlParameter("@telefono", (object?)cliente.Telefono ?? DBNull.Value)
                };

                bool result = DbHelper.ExecuteNonQuery(query, parametros) > 0;
                mensaje = result ? "Cliente agregado correctamente." : "No se pudo agregar el cliente.";
                return result;
            }
            catch (Exception ex)
            {
                mensaje = "Error al agregar cliente: " + ex.Message;
                return false;
            }
        }

        public bool ActualizarCliente(Cliente cliente, out string mensaje)
        {
            mensaje = string.Empty;
            try
            {
                if (!ClienteExiste(cliente.ClienteId))
                {
                    mensaje = $"El cliente con ID '{cliente.ClienteId}' no existe.";
                    return false;
                }

                string query = @"UPDATE Cliente 
                                 SET primerNombre=@nombre1, segundoNombre=@nombre2, 
                                     apellidoP=@apellidoP, apellidoM=@apellidoM, telefono=@telefono
                                 WHERE clienteId=@id";

                var parametros = new[]
                {
                    new SqlParameter("@id", cliente.ClienteId),
                    new SqlParameter("@nombre1", cliente.PrimerNombre),
                    new SqlParameter("@nombre2", (object?)cliente.SegundoNombre ?? DBNull.Value),
                    new SqlParameter("@apellidoP", cliente.ApellidoP),
                    new SqlParameter("@apellidoM", cliente.ApellidoM),
                    new SqlParameter("@telefono", (object?)cliente.Telefono ?? DBNull.Value)
                };

                bool result = DbHelper.ExecuteNonQuery(query, parametros) > 0;
                mensaje = result ? "Cliente actualizado correctamente." : "No se pudo actualizar el cliente.";
                return result;
            }
            catch (Exception ex)
            {
                mensaje = "Error al actualizar cliente: " + ex.Message;
                return false;
            }
        }

        public bool EliminarCliente(string clienteId, out string mensaje)
        {
            mensaje = string.Empty;
            try
            {
                if (!ClienteExiste(clienteId))
                {
                    mensaje = $"El cliente con ID '{clienteId}' no existe.";
                    return false;
                }

                string query = "UPDATE Cliente SET Activo = 0 WHERE clienteId=@id";
                var parametros = new[] { new SqlParameter("@id", clienteId) };
                bool result = DbHelper.ExecuteNonQuery(query, parametros) > 0;
                mensaje = result ? "Cliente eliminado correctamente." : "No se pudo eliminar el cliente.";
                return result;
            }
            catch (Exception ex)
            {
                mensaje = "Error al eliminar cliente: " + ex.Message;
                return false;
            }
        }

        public bool ClienteExiste(string clienteId)
        {
            string query = "SELECT COUNT(1) FROM Cliente WHERE clienteId=@id";
            var parametros = new[] { new SqlParameter("@id", clienteId) };
            return Convert.ToInt32(DbHelper.ExecuteScalar(query, parametros)) > 0;
        }

        // ========================
        // Obtener Clientes
        // ========================

        public List<Cliente> ObtenerClientes()
        {
            string query = "SELECT clienteId, primerNombre, segundoNombre, apellidoP, apellidoM, telefono FROM Cliente WHERE Activo = 1";
            DataTable dt = DbHelper.ExecuteQuery(query);

            var lista = new List<Cliente>();
            foreach (DataRow row in dt.Rows)
            {
                lista.Add(MapearCliente(row));
            }
            return lista;
        }

        public Cliente? ObtenerClientePorId(string clienteId)
        {
            string query = @"SELECT clienteId, primerNombre, segundoNombre, apellidoP, apellidoM, telefono
                             FROM Cliente WHERE clienteId=@id AND Activo = 1";

            var parametros = new[] { new SqlParameter("@id", clienteId) };
            DataTable dt = DbHelper.ExecuteQuery(query, parametros);

            if (dt.Rows.Count == 0) return null;
            return MapearCliente(dt.Rows[0]);
        }

        public List<Cliente> ObtenerClientesFrecuentes(int top = 5)
        {
            string query = @"
                SELECT TOP (@top) c.clienteId, c.primerNombre, c.segundoNombre, c.apellidoP, c.apellidoM, c.telefono
                FROM Venta v
                JOIN Cliente c ON v.clienteId = c.clienteId
                WHERE c.Activo = 1
                GROUP BY c.clienteId, c.primerNombre, c.segundoNombre, c.apellidoP, c.apellidoM, c.telefono
                ORDER BY COUNT(v.ventaId) DESC";

            var parametros = new[] { new SqlParameter("@top", top) };
            DataTable dt = DbHelper.ExecuteQuery(query, parametros);

            var lista = new List<Cliente>();
            foreach (DataRow row in dt.Rows)
            {
                lista.Add(MapearCliente(row));
            }
            return lista;
        }

        public List<Cliente> ObtenerClientesPorFecha(DateTime inicio, DateTime fin)
        {
            string query = @"
                SELECT DISTINCT c.clienteId, c.primerNombre, c.segundoNombre, c.apellidoP, c.apellidoM, c.telefono
                FROM Venta v
                JOIN Cliente c ON v.clienteId = c.clienteId
                WHERE v.fechaVenta BETWEEN @inicio AND @fin AND c.Activo = 1";

            var parametros = new[]
            {
                new SqlParameter("@inicio", inicio),
                new SqlParameter("@fin", fin)
            };

            DataTable dt = DbHelper.ExecuteQuery(query, parametros);
            var lista = new List<Cliente>();
            foreach (DataRow row in dt.Rows)
            {
                lista.Add(MapearCliente(row));
            }
            return lista;
        }

        public List<Cliente> BuscarClientes(string criterio)
        {
            string query = @"
                SELECT clienteId, primerNombre, segundoNombre, apellidoP, apellidoM, telefono
                FROM Cliente
                WHERE Activo = 1 AND
                      (primerNombre LIKE @c OR segundoNombre LIKE @c OR apellidoP LIKE @c OR apellidoM LIKE @c)";

            var parametros = new[] { new SqlParameter("@c", $"%{criterio}%") };
            DataTable dt = DbHelper.ExecuteQuery(query, parametros);

            var lista = new List<Cliente>();
            foreach (DataRow row in dt.Rows)
            {
                lista.Add(MapearCliente(row));
            }
            return lista;
        }

        // ========================
        // Métodos Auxiliares
        // ========================

        private Cliente MapearCliente(DataRow row)
        {
            return new Cliente
            {
                ClienteId = SafeString(row["clienteId"]),
                PrimerNombre = SafeString(row["primerNombre"]),
                SegundoNombre = SafeString(row["segundoNombre"]),
                ApellidoP = SafeString(row["apellidoP"]),
                ApellidoM = SafeString(row["apellidoM"]),
                Telefono = SafeLongNullable(row["telefono"])
            };
        }

        private string SafeString(object value) => value == DBNull.Value ? "" : value.ToString()!;
        private long? SafeLongNullable(object value) =>
            value == DBNull.Value ? null : Convert.ToInt64(value);
    }
}